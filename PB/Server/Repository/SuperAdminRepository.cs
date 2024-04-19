using PB.DatabaseFramework;
using PB.Shared.Helpers;
using System.Data;
using PB.Shared.Tables;
using PB.Shared.Enum;
using PB.Shared.Models.SuperAdmin.Client;
using PB.Model;
using Org.BouncyCastle.Asn1.Cmp;
using PB.Shared.Models.SuperAdmin.Custom;
using PB.Client.Pages.SuperAdmin;
using PB.Shared.Tables.Accounts.JournalMaster;
using PB.Shared.Tables.Accounts.Ledgers;
using PB.EntityFramework;
using PB.Shared.Models;
using PB.Shared.Tables.Tax;

namespace PB.Server.Repository
{
    public interface ISuperAdminRepository
    {
        #region Handling Client Registration Package Related Entries

        Task<ClientInvoiceSaveReturnModel?> HandleClientPackageAccountsInvoiceEntries(ClientRegistrationPackageDetailsModel model, IDbTransaction? tran = null);

        #endregion

        #region Handling Checkout Package Change Entries

        Task<ClientInvoiceSaveReturnModel?> HandleCheckoutPackageChange(ClientRegistrationPackageDetailsModel model, IDbTransaction? tran = null);

        #endregion

        #region Daily Package Expire Checkup

        Task<List<InvoiceExpiryDetailsModel>?> DaywisePackageExpireCheckup();

        #endregion

        #region Client Payment Updation

        Task<ClientInvoiceSaveReturnModel?> InsertPaymentVerificationEntries(PaymentVerificationPostModel verificationModel, int currentUserID, IDbTransaction? tran = null);
        Task<ClientInvoiceSaveReturnModel?> InsertClientPaymentRejectionEntries(int invoiceID, int currentUserID, IDbTransaction? tran = null);

        #endregion

        #region Updating Clients Existing package

        Task<ClientInvoiceSaveReturnModel?> HandleUpdateClientPackage(ClientRegistrationPackageDetailsModel model, IDbTransaction? tran = null);

        #endregion

        #region Other General Functions

        Task<int> GeneratePaymentStatus(int clientID, IDbTransaction? tran = null);
        Task<int> GetClientPaymentStatus(int UserID, int ClientID, IDbTransaction? tran = null);

        #endregion

        #region Importing templates for all WA Accounts

        Task ImportAllWAAccountTemplates();

        #endregion

        #region Get Package Role Groups

        Task<List<int>?> FetchPackageRoleGroups(int packageID, IDbTransaction? tran = null);

        #endregion

        #region Tax category

        Task<TaxCategoryModel> GetTaxCategoryDetailsById(int taxCategoryID);
        Task<List<TaxCategoryItemModel>> GetTaxCategoryItems(int taxCategoryID);

        #endregion
    }

    public class SuperAdminRepository : ISuperAdminRepository
    {
        private readonly IDbContext _dbContext;
        private readonly IAccountRepository _account;
        private readonly IWhatsappRepository _whatsapp;

        public SuperAdminRepository(IDbContext dbCotext, IAccountRepository account, IWhatsappRepository whatsapp)
        {
            this._dbContext = dbCotext;
            this._account = account;
            this._whatsapp = whatsapp;
        }

        #region Handling Client Registration Package Related Entries

