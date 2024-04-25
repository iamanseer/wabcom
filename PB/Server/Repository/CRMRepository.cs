using AutoMapper;
using PB.Client.Pages.CRM;
using PB.CRM.Model.Enum;
using PB.EntityFramework;
using PB.Model;
using PB.Model.Models;
using PB.Shared.Enum.CRM;
using PB.Shared.Models;
using PB.Shared.Models.Common;
using PB.Shared.Models.CRM;
using PB.Shared.Models.CRM.Customer;
using PB.Shared.Tables.CRM;
using PB.Shared.Tables.Inventory.Invoices;
using System.Collections.Generic;
using System.Data;

namespace PB.Server.Repository
{
    public interface ICRMRepository
    {
        #region Enquiry

        Task<string> GetAccessableEnquiriesForUser(int userEntityID, int currentBranchID);

        #endregion

        #region Quotation

        Task<QuotationModelNew> GetQuotationById(int quotationID, int currenttBranchID);
        Task<string> GetAccessableQuotationForUser(int userEntityID, int currentBranchID);
        QuotationItemModelNew HandleQuotationItemCalculations(QuotationItemModelNew quotationItem);
        Task<int> GenerateQuotationPdf(int quotationID, int branchID, int clientID);
        Task RemoveQuotationPdfFiles(int quotationID, int branchID, int quotationNumber);

        #endregion

        #region Follow-up

        Task<int> SaveFollowup(FollowUpModel followUpModel, IDbTransaction? tran = null);

        #endregion
    }
    public class CRMRepository : ICRMRepository
    {
        private readonly IDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IPDFRepository _pdf;
        private readonly IHostEnvironment _env;
        public CRMRepository(IDbContext dbContext, IMapper mapper, IPDFRepository pdf, IHostEnvironment env)
        {
            this._dbContext = dbContext;
            this._mapper = mapper;
            this._pdf = pdf;
            this._env = env;
        }

        #region Enquiry

