using PB.DatabaseFramework;
using PB.Shared.Helpers;
using System.Data;
using PB.Shared.Enum;
using PB.Shared.Models.SuperAdmin.Client;
using PB.Model;
using Org.BouncyCastle.Asn1.Cmp;
using PB.Shared.Models.SuperAdmin.Custom;
using PB.Client.Pages.SuperAdmin;
using PB.Shared.Tables.Accounts.JournalMaster;
using PB.Shared.Tables.Accounts.Ledgers;
using PB.Shared.Models.Court;
using PB.Shared.Tables.CourtClient;
using PB.EntityFramework;
using PB.Shared;
using PB.Shared.Enum.Court;
using PB.Shared.Tables.Tax;

namespace PB.Server.Repository
{
    public interface ICourtPackageRepository
    {
        #region Handling Customer Court Package Related Entries
        Task HandleCustomerCourtPackageAccountsPurchaseEntries(CustomerCourtPackageDetailsModel model);
        #endregion
    }
    public class CourtPackageRepository : ICourtPackageRepository
    {
        private readonly IDbContext _dbContext;
        private readonly IEmailSender _email;
        private readonly IAccountRepository _account;
        private readonly INotificationRepository _notification;
        private readonly IDbConnection _cn;

        public CourtPackageRepository(IDbContext dbContext, IEmailSender email, IAccountRepository account, INotificationRepository notification, IDbConnection cn)
        {
            this._dbContext =dbContext;
            this._email =email;
            this._account =account;
            this._notification =notification;
            this._cn =cn;
        }

        public async Task HandleCustomerCourtPackageAccountsPurchaseEntries(CustomerCourtPackageDetailsModel model)
        {
            var salesJournalMasterData = await GetCustomerSalesJournalMasterBasicDatas(model.CustomerEntityID, model.ClientID, model.PaymentTypeID);
            if (salesJournalMasterData is not null)
            {
                var packageDetails = await GetCourtPackageDetails(model.PackageID);
                if (packageDetails is not null)
                {
                    packageDetails.Tax = 0;
                    if (packageDetails.TaxCategoryItems.Count > 0)
                    {
                        if (packageDetails.Package.IncTax)
                            packageDetails.Package.Fee=packageDetails.Package.Fee / (1 + (packageDetails.TaxCategoryItems.Sum(taxCategoryItem => taxCategoryItem.Percentage)/100));
                        packageDetails.Tax = packageDetails.Package.Fee * (packageDetails.TaxCategoryItems.Sum(taxCategoryItem => taxCategoryItem.Percentage) / 100);
                    }
                    packageDetails.GrossFee = packageDetails.Package.Fee + packageDetails.Tax;

                    _cn.Open();
                    using var transaction = _cn.BeginTransaction();
                    try
                    {

                        #region Accounts Entry

                        AccJournalMaster JournalMaster = new()
                        {
                            Date = DateTime.UtcNow.Date,
                            EntityID = model.CustomerEntityID,
                            JournalNo = salesJournalMasterData.VoucherNumber.JournalNo,
                            JournalNoPrefix = salesJournalMasterData.VoucherNumber.JournalNoPrefix,
                            VoucherTypeID = salesJournalMasterData.SalesVoucherTypeID,
                            BranchID = salesJournalMasterData.BranchID,
                        };
                        int JournalMasterID = await _dbContext.SaveAsync(JournalMaster, transaction);

                        #region Debit

                        List<AccJournalEntry> JournalEntries = new();
                        AccJournalEntry clientJournalEntry = new()
                        {
                            LedgerID = salesJournalMasterData.ReceiptLedgerID,
                            Debit = packageDetails.GrossFee,
                            Credit = 0
                        };
                        JournalEntries.Add(clientJournalEntry);

                        #endregion

                        #region Credit

                        AccJournalEntry salesJournalEntry = new()
                        {
                            LedgerID = salesJournalMasterData.SalesLedgerID,
                            Debit = 0,
                            Credit = packageDetails.Package.Fee
                        };
                        JournalEntries.Add(salesJournalEntry);

                        if (packageDetails.TaxCategoryItems.Count > 0)
                        {
                            foreach (var item in packageDetails.TaxCategoryItems)
                            {
                                AccJournalEntry TaxCategoryItemJournalEntry = new()
                                {
                                    LedgerID = item.LedgerID,
                                    Debit = 0,
                                    Credit = packageDetails.Package.Fee * (item.Percentage / 100)
                                };
                                JournalEntries.Add(TaxCategoryItemJournalEntry);
                            }
                        }

                        #endregion

                        await _dbContext.SaveSubItemListAsync(JournalEntries, "JournalMasterID", JournalMasterID, transaction);

                        #endregion

                        #region Court Purchase Entry

                        int numberLength = 8;
                        //string PurchaseCode = await GenerateRandomNumber(numberLength,transaction);
                        DateTime EndDate = DateTime.UtcNow.AddMonths(packageDetails.Package.ValidityMonth);
                        CourtPackagePurchase purchase = new()
                        {
                            CourtPackageID=model.PackageID,
                            EntityID=model.CustomerEntityID,
                            StartDate= model.StartDate,
                            EndDate= model.EndDate,
                            JournalMasterID= JournalMasterID,
                            PurchaseCode= model.PurchaseCode,
                        };
                        await _dbContext.SaveAsync(purchase, transaction);
                        transaction.Commit();
                        #endregion
                    }
                    catch (PBException err)
                    {
                        transaction.Rollback();
                        throw new Exception(err.Message);
                    }
                    catch (Exception err)
                    {
                        transaction.Rollback();
                        throw new Exception(err.Message);
                    }
                }
            }

        }

