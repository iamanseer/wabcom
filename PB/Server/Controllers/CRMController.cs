using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using PB.Model.Models;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using PB.EntityFramework;
using PB.Shared.Models.CRM;
using PB.Shared.Models;
using PB.Shared.Enum;
using PB.Shared.Enum.CRM;
using PB.Model;
using System.Data;
using PB.Shared.Tables.CRM;
using AutoMapper;
using NPOI.SS.Formula.Functions;
using PB.Client.Pages.CRM;
using PB.Server.Repository;
using PB.Shared;
using PB.Client.Pages.SuperAdmin.Client;
using PB.Shared.Models.CRM.Enquiry;
using PB.CRM.Model.Enum;
using PB.Shared.Tables.Whatsapp;
using Hangfire.Common;
using PB.Shared.Models.Common;
using static NPOI.HSSF.Util.HSSFColor;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using PB.Shared.Models.Inventory.Item;
using PB.Shared.Models.CRM.Quotation;
using PB.Shared.Enum.Inventory;
using PB.Shared.Tables.Inventory.Invoices;
using PB.Shared.Models.CRM.Followup;
using PB.Shared.Models.CRM.Quotations;
using PB.Shared.Tables.Accounts.JournalMaster;

namespace PB.Server.Controllers
{
    public class CRMController : BaseController
    {
        private readonly IDbContext _dbContext;
        private readonly IDbConnection _cn;
        private readonly IMapper _mapper;
        private readonly IPDFRepository _pdf;
        private readonly ICommonRepository _common;
        private readonly ICRMRepository _cRMRepository;
        private readonly IWhatsappRepository _whatsapp;
        private readonly IItemRepository _item;
        public CRMController(IDbContext dbContext, IDbConnection cn, IMapper mapper, IPDFRepository pdf, ICommonRepository common, ICRMRepository cRMRepository, IWhatsappRepository whatsapp, IItemRepository item)
        {
            _dbContext = dbContext;
            _cn = cn;
            _mapper = mapper;
            _pdf = pdf;
            _common = common;
            _cRMRepository = cRMRepository;
            _whatsapp = whatsapp;
            _item = item;
        }

        #region Enquiry 