        public async Task<string> GetAccessableEnquiriesForUser(int userEntityID, int currentBranchID)
        {
            List<int> accessibleEnquiries = await _dbContext.GetListByQueryAsync<int>($@"SELECT E.EnquiryID
                                                                                            FROM Enquiry E
                                                                                            LEFT JOIN EnquiryAssignee EA ON E.EnquiryID = EA.EnquiryID AND EA.IsDeleted=0
                                                                                            WHERE (EA.EntityID = {userEntityID} OR EA.EnquiryID IS NULL) AND E.BranchID={currentBranchID} AND E.IsDeleted=0", null);
            if (accessibleEnquiries.Count > 0)
                return (string.Join(',', accessibleEnquiries));
            else
                return "";
        }

        #endregion

        #region Quotation

        public async Task<QuotationModelNew> GetQuotationById(int quotationID, int currenttBranchID)
        {
            var result = await _dbContext.GetByQueryAsync<QuotationModelNew>($@"
                                                                Select Q.*,vE.Name As CustomerName,Cust.TaxNumber,vE.Phone As MobileNumber,
                                                                WC.ContactID,vE.EmailAddress,vE.Phone AS MobileNumber,M.MediaID,M.FileName,CST.StateName As PlaceOfSupplyName,CZ.CountryID,
                                                                Concat(C.CurrencyName,' ( ',C.Symbol,' )') AS CurrencyName
                                                                From Quotation Q
                                                                Left Join viEntity vE On vE.EntityID=Q.CustomerEntityID
                                                                Left Join Customer Cust ON Cust.EntityID = Q.CustomerEntityID
																Left Join WhatsappContact WC ON WC.EntityID=Q.CustomerEntityID and WC.IsDeleted=0
                                                                Left Join Media M ON M.MediaID=Q.MediaID AND M.IsDeleted=0
                                                                Left Join Currency C ON C.CurrencyID=Q.CurrencyID AND C.IsDeleted=0
                                                                Left Join CountryState CST ON CST.StateID=Q.PlaceOfSupplyID AND CST.IsDeleted=0
                                                                Left Join viBranch B ON B.BranchID=Q.BranchID
                                                                Left Join CountryZone CZ ON CZ.ZoneID=B.ZoneID AND CZ.IsDeleted=0
                                                                Where Q.QuotationID={quotationID} and Q.BranchID={currenttBranchID} and Q.IsDeleted=0", null);

            result.QuotationItems = await _dbContext.GetListByQueryAsync<QuotationItemModelNew>($@"
                                                                Select I.ItemName,QI.*,T.TaxCategoryName,I.TaxPreferenceTypeID,I.TaxPreferenceName
                                                                From QuotationItem QI
																Left Join viItem I ON I.ItemVariantID=QI.ItemVariantID
                                                                Left Join viTaxCategory T ON T.TaxCategoryID=QI.TaxCategoryID
                                                                Where QI.QuotationID={quotationID} and QI.IsDeleted=0", null);

            if (result.QuotationItems.Count > 0)
            {
                for (int i = 0; i < result.QuotationItems.Count; i++)
                {
                    if (result.QuotationItems[i].TaxCategoryID != null)
                    {
                        result.QuotationItems[i].TaxCategoryItems = await _dbContext.GetListByQueryAsync<TaxCategoryItemModel>(@$"
                                                                Select TaxCategoryItemID,Concat(TaxCategoryItemName,'   [ ',Convert(smallint,Percentage),' ]') AS TaxCategoryItemName,Percentage,0 As Amount
                                                                From TaxCategoryItem T
                                                                Where T.TaxCategoryID={result.QuotationItems[i].TaxCategoryID} AND T.IsDeleted=0", null);
                    }
                    result.QuotationItems[i] = HandleQuotationItemCalculations(result.QuotationItems[i]);
                }
            }
            result.QuotationAssignees = await _dbContext.GetListAsync<QuotationAssignee>($"QuotationID={quotationID}", null);
            result.MailReciepients = await _dbContext.GetListAsync<QuotationMailRecipient>($"QuotationID={quotationID}", null);
            result.NeedShippingAddress = result.ShippingAddressID != null ? true : false;
            return result;
        }
        public async Task<string> GetAccessableQuotationForUser(int userEntityID, int currentBranchID)
        {
            List<int> accessibleQuotations = await _dbContext.GetListByQueryAsync<int>($@"SELECT Q.QuotationID 
                                                                                            FROM Quotation Q 
                                                                                            LEFT JOIN QuotationAssignee QA ON Q.QuotationID = QA.QuotationID AND QA.IsDeleted=0 
                                                                                            WHERE (QA.QuotationID IS NULL OR QA.EntityID={userEntityID}) AND Q.BranchID={currentBranchID} AND Q.IsDeleted=0
                ", null);
            if (accessibleQuotations.Count > 0)
                return (string.Join(',', accessibleQuotations));
            else
                return "";
        }
        public QuotationItemModelNew HandleQuotationItemCalculations(QuotationItemModelNew quotationItem)
        {
            quotationItem.TotalAmount = quotationItem.Quantity * quotationItem.Rate;
            quotationItem.NetAmount = quotationItem.TotalAmount - quotationItem.Discount;
            quotationItem.TaxAmount = quotationItem.NetAmount * quotationItem.TaxPercentage / 100;
            quotationItem.GrossAmount = quotationItem.NetAmount + quotationItem.TaxAmount;
            if (quotationItem.TaxCategoryItems.Count > 0)
            {
                quotationItem.TaxCategoryItems
                                .ForEach(taxCategoryItem => taxCategoryItem.TaxAmount = Math.Round((quotationItem.NetAmount * (taxCategoryItem.Percentage / 100)), 2));
            }
            return quotationItem;
        }
        public async Task<int> GenerateQuotationPdf(int quotationID, int branchID, int clientID)
        {
            Random random = new Random();
            int randomNumber = random.Next(0, 100);
            var quotationNumber = await _dbContext.GetByQueryAsync<int>($"Select QuotationNo From Quotation Where QuotationID={quotationID}", null);
            string pdfName = "pdf_quotation_" + branchID + '_' + quotationNumber + '_' + randomNumber;
            int invoiceMediaID = await _pdf.CreateQuotationPdf(quotationID, branchID, pdfName);
            await RemoveQuotationPdfFiles(quotationID, branchID, quotationNumber);
            return invoiceMediaID;
        }
        public async Task RemoveQuotationPdfFiles(int quotationID, int branchID, int quotationNumber)
        {
            string actualFolderPath = Path.Combine(_env.ContentRootPath, "wwwroot", "gallery", "quotation");
            string searchLikePattern = "pdf_quotation_" + branchID + '_' + quotationNumber + "_*";
            string? currentFileName = await _dbContext.GetByQueryAsync<string>(@$"Select FileName
                                                                                From Quotation Q
                                                                                Left Join Media M ON Q.MediaID=M.MediaID
                                                                                Where Q.QuotationID={quotationID}", null);

            var filesToRemove = Directory.GetFiles(actualFolderPath, searchLikePattern).ToList();
            if (filesToRemove is not null && filesToRemove.Count > 0)
            {
                if (!string.IsNullOrEmpty(currentFileName))
                {
                    if (!string.IsNullOrEmpty(currentFileName))
                    {
                        int lastIndexOfSeparator = currentFileName.LastIndexOf('/');
                        if (lastIndexOfSeparator != -1)
                            currentFileName = currentFileName.Substring(lastIndexOfSeparator + 1);
                        currentFileName = Path.Combine(_env.ContentRootPath, "wwwroot", "gallery", "quotation", currentFileName);
                        filesToRemove.Remove(filesToRemove.Where(fileName => fileName == currentFileName).First());
                    }
                }
                foreach (var fileToRemove in filesToRemove)
                {
                    File.Delete(fileToRemove);
                }
            }
        }

        #endregion

        #region Follow-up

        public async Task<int> SaveFollowup(FollowUpModel followUpModel, IDbTransaction? tran = null)
        {
            if (followUpModel.FollowUpStatusID is null)
                throw new Exception("Please choose a followup status");

            var followupStatus = await _dbContext.GetAsync<FollowupStatus>(followUpModel.FollowUpStatusID.Value, tran);
            switch (followupStatus.Type)
            {
                case (int)FollowUpTypes.Enquiry:
                    await _dbContext.ExecuteAsync("Update Enquiry Set CurrentFollowupNature=@Nature Where EnquiryID=@EnquiryID", new { Nature = followupStatus.Nature, EnquiryID = followUpModel.EnquiryID }, tran);
                    break;
                case (int)FollowUpTypes.Quotation:
                    await _dbContext.ExecuteAsync("Update Quotation Set CurrentFollowupNature=@Nature Where QuotationID=@QuotationID", new { Nature = followupStatus.Nature, QuotationID = followUpModel.QuotationID }, tran);
                    break;
                case (int)FollowUpTypes.Invoice:
                    await _dbContext.ExecuteAsync("Update Invoice Set CurrentFollowupNature=@Nature Where InvoiceID=@InvoiceID", new { Nature = followupStatus.Nature, InvoiceID = followUpModel.InvoiceID }, tran);
                    break;
            }
            var followUp = _mapper.Map<FollowUp>(followUpModel);
            followUp.FollowUpID = await _dbContext.SaveAsync(followUp, tran);
            return followUp.FollowUpID;
        }

        #endregion




	}
}