        private async Task<CourtSalesJournalMasterBasicDataModel> GetCustomerSalesJournalMasterBasicDatas(int customerEntityID, int ClientID, int PaymentTypeID, IDbTransaction? tran = null)
        {
            CourtSalesJournalMasterBasicDataModel salesJournalMasterData = new();

            salesJournalMasterData.BranchID = await _dbContext.GetByQueryAsync<int>(@$"Select Top 1 BranchID 
                                                                        From Branch B
                                                                        Where B.ClientID={ClientID} and B.IsDeleted=0
            ", null, tran);

            switch (PaymentTypeID)
            {
                case (int)PaymentTypes.Bank: //Bank
                    salesJournalMasterData.ReceiptLedgerID = await _dbContext.GetFieldsAsync<AccLedger, int>("LedgerID", $"LedgerTypeID={(int)LedgerTypes.Bank} AND ClientID=@ClientID AND IsDeleted=0", new { ClientID }, tran);
                    break;
                case (int)PaymentTypes.Cash : //Cash
                    salesJournalMasterData.ReceiptLedgerID = await _dbContext.GetFieldsAsync<AccLedger, int>("LedgerID", $"LedgerTypeID={(int)LedgerTypes.Cash} AND ClientID=@ClientID AND IsDeleted=0", new { ClientID }, tran);
                    break;
                case (int)PaymentTypes.Credit: //Credit
                    salesJournalMasterData.ReceiptLedgerID = await _account.GetEntityLedgerID(customerEntityID, ClientID, tran);
                    break;
            }
            salesJournalMasterData.SalesLedgerID = await _dbContext.GetFieldsAsync<AccLedger, int>("LedgerID", $"LedgerTypeID={(int)LedgerTypes.Sales} AND ClientID=@ClientID AND IsDeleted=0", new { ClientID }, tran);

            salesJournalMasterData.SalesVoucherTypeID = await _dbContext.GetByQueryAsync<int>($@"
                                                                        Select TOP 1 VoucherTypeID
                                                                        From AccVoucherType
                                                                        Where ClientID={ClientID} AND IsDeleted=0 AND VoucherTypeNatureID={(int)VoucherTypeNatures.Receipt}
            ", null, tran);

            salesJournalMasterData.VoucherNumber = await _account.GetVoucherNumber(salesJournalMasterData.SalesVoucherTypeID, salesJournalMasterData.BranchID, tran);

            return salesJournalMasterData ?? new();
        }
        private async Task<CourtPackageDetailsModel> GetCourtPackageDetails(int packageID, IDbTransaction? tran = null)
        {
            CourtPackageDetailsModel result = new();
            result.Package = await _dbContext.GetAsync<CourtPackage>(packageID, tran);

            if (result.Package.TaxCategoryID is not null)
                result.TaxCategoryItems = await _dbContext.GetListByQueryAsync<TaxCategoryItem>(@$"
                                Select * From TaxCategoryItem Where TaxCategoryID={result.Package.TaxCategoryID} And IsDeleted=0
                ", null, tran);

            return result;
        }
        private async Task<string> GenerateRandomNumber(int length,IDbTransaction transaction=null)
        {
            const string digits = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            char[] randomChars = new char[length];
            do
            {
                Random random = new Random();
                for (int i = 0; i < length; i++)
                {
                    randomChars[i] = digits[random.Next(0, digits.Length)];
                }
            } while (await _dbContext.GetAsync<CourtPackagePurchase>($"PurchaseCode=@randomChars", new {randomChars=new string(randomChars)}, transaction)!=null);
            return new string(randomChars);
        }

    }
}