        [HttpPost("get-enquiry-paged-list")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Enquiry")]
        public async Task<IActionResult> GetEnquiryList(PagedListPostModelWithFilter searchModel)
        {
            string dateEnquiryCondition = ""; string dateQuotationCondition = "";
            if (searchModel.PeriodTypeID != 0)
            {
                var dateContition = await _cRMRepository.GetAllDateFilter(searchModel.PeriodTypeID);
                dateEnquiryCondition = $@" and Convert(date,E.Date) between '{dateContition.FromDate}' and '{dateContition.ToDate}' ";
            }
            PagedListQueryModel query = searchModel;
            query.Select = $@"Select EnquiryNo,Date,
                                Case When VE.EntityID is null then 'System' else  VE.Name end AS AddedBy,
                                LeadQuality,E.EnquiryID,E.LeadThroughID,CurrentFollowupNature,EAS.Assignee,LT.Name AS LeadThroughName
                                ,Case When Type={(int)CustomerTypes.Individual} Then EP.FirstName else EI.Name end as CustomerName
                                From Enquiry E 
                                Join Customer C on C.EntityID=E.CustomerEntityID and C.IsDeleted=0
                                Left Join EntityPersonalInfo EP ON EP.EntityID=C.EntityID and EP.IsDeleted=0
		                                Left Join EntityInstituteInfo EI on EI.EntityID=C.EntityID and EI.IsDeleted=0
                                Left Join LeadThrough LT ON LT.LeadThroughID=E.LeadThroughID AND LT.IsDeleted=0
                                LEFT JOIN (
                                            Select STRING_AGG(E.Name, ',') AS Assignee,EA.EnquiryID
                                            From EnquiryAssignee EA
                                            Join viEntity E ON E.EntityID=EA.EntityID
			                                Where EA.IsDeleted=0
                                            Group By EA.EnquiryID
                                            ) EAS ON E.EnquiryID=EAS.EnquiryID
                                LEFT JOIN viEntity VE on VE.EntityID=E.UserEntityID";

            query.WhereCondition = $"E.IsDeleted=0 and E.BranchID={CurrentBranchID} and (ISNULL(E.CurrentFollowupNature,0)={searchModel.CurrentFollowupNature} or {searchModel.CurrentFollowupNature}=-1) and (E.IsInCart=0 or ISNULL(E.IsInCart,0)=0) {dateEnquiryCondition}";
            var enquiryIDs = await _cRMRepository.GetAccessableEnquiriesForUser(CurrentEntityID, CurrentBranchID);
            if (CurrentUserTypeID > (int)UserTypes.Client && !string.IsNullOrEmpty(enquiryIDs))
                query.WhereCondition += $" and E.EnquiryID IN ({enquiryIDs})";
            query.OrderByFieldName = searchModel.OrderByFieldName;
            query.SearchLikeColumnNames = new() { "EI.Name", "EP.FirstName" , "VE.Name" };
            query.WhereCondition += _common.GeneratePagedListFilterWhereCondition(searchModel);
            return Ok(await _dbContext.GetPagedList<EnquiryListModel>(query, null));
        }

        [HttpGet("get-enquiry/{enquiryId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Enquiry")]
        public async Task<IActionResult> GetEnquiry(int enquiryId)
        {
            var result = await _dbContext.GetByQueryAsync<EnquiryModel>(@$"Select E.*,V.Name As CustomerName,V.Phone,V.Phone As MobileNumber,V.EmailAddress,UE.Name As Username,Q.QuotationID,WC.ContactID,LT.LeadThroughID,LT.Name As LeadThroughName
																				        From Enquiry E
																				        Left Join viEntity V on V.EntityID=E.CustomerEntityID
																				        Left Join viEntity UE on UE.EntityID=E.UserEntityID
																				        Left Join WhatsappContact WC ON E.CustomerEntityID=WC.EntityID and WC.IsDeleted=0
																				        Left Join Quotation Q ON Q.EnquiryID=E.EnquiryID and Q.IsDeleted=0
                                                                                        Left Join LeadThrough LT ON LT.LeadThroughID=E.LeadThroughID and LT.IsDeleted=0
                                                                                        Where E.EnquiryID={enquiryId} and E.IsDeleted=0 and E.BranchID={CurrentBranchID}", null);

            result.EnquiryItem = await _dbContext.GetListByQueryAsync<EnquiryItemModel>(@$"Select ROW_NUMBER() OVER(ORDER BY (SELECT 1)) AS RowIndex,
                                                                                        I.ItemName,
																						Quantity,EI.Description,EnquiryItemID,I.ItemVariantID,EI.EnquiryID
                                                                                        From EnquiryItem EI
																						Left Join viItem I ON I.ItemVariantID=EI.ItemVariantID
                                                                                        Where EnquiryID={enquiryId} and EI.IsDeleted=0", null);

            result.FollowupAssignees = await _dbContext.GetListByQueryAsync<EnquiryAssigneeModel>($@"Select EA.*,UE.Name As Name
                                                                                        From EnquiryAssignee EA
                                                                                        Join viEntity UE ON UE.EntityID=EA.EntityID 
                                                                                        Where EA.EnquiryID={enquiryId} And EA.IsDeleted=0", null);

            result.Histories = await _dbContext.GetListByQueryAsync<FollowUpModel>($@"Select F.* ,U.Name As Username,FS.StatusName As Status
                                                                                        From FollowUp F 
                                                                                        Left Join viEntity U ON U.EntityID=F.EntityID
                                                                                        Left Join FollowupStatus FS ON FS.FollowUpStatusID=F.FollowUpStatusID and FS.IsDeleted=0
                                                                                        Where F.EnquiryID={enquiryId} and F.IsDeleted=0
                                                                                        Order By F.FollowUpDate Desc", null);

            return Ok(result);
        }

        [HttpGet("get-enquiry-new/{enquiryId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Enquiry")]
        public async Task<IActionResult> GetEnquiryNew(int enquiryId)
        {
            var result = await _dbContext.GetByQueryAsync<EnquiryModel>(@$"Select E.*,V.Name As CustomerName,V.Phone,V.Phone As MobileNumber,V.EmailAddress,UE.Name As Username,Q.QuotationID,WC.ContactID,LT.LeadThroughID,LT.Name As LeadThroughName
																				        From Enquiry E
																				        Left Join viEntity V on V.EntityID=E.CustomerEntityID
																				        Left Join viEntity UE on UE.EntityID=E.UserEntityID
																				        Left Join WhatsappContact WC ON E.CustomerEntityID=WC.EntityID and WC.IsDeleted=0
																				        Left Join Quotation Q ON Q.EnquiryID=E.EnquiryID and Q.IsDeleted=0
                                                                                        Left Join LeadThrough LT ON LT.LeadThroughID=E.LeadThroughID and LT.IsDeleted=0
                                                                                        Where E.EnquiryID={enquiryId} and E.IsDeleted=0 and E.BranchID={CurrentBranchID}", null);

            result.EnquiryItem = await _dbContext.GetListByQueryAsync<EnquiryItemModel>(@$"Select ROW_NUMBER() OVER(ORDER BY (SELECT 1)) AS RowIndex,
                                                                                        I.ItemName,
																						Quantity,EI.Description,EnquiryItemID,I.ItemVariantID,EI.EnquiryID
                                                                                        From EnquiryItem EI
																						Left Join viItem I ON I.ItemVariantID=EI.ItemVariantID
                                                                                        Where EnquiryID={enquiryId} and EI.IsDeleted=0", null);

            result.FollowupAssignees = await _dbContext.GetListByQueryAsync<EnquiryAssigneeModel>($@"Select EA.*,UE.Name As Name
                                                                                        From EnquiryAssignee EA
                                                                                        Join viEntity UE ON UE.EntityID=EA.EntityID 
                                                                                        Where EA.EnquiryID={enquiryId} And EA.IsDeleted=0", null);

            result.Histories = await _dbContext.GetListByQueryAsync<FollowUpModel>($@"Select F.* ,U.Name As Username,FS.StatusName As Status
                                                                                        From FollowUp F 
                                                                                        Left Join viEntity U ON U.EntityID=F.EntityID
                                                                                        Left Join FollowupStatus FS ON FS.FollowUpStatusID=F.FollowUpStatusID and FS.IsDeleted=0
                                                                                        Where F.EnquiryID={enquiryId} and F.IsDeleted=0
                                                                                        Order By F.FollowUpDate Desc", null);

            return Ok(result);
        }

        [HttpGet("create-enquiry-from-chat/{contactId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Enquiry")]
        public async Task<IActionResult> CreateEnquiryFromChatSession(int contactId)
        {
            var contact = await _dbContext.GetAsync<WhatsappContact>(contactId);
            if (contact != null)
            {
                var sessionId = await _dbContext.GetByQueryAsync<int>($"Select Max(SessionID) from WhatsappChatSession Where ContactID={contactId}", null);
                var chatSession = await _dbContext.GetAsync<WhatsappChatSession>(sessionId);

                EnquiryModel res = new()
                {
                    ContactID = chatSession.ContactID,
                    Date = DateTime.UtcNow,
                    LeadThroughID = (int)LeadThroughs.Whatsapp,
                    Description = await _whatsapp.GetEnquiryDescription(sessionId),
                    CustomerEntityID = await _whatsapp.GetContactEntityID(contact.ContactID, CurrentClientID),
                    FirstFollowUpDate = DateTime.UtcNow,
                    CustomerName = contact.Name
                };
                return Ok(res);
            }
            return BadRequest(new Error("Contact not found"));
        }

        [HttpPost("save-enquiry")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Enquiry")]
        public async Task<IActionResult> SaveEnquiry(EnquiryModel model)
        {
            #region Enquiry Number

            if (model.EnquiryID == 0)
            {
                model.EnquiryNo = await _dbContext.GetByQueryAsync<int>($@"
                                                                Select isnull(max(EnquiryNo)+1,1) AS EnquiryNo
                                                                From Enquiry
                                                                Where BranchID={CurrentBranchID} and IsDeleted=0", null);
            }

            #endregion

            #region Enquiry

            Enquiry enquiry = _mapper.Map<Enquiry>(model);
            enquiry.BranchID = CurrentBranchID;
            if (model.EnquiryID == 0)
            {
                enquiry.UserEntityID = CurrentEntityID;
            }
            enquiry.EnquiryID = await _dbContext.SaveAsync(enquiry);

            #endregion

            #region Enquiry Item

            if (model.EnquiryItem.Count > 0)
            {
                List<EnquiryItem> enquiryItems = _mapper.Map<List<EnquiryItem>>(model.EnquiryItem);

                if (enquiryItems.Count > 0)
                {
                    await _dbContext.SaveSubItemListAsync(enquiryItems, "EnquiryID", enquiry.EnquiryID);
                }
            }

            #endregion

            #region Enquiry Assignees

            List<EnquiryAssignee> enquiryAssigneesList = new();

            enquiryAssigneesList = _mapper.Map<List<EnquiryAssignee>>(model.FollowupAssignees.Where(fa => fa.IsAssigned).ToList());

            if (enquiryAssigneesList.Count > 0)
            {
                await _dbContext.SaveSubItemListAsync(enquiryAssigneesList, "EnquiryID", enquiry.EnquiryID);
            }

            #endregion

            //Notification
            if (model.EnquiryID is 0)
                await _common.SendEnquiryPushAndNotification(CurrentClientID, enquiry.EnquiryID,CurrentEntityID, null);
            return Ok(new EnquiryAddResultModel() { EnquiryID = enquiry.EnquiryID });
        }

        [HttpGet("get-enquiry-menu-list")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Enquiry")]
        public async Task<IActionResult> GetEnquiryFollowupMenuList()
        {
            string whereCondition = $"Where E.IsDeleted=0 and E.BranchID={CurrentBranchID}";
            var enquiryIDs = await _cRMRepository.GetAccessableEnquiriesForUser(CurrentEntityID, CurrentBranchID);
            if (CurrentUserTypeID > (int)UserTypes.Client && !string.IsNullOrEmpty(enquiryIDs))
                whereCondition += $" and E.EnquiryID IN ({enquiryIDs})";
            var result = await _dbContext.GetListByQueryAsync<ViewPageMenuModel>($@"Select Case When NextFollowUpDate is null then 1 else  DATEDIFF(DAY,Today,NextFollowUpDate) end as Days,
								                                                    ID,MenuName,EnquiryNo
								                                                    From
										                                                    (Select E.EnquiryID as ID,CONVERT(varchar(20), EnquiryNo)+'-' + V.Name as MenuName,EnquiryNo,
															                                        Case When F.FollowUpID Is Null Then E.FirstFollowUpDate Else F.NextFollowUpDate End As NextFollowUpDate,
															                                        Convert(date,DATEADD(MINUTE,330, getutcdate())) Today
															                                        From Enquiry E
															                                        Join viEntity V on V.EntityID=E.CustomerEntityID
                                                                                                    LEFT JOIN ( Select EnquiryID,Max(FollowupID) As FollowupID 
                                                                                                                From FollowUp 
                                                                                                                Where IsDeleted=0 and FollowUpType={(int)FollowUpTypes.Enquiry}
                                                                                                                Group by EnquiryID
                                                                                                                ) EF on EF.EnquiryID=E.EnquiryID
															                                        Left Join FollowUp F ON F.FollowupID=EF.FollowupID and F.IsDeleted=0
															                                        {whereCondition}) as A
								                                                    Order by EnquiryNo DESC", null);

            return Ok(result);
        }

        [HttpGet("get-enquiry-followup-menu-list")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "FollowUp,Enquiry")]
        public async Task<IActionResult> GetEnquiryFollowupMenuListFromFollowup()
        {
            string whereCondition = $"Where E.IsDeleted=0 and E.BranchID={CurrentBranchID}";
            if (CurrentUserTypeID > (int)UserTypes.Client)
            {
                string commaSeperatedEnquiryIDs = await _cRMRepository.GetAccessableEnquiriesForUser(CurrentEntityID, CurrentBranchID);
                if (!string.IsNullOrEmpty(commaSeperatedEnquiryIDs))
                {
                    whereCondition += $" and E.EnquiryID in ({commaSeperatedEnquiryIDs})";
                }
            }
            var result = await _dbContext.GetListByQueryAsync<ViewPageMenuModel>($@"
                                                                                        Select Case When NextFollowUpDate is null then 1 else  DATEDIFF(DAY,Today,NextFollowUpDate) end as Days,
								                                                        ID,MenuName
								                                                        From
										                                                        (Select E.EnquiryID as ID,CONVERT(varchar(20), EnquiryNo)+'-' + V.Name as MenuName,
															                                            Case When F.FollowUpID Is Null Then E.FirstFollowUpDate Else F.NextFollowUpDate End As NextFollowUpDate,
															                                            Convert(date,DATEADD(MINUTE,330, getutcdate())) Today
															                                            From Enquiry E
															                                            Join viEntity V on V.EntityID=E.CustomerEntityID
                                                                                                        LEFT JOIN ( Select EnquiryID,Max(FollowupID) As FollowupID 
                                                                                                                    From FollowUp 
                                                                                                                    Where IsDeleted=0 and FollowUpType={(int)FollowUpTypes.Enquiry}
                                                                                                                    Group by EnquiryID
                                                                                                                    ) EF on EF.EnquiryID=E.EnquiryID
															                                            Left Join FollowUp F ON F.FollowupID=EF.FollowupID and F.IsDeleted=0
															                                            {whereCondition}) as A
								                                                        Order by 1,NextFollowUpDate
            ", null);
            return Ok(result);
        }

        [HttpGet("get-enquiry-details/{enquiryID}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Enquiry")]
        public async Task<IActionResult> GetEnquiryDetails(int enquiryID)
        {
            var followupModel = await _dbContext.GetByQueryAsync<EnquiryFollowUpModel>($@"
                                                                        Select E.*,Concat(E.EnquiryNo,' - ',CE.Name) As EnquiryName,CE.Phone,CE.EmailAddress,EI.ItemsCount,
                                                                        CE.Name As CustomerName,UE.EntityID,E.Description,Q.QuotationID,CE.Phone As MobileNumber,LT.LeadThroughID,LT.Name As LeadThroughName,WC.ContactID,
                                                                        Case 
                                                                            when UE.EntityID is null Then 'System' 
                                                                            Else UE.Name End As Username
                                                                        From Enquiry E
                                                                        Left Join viEntity CE ON CE.EntityID=E.CustomerEntityID
                                                                        Left Join WhatsappContact WC ON WC.EntityID=E.CustomerEntityID and WC.IsDeleted=0
                                                                        Left Join viEntity UE ON UE.EntityID=E.UserEntityID
																		Left Join (
                                                                                        Select EIS.EnquiryID,Count(*) As ItemsCount
																					    From EnquiryItem EIS
																					    Where EIS.EnquiryID={enquiryID} and EIS.IsDeleted=0
																					    Group By EIS.EnquiryID
																			    ) EI ON EI.EnquiryID=E.EnquiryID 
                                                                        Left Join Quotation Q ON Q.EnquiryID=E.EnquiryID
                                                                        Left Join LeadThrough LT ON LT.LeadThroughID=E.LeadThroughID
                                                                        Where E.EnquiryID={enquiryID} and E.BranchID={CurrentBranchID} and E.IsDeleted=0", null);

            followupModel.FollowupAssignees = await _dbContext.GetListByQueryAsync<string>($@"
                                                                        Select E.Name
                                                                        From EnquiryAssignee EA
                                                                        Left Join viEntity E ON E.EntityID=EA.EntityID
                                                                        Where EA.EnquiryID={enquiryID} and EA.IsDeleted=0", null);

            followupModel.Histories = await _dbContext.GetListByQueryAsync<FollowUpModel>($@"
                                                                        Select F.* ,U.Name As Username,FS.StatusName As Status,ShortDescription
                                                                        From FollowUp F 
                                                                        Left Join viEntity U ON U.EntityID=F.EntityID
                                                                        Left Join FollowupStatus FS ON FS.FollowUpStatusID=F.FollowUpStatusID and FS.IsDeleted=0
                                                                        Where F.EnquiryID={enquiryID} and F.IsDeleted=0
                                                                        Order By F.FollowUpDate Desc", null);

            followupModel.EnquiryItem = await _dbContext.GetListByQueryAsync<EnquiryItemModel>(@$"
																		Select ROW_NUMBER() OVER(ORDER BY (SELECT 1)) AS RowIndex,
                                                                        I.ItemName,
																		Quantity,EI.Description,EnquiryItemID,I.ItemVariantID,EI.EnquiryID
                                                                        From EnquiryItem EI
																		Left Join viItem I ON I.ItemVariantID=EI.ItemVariantID
                                                                        Where EnquiryID={enquiryID} and EI.IsDeleted=0", null);

            return Ok(followupModel);
        }

        [HttpGet("delete-enquiry")]
        public async Task<IActionResult> DeleteEnquiry(int Id)
        {
            int currentNature = await _dbContext.GetFieldsAsync<Enquiry, int>("CurrentFollowupNature", $"EnquiryID={Id} and IsDeleted=0 and ClientID={CurrentClientID}", null);
            if (currentNature == (int)FollowUpNatures.ClosedWon || currentNature == (int)FollowUpNatures.Followup)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "Invalid operation",
                    ResponseMessage = "The nature of the enquiry follow-up status should be in 'closed' or it should be a new enquiry"
                });
            }

            int logSummaryID = await _dbContext.InsertDeleteLogSummary(Id, "Enquiry deleted by User :" + CurrentUserID);
            await _dbContext.DeleteAsync<Enquiry>(Id, null, logSummaryID);
            await _dbContext.ExecuteAsync($"Update EnquiryItem Set IsDeleted=1 Where EnquiryID={Id}");
            await _dbContext.ExecuteAsync($"Update EnquiryAssignee Set IsDeleted=1 Where EnquiryID={Id}");
            return Ok(true);
        }

        [HttpPost("get-customers-excel-file")]
        public async Task<IActionResult> GetCustomerExcelFile(PagedListPostModelWithFilter filterModel)
        {
            string whereCondition = $"Where EN.BranchID={CurrentBranchID} AND EN.IsDeleted=0";
            whereCondition += _common.GeneratePagedListFilterWhereCondition(filterModel);
            var result = await _dbContext.GetListByQueryAsync<EnquiryExcelDataModel>($@"
                                        Select CONCAT(VE.FirstName,' ',VE.LastName) AS CustomerName,
                                        Case	
	                                        When E.CountryID IS NOT NULL Then CONCAT(CN.ISDCode,REPLACE(E.Phone,' ','')) 
	                                        Else E.Phone
	                                        End As MobileNumber
                                        ,E.CountryID
                                        From Enquiry EN
                                        Left Join Entity E ON EN.CustomerEntityID=E.EntityID AND E.IsDeleted=0
                                        Left Join Country CN ON CN.CountryID=E.CountryID AND CN.IsDeleted=0
                                        Left Join viEntity VE ON E.EntityID=VE.EntityID 
                                        {whereCondition}
                                        Group By EN.CustomerEntityID,E.Phone,E.CountryID,CN.ISDCode,VE.FirstName,VE.LastName
            ", null);

            IWorkbook workbook;
            workbook = new XSSFWorkbook();
            ISheet excelSheet = workbook.CreateSheet("Customer's List");
            excelSheet.DefaultColumnWidth = 20;
            // Create cell style for header row
            ICellStyle headerStyle = workbook.CreateCellStyle();
            headerStyle.FillForegroundColor = IndexedColors.BlueGrey.Index;
            headerStyle.FillPattern = FillPattern.SolidForeground;
            headerStyle.BorderBottom = BorderStyle.Thin;
            headerStyle.BottomBorderColor = IndexedColors.White.Index;

            // Create font for header row
            IFont headerFont = workbook.CreateFont();
            headerFont.IsBold = true;
            headerFont.Color = IndexedColors.White.Index;
            headerFont.FontHeightInPoints = 12;
            headerStyle.SetFont(headerFont);

            #region SetColumnWidth

            excelSheet.SetColumnWidth(0, 10 * 256);
            excelSheet.SetColumnWidth(1, 15 * 256);
            excelSheet.SetColumnWidth(8, 10 * 256);
            excelSheet.SetColumnWidth(9, 10 * 256);
            excelSheet.SetColumnWidth(10, 23 * 256);

            #endregion

            IRow row = excelSheet.CreateRow(0);

            excelSheet.DefaultRowHeightInPoints = 20;
            row.CreateCell(0).SetCellValue("Customer Name");
            row.CreateCell(1).SetCellValue("Mobile Number");

            int rowindex = 0;
            for (int i = 0; i < row.LastCellNum; i++)
            {
                row.GetCell(i).CellStyle = headerStyle;
            }

            foreach (var item in result)
            {
                row = excelSheet.CreateRow(++rowindex);
                if (!string.IsNullOrEmpty(item.CustomerName))
                {
                    row.CreateCell(0).SetCellValue(item.CustomerName);
                }
                else
                {
                    row.CreateCell(0).SetCellValue("--");
                }
                row.CreateCell(1).SetCellValue(item.MobileNumber);
            }
            byte[]? fileContents = null;
            using (var memoryStream = new MemoryStream())
            {
                workbook.Write(memoryStream);
                fileContents = memoryStream.ToArray();
            }

            return Ok(fileContents);
        }


        [HttpGet("get-enquiry-item-details/{Id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Enquiry")]
        public async Task<IActionResult> GetEnquiryItemDetails(int Id)
        {
            EnquiryItemModelNew result = await _dbContext.GetByQueryAsync<EnquiryItemModelNew>(@$"Select IM.ItemVariantID,IM.ItemName,IM.Description,IM.Price,IM.TaxPreferenceTypeID,IM.TaxPreferenceName,IM.CurrentStock,IM.IsGoods
                                                                                            From viItem IM 
                                                                                            Where IM.ItemVariantID={Id}", null);
            return Ok(result??new());
        }
        #endregion

        #region Quotation

        [HttpGet("convert-enquiry-to-quotation/{enquiryID}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Quotation")]
        public async Task<IActionResult> CreateQuotationByEnquiry(int enquiryID)
        {
            QuotationModelNew quotation = await _dbContext.GetByQueryAsync<QuotationModelNew>($@"
                                               Select E.CustomerEntityID,CE.Name AS CustomerName,Cust.TaxNumber,E.EnquiryID,CS.QuotationSubject As Subject,CS.QuotationCustomerNote As CustomerNote,CS.QuotationTermsAndCondition As TermsAndCondition,CNT.CountryID,S.StateID As PlaceOfSupplyID,S.StateName As PlaceOfSupplyName,
                                                Case When CS.SettingID IS NUll THEN 0 
                                                     When CS.SettingID IS NOT NULL THEN CS.QuotationNeedShippingAddress
                                                     Else  CS.QuotationNeedShippingAddress END AS NeedShippingAddress,C.CurrencyID,Case When C.CurrencyID is null then C.CurrencyName else Concat(C.CurrencyName,' ( ',C.Symbol,' )') end AS CurrencyName
                                                From Enquiry E
                                                Join viEntity CE ON CE.EntityID=E.CustomerEntityID
                                                Left Join Customer Cust ON Cust.EntityID = E.CustomerEntityID
                                                Left Join ClientSetting CS on CS.ClientID={CurrentClientID}
                                                Left Join viBranch B ON B.BranchID={CurrentBranchID}
                                                Left Join EntityAddress EA ON EA.AddressID=B.AddressID AND EA.IsDeleted=0
                                                Left Join CountryState S ON S.StateID=EA.StateID And S.IsDeleted=0
                                                Left Join Country CNT ON CNT.CountryID=S.CountryID AND CNT.IsDeleted=0
                                                Left Join Currency C ON C.CurrencyID=CNT.CurrencyID AND C.IsDeleted=0
                                                Where E.EnquiryID={enquiryID} and E.IsDeleted=0 and E.BranchID={CurrentBranchID}", null);

            var enquiryItems = await _dbContext.GetListByQueryAsync<EnquiryItem>($"Select * From EnquiryItem Where EnquiryID={enquiryID} And IsDeleted=0", null);
            int placeOfSupplyID = await _dbContext.GetFieldsAsync<ViBranch, int>("IsNull(StateID,0)", $"BranchID={CurrentBranchID}", null);
            if (enquiryItems is not null)
            {
                foreach (var enquiryItem in enquiryItems)
                {
                    ItemVariantDetail itemModel = await _item.GetItemModelDetails(enquiryItem.ItemVariantID.Value, CurrentBranchID, placeOfSupplyID);
                    var quotationItem = _mapper.Map<QuotationItemModelNew>(itemModel);
                    quotationItem.Quantity = 1;
                    if (enquiryItem.Quantity > 1)
                        quotationItem.Quantity = enquiryItem.Quantity;
                    quotationItem.Rate = itemModel.Price;
                    quotationItem = _cRMRepository.HandleQuotationItemCalculations(quotationItem);
                    quotation.QuotationItems.Add(quotationItem);
                }
            }
            quotation.CurrentFollowupNature = (int)FollowUpNatures.New;
            quotation.EnquiryID = enquiryID;
            return Ok(quotation);
        }

        [HttpPost("get-quotation-item-details")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Quotation")]
        public async Task<IActionResult> GetQuotationItemDetails(ItemSelectedPostModel itemSelectedPostModel)
        {
            var itemModel = await _item.GetItemModelDetails(itemSelectedPostModel.ItemVariantID, CurrentBranchID, itemSelectedPostModel.PlaceOfSupplyID);
            var quotationItem = _mapper.Map<QuotationItemModelNew>(itemModel);
            quotationItem.Rate = itemModel.Price;
            quotationItem = _cRMRepository.HandleQuotationItemCalculations(quotationItem);
            return Ok(quotationItem);
        } 

        [HttpPost("get-updated-quotation-items")]
        public async Task<IActionResult> GetUpdatedQuotationItems(UpdateItemVariantsPostRequestModel postModel)
        {
            List<QuotationItemModelNew> quotationItems = new();
            foreach (var itemModelID in postModel.ItemVariantIDs)
            {
                ItemVariantDetail itemModel = await _item.GetItemModelDetails(itemModelID, CurrentBranchID, postModel.PlaceOfSupplyID);
                var quotationItem = _mapper.Map<QuotationItemModelNew>(itemModel);
                quotationItem.Rate = itemModel.Price;
                quotationItem = _cRMRepository.HandleQuotationItemCalculations(quotationItem);
                quotationItems.Add(quotationItem);
            }
            return Ok(quotationItems);
        }

        [HttpGet("get-quotation/{quotationID}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Quotation")]
        public async Task<IActionResult> GetQuotation(int quotationID)
        {
            return Ok(await _cRMRepository.GetQuotationById(quotationID, CurrentBranchID));
        }

        [HttpPost("save-quotation")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Quotation")]
        public async Task<IActionResult> SaveQuotation(QuotationModelNew model)
        {
            if (model.QuotationID > 0 && model.QuotationNo == 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ResponseMessage = "Not a valid quotation number"
                });
            }
            //int QuotationNo = await _dbContext.GetByQueryAsync<int>(@$"
            //                           Select IsNull(Count(QuotationNo),0) QuotationNo
            //                           From Quotation
            //                           Where QuotationNo={model.QuotationNo} and QuotationID<>{model.QuotationID} and  BranchID={CurrentBranchID}   and IsDeleted=0",null);

            //if (QuotationNo != 0)
            //{
            //    return BadRequest(new BaseErrorResponse()
            //    {
            //        ErrorCode = 0,
            //        ResponseTitle = "Duplication not allowed",
            //        ResponseMessage = "Quotation number already exist,try different quotation numner"
            //    });
            //}
            model.BranchID = CurrentBranchID;
            model.ClientID = CurrentClientID;
            model.UserEntityID = model.UserEntityID == null ? CurrentEntityID : model.UserEntityID;
            try
            {
                _cn.Open();
                using (var tran = _cn.BeginTransaction())
                {
                    #region Converted Enquiry Status Updation

                    if (model.EnquiryID != null)
                    {
                        FollowUpModel followUpModel = new()
                        {
                            FollowUpStatusID = await _dbContext.GetByQueryAsync<int?>("Select Top 1 FollowUpStatusID From FollowUpStatus Where Nature=@Nature And Type=@Type And (ClientID=@ClientID Or IsNull(ClientID,0)=0) And IsDeleted=0", new { Nature = (int)FollowUpNatures.ClosedWon, Type = (int)FollowUpTypes.Enquiry, ClientID = CurrentClientID }, tran),
                            EntityID = CurrentEntityID,
                            FollowUpType = (int)FollowUpTypes.Enquiry,
                            EnquiryID = model.EnquiryID.Value,
                        };
                        await _cRMRepository.SaveFollowup(followUpModel, tran);
                    }

                    #endregion

                    #region Quotation Number

                    //if (model.QuotationID == 0 || model.QuotationNo == 0)
                    //{
                    //    model.QuotationNo = await _dbContext.GetByQueryAsync<int>($@"
                    //                                            Select isnull(max(QuotationNo)+1,1) AS QuotationNo
                    //                                            From Quotation
                    //                                            Where BranchID={CurrentBranchID} and IsDeleted=0", null, tran);
                    //}



                    #endregion

                    #region Quotation

                    var quotation = _mapper.Map<Quotation>(model);
                    quotation.QuotationID = await _dbContext.SaveAsync(quotation, tran);

                    #endregion

                    #region Quotation Items

                    var quotationItems = _mapper.Map<List<QuotationItem>>(model.QuotationItems);
                    await _dbContext.SaveSubItemListAsync(quotationItems, "QuotationID", quotation.QuotationID, tran);

                    #endregion

                    #region Quotation Assignees

                    if (model.QuotationAssignees is not null && model.QuotationAssignees.Count > 0)
                    {
                        var quotationAssignees = _mapper.Map<List<QuotationAssignee>>(model.QuotationAssignees);
                        if (quotationAssignees.Count > 0)
                        {
                            await _dbContext.SaveSubItemListAsync(quotationAssignees, "QuotationID", quotation.QuotationID, tran);
                        }
                    }

                    #endregion

                    #region Mail Recipients

                    if (model.MailReciepients is not null && model.MailReciepients.Count > 0)
                    {
                        var quotationMailRecipients = _mapper.Map<List<QuotationMailRecipient>>(model.MailReciepients);
                        if (quotationMailRecipients.Count > 0)
                        {
                            await _dbContext.SaveSubItemListAsync(quotationMailRecipients, "QuotationID", quotation.QuotationID, tran);
                        }
                    }

                    #endregion

                    tran.Commit();

                    #region Existing Quotationn pdf remove

                    if(model.QuotationID > 0)
                    {
                        await _dbContext.ExecuteAsync($"Update Quotation Set MediaID=NULL Where QuotationID={model.QuotationID}", null);
                        await _cRMRepository.RemoveQuotationPdfFiles(model.QuotationID, CurrentBranchID, model.QuotationNo);
                    }

                    #endregion

                    if (model.GenerateQuotationPdf)
                    {
                        //Notification
                        if (model.QuotationID is 0)
                            await _common.SendQuotationPushAndNotification(CurrentClientID, quotation.QuotationID, CurrentEntityID, null);
                        //await _cRMRepository.GenerateQuotationPdf(quotation.QuotationID, CurrentBranchID, CurrentClientID);
                        await _cRMRepository.GenerateNewQuotationPdf(quotation.QuotationID, CurrentBranchID, CurrentClientID);
                        MailDetailsModel quotationMailDetails = await _common.GetQuotationPdfMailDetails(quotation.QuotationID, CurrentBranchID);
                        quotationMailDetails.ID = quotation.QuotationID;
                        return Ok(quotationMailDetails);
                    }
                    else
                    {
                        //Notification
                        if (model.QuotationID is 0)
                            await _common.SendQuotationPushAndNotification(CurrentClientID, quotation.QuotationID, CurrentEntityID, null);
                        return Ok(new QuotationSuccessModel() { QuotationID = quotation.QuotationID });
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ResponseMessage = ex.Message,
                });
            }
        }

        [HttpGet("delete-quotation")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Quotation")]
        public async Task<IActionResult> DeleteQuotation(int Id)
        {
            int currentNature = await _dbContext.GetFieldsAsync<Quotation, int>("CurrentFollowupNature", $"QuotationID={Id} and IsDeleted=0 and ClientID={CurrentClientID}", null);
            if (currentNature == (int)FollowUpNatures.ClosedWon || currentNature == (int)FollowUpNatures.Followup)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "Invalid operation",
                    ResponseMessage = "The nature of the quotation should be in 'closed' or it should be a new quotation"
                });
            }

            await _dbContext.DeleteAsync<Quotation>(Id);
            await _dbContext.ExecuteAsync($"Update QuotationItem Set IsDeleted=1 Where QuotationID={Id} And BranchID={CurrentBranchID}");
            await _dbContext.ExecuteAsync($"Update QuotationAssignee Set IsDeleted=1 Where QuotationID={Id} And BranchID={CurrentBranchID}");
            return Ok(true);
        }

        [HttpGet("generate-quotation-pdf/{quotationID}")]
        public async Task<IActionResult> GenerateQuotationPdf(int quotationID)
        {
            PdfGeneratedResponseModel pdfGeneratedResponseModel = new()
            {
                MediaID = await _cRMRepository.GenerateQuotationPdf(quotationID, CurrentBranchID, CurrentClientID),
                MailDetails = await _common.GetQuotationPdfMailDetails(quotationID, CurrentBranchID), 
            };
            pdfGeneratedResponseModel.MailDetails.ID = quotationID;
            return Ok(pdfGeneratedResponseModel);
        }

        [HttpGet("get-quotation-pdf/{quotationID}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Quotation")]
        public async Task<IActionResult> GetQuotationPDFDetails(int quotationID)
        {
            StringModel htmlContent = new();
            htmlContent.Value = await _pdf.GetQuotationPdfHtmlContent(quotationID, CurrentBranchID);
            return Ok(htmlContent);
        }

        [HttpGet("get-quotation-mail-details/{quotationID}")]
        public async Task<IActionResult> GetInvoicePdfMailDetails(int quotationID)
        {
            MailDetailsModel invoiceMailDetails = await _common.GetQuotationPdfMailDetails(quotationID, CurrentBranchID);
            invoiceMailDetails.ID = quotationID;
            return Ok(invoiceMailDetails);
        }

        [HttpGet("get-quotation-menu-list")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Quotation")]
        public async Task<IActionResult> GetQuotationMenuList()
        {
            string whereCondition = $"Where Q.IsDeleted=0 and Q.BranchID={CurrentBranchID}";
            if (CurrentUserTypeID > (int)UserTypes.Client)
            {
                string commaSeparatedIds = await _cRMRepository.GetAccessableQuotationForUser(CurrentEntityID, CurrentBranchID);
                if (!string.IsNullOrEmpty(commaSeparatedIds))
                    whereCondition += $" AND Q.QuotationID in ({commaSeparatedIds})";
            }

            var result = await _dbContext.GetListByQueryAsync<ViewPageMenuModel>($@"Select Case When NextFollowUpDate is null then 1 else  DATEDIFF(DAY,Today,NextFollowUpDate) end as Days,
								                                                        ID,MenuName,QuotationNo
								                                                        From
										                                                        (Select Q.QuotationID as ID,CONVERT(varchar(20), QuotationNo)+'-' + V.Name as MenuName,QuotationNo,
															                                            Case When F.FollowUpID Is Null Then Q.FirstFollowUpDate Else F.NextFollowUpDate End As NextFollowUpDate,
															                                            Convert(date,DATEADD(MINUTE,{TimeOffset}, getutcdate())) Today
															                                            From Quotation Q
															                                            Join viEntity V on V.EntityID=Q.CustomerEntityID
                                                                                                        LEFT JOIN ( Select QuotationID,Max(FollowupID) As FollowupID 
                                                                                                                    From FollowUp 
                                                                                                                    Where IsDeleted=0 and FollowUpType={(int)FollowUpTypes.Quotation}
                                                                                                                    Group by QuotationID
                                                                                                                    ) EF on EF.QuotationID=Q.QuotationID
															                                            Left Join FollowUp F ON F.FollowupID=EF.FollowupID and F.IsDeleted=0
															                                            {whereCondition}) as A
								                                                        Order by QuotationNo DESC", null);
            return Ok(result);
        }

        [HttpGet("get-quotation-followup-menu-list")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "FollowUp")]
        public async Task<IActionResult> GetQuotationFollowupMenuList()
        {
            string whereCondition = $"Where Q.IsDeleted=0 and Q.BranchID={CurrentBranchID}";
            if (CurrentUserTypeID > (int)UserTypes.Client)
            {
                string commaSeparatedIds = await _cRMRepository.GetAccessableQuotationForUser(CurrentEntityID, CurrentUserTypeID);
                if (!string.IsNullOrEmpty(commaSeparatedIds))
                    whereCondition += $" AND Q.QuotationID in ({commaSeparatedIds})";
            }
            var result = await _dbContext.GetListByQueryAsync<ViewPageMenuModel>($@"
                                                                                        Select Case When NextFollowUpDate is null then 1 else  DATEDIFF(DAY,Today,NextFollowUpDate) end as Days,
								                                                        ID,MenuName
								                                                        From
										                                                        (Select Q.QuotationID as ID,CONVERT(varchar(20), QuotationNo)+'-' + V.Name as MenuName,
															                                            Case When F.FollowUpID Is Null Then Q.FirstFollowUpDate Else F.NextFollowUpDate End As NextFollowUpDate,
															                                            Convert(date,DATEADD(MINUTE,330, getutcdate())) Today
															                                            From Quotation Q
															                                            Join viEntity V on V.EntityID=Q.CustomerEntityID
                                                                                                        LEFT JOIN ( Select QuotationID,Max(FollowupID) As FollowupID 
                                                                                                                    From FollowUp 
                                                                                                                    Where IsDeleted=0 and FollowUpType={(int)FollowUpTypes.Quotation}
                                                                                                                    Group by QuotationID
                                                                                                                    ) EF on EF.QuotationID=Q.QuotationID
															                                            Left Join FollowUp F ON F.FollowupID=EF.FollowupID and F.IsDeleted=0
															                                            {whereCondition}) as A
								                                                        Order by 1,NextFollowUpDate
            ", null);
            return Ok(result);
        }

        [HttpGet("get-quotation-details/{quotationID}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Quotation")]
        public async Task<IActionResult> GetCompleteQuotationDetails(int quotationID)
        {
            var res = await _dbContext.GetByQueryAsync<QuotationFollowupModel>($@"
                                                                        Select Q.*,Concat(Q.QuotationNo,' - ',CE.Name) As QuotationName,
                                                                        CE.Name As CustomerName,CE.EmailAddress,I.InvoiceID,
                                                                        Case 
                                                                            when UE.EntityID is null then 'System'
                                                                            else UE.Name 
                                                                            end As Username,
																		QI.ItemsCount,QI.TotalAmount,Q.MediaID,CE.Phone As MobileNumber,WC.ContactID,M.FileName,C.TaxNumber 
                                                                        From Quotation Q
                                                                        Left Join viEntity CE ON CE.EntityID=Q.CustomerEntityID
                                                                        Left Join Customer C ON C.EntityID=Q.CustomerEntityID
                                                                        Left Join viEntity UE ON UE.EntityID=Q.UserEntityID
																		Left Join (
																							Select QIS.QuotationID,Count(*) As ItemsCount,Sum(IsNull(QIS.GrossAmount,0)) As TotalAmount
																							From QuotationItem QIS
																							Where QIS.QuotationID={quotationID} and QIS.IsDeleted=0
																							Group By QIS.QuotationID
																					) QI ON QI.QuotationID=Q.QuotationID 
																		Left Join WhatsappContact WC ON WC.EntityID=Q.CustomerEntityID and WC.IsDeleted=0
                                                                        Left Join Media M ON M.MediaID=Q.MediaID AND M.IsDeleted=0
                                                                        Left Join Invoice I ON I.QuotationID=Q.QuotationID And I.IsDeleted=0
                                                                        Where Q.QuotationID={quotationID} and Q.BranchID={CurrentBranchID} and Q.IsDeleted=0", null);
            res.FollowupAssignees = await _dbContext.GetListByQueryAsync<string>($@"
                                                                        Select E.Name
                                                                        From QuotationAssignee QA
                                                                        Left Join viEntity E ON E.EntityID=QA.EntityID
                                                                        Where QA.QuotationID={quotationID} and QA.IsDeleted=0", null);
            res.Histories = await _dbContext.GetListByQueryAsync<FollowUpModel>($@"
                                                                        Select F.* ,U.Name As Username,FS.StatusName As Status,ShortDescription
                                                                        From FollowUp F 
                                                                        Left Join viEntity U ON U.EntityID=F.EntityID
                                                                        Left Join FollowupStatus FS ON FS.FollowUpStatusID=F.FollowUpStatusID and FS.IsDeleted=0
                                                                        Where F.QuotationID={quotationID} and F.IsDeleted=0 
                                                                        Order By F.FollowUpDate Desc", null);
            res.QuotationItems = await _dbContext.GetListByQueryAsync<QuotationItemModel>($@"
                                                                        Select I.ItemName,QI.*,(QI.Rate * QI.Quantity) AS TotalAmount
                                                                        From QuotationItem QI
																        Left Join viItem I ON I.ItemVariantID=QI.ItemVariantID
                                                                        Where QI.QuotationID={quotationID} and QI.IsDeleted=0", null);
            return Ok(res);
        }

        [HttpPost("get-quotation-paged-list")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Quotation")]
        public async Task<IActionResult> GetQuotationPagedList(PagedListPostModelWithFilter searchModel)
        {
            PagedListQueryModel query = searchModel;
            query.Select = $@"Select QuotationID,Date As AddedOn,QuotationNo,Case When Type={(int)CustomerTypes.Individual} Then EP.Name else EP.Name end as CustomerName,
                                    Case When Type={(int)CustomerTypes.Individual} Then I.FirstName else EP.Name end as ContactName,
                                    uE.Name As Username,CurrentFollowupNature,ExpiryDate As ExpireOn,Prefix,Suffix,
                                    cE.Name as CreatedFor
                                    From Quotation Q
                                    Join Customer C on C.EntityID=Q.CustomerEntityID and C.IsDeleted=0
                                    Left Join viEntity EP ON EP.EntityID=C.EntityID --and EP.IsDeleted=0
                                    Left Join EntityPersonalInfo I ON I.EntityID=EP.EntityID and I.IsDeleted=0
                                    --Left Join EntityInstituteInfo EI on EI.EntityID=C.EntityID and EI.IsDeleted=0
                                    Left Join viEntity uE ON uE.EntityID=Q.UserEntityID
                                    Left Join viEntity cE ON cE.EntityID=Q.QuotationCreatedFor";

            query.WhereCondition = $"Q.IsDeleted=0 and Q.BranchID={CurrentBranchID}";
            query.OrderByFieldName = searchModel.OrderByFieldName;
            if (CurrentUserTypeID > (int)UserTypes.Client)
            {
                var quotationIdList = await _cRMRepository.GetAccessableQuotationForUser(CurrentEntityID, CurrentBranchID);
                if (!string.IsNullOrEmpty(quotationIdList))
                    query.WhereCondition += $" and Q.QuotationID IN ({quotationIdList})";
            }
            query.SearchLikeColumnNames = new() { "EI.Name", "EP.FirstName", "uE.Name" };
            query.WhereCondition += _common.GeneratePagedListFilterWhereCondition(searchModel);
            return Ok(await _dbContext.GetPagedList<QuotationListModel>(query, null));
        }


        [HttpGet("get-staff-phone-number/{entityID}")]
        public async Task<IActionResult>GetStaffNo(int entityID)
        {
            var res = await _dbContext.GetByQueryAsync<IdnValuePair>($@"Select EntityID as ID,Phone as Value
                                                                                    From viEntity
                                                                                    Where EntityID={entityID}", null);
            return Ok(res??new());
        }
        #endregion

        #region Follow-up

        [HttpPost("get-follow-up-paged-list")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "FollowUp")]
        public async Task<IActionResult> GetFollowupPagedList(PagedListPostModelWithFilter searchModel)
        {
            var result = new FollowupPagedListModel();
            PagedListQueryModel query = searchModel;
            query.Select = $@"Select vF.* ,UE.Name AS FollowupBy,
                                Case When Type=1 Then EAS.Assignee 
	                                 When Type=2 Then QAS.Assignee end as AssigneeName
                                From viFollowup vF
                                Left JOIN viEntity UE ON vF.EntityID=UE.EntityID
                                LEFT JOIN (
                                            Select STRING_AGG(E.Name, ',') AS Assignee,EA.EnquiryID
                                            From EnquiryAssignee EA
                                            Join viEntity E ON E.EntityID=EA.EntityID
			                                Where EA.IsDeleted=0
                                            Group By EA.EnquiryID
                                            ) EAS ON vF.EnquiryID=EAS.EnquiryID
                                LEFT JOIN (
                                            Select STRING_AGG(E.Name, ',') AS Assignee,QA.QuotationID
                                            From QuotationAssignee QA
                                            Join viEntity E ON E.EntityID=QA.EntityID
			                                Where QA.IsDeleted=0
                                            Group By QA.QuotationID
                                            ) QAS ON vF.QuotationID=QAS.QuotationID";
            query.WhereCondition = $"vF.BranchID={CurrentBranchID} AND vF.CurrentFollowupNature NOT IN({(int)FollowUpNatures.ClosedWon},{(int)FollowUpNatures.Dropped})";
            query.OrderByFieldName = searchModel.OrderByFieldName;
            if (CurrentUserTypeID > (int)UserTypes.Client)
            {
                string? commaSeparatedEnquiryIDs = await _cRMRepository.GetAccessableEnquiriesForUser(CurrentEntityID, CurrentBranchID);
                string? commaSeparatedQuotationIDs = await _cRMRepository.GetAccessableQuotationForUser(CurrentEntityID, CurrentBranchID);
                if (!string.IsNullOrEmpty(commaSeparatedQuotationIDs) && !string.IsNullOrEmpty(commaSeparatedEnquiryIDs))
                {
                    query.WhereCondition += $"AND (vF.EnquiryID IN ({commaSeparatedEnquiryIDs}) OR vF.QuotationID IN ({commaSeparatedQuotationIDs}))";
                }
                if (!string.IsNullOrEmpty(commaSeparatedQuotationIDs) && string.IsNullOrEmpty(commaSeparatedEnquiryIDs))
                {
                    query.WhereCondition += $"AND (vF.QuotationID IN ({commaSeparatedQuotationIDs}))";
                }
                if (string.IsNullOrEmpty(commaSeparatedQuotationIDs) && !string.IsNullOrEmpty(commaSeparatedEnquiryIDs))
                {
                    query.WhereCondition += $"AND (vF.EnquiryID IN ({commaSeparatedEnquiryIDs}))";
                }
            }
            query.OrderByFieldName = searchModel.OrderByFieldName;
            query.WhereCondition += _common.GeneratePagedListFilterWhereCondition(searchModel, "CustomerName");
            result.FollowupList = await _dbContext.GetPagedList<FollowupListModel>(query, null);
            result.UserTypeID = CurrentUserTypeID;
            return Ok(result);
        }

        [HttpGet("get-followup/{followUpID}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "FollowUp")]
        public async Task<IActionResult> GetFollowup(int followUpID)
        {
            var result = await _dbContext.GetByQueryAsync<FollowUpModel>($@"
                                                                Select F.*,E.Name As UserName,FS.StatusName As Status
                                                                From FollowUp F
                                                                Join viEntity E ON E.EntityID=F.EntityID
                                                                Left Join FollowUpStatus FS ON FS.FollowUpStatusID=F.FollowUpStatusID and FS.IsDeleted=0
                                                                Where F.FollowUpID={followUpID} and F.IsDeleted=0
            ", null);

            return Ok(result);
        }

        [HttpPost("save-followup")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "FollowUp")]
        public async Task<IActionResult> SaveFollowup(FollowUpModel model)
        {
            var folloupNature = await _dbContext.GetAsync<FollowupStatus>(model.FollowUpStatusID.Value);
            FollowUpNotificationPostModel notificationPostModel = new();
            if (folloupNature != null)
            {
                if (model.FollowUpType == (int)FollowUpTypes.Enquiry)
                {
                    await _dbContext.ExecuteAsync("Update Enquiry Set CurrentFollowupNature=@Nature Where EnquiryID=@EnquiryID", new { Nature = folloupNature.Nature, EnquiryID = model.EnquiryID });
                    notificationPostModel = new()
                    {
                        ID=model.EnquiryID,
                        FollowupType= (int)FollowUpTypes.Enquiry,
                    };
                }

                if (model.FollowUpType == (int)FollowUpTypes.Quotation)
                {
                    await _dbContext.ExecuteAsync("Update Quotation Set CurrentFollowupNature=@Nature Where QuotationID=@QuotationID", new { Nature = folloupNature.Nature, QuotationID = model.QuotationID });
                    notificationPostModel = new()
                    {
                        ID = model.QuotationID,
                        FollowupType = (int)FollowUpTypes.Quotation,
                    };
                }
            }

            var followUp = _mapper.Map<FollowUp>(model);
            followUp.EntityID = CurrentEntityID;
            followUp.FollowUpID = await _dbContext.SaveAsync(followUp);

            //Notification
            await _common.SendFollowupPushAndNotification(notificationPostModel,CurrentClientID, followUp.FollowUpID, CurrentEntityID, null);
            return Ok(new FollowupAddResultModel() { FollowupID = followUp.FollowUpID });
        }

        [HttpGet("delete-followup/{followupID}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "FollowUp")]
        public async Task<IActionResult> DeleteFollowUp(int followupID)
        {
            var followUp = await _dbContext.GetAsync<FollowUp>(followupID);
            var logSummaryID = await _dbContext.InsertDeleteLogSummary(followupID, "FollowUp : " + followupID + " deleted by User " + CurrentUserID);
            await _dbContext.ExecuteAsync($"Update FollowUp Set IsDeleted=1 Where FollowUpID={followupID}", null);
            switch (followUp.FollowUpType)
            {
                case (int)FollowUpTypes.Enquiry:
                    var updateEnquiryFollowUp = await _dbContext.GetByQueryAsync<FollowUp>($@"
                                                    Select TOP 1 * 
                                                    From FollowUp 
                                                    Where EnquiryID={followUp.EnquiryID} and IsDeleted=0
                                                    Order By FollowUpDate Desc", null);
                    if (updateEnquiryFollowUp != null)
                    {
                        var enquiryFolloupNature = await _dbContext.GetAsync<FollowupStatus>(updateEnquiryFollowUp.FollowUpStatusID.Value);
                        await _dbContext.ExecuteAsync("Update Enquiry Set CurrentFollowupNature=@Nature Where EnquiryID=@EnquiryID", new { Nature = enquiryFolloupNature.Nature, EnquiryID = updateEnquiryFollowUp.EnquiryID });
                    }
                    else
                    {
                        await _dbContext.ExecuteAsync("Update Enquiry Set CurrentFollowupNature=0 Where EnquiryID=@EnquiryID", new { EnquiryID = followUp.EnquiryID });
                    }
                    break;

                case (int)FollowUpTypes.Quotation:
                    var updateQuotationFollowUp = await _dbContext.GetByQueryAsync<FollowUp>($@"
                                                    Select TOP 1 *
                                                    From FollowUp 
                                                    Where QuotationID={followUp.QuotationID} and IsDeleted=0
                                                    Order By FollowUpDate Desc", null);
                    if (updateQuotationFollowUp != null)
                    {
                        var quotationFolloupNature = await _dbContext.GetAsync<FollowupStatus>(updateQuotationFollowUp.FollowUpStatusID.Value);
                        await _dbContext.ExecuteAsync("Update Quotation Set CurrentFollowupNature=@Nature Where QuotationID=@QuotationID", new { Nature = quotationFolloupNature.Nature, QuotationID = updateQuotationFollowUp.QuotationID });
                    }
                    else
                    {
                        await _dbContext.ExecuteAsync("Update Quotation Set CurrentFollowupNature=0 Where QuotationID=@QuotationID", new { QuotationID = followUp.QuotationID });
                    }
                    break;
            }
            return Ok(true);
        }

        [HttpGet("get-current-username")]
        public async Task<IActionResult> GetCurrentUsername()
        {
            StringModel result = new();
            result.Value = await _dbContext.GetByQueryAsync<string>($@"
                                                    Select vE.Name
                                                    From viEntity vE
                                                    Where EntityID={CurrentEntityID}
            ", null);
            return Ok(result);
        }

        [HttpGet("get-follow-up-notifications")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetFollowupNotifications()
        {
            var followupNotifications = await _dbContext.GetListByQueryAsync<HeaderNotifyCountModel>(@$"
                                                Select Count(*) AS NotifyCount, 'Pending' AS NotifyValue, -1 AS Nature
                                                From viFollowup F
                                                Where F.Days < 0 AND F.BranchID={CurrentBranchID} AND CurrentFollowupNature Not in ({(int)FollowUpNatures.Dropped},{(int)FollowUpNatures.ClosedWon})

                                                UNION ALL

                                                Select Count(*) AS NotifyCount, 'Todays' AS NotifyValue, 0 AS Nature
                                                From viFollowup F
                                                Where F.Days = 0 AND F.BranchID={CurrentBranchID} AND CurrentFollowupNature Not in ({(int)FollowUpNatures.Dropped},{(int)FollowUpNatures.ClosedWon})

                                                UNION ALL

                                                Select Count(*) AS NotifyCount, 'Upcoming' AS NotifyValue, 1 AS Nature
                                                From viFollowup F
                                                Where F.Days > 0 AND F.BranchID={CurrentBranchID} AND CurrentFollowupNature Not in ({(int)FollowUpNatures.Dropped},{(int)FollowUpNatures.ClosedWon})
            ", null);
            return Ok(followupNotifications);
        }

        #endregion

        #region Follow-up Status

        [HttpGet("get-followup-status-list")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "ClientSetting")]
        public async Task<IActionResult> GetFollowUpStatus()
        {
            var result = await _dbContext.GetListByQueryAsync<FollowupStatusModel>($@"Select * 
                                                                                        From FollowUpStatus 
                                                                                        Where (ClientID={CurrentClientID} OR ClientID IS NULL) and IsDeleted=0", null);
            return Ok(result);
        }

        [HttpPost("save-followup-status")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "ClientSetting")]
        public async Task<IActionResult> SaveFollowUpStatus(FollowupStatus model)
        {
            var Count = await _dbContext.GetByQueryAsync<int>(@$"Select Count(*) 
                                                                    from FollowupStatus
                                                                    where LOWER(TRIM(StatusName)) =LOWER(TRIM(@StatusName))
                                                                        and FollowUpStatusID<>@FollowUpStatusID 
                                                                        and Type=@Type
                                                                        and (ClientID={CurrentClientID} OR ClientID IS NULL)
                                                                        and IsDeleted=0", model);
            if (Count != 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "Followup Status already exist",
                    ResponseMessage = "The Followup Status already exist,try different one"
                });
            }
            try
            {
                model.ClientID = CurrentClientID;
                model.FollowUpStatusID = await _dbContext.SaveAsync<FollowupStatus>(model);
                return Ok(new FollowupStatusAddResultModel() { FollowUpStatusID = model.FollowUpStatusID, StatusName = model.StatusName });
            }
            catch (Exception err)
            {

                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "Something went wrong",
                    ResponseMessage = err.Message
                });
            }

        }

        [HttpPost("get-all-followup-status")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "ClientSetting")]
        public async Task<IActionResult> GetFollowups(FollowupStatusSearchModel model)
        {
            var result = await _dbContext.GetListByQueryAsync<FollowupStatusListModel>($@"Select * From FollowUpStatus
                                                                                            Where Nature={model.Nature} and Type={model.Type} and IsDeleted=0", null);

            return Ok(result);
        }

        [HttpGet("get-followup-status/{followupStatusID}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetFollowUp(int followupStatusID)
        {
            var result = await _dbContext.GetByQueryAsync<FollowupStatus>($@"Select * 
                                                                            From FollowUpStatus 
                                                                            Where FollowUpStatusID={followupStatusID} and IsDeleted=0", null);
            return Ok(result);
        }

        [HttpGet("delete-status/{Id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "ClientSetting")]
        public async Task<IActionResult> DeleteStatus(int Id)
        {
            int enquiryCount = await _dbContext.GetByQueryAsync<int>($@"SELECT Count(*) AS DataCount 
                                                                           from FollowUp F
                                                                            LEFT JOIN Enquiry E ON E.EnquiryID=F.EnquiryID
                                                                            WHERE E.IsDeleted =0 and F.IsDeleted=0 and FollowUpStatusID={Id}
                                                                            GROUP BY FollowUpStatusID", null);

            if (enquiryCount > 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "Invalid operation",
                    ResponseMessage = "Enquiry exist  with the FollowupStatus you are trying to remove"
                });
            }

            int quotationCount = await _dbContext.GetByQueryAsync<int>($@"SELECT Count(*) AS DataCount 
                                                                           from FollowUp F
                                                                            LEFT JOIN Quotation E ON E.QuotationID=F.QuotationID
                                                                            WHERE E.IsDeleted =0 and F.IsDeleted=0 and FollowUpStatusID={Id}
                                                                            GROUP BY FollowUpStatusID", null);

            if (quotationCount > 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "Invalid operation",
                    ResponseMessage = "Quotation entry exist with the FollowupStatus you are trying to remove"
                });
            }
            await _dbContext.ExecuteAsync($"UPDATE FollowUpStatus SET IsDeleted=1 Where FollowUpStatusID={Id}");
            return Ok(true);
        }

        [HttpPost("get-followup-status-paged-list")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "ClientSetting")]
        public async Task<IActionResult> GetStatusList(FollowupStatusPostModel model)
        {
            PagedListQueryModel query = model;
            query.Select = $@"Select FollowupStatusID,StatusName,Nature,ClientID
								From FollowupStatus";
            query.WhereCondition = $"Type={model.StatusTypeID} and IsDeleted=0 and (ClientID={CurrentClientID} OR ClientID IS NULL)";
            query.OrderByFieldName = model.OrderByFieldName;
            query.SearchString = model.SearchString;
            query.SearchLikeColumnNames = new() { "StatusName" };
            var res = await _dbContext.GetPagedList<FollowupStatusListViewModel>(query, null);
            return Ok(res);
        }

        [HttpGet("get-followup-status-nature/{followupStatusID}")]
        public async Task<IActionResult> GetFollowupStatusNature(int followupStatusID)
        {
            return Ok(await _dbContext.GetFieldsAsync<FollowupStatus, int>("Nature", $"FollowUpStatusID={followupStatusID}", null));
        }

        #endregion

        #region Business Type


        [HttpGet("get-business-type-list")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "ClientSetting")]
        public async Task<IActionResult> GetBusinessType()
        {
            var result = await _dbContext.GetListByQueryAsync<BusinessTypeModel>($@"Select * 
                                                                                        From BusinessType 
                                                                                        Where (ClientID={CurrentClientID} OR ClientID IS NULL) and IsDeleted=0", null);
            return Ok(result);
        }



        [HttpPost("save-business-type")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "ClientSetting")]
        public async Task<IActionResult> SaveBusinessType(BusinessTypeModel model)
        {
            var Count = await _dbContext.GetByQueryAsync<int>(@$"Select Count(*) 
                                                                    from BusinessType
                                                                    where LOWER(TRIM(BusinessTypeName)) =LOWER(TRIM(@BusinessTypeName))
                                                                        and BusinessTypeID<>@BusinessTypeID 
                                                                        and (ClientID={CurrentClientID} OR ClientID IS NULL)
                                                                        and IsDeleted=0", model);
            if (Count != 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "Business type already exist",
                    ResponseMessage = "The business type already exist,try different one"
                });
            }
            try
            {
                var businessType = _mapper.Map<BusinessType>(model);
                businessType.ClientID = CurrentClientID;
                businessType.BusinessTypeID = await _dbContext.SaveAsync(businessType);
                return Ok(new BusinessTypeAddResultModel() { BusinessTypeID = businessType.BusinessTypeID, BusinessTypeName = model.BusinessTypeName });
            }
            catch (Exception err)
            {

                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "Something went wrong",
                    ResponseMessage = err.Message
                });
            }

        }


        [HttpGet("get-business-type/{businessTypeID}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetBusinessTypes(int businessTypeID)
        {
            var result = await _dbContext.GetByQueryAsync<BusinessTypeModel>($@"Select * 
                                                                            From BusinessType 
                                                                            Where BusinessTypeID={businessTypeID} and IsDeleted=0", null);
            return Ok(result);
        }


        #endregion

        #region New Quotation Pdf

        [HttpGet("view-quotation-pdf/{quotationID}")]
        public async Task<IActionResult>ViewPdf(int quotationID)
        {

            StringModel htmlContent = new();
            htmlContent.Value = await _pdf.GenerateAllQuotationContent(quotationID, CurrentBranchID,null);
            return Ok(htmlContent);
        }



        [HttpGet("generate-new-quotation-pdf/{quotationID}")]
        public async Task<IActionResult> GenerateNewQuotationPdf(int quotationID)
        {
            PdfGeneratedResponseModel pdfGeneratedResponseModel = new()
            {
                MediaID = await _cRMRepository.GenerateNewQuotationPdf(quotationID, CurrentBranchID, CurrentClientID),
                MailDetails = await _common.GetQuotationPdfMailDetails(quotationID, CurrentBranchID),
            };
            pdfGeneratedResponseModel.MailDetails.ID = quotationID;
            return Ok(pdfGeneratedResponseModel);
        }

        #endregion

        #region Quotation No/Prefix/Suffix


        [HttpGet("get-quotation-number")]
        public async Task<IActionResult> GetInvoiceNo()
        {
            int quotationNo = await _dbContext.GetByQueryAsync<int>($@"Select Isnull(Max(QuotationNo)+1,1) AS QuotationNo
                                                                        From Quotation
                                                                        Where BranchID={CurrentBranchID} and IsDeleted=0", null);
            return Ok(quotationNo);
        }





        #endregion
    }
}
