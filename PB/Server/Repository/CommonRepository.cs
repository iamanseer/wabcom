
using PB.DatabaseFramework;
using PB.Model;
using PB.Shared.Helpers;
using System.Data;
using PB.Shared.Tables;
using PB.Shared.Models;
using System.Text;
using PB.Shared.Models.SuperAdmin.ClientInvoice;
using PB.Model.Models;
using PB.Shared.Tables.Whatsapp;
using PB.EntityFramework;
using DinkToPdf;
using PB.Shared.Models.Inventory.Items;
using PB.Shared.Models.Inventory.Invoices;
using NPOI.SS.Formula.Functions;
using Microsoft.Extensions.Primitives;
using PB.Shared.Tables.CRM;
using PB.Client.Pages.CRM;
using Bee.NumberToWords;
using Bee.NumberToWords.Enums;
using NPOI.SS.UserModel;
using PB.Shared.Enum.Reports;
using PB.Shared.Models.Reports;
using PB.Shared.Models.eCommerce.SEO;
using static NPOI.HSSF.Util.HSSFColor;

namespace PB.Server.Repository
{
    public interface ICommonRepository
    {
        #region Client Invoice mail
        Task<ClientInvoiceMailDetailsModel> GetInvoiceEMailDetailsForClient(int InvoiceID, IDbTransaction? tran = null);
        Task SendInvoiceEmailToClient(ClientInvoiceMailDetailsModel sendModel);

        #endregion

        #region Quotation mail
        Task<MailDetailsModel> GetQuotationPdfMailDetails(int quotationID, int branchID, IDbTransaction? tran = null);

        #endregion

        #region Invoice mail
        Task<MailDetailsModel> GetInvoicePdfMailDetails(int invoiceID, int branchID, IDbTransaction? tran = null);

        #endregion

        #region Other General Function

        Task<List<string>> GetRoles(int userID, int clientID, int userTypeID, int branchID = 0);
        string GeneratePagedListFilterWhereCondition(PagedListPostModelWithFilter searchModel, string searchLikeFieldName = "");
        Task SendEnquiryPushAndNotification(int clientID, int enquiryID, IDbTransaction? tran = null);
        Task SendPdfDocument(MailDetailsModel model, IDbTransaction? tran = null);

        #endregion
        #region NewsLetter
        Task SendNewsLetter(NewLetterModel model, int? NewsLetterID);
        #endregion
    }

    public class CommonRepository : ICommonRepository
    {
        private readonly IHostEnvironment _env;
        private readonly IDbContext _dbContext;
        private readonly IEmailSender _email;
        private readonly INotificationRepository _notification;

        public CommonRepository(IHostEnvironment env, IDbContext dbContext, IEmailSender email, INotificationRepository notification)
        {
            this._env = env;
            this._dbContext = dbContext;
            this._email = email;
            this._notification = notification;
        }