        public async Task<ClientInvoiceSaveReturnModel?> HandleClientPackageAccountsInvoiceEntries(ClientRegistrationPackageDetailsModel model, IDbTransaction? tran = null)
        {
            ClientInvoiceSaveReturnModel? returnModel = null;

            var salesJournalMasterData = await GetSalesJournalMasterBasicDatas(model.ClientEntityID, tran);

            if (salesJournalMasterData is not null)
            {
                var packageDetails = await GetPackageDetails(model.PackageID, tran);
                if (packageDetails is not null)
                {
                    packageDetails.NetFee = packageDetails.Package.Fee - model.Discount;
                    packageDetails.Tax = 0;
                    if (packageDetails.TaxCategoryItems.Count > 0)
                    {
                        packageDetails.Tax = packageDetails.NetFee * ((packageDetails.TaxCategoryItems.Sum(taxCategoryItem => taxCategoryItem.Percentage)) / 100);
                    }
                    packageDetails.GrossFee = packageDetails.NetFee + packageDetails.Tax;

                    #region Accounts Entry

                    AccJournalMaster JournalMaster = new();

                    List<AccJournalEntry> JournalEnries = new();

                    if (model.InvoiceJournalMasterID == 0)
                    {
                        JournalMaster = new()
                        {
                            Date = DateTime.UtcNow.Date,
                            EntityID = model.ClientEntityID,
                            JournalNo = salesJournalMasterData.VoucherNumber.JournalNo,
                            JournalNoPrefix = salesJournalMasterData.VoucherNumber.JournalNoPrefix,
                            VoucherTypeID = salesJournalMasterData.SalesVoucherTypeID,
                            BranchID = salesJournalMasterData.ProgbizBranchID,
                        };

                        #region Debit

                        AccJournalEntry clientJournalEntry = new()
                        {
                            LedgerID = salesJournalMasterData.ClientLedgerID,
                            Debit = packageDetails.GrossFee,
                            Credit = 0
                        };
                        JournalEnries.Add(clientJournalEntry);

                        if (model.Discount > 0)
                        {
                            AccJournalEntry discountJournalEntry = new()
                            {
                                LedgerID = salesJournalMasterData.DiscountLedgerID,
                                Debit = model.Discount,
                                Credit = 0
                            };
                            JournalEnries.Add(discountJournalEntry);
                        }

                        #endregion

                        #region Credit

                        AccJournalEntry salesJournalEntry = new()
                        {
                            LedgerID = salesJournalMasterData.SalesLedgerID,
                            Debit = 0,
                            Credit = packageDetails.Package.Fee
                        };
                        JournalEnries.Add(salesJournalEntry);

                        if (packageDetails.TaxCategoryItems.Count > 0)
                        {
                            foreach (var item in packageDetails.TaxCategoryItems)
                            {
                                AccJournalEntry TaxCategoryItemJournalEntry = new()
                                {
                                    LedgerID = item.LedgerID,
                                    Debit = 0,
                                    Credit = packageDetails.NetFee * (item.Percentage / 100)
                                };
                                JournalEnries.Add(TaxCategoryItemJournalEntry);
                            }
                        }

                        #endregion

                    }
                    else
                    {
                        JournalMaster = await _dbContext.GetAsync<AccJournalMaster>(model.InvoiceJournalMasterID, tran);
                        JournalMaster.Date = DateTime.UtcNow;

                        JournalEnries = await _dbContext.GetListByQueryAsync<AccJournalEntry>(@$"
                                                                Select * From AccJournalEntry Where JournalMasterID={model.InvoiceJournalMasterID} And IsDeleted=0
                        ", null, tran);

                        if (JournalEnries.Count > 0)
                        {
                            #region Debit

                            JournalEnries.Where(journalEntry => journalEntry.LedgerID == salesJournalMasterData.ClientLedgerID).First().Debit = packageDetails.GrossFee;
                            if (model.Discount > 0)
                            {
                                JournalEnries.Where(journalEntry => journalEntry.LedgerID == salesJournalMasterData.DiscountLedgerID).First().Debit = model.Discount;
                            }

                            #endregion

                            #region Credit

                            JournalEnries.Where(journalEntry => journalEntry.LedgerID == salesJournalMasterData.SalesLedgerID).First().Credit = packageDetails.Package.Fee;
                            if (packageDetails.TaxCategoryItems.Count > 0)
                            {
                                foreach (var taxCategoryitem in packageDetails.TaxCategoryItems)
                                {
                                    decimal amount = packageDetails.NetFee * (taxCategoryitem.Percentage / 100);
                                    JournalEnries.Where(journalEntry => journalEntry.LedgerID == taxCategoryitem.LedgerID).First().Credit = amount;
                                }
                            }

                            #endregion
                        }
                    }

                    ClientPackageAccountsModel clientPackageAccountsModel = new ClientPackageAccountsModel()
                    {
                        JournalMaster = JournalMaster,
                        JournalEntries = JournalEnries
                    };

                    returnModel = new();
                    returnModel.JournalMasterID = await InsertAccountEntries(clientPackageAccountsModel, tran);

                    #endregion

                    #region Invoice Entry

                    if (model.InvoiceID == 0)
                    {
                        returnModel.InvoiceID = await _dbContext.SaveAsync(new ClientInvoice()
                        {
                            InvoiceDate = DateTime.UtcNow,
                            ClientID = model.ClientID,
                            DisconnectionDate = DateTime.UtcNow.AddDays(5),
                            InvoiceJournalMasterID = JournalMaster.JournalMasterID,
                            PackageID = packageDetails.Package.PackageID,
                            PaidStatus = model.PaymentStatus,
                            Fee = packageDetails.Package.Fee,
                            Discount = model.Discount,
                            TaxCategoryID = packageDetails.Package.TaxCategoryID,
                            TaxPercentage = packageDetails.TaxCategoryItems.Sum(taxCategoryItem => taxCategoryItem.Percentage),
                            NetFee = packageDetails.NetFee,
                            Tax = packageDetails.Tax,
                            GrossFee = packageDetails.GrossFee,
                            InvoiceNo = await _dbContext.GetByQueryAsync<int>($@"Select ISNULL(Max(InvoiceNo),0 ) + 1 AS InvoiceNo 
                                                                        From ClientInvoice 
                                                                        Where IsDeleted=0 ", null, tran),

                        }, tran);
                    }
                    else
                    {
                        var clientInvoice = await _dbContext.GetAsync<ClientInvoice>(model.InvoiceID, tran);
                        if (clientInvoice is not null)
                        {
                            clientInvoice.PackageID = model.PackageID;
                            clientInvoice.PaidStatus = model.PaymentStatus;
                            clientInvoice.Fee = packageDetails.Package.Fee;
                            clientInvoice.Discount = model.Discount;
                            clientInvoice.TaxCategoryID = packageDetails.Package.TaxCategoryID;
                            clientInvoice.TaxPercentage = packageDetails.TaxCategoryItems.Sum(taxCategoryItem => taxCategoryItem.Percentage);
                            clientInvoice.NetFee = packageDetails.NetFee;
                            clientInvoice.Tax = packageDetails.Tax;
                            clientInvoice.GrossFee = packageDetails.GrossFee;
                            returnModel.InvoiceID = await _dbContext.SaveAsync(clientInvoice, tran);
                        }
                    }

                    #endregion
                }
            }

            if (returnModel is not null) return returnModel;
            else return null;
        }

        #endregion

        #region Handling Checkout Package Change Entries

        public async Task<ClientInvoiceSaveReturnModel?> HandleCheckoutPackageChange(ClientRegistrationPackageDetailsModel model, IDbTransaction? tran = null)
        {

            #region Client Package Updation

            ClientCustom Client = await _dbContext.GetAsync<ClientCustom>(model.ClientID, tran);
            if (Client != null)
            {
                Client.PackageID = model.PackageID;
                await _dbContext.SaveAsync(Client, tran);
            }

            #endregion

            #region Accounts Entry and Client Invoice Updation

            var result = await HandleClientPackageAccountsInvoiceEntries(model, tran);

            #endregion

            return result;

        }

        #endregion

        #region Daily Package Expire Checkup

        public async Task<List<InvoiceExpiryDetailsModel>?> DaywisePackageExpireCheckup()
        {

            var expiringInvoiceDetailsList = await GetClientPackageInvoiceExpiringDetailsList();

            if (expiringInvoiceDetailsList != null && expiringInvoiceDetailsList.Count > 0)
            {
                return expiringInvoiceDetailsList;


                //foreach (var listItem in expiringInvoiceDetailsList)
                //{
                //    ClientRegistrationPackageDetailsModel newInvoiceModel = new()
                //    {
                //        ClientID = listItem.ClientID,
                //        ClientEntityID = listItem.ClientEntityID,
                //        PackageID = listItem.PackageID,
                //        PaymentStatus = (int)PaymentStatus.Pending,
                //    };

                //    var result = await HandleClientRegisterPackageEntries(newInvoiceModel);
                //    if (result is not null)
                //    {
                //        var packageModel = await _dbContext.GetByQueryAsync<PacageIDnNameModel>(@$"
                //                                                Select MP.PackageID,MP.PackageName
                //                                                From ClientInvoice CI
                //                                                Left Join MembershipPackage MP ON MP.PackageID=CI.PackageID AND MP.IsDeleted=0
                //                                                Where CI.InvoiceID={result.InvoiceID} AND CI.IsDeleted=0
                //        ");

                //        if (packageModel is not null)
                //        {
                //            string pdfName = "pdf_invoice_" + packageModel.PackageName?.ToLower().Replace(' ', '_');

                //            int pdfMediaID = await _newPDf.CreateClientInvoicePdf(result.InvoiceID, pdfName);

                //            if (pdfMediaID > 0)
                //            {
                //                var mailDetails = await _newCommon.GetInvoiceEMailDetailsForClient(result.InvoiceID);
                //                if (mailDetails is not null)
                //                    await _newCommon.SendInvoiceEmailToClient(mailDetails);

                //                await _notification.SendNotificationWithPush(listItem.ClientEntityID, $"A new invoice has been generated. Kindly make payment before the due date {listItem.EndDate} to avoid any package disconnection.", "New Invoice", NotificationTypes.Invoice.ToString(), 1, "", null, notificationTypeId: (int)NotificationTypes.Invoice);
                //            }
                //        }
                //    }
                //}
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region Client Payment Verification And Rejection

        public async Task<ClientInvoiceSaveReturnModel?> InsertPaymentVerificationEntries(PaymentVerificationPostModel verificationModel, int currentUserID, IDbTransaction? tran = null)
        {
            ClientInvoiceSaveReturnModel? returnModel = null;

            var invoiceDatails = await GetInvoiceDetails(verificationModel.InvoiceID, tran);

            if (invoiceDatails is not null)
            {
                var recieptJournalMasterDatas = await GetRecieptJournalMasterBasicData(invoiceDatails.ClientEntityID, verificationModel.RecieptVoucherTypeID, tran);
                if (recieptJournalMasterDatas is not null)
                {
                    List<AccJournalEntry> JournalEnries = new();

                    #region Accounts Entry

                    AccJournalMaster JournalMaster = new()
                    {
                        Date = DateTime.UtcNow,
                        EntityID = invoiceDatails.ClientEntityID,
                        JournalNo = recieptJournalMasterDatas.Vouchernumber.JournalNo,
                        JournalNoPrefix = recieptJournalMasterDatas.Vouchernumber.JournalNoPrefix,
                        VoucherTypeID = verificationModel.RecieptVoucherTypeID,
                        BranchID = recieptJournalMasterDatas.ProgbizBranchID,
                        IsSuccess = true,
                        IsVerified = true,
                        VerifiedBy = currentUserID,
                        VerifiedOn = DateTime.UtcNow.Date,
                    };

                    #region Debit

                    AccJournalEntry bankRecieptJournalEntry = new()
                    {
                        LedgerID = recieptJournalMasterDatas.BankLedgerID,
                        Debit = invoiceDatails.GrossFee,
                        Credit = 0
                    };

                    JournalEnries.Add(bankRecieptJournalEntry);

                    #endregion

                    #region Credit

                    AccJournalEntry clientAccountJournalEntry = new()
                    {
                        LedgerID = recieptJournalMasterDatas.ClientLedgerID,
                        Debit = 0,
                        Credit = invoiceDatails.GrossFee
                    };

                    JournalEnries.Add(clientAccountJournalEntry);

                    #endregion

                    returnModel = returnModel ?? new();

                    ClientPackageAccountsModel clientPackageAccountsModel = new ClientPackageAccountsModel()
                    {
                        JournalMaster = JournalMaster,
                        JournalEntries = JournalEnries
                    };

                    returnModel.JournalMasterID = await InsertAccountEntries(clientPackageAccountsModel, tran);

                    #endregion

                    #region Invoice Updation

                    ClientInvoice Invoice = await _dbContext.GetAsync<ClientInvoice>(invoiceDatails.InvoiceID, tran);
                    Invoice.ReceiptJournalMasterID = returnModel.JournalMasterID;
                    Invoice.StartDate = invoiceDatails.StartDate;
                    Invoice.EndDate = invoiceDatails.EndDate;
                    Invoice.DisconnectionDate = invoiceDatails.DisconnectionDate;
                    Invoice.PaidStatus = (int)PaymentStatus.Verified;
                    returnModel.InvoiceID = await _dbContext.SaveAsync(Invoice, tran);

                    #endregion

                    #region User login status updation

                    await _dbContext.ExecuteAsync($"UPDATE Users SET LoginStatus=1 Where ClientID={invoiceDatails.ClientID}", null, tran);

                    #endregion

                }
            }

            if (returnModel is not null) return returnModel;
            else return null;
        }

        public async Task<ClientInvoiceSaveReturnModel?> InsertClientPaymentRejectionEntries(int invoiceID, int currentUserID, IDbTransaction? tran = null)
        {
            ClientInvoiceSaveReturnModel? returnModel = null;

            var invoiceDatails = await GetInvoiceDetails(invoiceID, tran);

            if (invoiceDatails != null)
            {
                #region Client Invoice Rejection

                await _dbContext.ExecuteAsync(@$"UPDATE ClientInvoice
                                                SET PaidStatus={(int)PaymentStatus.Rejected}
                                                Where InvoiceID={invoiceDatails.InvoiceID}
                ", null, tran);

                #endregion

                #region Accounts Entry Deletion

                await _dbContext.ExecuteAsync(@$"UPDATE AccJournalMaster
                                                SET IsDeleted=1,IsSuccess=0 
                                                Where JournalMasterID={invoiceDatails.InvoiceJournalMasterID}
                ", null, tran);

                await _dbContext.ExecuteAsync(@$"UPDATE AccJournalEntry 
                                                SET IsDeleted=1 
                                                Where JournalMasterID={invoiceDatails.InvoiceJournalMasterID}
                ", null, tran);

                #endregion

                ClientRegistrationPackageDetailsModel clientPackageDetailsModel = new()
                {
                    ClientID = invoiceDatails.ClientID,
                    ClientEntityID = invoiceDatails.ClientEntityID,
                    PackageID = invoiceDatails.PackageID,
                    PaymentStatus = (int)PaymentStatus.Pending,
                };

                returnModel = await HandleClientPackageAccountsInvoiceEntries(clientPackageDetailsModel, tran);

            }

            if (returnModel is not null) return returnModel;
            else return null;
        }

        #endregion

        #region Updating Clients Existing package

        public async Task<ClientInvoiceSaveReturnModel?> HandleUpdateClientPackage(ClientRegistrationPackageDetailsModel model, IDbTransaction? tran = null)
        {
            #region Old Invoice Updation

            await _dbContext.ExecuteAsync($@"UPDATE ClientInvoice
                                                SET EndDate='{DateTime.UtcNow}',DisconnectionDate='{DateTime.UtcNow}'
                                                Where InvoiceID={model.InvoiceID} and ClientID={model.ClientID}
            ", null, tran);

            #endregion

            #region Client package updation

            await _dbContext.ExecuteAsync($@"UPDATE Client
                                                SET PackageID={model.PackageID}
                                                Where ClientID={model.ClientID}
            ", null, tran);

            #endregion

            #region New Package Accounts Entry and Invoice Entry

            model.InvoiceID = 0;
            model.PaymentStatus = (int)PaymentStatus.Pending;
            var result = await HandleClientPackageAccountsInvoiceEntries(model, tran);
            return result;

            #endregion
        }

        #endregion

        #region Other General Functions

        public async Task<int> GeneratePaymentStatus(int clientID, IDbTransaction? tran = null)
        {

            var paidStatus = await _dbContext.GetByQueryAsync<int?>($@"Select I.PaidStatus From(
                                                            Select ClientID,Max(InvoiceID)as InvoiceID From ClientInvoice
                                                            Where isDeleted=0 
                                                            Group By ClientID) as A 
                                                            Left Join Clientinvoice I on I.InvoiceID=A.InvoiceID  and I.IsDeleted=0
                                                            Where A.ClientID={clientID} and ReceiptJournalMasterID IS NULL and Isdeleted=0", null, tran);

            if (paidStatus == (int)PaymentStatus.CheckoutNotComplete)
            {
                return ((int)RenewalStatus.NotCompleted);
            }
            else
            {

                var res = await _dbContext.GetByQueryAsync<int?>($@"Select A.InvoiceID From(
                                                        Select ClientID,Max(InvoiceID)as InvoiceID From ClientInvoice
                                                        Where isDeleted=0 
                                                        Group By ClientID) as A 
                                                        Left Join Clientinvoice I on I.InvoiceID=A.InvoiceID  and I.IsDeleted=0
                                                        Where A.ClientID={clientID} and ReceiptJournalMasterID Is NULL and Isdeleted=0 and PaidStatus in({(int)PaymentStatus.Pending},{(int)PaymentStatus.Rejected})", null, tran);

                if (res != null)
                {
                    var Dates = await _dbContext.GetByQueryAsync<ClientPaymentDateModel>($@"Select DisconnectionDate,DueDate,InvoiceDate From ClientInvoice
                                                                                        Where ClientID={clientID} and InvoiceID={res} and ReceiptJournalMasterID IS NULL and Isdeleted=0", null, tran);

                    if (Dates.DisconnectionDate < DateTime.UtcNow)
                    {

                        return ((int)RenewalStatus.Disconnected);
                    }
                    else if (Dates.DueDate < DateTime.UtcNow)
                    {
                        return ((int)RenewalStatus.Due);
                    }
                    else
                    {
                        return ((int)RenewalStatus.Generated);
                    }
                }
            }

            return ((int)RenewalStatus.Paid);
        }
        public async Task<int> GetClientPaymentStatus(int UserID, int ClientID, IDbTransaction? tran = null)
        {
            var res = await _dbContext.GetByQueryAsync<int?>($@"Select A.InvoiceID From(
                                                        Select ClientID,Max(InvoiceID)as InvoiceID From ClientInvoice
                                                        Where isDeleted=0 
                                                        Group By ClientID) as A 
                                                        Left Join Clientinvoice I on I.InvoiceID=A.InvoiceID  and I.IsDeleted=0
                                                        Where A.ClientID={ClientID} and ReceiptJournalMasterID Is NULL and Isdeleted=0 and PaidStatus in({(int)PaymentStatus.Pending},{(int)PaymentStatus.Rejected})", null, tran);

            if (res != null)
            {
                var Dates = await _dbContext.GetByQueryAsync<ClientPaymentDateModel>($@"Select DisconnectionDate,InvoiceDate From ClientInvoice
                                                                                        Where ClientID={ClientID} and InvoiceID={res} and ReceiptJournalMasterID IS NULL and Isdeleted=0", null, tran);

                if (Dates.DisconnectionDate < DateTime.UtcNow)
                {

                    return ((int)RenewalStatus.Disconnected);
                }
            }
            return ((int)RenewalStatus.Paid);
        }

        #endregion

        #region Client Payment Updation Related function

        private async Task<ClientInvoiceBasicDetailsModel> GetInvoiceDetails(int invoiceID, IDbTransaction? tran = null)
        {
            var result = await _dbContext.GetByQueryAsync<ClientInvoiceBasicDetailsModel>(@$"
                                                    Select CI.InvoiceID,CI.InvoiceJournalMasterID,CI.ClientID,CI.Fee,CI.NetFee,CI.GrossFee,CI.PackageID,C.EntityID As ClientEntityID,E.EmailAddress as Email,MP.PackageID,MP.PackageName,MP.Fee,MonthCount
                                                    From Client C
                                                    Join ClientInvoice CI on C.ClientID=CI.ClientID and CI.IsDeleted=0
                                                    Join MembershipPackage MP on MP.PackageID=CI.PackageID and MP.IsDeleted=0
                                                    Join viEntity E ON E.EntityID=C.EntityID
                                                    Join Users U on U.ClientID=C.ClientID and U.IsDeleted=0
                                                    Join MembershipPlan P on P.PlanID=MP.PlanID and P.IsDeleted=0
                                                    Where CI.InvoiceID={invoiceID} and C.IsDeleted=0
            ", null, tran);

            result.StartDate = result.StartDate is null ? DateTime.UtcNow : result.StartDate;
            result.EndDate = result.StartDate.Value.AddMonths(result.MonthCount);
            result.DisconnectionDate = result.EndDate.Value.AddDays(5);

            return result;
        }



        #endregion

        #region Daily Package Expire Checkup Related function

        private async Task<List<InvoiceExpiryDetailsModel>?> GetClientPackageInvoiceExpiringDetailsList()
        {
            var expiringList = await _dbContext.GetByQueryAsync<PackageExpireModel>(@$"
                                                    Select Days,String_AGG(ClientID,',') ClientIDs 
                                                    From
                                                    (	SELECT DATEDIFF(Day, GETUTCDATE(), EndDate)AS Days,ClientID
	                                                    FROM ClientInvoice
	                                                    Group By  EndDate,ClientID
                                                    ) as A
                                                    Where Days=5
                                                    Group By  Days
            ", null);

            if (expiringList != null && !string.IsNullOrEmpty(expiringList.ClientIDs))
            {
                var expiringInvoiceDetals = await GetExpiringInvoiceDetails(expiringList.ClientIDs);
                return expiringInvoiceDetals;
            }

            return null;
        }

        private async Task<List<InvoiceExpiryDetailsModel>> GetExpiringInvoiceDetails(string clientIDs)
        {
            var result = await _dbContext.GetListByQueryAsync<InvoiceExpiryDetailsModel>(@$"
                                                    Select C.EntityID As ClientEntityID,C.ClientID,C.PackageID,B.InvoiceID
                                                    From Client C
	                                                    Left Join 
	                                                    (
		                                                    Select PaidStatus as PaymentStatus,A.InvoiceID,I.ClientID,Fee,EndDate,DisconnectionDate 
		                                                    From
		                                                    (
			                                                    Select ClientID,Max(InvoiceID)as InvoiceID 
			                                                    From ClientInvoice
			                                                    Where isDeleted=0 
			                                                    Group By ClientID
		                                                    ) as A 
		                                                    Left Join Clientinvoice I on I.InvoiceID=A.InvoiceID 
	                                                    ) as B on B.ClientID=C.ClientID
                                                    Join MembershipPackage MP on MP.PackageID=C.PackageID and MP.IsDeleted=0
                                                    --Join Users U on U.ClientID=C.ClientID and U.IsDeleted=0
                                                    Join MembershipPlan P on P.PlanID=MP.PlanID and P.IsDeleted=0
                                                    Where C.ClientID IN ({clientIDs}) and C.IsDeleted=0
            ", null);

            return result;
        }

        #endregion

        #region Client Package Accounts Invoice Related function

        private async Task<SalesJournalMasterBasicDataModel> GetSalesJournalMasterBasicDatas(int clientEntityID, IDbTransaction? tran = null)
        {
            SalesJournalMasterBasicDataModel salesJournalMasterData = new();

            salesJournalMasterData.ProgbizBranchID = await _dbContext.GetByQueryAsync<int>(@$"Select Top 1 BranchID 
                                                                        From Branch B
                                                                        Where B.ClientID={PDV.ProgbizClientID} and B.IsDeleted=0
            ", null, tran);

            salesJournalMasterData.ClientLedgerID = await _account.GetEntityLedgerID(clientEntityID, PDV.ProgbizClientID, tran);

            salesJournalMasterData.SalesLedgerID = await _dbContext.GetFieldsAsync<AccLedger, int>("LedgerID", $"LedgerName=@LedgerName AND ClientID=@ClientID AND IsDeleted=0", new { ClientID = PDV.ProgbizClientID, LedgerName = "Sales" }, tran);

            salesJournalMasterData.DiscountLedgerID = await _dbContext.GetFieldsAsync<AccLedger, int>("LedgerID", $"LedgerName=@LedgerName AND ClientID=@ClientID AND IsDeleted=0", new { ClientID = PDV.ProgbizClientID, LedgerName = "Discount Paid" }, tran);

            salesJournalMasterData.SalesVoucherTypeID = await _dbContext.GetByQueryAsync<int>($@"
                                                                        Select TOP 1 VoucherTypeID
                                                                        From AccVoucherType
                                                                        Where ClientID={PDV.ProgbizClientID} AND IsDeleted=0 AND VoucherTypeNatureID={(int)VoucherTypeNatures.Sales}
            ", null, tran);

            salesJournalMasterData.VoucherNumber = await _account.GetVoucherNumber(salesJournalMasterData.SalesVoucherTypeID, salesJournalMasterData.ProgbizBranchID, tran);

            return salesJournalMasterData ?? new();

        }

        private async Task<PaymentRecieptJournalMasterBasicDataModel> GetRecieptJournalMasterBasicData(int clientEntityID, int receiptVoucherTypeID,  IDbTransaction? tran = null)
        {
            PaymentRecieptJournalMasterBasicDataModel basicData = new();

            basicData.ProgbizBranchID = await _dbContext.GetByQueryAsync<int>(@$"Select Top 1 BranchID 
                                                                        From Branch B
                                                                        Where B.ClientID={PDV.ProgbizClientID} and B.IsDeleted=0
            ", null, tran);

            basicData.ClientLedgerID = await _account.GetEntityLedgerID(clientEntityID, PDV.ProgbizClientID, tran);

            basicData.BankLedgerID = await _dbContext.GetFieldsAsync<AccLedger, int>("LedgerID", $"LedgerName=@LedgerName AND ClientID={PDV.ProgbizClientID} AND IsDeleted=0", new { LedgerName = "Bank" }, tran);

            basicData.Vouchernumber = await _account.GetVoucherNumber(receiptVoucherTypeID, basicData.ProgbizBranchID, tran);

            return basicData ?? new();

        }

        private async Task<MembershipPackageDetailsModel> GetPackageDetails(int packageID, IDbTransaction? tran = null)
        {
            MembershipPackageDetailsModel result = new();
            result.Package = await _dbContext.GetAsync<MembershipPackage>(packageID, tran);

            if (result.Package.TaxCategoryID is not null)
                result.TaxCategoryItems = await _dbContext.GetListByQueryAsync<TaxCategoryItem>(@$"
                                Select * From TaxCategoryItem Where TaxCategoryID={result.Package.TaxCategoryID} And IsDeleted=0
                ", null, tran);

            return result;
        }

        private async Task<int> InsertAccountEntries(ClientPackageAccountsModel accountsModel, IDbTransaction? tran = null)
        {

            #region Journal Master

            accountsModel.JournalMaster.JournalMasterID = await _dbContext.SaveAsync(accountsModel.JournalMaster, tran);

            #endregion

            #region Journal Entry

            await _dbContext.SaveSubItemListAsync(accountsModel.JournalEntries, "JournalMasterID", accountsModel.JournalMaster.JournalMasterID, tran);

            #endregion

            return accountsModel.JournalMaster.JournalMasterID;

        }

        #endregion

        #region Importing templates for all WA Accounts

        public async Task ImportAllWAAccountTemplates()
        {
            var clientList = await _dbContext.GetListByQueryAsync<int>("Select ClientID From Client Where IsDeleted=0",null);
            if (clientList is not null)
            {
                foreach (var clientID in clientList)
                {
                    await _whatsapp.SyncTemplatesWithFB(clientID);
                }
            }
        }

        #endregion

        #region Get Package Features

        public async Task<List<int>?> FetchPackageRoleGroups(int packageID, IDbTransaction? tran = null)
        {
            var packageRoleGroups = await _dbContext.GetListByQueryAsync<int>($@"Select Distinct RG.RoleGroupID
                                                                            From MembershipPackage P
                                                                            Join MembershipPackageRole PR ON PR.PackageID= P.PackageID AND P.IsDeleted=0
                                                                            Join Role R ON R.RoleID=PR.RoleID AND R.IsDeleted=0
                                                                            Join RoleGroup RG ON RG.RoleGroupID=R.RoleGroupID AND RG.IsDeleted=0
                                                                            Where P.IsDeleted=0 And P.PackageID={packageID}", null, tran);
            return packageRoleGroups;
        }

        #endregion

        #region Tax category

        public async Task<TaxCategoryModel> GetTaxCategoryDetailsById(int taxCategoryID)
        {
            var taxCategory = await _dbContext.GetByQueryAsync<TaxCategoryModel>($@"Select *
                                                                                From TaxCategory 
                                                                                Where TaxCategoryID={taxCategoryID} And IsDeleted=0", null);

            taxCategory.TaxCategoryItems = await GetTaxCategoryItems(taxCategoryID);
            return taxCategory;
        }
        public async Task<List<TaxCategoryItemModel>> GetTaxCategoryItems(int taxCategoryID)
        {
            return await _dbContext.GetListByQueryAsync<TaxCategoryItemModel>(@$"Select TI.*,L.LedgerName
                                                                                From TaxCategoryItem TI
                                                                                Join AccLedger L ON L.LedgerID=TI.LedgerID
                                                                                Where TI.TaxCategoryID={taxCategoryID} And TI.IsDeleted=0", null);
        }

        #endregion
    }
}