        #region Client Invoice Detals and Its PDF
        public async Task<ClientInvoiceMailDetailsModel> GetInvoiceEMailDetailsForClient(int InvoiceID, IDbTransaction? tran = null)
        {
            var details = await _dbContext.GetByQueryAsync<ClientInvoiceMailDetailsModel>($@"Select PackageName,InvoiceDate,DisconnectionDate,EmailAddress As MailRecipients,Name,InvoiceNo,FileName
                                                                                            From ClientInvoice CI
                                                                                            Left Join Client C on C.ClientID=CI.ClientID and C.IsDeleted=0
                                                                                            Left Join MembershipPackage MP on MP.PackageID=CI.PackageID and MP.IsDeleted=0
                                                                                            Left Join viEntity V on V.EntityID=C.EntityID
                                                                                            Left Join Media m on M.MediaID=CI.InvoiceMediaID and M.IsDeleted=0
                                                                                            Where InvoiceID={InvoiceID} and CI.IsDeleted=0
            ", null, tran);

            details.Subject = $"Subject:Payment Reminder - Invoice for {details.PackageName}";

            details.Message = $@"
                       <div style = 'font-family: Helvetica,Arial,sans-serif;min-width:1000px;overflow:auto;line-height:2;direction:ltr;'>
                            <div style = 'margin:50px auto;width:70%;padding:20px 0'>
                                <div style = 'border-bottom:1px solid #eee' >
                                    <a href = '' style = 'font-size:1.4em;color: #00466a;text-decoration:none;font-weight:600'> </a>
                                </div>
                                <p style = 'font-size:1.1em'>Dear {details.Name},</p>
                                <p>We hope this message finds you well. We want to bring to your attention that a new invoice has been generated for the services related to '{details.PackageName}' We kindly request that you make the payment on or before the due date mentioned below to avoid any disruption in the package's delivery: </p> </p>
                                <br>
                                Invoice Number: {details.InvoiceNo}
                                <br>
                                Invoice Date: {details.InvoiceDate.Value.ToString("dd/MM/yyyy")}
                                <br>
                                Due Date: {details.DisconnectionDate.Value.ToString("dd/MM/yyyy")}

                                <p>Please transfer the due amount to the following bank account:<br>
                                Bank Account Name : PROGBIZ PRIVATE LIMITED 
                                <br>
                                Account No: 120000847586 
                                <br>
                                Bank Name : Canara Bank 
                                <br>
                                Branch : Chakkarakkal 
                                <br>
                                IFSC Code : CNRB0004698 
                                <br>
                                MICR Code : 670015006 <br>

                                <h2 style = 'background: #00466a;text-align:right;width: max-content;padding: 0 10px;color: #fff;border-radius: 4px;'> </h2> 
                               <hr style = 'border: none; border - top:1px solid #eee' />
                            <div style='float:right; padding: 8px 0; color:#aaa;font-size:0.8em;line-height:1;font-weight:300'>
                                <p> Regards,</p>
                                    < p>  {PDV.BrandName} team</p>
                                </div>
                            </div>
                        </div>
                ";

            return details ?? new();
        }
        public async Task SendInvoiceEmailToClient(ClientInvoiceMailDetailsModel sendModel)
        {
            List<string>? attachments = null;

            if (!string.IsNullOrEmpty(sendModel.FileName))
                attachments = new() { Path.Combine(_env.ContentRootPath, "wwwroot") + sendModel.FileName };

            if (!string.IsNullOrEmpty(sendModel.MailRecipients) && !string.IsNullOrEmpty(sendModel.Subject) && !string.IsNullOrEmpty(sendModel.Message))
                await _email.SendHtmlEmailAsync(sendModel.MailRecipients, sendModel.Subject, sendModel.Message, null, 1, attachments);
        }


        #endregion

        #region Quotation Mail
        public async Task<MailDetailsModel> GetQuotationPdfMailDetails(int quotationID, int branchID, IDbTransaction? tran = null)
        {
            MailDetailsModel quotationSendingDetails = await _dbContext.GetByQueryAsync<MailDetailsModel>($@"
                                                        SELECT 
                                                        CASE
                                                            WHEN EM.Email IS NOT NULL THEN CONCAT(CE.EmailAddress, ',',EM.Email)
                                                            ELSE CE.EmailAddress
                                                            END AS MailRecipients,
                                                        M.FileName,
                                                        E.Name AS CompanyName,
                                                        Q.Subject
                                                        FROM Quotation Q
                                                        JOIN viEntity CE ON Q.CustomerEntityID=CE.EntityID
                                                        LEFT JOIN Media M ON M.MediaID=Q.MediaID AND M.IsDeleted=0
                                                        LEFT JOIN Branch B ON B.BranchID=Q.BranchID AND B.IsDeleted=0
                                                        LEFT JOIN viEntity E ON B.EntityID=E.EntityID
                                                        LEFT JOIN 
			                                                        (
				                                                        SELECT STRING_AGG(ME.EmailAddress, ',') AS Email,QM.QuotationID
																		FROM QuotationMailRecipient QM
																		JOIN viEntity ME ON ME.EntityID=QM.EntityID
																		WHERE QM.IsDeleted=0 AND QM.QuotationID={quotationID}
				                                                        GROUP BY QM.QuotationID
			                                                        ) EM ON Q.QuotationID=EM.QuotationID
                                                        WHERE Q.IsDeleted=0 AND Q.QuotationID={quotationID} AND Q.BranchID={branchID}", null, tran);

            quotationSendingDetails.Message = @$"I hope this email finds you well. I wanted to inform you that the PDF for the latest quotation has been successfully generated. 
Attached to this email, you will find the corresponding PDF document containing all the details of the quotation.
Should you have any questions or require further information, please don't hesitate to reach out to us.

Thank you and have a great day.

Best regards,

{quotationSendingDetails.CompanyName} ";

            return quotationSendingDetails;
        }

        #endregion

        #region Invoice Mail

        public async Task<MailDetailsModel> GetInvoicePdfMailDetails(int invoiceID, int branchID, IDbTransaction? tran = null)
        {
            MailDetailsModel invoiceSendingDetails = await _dbContext.GetByQueryAsync<MailDetailsModel>($@"
                                                                             SELECT 
                                                                             CASE
                                                                                 WHEN EM.Email IS NOT NULL THEN CONCAT(CE.EmailAddress, ',',EM.Email)
                                                                                 ELSE CE.EmailAddress
                                                                                 END AS MailRecipients,
                                                                             M.FileName,
                                                                             E.Name AS CompanyName,
                                                                             I.Subject
                                                                             FROM Invoice I
                                                                             JOIN viEntity CE ON I.CustomerEntityID=CE.EntityID
                                                                             LEFT JOIN Media M ON M.MediaID=I.MediaID AND M.IsDeleted=0
                                                                             LEFT JOIN Branch B ON B.BranchID=I.BranchID AND B.IsDeleted=0
                                                                             LEFT JOIN viEntity E ON B.EntityID=E.EntityID
                                                                             LEFT JOIN 
                                                                                         (
                                                                                             SELECT STRING_AGG(ME.EmailAddress, ',') AS Email,IM.InvoiceID
																	                                                                            FROM InvoiceMailReceipient IM
																	                                                                            JOIN viEntity ME ON ME.EntityID=IM.EntityID
																	                                                                            WHERE IM.IsDeleted=0 AND IM.InvoiceID={invoiceID}
                                                                                             GROUP BY IM.InvoiceID
                                                                                         ) EM ON I.InvoiceID=EM.InvoiceID
                                                                             WHERE I.IsDeleted=0 AND I.InvoiceID={invoiceID} AND I.BranchID={branchID}", null, tran);

            invoiceSendingDetails.Message = @$"I hope this email finds you well. I wanted to inform you that the PDF for the latest quotation has been successfully generated. 
Attached to this email, you will find the corresponding PDF document containing all the details of the invoice.
Should you have any questions or require further information, please don't hesitate to reach out to us.
Thank you and have a great day.
Best regards,
{invoiceSendingDetails.CompanyName} ";

            return invoiceSendingDetails;
        }

        #endregion

        #region Other General Function

        public async Task<List<string>> GetRoles(int userID, int clientID, int userTypeID, int branchID = 0)
        {
            if (userTypeID == (int)DefaultUserTypes.SuperAdmin || userTypeID == (int)DefaultUserTypes.Developer)
            {
                if (clientID == 0 && branchID == 0)
                {
                    var roles = await _dbContext.GetListByQueryAsync<string>($@"SELECT  RoleName FROM Role Where IsForSupport=1", null);
                    roles.Add("SuperAdmin");
                    return roles;
                }
                else
                    return await _dbContext.GetListByQueryAsync<string>($@"SELECT  RoleName FROM Role Where IsForSupport=0", null);
            }
            else
            {
                if (userTypeID == (int)DefaultUserTypes.Support && clientID != 0 && branchID != 0)
                {
                    return await _dbContext.GetListByQueryAsync<string>($@"SELECT  RoleName FROM Role Where IsForSupport=0", null);
                }

                if (userTypeID == (int)DefaultUserTypes.Client && clientID != 0)
                {

                    return await _dbContext.GetListByQueryAsync<string>($@"SELECT  Distinct RoleName
                                FROM Client C
                                Left Join MembershipPackageRole PP on PP.PackageID=C.PackageID and PP.IsDeleted=0
                                Left JOIN Role P on P.RoleID=PP.RoleID and P.IsDeleted=0
                                Where C.ClientID={clientID} and C.IsDeleted=0", null);
                }
                else
                {
                    return await _dbContext.GetListByQueryAsync<string>($@"SELECT  Distinct RoleName
                                    FROM UserTypeRole PP
                                    JOIN Role P on P.RoleID=PP.RoleID
                                    LEFT JOIN UserType T on T.UserTypeID=PP.UserTypeID
                                    LEFT JOIN Users U on U.UserTypeID=T.UserTypeID or U.UserID=PP.UserID
			                        Where U.UserID={userID} and ISNULL(PP.HasAccess,0)=1 and PP.IsDeleted=0", null);
                }
            }
        }
        public string GeneratePagedListFilterWhereCondition(PagedListPostModelWithFilter searchModel, string searchLikeFieldName = "")
        {
            string dateWhereCondition = "";
            string enumWhereCondition = "";
            string booleanWhereCondition = "";
            string fieldWhereCondition = "";
            string query = "";

            //Generating date filter where condition
            if (searchModel.FilterByDateOptions != null && searchModel.FilterByDateOptions.Count > 0)
            {
                dateWhereCondition = "";
                for (short i = 0; i < searchModel.FilterByDateOptions.Count; i++)
                {
                    var dateFilterItem = searchModel.FilterByDateOptions[i];
                    if (dateFilterItem.StartDate != null && dateFilterItem.EndDate != null)
                    {
                        string StartDate = dateFilterItem.StartDate.Value.ToString("yyyy-MM-dd");
                        string EndDate = dateFilterItem.EndDate.Value.ToString("yyyy-MM-dd");
                        switch (i)
                        {
                            case 0:

                                dateWhereCondition = $" and (Convert(nvarchar,{dateFilterItem.FieldName},23) Between '{StartDate}' and '{EndDate}')";
                                break;

                            case > 0:

                                dateWhereCondition += $" and (Convert(nvarchar,{dateFilterItem.FieldName},23) Between '{StartDate}' and '{EndDate}')";

                                break;
                        }
                    }
                }
            }

            //Generating IDnValue filter where condition
            if (searchModel.FilterByIdOptions != null && searchModel.FilterByIdOptions.Count > 0)
            {
                enumWhereCondition = "";
                for (short i = 0; i < searchModel.FilterByIdOptions.Count; i++)
                {
                    var enumFilterItem = searchModel.FilterByIdOptions[i];
                    switch (i)
                    {
                        case 0:

                            if (enumFilterItem.SelectedEnumValues != null && enumFilterItem.SelectedEnumValues.Count > 0)
                            {
                                if (enumFilterItem.SelectedEnumValues.Count == 1)
                                {
                                    enumWhereCondition = $" and {enumFilterItem.FieldName}={enumFilterItem.SelectedEnumValues[0]}";
                                }
                                else
                                {
                                    string commaSeparatedEnumValues = string.Join(",", enumFilterItem.SelectedEnumValues);
                                    enumWhereCondition = $" and {enumFilterItem.FieldName} in ({commaSeparatedEnumValues})";
                                }
                            }

                            break;

                        case > 0:

                            if (enumFilterItem.SelectedEnumValues != null && enumFilterItem.SelectedEnumValues.Count > 0)
                            {
                                if (enumFilterItem.SelectedEnumValues.Count == 1)
                                {
                                    enumWhereCondition += $" and {enumFilterItem.FieldName}={enumFilterItem.SelectedEnumValues[0]}";
                                }
                                else
                                {
                                    string commaSeparatedEnumValues = string.Join(",", enumFilterItem.SelectedEnumValues);
                                    enumWhereCondition += $" and {enumFilterItem.FieldName} in ({commaSeparatedEnumValues})";
                                }
                            }

                            break;
                    }
                }
            }

            //Generating boolean filter where condition
            if (searchModel.FilterByBooleanOptions != null && searchModel.FilterByBooleanOptions.Count > 0)
            {
                booleanWhereCondition = "";
                for (short i = 0; i < searchModel.FilterByBooleanOptions.Count; i++)
                {

                    var booleanFilterItem = searchModel.FilterByBooleanOptions[i];
                    switch (i)
                    {
                        case 0:

                            booleanWhereCondition = $" AND {booleanFilterItem.FieldName}={booleanFilterItem.Value}";

                            break;

                        case > 0:

                            booleanWhereCondition += $" AND {booleanFilterItem.FieldName}={booleanFilterItem.Value}";

                            break;
                    }
                }
            }


            //Generating Field filter where condition
            if (searchModel.FilterByFieldOptions != null && searchModel.FilterByFieldOptions.Count > 0)
            {
                fieldWhereCondition = "";
                for (short i = 0; i < searchModel.FilterByFieldOptions.Count; i++)
                {

                    var fieldFilterItem = searchModel.FilterByFieldOptions[i];
                    switch (i)
                    {
                        case 0:

                            fieldWhereCondition = $" AND {fieldFilterItem.FieldName} IS NOT NULL";

                            break;

                        case > 0:

                            fieldWhereCondition += $" AND {fieldFilterItem.FieldName} IS NOT NULL";

                            break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(dateWhereCondition))
            {
                query += dateWhereCondition;
                dateWhereCondition = string.Empty;
            }

            if (!string.IsNullOrEmpty(enumWhereCondition))
            {
                query += enumWhereCondition;
                enumWhereCondition = string.Empty;
            }

            if (!string.IsNullOrEmpty(booleanWhereCondition))
            {
                query += booleanWhereCondition;
                booleanWhereCondition = string.Empty;
            }


            if (!string.IsNullOrEmpty(fieldWhereCondition))
            {
                query += fieldWhereCondition;
                fieldWhereCondition = string.Empty;
            }


            if (!string.IsNullOrEmpty(searchModel.SearchString) && !string.IsNullOrEmpty(searchLikeFieldName))
            {
                query += $" and {searchLikeFieldName} like '%{searchModel.SearchString}%'";
            }

            return query;
        }
        public async Task SendEnquiryPushAndNotification(int clientID, int enquiryID, IDbTransaction? tran = null)
        {
            List<int> UserEntities = await _dbContext.GetListByQueryAsync<int>($@"Select EntityID From Users Where ClientID=@ClientID AND IsDeleted=0 AND LoginStatus=1", new { ClientID = clientID }, tran);
            if (UserEntities.Count > 0)
            {
                IdnValuePair Data = await _dbContext.GetByQueryAsync<IdnValuePair>($@"
                                                                    Select E.EnquiryNo AS ID,CE.Name AS Value
                                                                    From Enquiry E 
                                                                    Left Join viEntity CE ON CE.EntityID=E.CustomerEntityID
                                                                    Where E.EnquiryID={enquiryID}
                ", null);

                string? tittle = "Enquiry(#" + Data.ID + ") ";
                string? message = "created by " + Data.Value;

                foreach (int entityID in UserEntities)
                {
                    await _notification.SendPush(entityID, message, tittle, "enquiry", 1, enquiryID.ToString());
                    await _notification.SendSignalRPush(entityID, tittle + message);
                }
            }
        }
        public async Task SendPdfDocument(MailDetailsModel model, IDbTransaction? tran = null)
        {

            int? mailSettingId = await _dbContext.GetByQueryAsync<int?>($@"Select MailSettingsID FROM MailSettings WHERE ClientID={model.ClientID}", null);
            mailSettingId = (mailSettingId == null) ? 1 : mailSettingId;

            List<string> attachments = new() { Path.Combine(_env.ContentRootPath, "wwwroot") + model.FileName };

            if (!string.IsNullOrEmpty(model.MailRecipients) && !string.IsNullOrEmpty(model.Subject) && !string.IsNullOrEmpty(model.Message))
                await _email.SendHtmlEmailAsync(model.MailRecipients, model.Subject, model.Message, tran, mailSettingId.Value, attachments);

        }

        #endregion
        #region NewsLetter
        public async Task SendNewsLetter(NewLetterModel model, int? NewsLetterID)
        {
            var Emails = await _dbContext.GetByQueryAsync<string>(@$"SELECT STRING_AGG(Email, ',') 
                                                            FROM EC_NewsLetterSubscription 
                                                           WHERE IsDeleted = 0", null);

            if (!string.IsNullOrEmpty(Emails) && !string.IsNullOrEmpty(model.Subject) && !string.IsNullOrEmpty(model.BodeyContent))
                await _email.SendHtmlEmailAsync(Emails, model.Subject, model.BodeyContent,null);

        }
        #endregion
    }
 }
