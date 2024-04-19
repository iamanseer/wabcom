using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PB.Shared.Models;
using PB.Model;
using PB.Shared;
using PB.Shared.Enum;
using PB.Shared.Models.Common;
using PB.Shared.Models.Court;
using PB.Shared.Tables;
using PB.Shared.Tables.CourtClient;
using System.Data;
using PB.Model.Models;
using PB.EntityFramework;
using PB.Shared.Models.CRM.Customer;
using PB.Shared.Enum.Court;
using PB.Shared.Helpers;
using PB.Shared.Tables.Accounts.JournalMaster;
using PB.Shared.Tables.Accounts.VoucherTypes;
using PB.Server.Repository;
using PB.Shared.Enum.Accounts;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using PB.Client.Pages.Court;
using PB.Shared.Tables.Court;
using PB.Shared.Models.Accounts.VoucherTypes;
using System.Runtime.InteropServices;
using System.Xml;
using System.Diagnostics.Eventing.Reader;
using PB.DatabaseFramework;

namespace PB.Server.Controllers
{
    [Route("api/court")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CourtController : BaseController
    {
        private readonly IDbContext _dbContext;
        private readonly IDbConnection _cn;
        private readonly IMapper _mapper;
        private readonly IAccountRepository _accounts;
        private readonly ICommonRepository _common;
        private readonly INotificationRepository _notification;

        public CourtController(IDbContext dbContext, IDbConnection cn, IMapper mapper, IAccountRepository accounts, ICommonRepository common, INotificationRepository notification)
        {
            _dbContext=dbContext;
            _cn=cn;
            _mapper=mapper;
            _accounts=accounts;
            _common=common;
            _notification=notification;
        }



        #region Hall

        [HttpPost("add-new-hall")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> SaveNewHall(NewHallModel model)
        {
            var Count = await _dbContext.GetByQueryAsync<int>(@$"Select Count(*) 
                                                                    from Hall
                                                                    where LOWER(TRIM(HallName))=LOWER(TRIM(@HallName)) and IsDeleted=0 and BranchID={CurrentBranchID}", model);
            if (Count != 0)
            {
                return BadRequest(new Error("The HallName already exist,try different one", "Hall Name already exist"));
            }

            _cn.Open();
            using var tran = _cn.BeginTransaction();
            try
            {
                int logSummaryID = await _dbContext.InsertAddEditLogSummary(0, "Hall " + model.HallName, tran);

                var hall = new Hall()
                {
                    HallName = model.HallName,
                    BranchID = CurrentBranchID,
                    IsActive = true,
                };
                hall.HallID = await _dbContext.SaveAsync(hall, tran, logSummaryID);

                List<HallSection> sectionList = new();

                for (int i = 0; i < model.AtomCourtCount; i++)
                {
                    var section = new HallSection()
                    {
                        SectionName = (i + 1).ToString(),
                        HallID = hall.HallID
                    };

                    section.SectionID = await _dbContext.SaveAsync(section, tran, logSummaryID);

                    var court = new Court()
                    {
                        CourtName = "Court " + (i + 1).ToString(),
                        GameID = model.GameID,
                        HallID = hall.HallID,
                        IsActive = true,
                        PriceGroupID=model.PriceGroupID
                    };
                    court.CourtID = await _dbContext.SaveAsync(court, tran, logSummaryID);

                    CourtSectionMap map = new()
                    {
                        CourtID = court.CourtID,
                        SectionID = section.SectionID,
                    };
                    await _dbContext.SaveAsync(map, tran, logSummaryID);
                }
                tran.Commit();

                return Ok(new Success());
            }
            catch (Exception err)
            {
                tran.Rollback();
                return BadRequest(new Error(err.Message));
            }

        }

        [HttpPost("save-hall")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> SaveHall(HallModel model)
        {
            var Count = await _dbContext.GetByQueryAsync<int>(@$"Select Count(*) 
                                                                    from Hall
                                                                    where LOWER(TRIM(HallName))=LOWER(TRIM(@HallName)) and HallID<>@HallID  and IsDeleted=0 and BranchID={CurrentBranchID}", model);
            if (Count != 0)
            {
                return BadRequest(new Error("The HallName already exist,try different one", "Hall Name already exist"));
            }

            var hall = await _dbContext.GetAsync<Hall>(model.HallID);
            if (hall != null)
            {
                hall.HallName = model.HallName;
                hall.IsActive = model.IsActive;
                await _dbContext.SaveAsync(hall);
            }
            return Ok(new Success());

        }

        [HttpPost("get-hall-list")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetHallList(PagedListPostModelWithFilter model)
        {
            PagedListQueryModel query = model;

            query.Select = $@"Select HallID,HallName,BranchID
								From Hall";

            query.WhereCondition = $" IsDeleted=0 and BranchID={CurrentBranchID}";
            query.SearchLikeColumnNames=new() { "HallName" };
            query.OrderByFieldName = model.OrderByFieldName;
            var res = await _dbContext.GetPagedList<HallModel>(query, null);
            return Ok(res);
        }

        [HttpGet("get-hall-view/{HallId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetViewHall(int HallId)
        {

            var res = await _dbContext.GetByQueryAsync<HallViewModel>($@"Select H.HallID,HallName,BranchID,HS.AtomCourtCount
                                                                    From Hall H
																	LEFT JOIN (SELECT COUNT(*) AS AtomCourtCount,HallID 
																				FROM HallSection 
                                                                                WHERE IsDeleted =0
																				GROUP BY HallID) AS HS ON HS.HallID=H.HallID
                                                                    Where H.HallID={HallId} and H.IsDeleted=0", null);
            res.Sections = await _dbContext.GetListByQueryAsync<HallSectionModel>($@"Select ROW_NUMBER() OVER(ORDER BY SectionID) AS RowIndex,SectionID,HallName+'-'+SectionName as SectionName,S.HallID
                                                                    FROM HallSection S
                                                                    JOIN Hall H on H.HallID=S.HallID
                                                                    WHERE S.HallID={HallId} and S.IsDeleted = 0", null);

            res.HallTimings = await _dbContext.GetListByQueryAsync<HallTimingModel>($@"Select ROW_NUMBER() OVER(ORDER BY (SELECT 1)) AS RowIndex,HallTimingID,DayID,StartHourID,EndHourID,HallID,HMS.HourName AS StartHourName,HME.HourName AS EndHourName
                                                                                    FROM HallTiming HT
																					LEFT JOIN HourMaster HMS ON HMS.HourID = HT.StartHourID
																					LEFT JOIN HourMaster HME ON HME.HourID = HT.EndHourID
                                                                                    WHERE HallID={HallId} and HT.IsDeleted = 0", null);
            return Ok(res ?? new());
        }

        [HttpGet("get-hall/{HallId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetHall(int HallId)
        {

            var res = await _dbContext.GetByQueryAsync<HallModel>($@"Select H.HallID,HallName,BranchID,HS.AtomCourtCount
                                                                    From Hall H
																	LEFT JOIN (SELECT COUNT(*) AS AtomCourtCount,HallID 
																				FROM HallSection 
                                                                                WHERE IsDeleted =0
																				GROUP BY HallID) AS HS ON HS.HallID=H.HallID
                                                                    
                                                                    Where H.HallID={HallId} and H.IsDeleted=0", null);

            return Ok(res ?? new());
        }

        [HttpGet("delete-hall")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteHall(int Id)
        {
            var courtCount = await _dbContext.GetByQueryAsync<int>($"Select Count(*) from CourtSectionMap Where SectionID in(Select SectionID from HallSection Where HallID={Id} and IsDeleted=0) and IsDeleted=0", null);
            if (courtCount > 0)
                return BadRequest(new Error("Court is exist on this hall"));

            await _dbContext.DeleteSubItemsAsync<HallSection>("HallID", Id);
            await _dbContext.DeleteSubItemsAsync<HallTiming>("HallID", Id);
            await _dbContext.DeleteAsync<Hall>(Id);
            return Ok(true);
        }

        [HttpGet("get-hall-menu-list")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetHallMenuList()
        {

            var result = await _dbContext.GetListByQueryAsync<ViewPageMenuModel>($@"
                                                                            Select HallID as ID,HallName as MenuName
                                                                            From Hall 
                                                                            Where IsDeleted=0 and BranchID={CurrentBranchID}
                                                                            Order By 1 Desc
            ", null);

            return Ok(result);
        }

        [HttpGet("delete-hall-section")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteHallSection(int Id)
        {
            var courtCount = await _dbContext.GetByQueryAsync<int>($"Select Count(*) from CourtSectionMap Where SectionID={Id} and IsDeleted=0", null);
            if (courtCount > 0)
                return BadRequest(new Error("Court is exist on this section"));

            await _dbContext.DeleteAsync<HallSection>(Id);
            return Ok(true);
        }
        #endregion

        #region Court
        [HttpPost("save-court")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> SaveCourt(CourtModel model)
        {
            var Count = await _dbContext.GetByQueryAsync<int>(@$"Select Count(*) 
                                                                    from Court
                                                                    where LOWER(TRIM(CourtName))=LOWER(TRIM(@CourtName)) and CourtID<>@CourtID and HallID=@HallID  and IsDeleted=0", model);
            if (Count != 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "Court Name already exist",
                    ResponseMessage = "The CourtName already exist,try different one"
                });
            }
            try
            {
                model.BranchID = CurrentBranchID;
                var court = _mapper.Map<Court>(model);

                court.CourtID = await _dbContext.SaveAsync(court);
                List<CourtSectionMap> courtMap = new();
                foreach (var mapRow in model.CourtSections)
                {
                    CourtSectionMap courtSectionMap = new()
                    {
                        MapID = mapRow.MapID,
                        SectionID = mapRow.SectionID,
                        CourtID = court.CourtID,
                    };
                    courtMap.Add(courtSectionMap);
                }
                await _dbContext.SaveSubItemListAsync(courtMap, "CourtID", court.CourtID);

                CourtAddResultModel returnObject = new() { CourtID = court.CourtID, CourtName = court.CourtName };
                return Ok(returnObject);
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

        [HttpPost("get-court-list")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetCourtList(PagedListPostModelWithFilter model)
        {
            PagedListQueryModel query = model;

            query.Select = $@"Select distinct C.CourtID,C.CourtName,GameName,HallName
								From Court C
                                LEFT JOIN GameMaster GM ON GM.GameID=C.GameID
								LEFT JOIN CourtSectionMap CSM ON CSM.CourtID=C.CourtID and CSM.IsDeleted =0
								LEFT JOIN Hall H ON H.HallID = C.HallID";

            query.WhereCondition = $" C.IsDeleted=0 and BranchID={CurrentBranchID} ";
            query.SearchLikeColumnNames = new() { "C.CourtName","GameName","HallName" };
            query.OrderByFieldName = model.OrderByFieldName;
            var res = await _dbContext.GetPagedList<CourtModel>(query, null);
            return Ok(res);
        }

        [HttpGet("get-court-view/{CourtId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetViewCourt(int CourtId)
        {

            var res = await _dbContext.GetByQueryAsync<CourtModel>($@"Select C.CourtID,CourtName,G.GameID,GameName,Sections,HallName,H.HallID,C.IsActive,PriceGroupName,C.PriceGroupID
                                                                From Court C
                                                                LEFT JOIN GameMaster G ON G.GameID = C.GameID
                                                                LEFT JOIN Hall H on H.HallID=C.HallID
                                                                LEFT JOIN (Select CSM.CourtID AS CourtID, String_agg(SectionName,',') AS Sections
                                                                From CourtSectionMap CSM
                                                                LEFT JOIN HallSection HS ON HS.SectionID = CSM.SectionID
                                                                WHERE CSM.IsDeleted=0
																Group by CSM.CourtID)  AS res ON res.CourtID = C.CourtID 
																Left Join CourtPriceGroup P on P.PriceGroupID=C.PriceGroupID and P.IsDeleted=0
																WHERE C.CourtID={CourtId} and C.IsDeleted=0", null);

            res.CourtSections = await _dbContext.GetListByQueryAsync<CourtSectionMapModel>($@"Select CSM.SectionID,HallName+'-'+SectionName SectionName,CSM.MapID
                                                                From CourtSectionMap CSM
                                                                LEFT JOIN HallSection HS ON HS.SectionID = CSM.SectionID
                                                                LEFT JOIN Hall H on H.HallID=HS.HallID
                                                                WHERE CSM.CourtID ={CourtId} and CSM.IsDeleted=0", null);

            return Ok(res ?? new());
        }


        [HttpGet("delete-court")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteCourt(int Id)
        {
            await _dbContext.DeleteSubItemsAsync<CourtSectionMap>("CourtID", Id);
            await _dbContext.DeleteAsync<Court>(Id);
            return Ok(true);
        }

        [HttpGet("get-court-menu-list")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetCourtMenuList()
        {

            var result = await _dbContext.GetListByQueryAsync<ViewPageMenuModel>($@"
                                                                            Select distinct C.CourtID as ID,CourtName as MenuName
                                                                            From Court C
                                                                            LEFT JOIN CourtSectionMap CSM ON CSM.CourtID=C.CourtID and CSM.IsDeleted =0
								                                            LEFT JOIN HallSection HS ON HS.SectionID = CSM.SectionID
								                                            LEFT JOIN Hall H ON H.HallID = HS.HallID
                                                                            Where C.IsDeleted=0 and H.BranchID = {CurrentBranchID} 
                                                                            Order By 1 Desc
            ", null);

            return Ok(result);
        }
        #endregion

        #region HallTiming
        [HttpPost("save-hall-timing")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> SaveHallTiming(HallViewModel model)
        {

            try
            {
                List<HallTiming> timing = _mapper.Map<List<HallTiming>>(model.HallTimings);
                await _dbContext.SaveSubItemListAsync<HallTiming>(timing, "HallID", model.HallID);
                //LeadThroughAddResultModel returnObject = new() { LeadThroughID = lead.LeadThroughID, LeadThroughName = lead.Name };
                return Ok(new Success());
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

        #endregion

        #region Booking




        [HttpGet("get-court-booking-view/{BookingId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetViewCourtBooking(int BookingId)
        {

            var res = await _dbContext.GetByQueryAsync<CourtBookingViewModel>($@"SELECT B.BookingID,CONCAT('B-',B.BookingID) AS BookingNo,IsCancelled,
                                                                    B.CustomerEntityID,Name AS CustomerName,VE.Phone,VE.EmailAddress,B.Date,
                                                                    BC.BookingCourtID,CONCAT(HMF.HourName,'-',HMT.HourName) AS Timing,FromHourID,ToHourID,
                                                                    C.CourtID,C.CourtName,
                                                                    C.GameID,GM.GameName
                                                                    FROM Booking B
                                                                    LEFT JOIN viEntity VE ON VE.EntityID = B.CustomerEntityID
                                                                    LEFT JOIN BookingCourt BC ON BC.BookingID = B.BookingID
                                                                    LEFT JOIN Court C ON C.CourtID = BC.CourtID
                                                                    LEFT JOIN GameMaster GM ON GM.GameID = C.GameID
                                                                    LEFT JOIN HourMaster HMF ON HMF.HourID = BC.FromHourID
                                                                    LEFT JOIN HourMaster HMT ON HMT.HourID = BC.ToHourID
																    WHERE B.BookingID={BookingId} and B.IsDeleted=0", null);


            return Ok(res ?? new());
        }


        [HttpGet("get-court-booking-menu-list")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetCourtBookingMenuList()
        {

            var result = await _dbContext.GetListByQueryAsync<ViewPageMenuModel>($@"
                                                                            Select BookingID as ID,CONCAT('B-',BookingID,'-',Name) as MenuName
                                                                            From Booking B 
                                                                            LEFT JOIN ViEntity VE ON VE.EntityID= B.CustomerEntityID
                                                                            Where IsDeleted=0 
                                                                            Order By 1 Desc
            ", null);

            return Ok(result);
        }

        [HttpGet("delete-court-booking")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteCourtBooking(int Id)
        {
            int bookingCourtID = await _dbContext.GetByQueryAsync<int>($@"Select BookingCourtID From BookingCourt Where BookingID={Id}", null);
            await _dbContext.DeleteSubItemsAsync<BookingSection>("BookingCourtID", bookingCourtID);
            await _dbContext.DeleteAsync<BookingCourt>(bookingCourtID);
            await _dbContext.DeleteAsync<Booking>(Id);
            return Ok(true);
        }
        #endregion

        [HttpGet("get-all-hall-section/{HallID}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetAllHallSection(int HallID)
        {

            var res = await _dbContext.GetListByQueryAsync<CourtSectionMapModel>($@"Select HS.SectionID ,SectionName as SectionName
                                                                        From HallSection HS
                                                                        JOIN Hall H on H.HallID=HS.HallID
                                                                       Where HS.IsDeleted=0 and HS.HallID={HallID}", null);
            return Ok(res ?? new());
        }



        [HttpGet("get-all-court")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetAllCourt()
        {
            var res = await _dbContext.GetListByQueryAsync<IdnValuePair>($@"Select C.CourtID as ID,CourtName as Value 
                                                                        From Court C
                                                                        LEFT JOIN Hall H ON H.HallID = C.HallID
                                                                        Where C.IsDeleted=0 and H.BranchID={CurrentBranchID}", null);
            return Ok(res ?? new());
        }


        #region CourtCustomer
        [HttpGet("get-court-customer/{customerEntityID}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Customer")]
        public async Task<IActionResult> GetCourtCustomer(int customerEntityID)
        {

            var customer = await _dbContext.GetByQueryAsync<CourtCustomerModel>($@"
                                                    Select EP.FirstName  as Name,
                                                    EP.EntityPersonalInfoID,E.EntityTypeID,E.EmailAddress,E.Phone,E.MediaID,E.EntityID,C.EntityID,
                                                    C.Status,C.Type,C.Remarks,C.CustomerID,EntityInstituteInfoID,WC.ContactID,E.CountryID,CT.CountryName,CT.ISDCode,C.TaxNumber
                                                    From CourtCustomer C
                                                    Left Join EntityPersonalInfo EP ON EP.EntityID=C.EntityID and EP.IsDeleted=0
                                                    Left Join Entity E ON E.EntityID=C.EntityID and E.IsDeleted=0 
                                                    Left Join Country CT on CT.CountryID=E.CountryID and CT.IsDeleted=0
                                                    Left Join WhatsappContact WC ON C.EntityID=WC.EntityID AND WC.IsDeleted=0
                                                    Where C.EntityID={customerEntityID} and C.ClientID={CurrentClientID} and C.IsDeleted=0
            ", null);

            //customer.Addresses = await _dbContext.GetListByQueryAsync<AddressModel>($@"
            //                                        Select EA.*,C.CountryName 
            //                                        From EntityAddress EA
            //                                        Left Join Country C ON EA.CountryID=C.CountryID and C.IsDeleted=0  
            //                                        Where EA.EntityID={customer.EntityID} and EA.IsDeleted=0
            //");

            //customer.ContactPersons = await _dbContext.GetListByQueryAsync<CustomerContactPersonModel>($@"
            //                                        SELECT ROW_NUMBER() OVER(ORDER BY (SELECT 1)) AS RowIndex,E.EmailAddress As Email,E.Phone As Phone,EIP.FirstName As Name,EIP.EntityPersonalInfoID,CCP.*
            //                                        From CustomerContactPerson CCP
            //                                        Left Join EntityPersonalInfo EIP ON CCP.EntityID=EIP.EntityID and EIP.IsDeleted=0
            //                                        Left Join Entity E ON E.EntityID=CCP.EntityID and E.IsDeleted=0
            //                                        Where CCP.CustomerEntityID={customer.EntityID} and CCP.IsDeleted=0               
            //");

            return Ok(customer ?? new());
        }

        [HttpPost("save-court-customer")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> SaveCourtCustomer(CustomerModel customerModel)
        {

            var phone = await _dbContext.GetByQueryAsync<string>(@$"Select Phone 
                                                                            from Entity
                                                                            where Phone=@Phone and EntityID<>@EntityID and ClientID={CurrentClientID} and IsDeleted=0
            ", customerModel);

            if (!string.IsNullOrEmpty(phone))
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "Invalid submission",
                    ResponseMessage = "The Phone number already exist,try different phone number"
                });
            }

            var email = await _dbContext.GetByQueryAsync<string>(@$"Select EmailAddress 
                                                                            from Entity
                                                                            where EmailAddress=@EmailAddress and EntityID<>@EntityID and EntityTypeID in({(int)EntityType.Client},{(int)EntityType.Branch},{(int)EntityType.User},{(int)EntityType.CourtCustomer}) and ClientID={CurrentClientID} and IsDeleted=0
            ", customerModel);

            if (!string.IsNullOrEmpty(email))
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "Invalid submission",
                    ResponseMessage = "The Email already exist,try different email address"
                });
            }

            _cn.Open();
            using (var tran = _cn.BeginTransaction())
            {
                try
                {
                    int logSummaryID = await _dbContext.InsertAddEditLogSummary(Convert.ToInt32(customerModel.EntityID), "CourtCustomer :" + customerModel.Name + " Added/Edited by User " + CurrentUserID, tran);

                    var customerEntity = new EntityCustom()
                    {
                        EntityID = Convert.ToInt32(customerModel.EntityID),
                        EntityTypeID = (int)EntityType.CourtCustomer,
                        Phone = customerModel.Phone,
                        EmailAddress = customerModel.EmailAddress,
                        ClientID = CurrentClientID,
                        CountryID = customerModel.CountryID
                    };
                    customerEntity.EntityID = await _dbContext.SaveAsync(customerEntity, tran, logSummaryID: logSummaryID);
                    customerModel.EntityID = customerEntity.EntityID;



                    var entityPersonal = new EntityPersonalInfo()
                    {
                        EntityPersonalInfoID = customerModel.EntityPersonalInfoID,
                        EntityID = customerEntity.EntityID,
                        FirstName = customerModel.Name,
                    };
                    await _dbContext.SaveAsync(entityPersonal, tran);

                    CourtCustomer customer = new()
                    {
                        CustomerID = customerModel.CustomerID,
                        EntityID = customerEntity.EntityID,
                        ClientID = CurrentClientID,

                    };

                    customerModel.CustomerID = await _dbContext.SaveAsync(customer, tran, logSummaryID: logSummaryID);

                    CustomerAddResultModel returnObject = new() { EntityID = customerEntity.EntityID, Name = customerModel.Name };

                    tran.Commit();
                    return Ok(returnObject);
                }
                catch (Exception err)
                {
                    tran.Rollback();
                    return BadRequest(new BaseErrorResponse()
                    {
                        ErrorCode = 0,
                        ResponseTitle = "Something went wrong",
                        ResponseMessage = err.Message
                    });
                }
            }
        }
        #endregion

        [HttpGet("get-court-list")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetCourtList()
        {
            var res = await _dbContext.GetListByQueryAsync<CourtListModel>($@"Select C.CourtID,CourtName 
                                                                        From Court C
                                                                       Where C.IsDeleted=0", null);
            return Ok(res ?? new());
        }

        #region Booking By Ashique

        [HttpGet("get-hour-list")]
        public async Task<IActionResult> GetHourList()
        {
            var results = await _dbContext.GetListByQueryAsync<IdnValuePair>("SELECT HourID AS ID,HourName AS Value FROM HourMaster WHERE IsDeleted=0", null);
            return Ok(results);
        }

        [HttpGet("get-all-halls")]
        public async Task<IActionResult> GetAllHall()
        {
            return Ok(await _dbContext.GetListByQueryAsync<IdnValuePair>("Select HallID as ID,HallName as Value From Hall WHERE IsDeleted=0", null));
        }

        [HttpPost("get-court-price-details")]
        public async Task<IActionResult> GetCourtDetails(CourtPricePostModel model)
        {
            if (!model.Date.HasValue)
                return BadRequest(new Error() { ResponseMessage="Please choose valid date"});
            var res = await _dbContext.GetByQueryAsync<CourtPriceDetailsModel>($@"Select Rate,CurrencyName,C.PriceGroupID,PriceGroupItemID
                                                                                    From Court C
                                                                                    Left Join CourtPriceGroup G on G.PriceGroupID=C.PriceGroupID and G.IsDeleted=0
                                                                                    Left Join CourtPriceGroupItem I on I.PriceGroupID=G.PriceGroupID and @HourID between StartHourID and EndHourID-1 and DayID={Convert.ToInt32(model.Date.Value.DayOfWeek)} and I.IsDeleted=0
                                                                                    Left Join Currency CR on CR.CurrencyID=G.CurrencyID and CR.IsDeleted=0
                                                                                    where CourtID=@CourtID",model);
            if (res.PriceGroupID==null)
                return BadRequest(new Error() { ResponseMessage="Price group is not assigned for this court" });
            if (res.PriceGroupItemID==null)
            {
                string HourName = await _dbContext.GetFieldsAsync<HourMaster, string>("HourName", "HourID=@HourID", model);
                return BadRequest(new Error() { ResponseMessage=$"{HourName} is not included in any price group item of {model.Date.Value.DayOfWeek.ToString()}" });
            }
            return Ok(res);
        }

        [HttpPost("get-court-day-schedule")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetCourtDaySchedule(GetCourtDaySchedulePostModel model)
        {
            GetCourtDayScheduleResultModel res = new()
            {
                Courts = await _dbContext.GetListByQueryAsync<IdnValuePair>($@"Select C.CourtID as ID,CourtName as Value 
                                                                        From Court C
                                                                        Where C.IsDeleted=0 and C.HallID=@HallID", model),

                Hours = await _dbContext.GetListByQueryAsync<IdnValuePair>($@"SELECT H.HourID ID,H.HourName Value
                                                                                    FROM HourMaster H
                                                                                    JOIN HallTiming HT ON H.HourID BETWEEN HT.StartHourID AND HT.EndHourID-1
                                                                                    JOIN Hall HA on HA.HallID=HT.HallID
                                                                                    WHERE HT.DayID={(int)model.Date.DayOfWeek} and HA.HallID=@HallID", model),
                Bookings = await _dbContext.GetListByQueryAsync<BookingDetailsModel>($@"Select B.BookingID,H.HourID,C.CourtID,BookingNo,Status,BookingCourtID,IsNull(E.Name,'Guest') as Name,DateAdd(MINUTE,{TimeOffset},B.Date) as BookedOn,IsOnlinePayment,Remarks,Case when U.UserTypeID=6 then 0 else 1 end as IsPanelBooking,CounterCode
                                                                                            From BookingCourt BC
                                                                                            JOIN Booking B on B.BookingID=BC.BookingID 
                                                                                            JOIN Court C on C.CourtID=BC.CourtID and C.IsDeleted=0
                                                                                            JOIN HourMaster H on H.HourID between BC.FromHourID and BC.ToHourID and H.IsDeleted=0
                                                                                            LEFT JOIN viEntity E on E.EntityID=B.CustomerEntityID
                                                                                            Left Join AccJournalMaster J on J.JournalMasterID=B.JournalMasterID and J.IsDeleted=0
                                                                                            Left Join Users U on U.EntityID=B.BookedBy and U.IsDeleted=0
                                                                                            Left Join CourtCounterCode CC on CC.CounterCodeID=B.CounterCodeID
                                                                                            Where HallID=@HallID and BC.IsDeleted=0 and B.IsDeleted=0 and BC.Date=Convert(Date,@Date) and BC.IsCancelled=0 and 
                                                                                                  ((Status={(int)CourtBookingStatus.Booked} and IsSuccess=1) or (Status={(int)CourtBookingStatus.Carted} and DATEADD(MINUTE,{PDV.CourtCartMinute},B.Date)>GETUTCDATE()))", model)
            };

            return Ok(res);
        }

        [HttpPost("add-to-cart")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> AddToCart(CartItemModel model)
        {

            var alreadyCarted = await _dbContext.GetByQueryAsync<int>($@"Select Count(BC.BookingCourtID)
                From Booking B
                JOIN BookingCourt BC on BC.BookingID=B.BookingID
                Left Join AccJournalMaster J on J.JournalMasterID=B.JournalMasterID and J.IsDeleted=0
                Where B.IsDeleted=0 and BC.IsDeleted=0 and BC.IsCancelled=0 and BC.Date=Convert(Date,@Date)
                and @HourID between BC.FromHourID and BC.ToHourID and BC.CourtID=@CourtID
                and ((Status={(int)CourtBookingStatus.Booked} and IsSuccess=1) or (Status={(int)CourtBookingStatus.Carted} and DATEADD(MINUTE,{PDV.CourtCartMinute},B.Date)>GETUTCDATE()))", model);

            if (alreadyCarted > 0)
            {
                return BadRequest(new Error("Court is already carted by other customer"));
            }


            _cn.Open();
            using var tran = _cn.BeginTransaction();
            try
            {
                if (model.BookingID == 0)
                {
                    Booking booking = new()
                    {
                        Date = DateTime.UtcNow,
                        Status = (int)CourtBookingStatus.Carted,
                        BookingNo = await _dbContext.GetByQueryAsync<int>($"Select ISNULL(Max(BookingNo),10000)+1 from Booking Where BranchID={CurrentBranchID}", null, tran),
                        BranchID = CurrentBranchID,
                        CustomerEntityID=CurrentEntityID
                    };
                    model.BookingID = await _dbContext.SaveAsync(booking, transaction: tran);
                }
                else
                {
                    await _dbContext.ExecuteAsync("Update Booking Set Date=@Date Where BookingID=@BookingID", new { Date = DateTime.UtcNow, BookingID = model.BookingID }, tran);
                }
                var court = await _dbContext.GetByQueryAsync<CourtPriceDetailsModel>($@"Select Rate,CurrencyID,IncTax,TaxCategoryID,C.PriceGroupID,PriceGroupItemID
                                                                                    From Court C
                                                                                    Left Join CourtPriceGroup G on G.PriceGroupID=C.PriceGroupID and G.IsDeleted=0
                                                                                    Left Join CourtPriceGroupItem I on I.PriceGroupID=G.PriceGroupID and @HourID between StartHourID and EndHourID-1 and DayID={Convert.ToInt32(model.Date.DayOfWeek)} and I.IsDeleted=0
                                                                                    where CourtID=@CourtID",model,tran);
                if(court.PriceGroupID==null)
                        return BadRequest(new Error() { ResponseMessage="Price group is not assigned for this court" });
                if (court.PriceGroupItemID==null)
                {
                    string HourName = await _dbContext.GetFieldsAsync<HourMaster, string>("HourName", "HourID=@HourID", model, tran);
                    return BadRequest(new Error() { ResponseMessage=$"{HourName} is not included in any price group item of {model.Date.DayOfWeek.ToString()}" });
                }
                decimal price = 0, tax = 0, taxPercentage = 0;
                if (court.TaxCategoryID != null)
                {
                    taxPercentage = await _dbContext.GetByQueryAsync<decimal>($@"Select TaxPercentage from viTaxCategory Where TaxCategoryID={court.TaxCategoryID}", null, tran);
                    if (court.IncTax)
                    {
                        price = court.Rate / ((taxPercentage / 100) + 1);
                        tax = court.Rate - price;
                    }
                    else
                    {
                        price = court.Rate;
                        tax = price * taxPercentage / 100;
                    }
                }
                else
                {
                    price = court.Rate;
                }

                BookingCourt bookingCourt = new()
                {
                    Date = model.Date,
                    BookingID = model.BookingID,
                    CourtID = model.CourtID,
                    FromHourID = model.HourID,
                    ToHourID = model.HourID,
                    Rate = court.Rate,
                    CurrencyID = court.CurrencyID,
                    TaxCategoryID = court.TaxCategoryID,
                    Price = price,
                    Discount = 0,
                    NetPrice = price,
                    TaxPercentage = taxPercentage,
                    Tax = tax,
                    TotalPrice = price + tax
                };
                bookingCourt.BookingCourtID = await _dbContext.SaveAsync(bookingCourt, transaction: tran);


                List<CourtSectionMap> sectionList = await _dbContext.GetListByQueryAsync<CourtSectionMap>($@"Select * FROM CourtSectionMap WHERE IsDeleted =0 and CourtID=@CourtID", model, transaction: tran);

                List<BookingSection> bookingSections = new();

                foreach (var section in sectionList)
                {
                    bookingSections.Add(new BookingSection()
                    {
                        SectionID = section.SectionID,
                        HourID = model.HourID
                    });
                }
                await _dbContext.SaveSubItemListAsync(bookingSections, "BookingCourtID", bookingCourt.BookingCourtID, transaction: tran);

                tran.Commit();
                await SendBookingSignalRPush("carted");
                var res = await _dbContext.GetByQueryAsync<CourtBookingViewModelNew>($@"Select BC.BookingID,BC.BookingCourtID,CourtName,CurrencyName,BC.Rate,BC.Date,B.Date as AddedOn,H1.HourName as Time
                        From BookingCourt BC
                        JOIN Booking B on B.BookingID=BC.BookingID
                        JOIN Court C on C.CourtID=BC.CourtID
                        Join CourtPriceGroup G on G.PriceGroupID=C.PriceGroupID
                        JOIN Currency CR on CR.CurrencyID=G.CurrencyID
                        JOIN HourMaster H1 on H1.HourID=BC.FromHourID+1
                        Where BC.BookingCourtID=@BookingCourtID", bookingCourt);
                return Ok(res);
            }
            catch (Exception err)
            {
                tran.Rollback();
                return BadRequest(new Error());
            }
        }

        [HttpGet("remove-from-cart/{bookingCourtID}")]
        public async Task<IActionResult> RemoveItemFromCart(int bookingCourtID)
        {
            await _dbContext.DeleteSubItemsAsync<BookingSection>("BookingCourtID", bookingCourtID);
            await _dbContext.DeleteAsync<BookingCourt>(bookingCourtID);
            await SendBookingSignalRPush("cart removed");
            return Ok(new Success());
        }

        [HttpGet("clear-cart/{bookingId}")]
        public async Task<IActionResult> ClearCart(int bookingId)
        {
            await _dbContext.DeleteAsync<BookingSection>($"BookingCourtID in(Select BookingCourtID From BookingCourt Where BookingID={bookingId})", null);
            await _dbContext.DeleteAsync<BookingCourt>($"BookingID={bookingId}", null);
            await SendBookingSignalRPush("cart cleared");
            return Ok(new Success());
        }

        [HttpPost("search-customer")]
        public async Task<IActionResult> SearchCustomer(SearchCustomerWithPhoneModel model)
        {
            SearchCustomerWithPhoneResultModel res = new();
            var country = await _dbContext.GetAsync<Country>(model.CountryID.Value);
            var phoneNumbers = "'" + model.Phone + "','" + country.ISDCode + model.Phone + "','" + country.ISDCode.Replace("+", "") + model.Phone + "'";
            var entity = await _dbContext.GetAsync<Entity>($"EntityTypeID in({(int)EntityType.Customer}) and Phone in({phoneNumbers})", null);

            if (entity != null)
            {
                res.EmailAddress = entity.EmailAddress;
                res.EntityID = entity.EntityID;

                var personalInfo = await _dbContext.GetAsync<ViEntity>(entity.EntityID);
                if (personalInfo != null)
                {
                    res.Name = personalInfo.Name;
                }
                res.Balance = await _dbContext.GetByQueryAsync<decimal>($@"Select ISNULL(Sum(Debit)-Sum(Credit),0)
                    from AccJournalEntry J
                    JOIN AccJournalMaster JM on JM.JournalMasterID=J.JournalMasterID
                    JOIN AccLedger L on L.LedgerID=J.LedgerID and L.IsDeleted=0
                    Where L.EntityID=@EntityID and L.IsDeleted=0 and JM.IsDeleted=0 and IsSuccess=1", new { EntityID = res.EntityID });
            }
            return Ok(res);
        }

        [HttpGet("get-payment-summary/{bookingId}")]
        public async Task<IActionResult> GetPaymentSummary(int bookingId)
        {
            await _dbContext.ExecuteAsync("Update Booking Set Date=@Date Where BookingID=@BookingID", new { Date = DateTime.UtcNow, BookingID = bookingId });

            var res = await _dbContext.GetByQueryAsync<CourtPaymentSummaryModel>($@"Select CurrencyName,Sum(Price) as Price,Sum(Discount) as Discount,Sum(Tax) as Tax,Sum(TotalPrice) as TotalPrice
                From Booking B
                JOIN BookingCourt C on C.BookingID=B.BookingID
                JOIN Currency CR on CR.CurrencyID=C.CurrencyID
                Where B.IsDeleted=0 and C.IsDeleted=0 and DateAdd(MINUTE,{PDV.CourtCartMinute},B.Date)>GETUTCDATE() and B.BookingID={bookingId}
                Group by CurrencyName", null);
            return Ok(res);
        }

        [HttpPost("apply-discount")]
        public async Task<IActionResult> ApplyDiscount(ApplyDiscountPostModel model)
        {
            await _dbContext.ExecuteAsync($@"Update BookingCourt
                    Set Discount=0,NetPrice=Price,Tax=ROUND(Price*TaxPercentage/100, 2)
                    Where IsDeleted=0 and BookingID={model.BookingID}");

            await _dbContext.ExecuteAsync($@"Update BookingCourt
                    Set TotalPrice=NetPrice+Tax
                    Where IsDeleted=0 and BookingID={model.BookingID}");

            var bookingCourt = await _dbContext.GetSubListAsync<BookingCourt>("BookingID", model.BookingID);

            if (bookingCourt != null)
            {
                decimal discountAmount = 0, discPercentage = 0;
                decimal totalAmount = bookingCourt.Sum(s => s.TotalPrice);

                if (model.DiscountType == CourtDiscountType.Percentage)
                {
                    discountAmount = totalAmount * model.Value / 100;
                    discPercentage = model.Value;
                }
                else
                {
                    discountAmount = model.Value;
                    discPercentage = discountAmount / totalAmount * 100;
                }

                foreach (var item in bookingCourt)
                {
                    item.Discount=item.TotalPrice * discPercentage / 100;
                    item.TotalPrice = item.TotalPrice - item.Discount;
                    item.NetPrice = item.TotalPrice / ((item.TaxPercentage / 100) + 1);
                    item.Tax = item.TotalPrice - item.NetPrice;
                }
                await _dbContext.SaveListAsync(bookingCourt, needToDelete: false);
            }

            return await GetPaymentSummary(model.BookingID);
        }

        [HttpPost("confirm-booking")]
        public async Task<IActionResult> ConfirmBooking(ConfirmBookingModel model)
        {
            if (model.BookingID == null)
                return BadRequest(new Error("No Booking Found"));

            var country = await _dbContext.GetAsync<Country>(model.CountryID.Value);
            var phoneNumbers = "'" + model.Phone + "','" + country.ISDCode + model.Phone + "','" + country.ISDCode.Replace("+", "") + model.Phone + "'";
            var entity = await _dbContext.GetAsync<EntityCustom>($"EntityTypeID in({(int)EntityType.Customer}) and Phone in({phoneNumbers})", null);
            if (entity == null)
            {
                entity = new EntityCustom()
                {
                    ClientID = CurrentClientID,
                    EmailAddress = model.EmailAddress,
                    EntityTypeID = (int)EntityType.Customer,
                    Phone = model.Phone,
                    CountryID = model.CountryID
                };
                entity.EntityID = await _dbContext.SaveAsync(entity);

                var personalInfo = new EntityPersonalInfo()
                {
                    FirstName = model.Name,
                    EntityID = entity.EntityID
                };
                await _dbContext.SaveAsync(personalInfo);

            }
            else
            {
                entity.EmailAddress = model.EmailAddress;
                entity.CountryID = model.CountryID;
                await _dbContext.SaveAsync(entity);

                var personalInfo = await _dbContext.GetAsync<EntityPersonalInfo>($"EntityID=@EntityID", entity);
                if (personalInfo == null)
                    personalInfo = new();

                personalInfo.FirstName = model.Name;
                await _dbContext.SaveAsync(personalInfo);
            }

            var paymentSummary = await _dbContext.GetByQueryAsync<CourtPaymentSummaryModel>($@"Select Sum(Price) as Price,Sum(Discount) as Discount,Sum(NetPrice) as NetPrice,Sum(Tax) as Tax,Sum(TotalPrice) as TotalPrice
                From Booking B
                JOIN BookingCourt C on C.BookingID=B.BookingID
                JOIN Currency CR on CR.CurrencyID=C.CurrencyID
                Where B.IsDeleted=0 and C.IsDeleted=0 and DateAdd(MINUTE,{PDV.CourtCartMinute},B.Date)>GETUTCDATE() and B.BookingID={model.BookingID}", null);

            _cn.Open();
            using var tran = _cn.BeginTransaction();
            try
            {
                var booking = await _dbContext.GetAsync<Booking>(model.BookingID.Value, tran);
                if (booking != null && booking.Status == (int)CourtBookingStatus.Carted)
                {
                    int? voucherTypeID = await _dbContext.GetFieldsAsync<AccVoucherType, int?>("VoucherTypeID", $"VoucherTypeNatureID={(int)VoucherTypeNatures.Receipt} and ClientID={CurrentClientID}", null, tran);

                    if (voucherTypeID != null)
                    {
                        #region Accounts Entry

                        var voucherNumber = await _accounts.GetVoucherNumber(voucherTypeID.Value, CurrentBranchID, tran);

                        AccJournalMaster jm = new()
                        {
                            VoucherTypeID = voucherTypeID,
                            BranchID = CurrentBranchID,
                            JournalNo = voucherNumber.JournalNo,
                            JournalNoPrefix = voucherNumber.JournalNoPrefix,
                            Date = DateTime.Now.Date,
                            EntityID = entity.EntityID,
                            IsSuccess = true,
                            Name = model.Name,
                            NarrationType = (int)VoucherNarrationType.Single,
                            Particular = "Court Booking -" + booking.BookingNo,
                            Remarks = model.Remarks,
                        };
                        jm.JournalMasterID = await _dbContext.SaveAsync(jm, tran);


                        List<AccJournalEntry> journalEntries = new();

                        #region Debit Entries
                        if (model.Cash > 0)
                        {
                            journalEntries.Add(new()
                            {
                                Debit = model.Cash > paymentSummary.TotalPrice ? paymentSummary.TotalPrice : model.Cash,
                                Credit = 0,
                                LedgerID = await _dbContext.GetByQueryAsync<int>($@"Select Top 1 LedgerID from AccLedger Where ClientID={CurrentClientID} and LedgerTypeID={(int)LedgerTypes.Cash} and IsDeleted=0", null, tran),
                            });
                        }

                        if (model.Cash < paymentSummary.TotalPrice)//Means there is credit
                        {
                            journalEntries.Add(new()
                            {
                                Debit = paymentSummary.TotalPrice - model.Cash,
                                Credit = 0,
                                LedgerID = await _accounts.GetEntityLedgerID(entity.EntityID, CurrentClientID, tran),
                            });
                        }


                        if (paymentSummary.Discount > 0)
                        {
                            journalEntries.Add(new()
                            {
                                Debit = paymentSummary.Discount,
                                Credit = 0,
                                LedgerID = await _dbContext.GetByQueryAsync<int>($@"Select LedgerID from AccLedger Where ClientID={CurrentClientID} and IsDeleted=0 and LedgerName='Discount Paid'", null, tran),
                            });
                        }
                        #endregion

                        #region Credit Entries

                        journalEntries.Add(new()
                        {
                            Debit = 0,
                            Credit = paymentSummary.Price,
                            LedgerID = await _dbContext.GetByQueryAsync<int>($@"Select LedgerID from AccLedger Where ClientID={CurrentClientID} and IsDeleted=0 and LedgerName='Sales'", null, tran),
                        });

                        if (paymentSummary.Tax > 0)
                        {
                            var taxes = await _dbContext.GetListByQueryAsync<TaxSummaryModel>($@"Select LedgerID, CONVERT(DECIMAL(10,2),Round(Sum(NetPrice*Percentage)/100,2)) as TaxAmount
                                from TaxCategoryItem T
                                JOIN BookingCourt B on B.TaxCategoryID=T.TaxCategoryID
                                Where T.IsDeleted=0 and B.IsDeleted=0 and Percentage>0 and B.BookingID={model.BookingID}
                                Group by LedgerID", null, tran);

                            if (taxes != null)
                            {
                                foreach (var tax in taxes)
                                {
                                    journalEntries.Add(new()
                                    {
                                        Debit = 0,
                                        Credit = tax.TaxAmount,
                                        LedgerID = tax.LedgerID,
                                    });
                                }
                            }
                        }

                        if (model.Cash > paymentSummary.TotalPrice)
                        {
                            journalEntries.Add(new()
                            {
                                Debit = 0,
                                Credit = model.Cash - paymentSummary.TotalPrice,
                                LedgerID = await _accounts.GetEntityLedgerID(entity.EntityID, CurrentClientID, tran),
                            });
                        }

                        await _dbContext.SaveSubItemListAsync(journalEntries, "JournalMasterID", jm.JournalMasterID, tran);

                        #endregion

                        #endregion

                        booking.CustomerEntityID = entity.EntityID;
                        booking.Status = (int)CourtBookingStatus.Booked;
                        booking.JournalMasterID = jm.JournalMasterID;
                        booking.Date = DateTime.UtcNow;
                        booking.CashCollected = model.Cash;
                        booking.Credit = paymentSummary.TotalPrice - model.Cash;
                        booking.BookedBy = CurrentEntityID;
                        if (model.DiscountValue > 0)
                        {
                            booking.DiscountType = model.DiscountType;
                            booking.DiscountValue = model.DiscountValue;
                        }
                        await _dbContext.SaveAsync(booking, tran);
                    }
                    tran.Commit();
                    await SendBookingSignalRPush("booked");
                    return Ok(new ConfirmBookingResultModel() { BookingID=booking.BookingID, JournalMasterID = booking.JournalMasterID.Value });
                }
                else
                {
                    return BadRequest(new Error("Your cart is empty"));
                }
            }
            catch (Exception err)
            {
                tran.Rollback();
                return BadRequest(new Error());
            }
        }

        [HttpGet("get-booking-receipt/{bookingID}")]
        public async Task<IActionResult> GetBookingReceipt(int bookingID)
        {
            var res = await _dbContext.GetByQueryAsync<BookingReceiptModel>($@"Select BookingNo,DateAdd(MINUTE,{TimeOffset},B.Date) as BookedOn,E.Name,E.EmailAddress,E.Phone,DiscountType,DiscountValue,
                CashCollected,Credit,Status,DateAdd(MINUTE,{TimeOffset},CancelledOn) as CancelledOn,BB.Name as BookedBy, CB.Name as CancelledBy,Remarks
                from Booking B
                JOIN viEntity E on E.EntityID=B.CustomerEntityID
                LEFT JOIN viEntity BB on BB.EntityID=B.BookedBy
                LEFT JOIN viEntity CB on CB.EntityID=B.CancelledBy
                Left Join AccJournalMaster J on J.JournalMasterID=B.JournalMasterID and J.IsDeleted=0
                Where B.BookingID=@bookingID and Status in ({(int)CourtBookingStatus.Booked},{(int)CourtBookingStatus.Cancelled})", new { bookingID });

            if (res != null)
            {
                res.Courts = await _dbContext.GetListByQueryAsync<BookingReceiptCourtModel>($@"Select CourtName,ToHourID-FromHourID+1 as Slots,Convert(varchar, Date,34)+' | '+ H1.HourName+' - '+H2.HourName as Slot, B.Price,B.Discount,B.NetPrice,TaxCategoryName,B.Tax, B.TotalPrice,
                    IsCancelled,CB.Name as CancelledBy,DateAdd(MINUTE,{TimeOffset},CancelledOn) as CancelledOn,B.BookingCourtID
                    from BookingCourt B
                    JOIN Court C on C.CourtID=B.CourtID
                    JOIN HourMaster H1 on H1.HourID=B.FromHourID
                    JOIN HourMaster H2 on H2.HourID=ToHourID+1
                    LEFT JOIN TaxCategory T on T.TaxCategoryID=B.TaxCategoryID
                    LEFT JOIN viEntity CB on CB.EntityID=B.CancelledBy
                    Where B.BookingID=@bookingID and B.IsDeleted=0", new { bookingID });

                res.Discount = res.Courts.Sum(s => s.Discount);
                res.Total = res.Courts.Sum(s => s.TotalPrice) + res.Discount;
            }
            return Ok(res);
        }

        [HttpGet("cancel-booking/{bookingID}")]
        public async Task<IActionResult> CancelBooking(int bookingID)
        {
            var booking = await _dbContext.GetAsync<Booking>($"BookingID=@bookingID and Status={(int)CourtBookingStatus.Booked}", new { bookingID });
            if (booking == null)
                return BadRequest(new Error("Booking not found"));

            var expiredSlotCount = await _dbContext.GetByQueryAsync<int>($@"Select Count(*)
                from BookingCourt B
                JOIN HourMaster H on H.HourID=B.FromHourID
                Where BookingID={bookingID} and DATEADD(MINUTE,H.HourInMinute,CONVERT(datetime,Date))<DATEADD(MINUTE,{TimeOffset},GETUTCDATE()) and ISNULL(B.IsCancelled,0)=0 and B.IsDeleted=0", null);

            if (expiredSlotCount > 0)
                return BadRequest(new Error("There are " + expiredSlotCount + " slots expired"));

            _cn.Open();
            using var tran = _cn.BeginTransaction();
            try
            {
                #region Accounts

                int? voucherTypeID = await _dbContext.GetFieldsAsync<AccVoucherType, int?>("VoucherTypeID", $"VoucherTypeNatureID={(int)VoucherTypeNatures.Payment} and ClientID={CurrentClientID}", null, tran);
                int? jmjournalMasterID = null;
                if (voucherTypeID != null)
                {
                    var voucherNumber = await _accounts.GetVoucherNumber(voucherTypeID.Value, CurrentBranchID, tran);

                    AccJournalMaster jm = new()
                    {
                        VoucherTypeID = voucherTypeID,
                        BranchID = CurrentBranchID,
                        JournalNo = voucherNumber.JournalNo,
                        JournalNoPrefix = voucherNumber.JournalNoPrefix,
                        Date = DateTime.Now.Date,
                        EntityID = booking.CustomerEntityID,
                        IsSuccess = true,
                        NarrationType = (int)VoucherNarrationType.Single,
                        Particular = "Cancel Court Booking -" + booking.BookingNo,
                        Remarks = "Cancel Court Booking -" + booking.BookingNo,
                    };
                    jmjournalMasterID = await _dbContext.SaveAsync(jm, tran);

                    List<AccJournalEntry> journalEntries = new();

                    #region Debit Entries

                    decimal taxTotal = 0;
                    decimal netPrice = await _dbContext.GetByQueryAsync<decimal>($@"Select IsNull(Sum(NetPrice),0)
                        from BookingCourt
                        Where BookingID=@bookingID and IsCancelled=0 and IsDeleted=0", new { bookingID }, tran);

                    journalEntries.Add(new()
                    {
                        Debit = netPrice,
                        Credit = 0,
                        LedgerID = await _dbContext.GetByQueryAsync<int>($@"Select LedgerID from AccLedger Where ClientID={CurrentClientID} and IsDeleted=0 and LedgerName='Sales Return'", null, tran),
                    });

                    var taxes = await _dbContext.GetListByQueryAsync<TaxSummaryModel>($@"Select LedgerID, CONVERT(DECIMAL(10,2),Round(Sum(NetPrice*Percentage)/100,2)) as TaxAmount
                                from TaxCategoryItem T
                                JOIN BookingCourt B on B.TaxCategoryID=T.TaxCategoryID
                                Where T.IsDeleted=0 and B.IsDeleted=0 and Percentage>0 and B.BookingID={bookingID} and B.IsCancelled=0
                                Group by LedgerID", null, tran);

                    if (taxes != null)
                    {
                        foreach (var tax in taxes)
                        {
                            journalEntries.Add(new()
                            {
                                Debit = tax.TaxAmount,
                                Credit = 0,
                                LedgerID = tax.LedgerID,
                            });
                        }
                        taxTotal = taxes.Sum(s => s.TaxAmount);
                    }

                    #endregion

                    #region Credit Entries

                    journalEntries.Add(new()
                    {
                        Debit = 0,
                        Credit = taxTotal + netPrice,
                        LedgerID = await _accounts.GetEntityLedgerID(booking.CustomerEntityID.Value, CurrentClientID, tran),
                    });

                    #endregion

                    await _dbContext.SaveSubItemListAsync(journalEntries, "JournalMasterID", jmjournalMasterID.Value, tran);

                    #endregion
                }

                #endregion

                await _dbContext.DeleteAsync<BookingSection>("BookingCourtID in(Select BookingCourtID From BookingCourt Where BookingID=@bookingID and IsCancelled=0 and IsDeleted=0)", new { bookingID }, tran);

                await _dbContext.ExecuteAsync(@$"Update BookingCourt Set IsCancelled=1,CancelledOn=GETUTCDATE(),CancelledBy={CurrentEntityID},CancelJournalMasterID={jmjournalMasterID.Value}
                        Where BookingID=@bookingID and IsCancelled=0 and IsDeleted=0", new { bookingID }, tran);

                booking.Status = (int)CourtBookingStatus.Cancelled;
                booking.CancelJournalMasterID = jmjournalMasterID;
                booking.CancelledOn = DateTime.UtcNow;
                booking.CancelledBy = CurrentEntityID;
                await _dbContext.SaveAsync(booking, tran);

                tran.Commit();
                await SendBookingSignalRPush("booking cancelled");
                return Ok(new Success());
            }
            catch (Exception err)
            {
                tran.Rollback();
                return BadRequest(new Error());
            }
        }

        [HttpGet("cancel-slot/{bookingCourtID}")]
        public async Task<IActionResult> CancelSlot(int bookingCourtID)
        {
            var bookingCourt = await _dbContext.GetAsync<BookingCourt>(@bookingCourtID);
            if (bookingCourt == null)
                return BadRequest(new Error("Slot not found"));
            else if (bookingCourt.IsCancelled)
                return BadRequest(new Error("Slot not cancelled"));

            var expiredSlotCount = await _dbContext.GetByQueryAsync<int>($@"Select Count(*)
                from BookingCourt B
                JOIN HourMaster H on H.HourID=B.FromHourID
                Where BookingCourtID={bookingCourtID} and DATEADD(MINUTE,H.HourInMinute,CONVERT(datetime,Date))<DATEADD(MINUTE,{TimeOffset},GETUTCDATE()) and ISNULL(B.IsCancelled,0)=0 and B.IsDeleted=0", null);

            if (expiredSlotCount > 0)
                return BadRequest(new Error("The Slot is expired"));

            var booking = await _dbContext.GetAsync<Booking>(bookingCourt.BookingID.Value);

            _cn.Open();
            using var tran = _cn.BeginTransaction();
            try
            {
                #region Accounts

                int? voucherTypeID = await _dbContext.GetFieldsAsync<AccVoucherType, int?>("VoucherTypeID", $"VoucherTypeNatureID={(int)VoucherTypeNatures.Payment} and ClientID={CurrentClientID}", null, tran);
                int? jmjournalMasterID = null;
                if (voucherTypeID != null)
                {
                    var voucherNumber = await _accounts.GetVoucherNumber(voucherTypeID.Value, CurrentBranchID, tran);

                    var slotName = await _dbContext.GetByQueryAsync<string>($@"Select CourtName+' | '+ Convert(varchar,B.Date,13)+' | '+H1.HourName+'-'+H2.HourName +' (BNo:'+CONVERT(varchar, BookingNo)+')'
                        from BookingCourt B
                        JOIN HourMaster H1 on H1.HourID=B.FromHourID
                        JOIN HourMaster H2 on H2.HourID=B.ToHourID+1
                        JOIN Court C on C.CourtID=B.CourtID
                        JOIN Booking BB on BB.BookingID=B.BookingID
                        Where BookingCourtID={bookingCourtID}", new { bookingCourtID }, tran);

                    AccJournalMaster jm = new()
                    {
                        VoucherTypeID = voucherTypeID,
                        BranchID = CurrentBranchID,
                        JournalNo = voucherNumber.JournalNo,
                        JournalNoPrefix = voucherNumber.JournalNoPrefix,
                        Date = DateTime.Now.Date,
                        EntityID = booking.CustomerEntityID,
                        IsSuccess = true,
                        NarrationType = (int)VoucherNarrationType.Single,
                        Particular = "Cancel Slot " + slotName,
                        Remarks = "Cancel Slot " + slotName,
                    };
                    jmjournalMasterID = await _dbContext.SaveAsync(jm, tran);

                    List<AccJournalEntry> journalEntries = new();

                    #region Debit Entries


                    journalEntries.Add(new()
                    {
                        Debit = bookingCourt.NetPrice,
                        Credit = 0,
                        LedgerID = await _dbContext.GetByQueryAsync<int>($@"Select LedgerID from AccLedger Where ClientID={CurrentClientID} and IsDeleted=0 and LedgerName='Sales Return'", null, tran),
                    });

                    var taxes = await _dbContext.GetListByQueryAsync<TaxSummaryModel>($@"Select LedgerID, CONVERT(DECIMAL(10,2),Round(Sum(NetPrice*Percentage)/100,2)) as TaxAmount
                                from TaxCategoryItem T
                                JOIN BookingCourt B on B.TaxCategoryID=T.TaxCategoryID
                                Where T.IsDeleted=0 and B.IsDeleted=0 and Percentage>0 and B.BookingCourtID={bookingCourtID} and B.IsCancelled=0
                                Group by LedgerID", null, tran);

                    if (taxes != null)
                    {
                        foreach (var tax in taxes)
                        {
                            journalEntries.Add(new()
                            {
                                Debit = tax.TaxAmount,
                                Credit = 0,
                                LedgerID = tax.LedgerID,
                            });
                        }
                    }

                    #endregion

                    #region Credit Entries

                    journalEntries.Add(new()
                    {
                        Debit = 0,
                        Credit = bookingCourt.Tax + bookingCourt.NetPrice,
                        LedgerID = await _accounts.GetEntityLedgerID(booking.CustomerEntityID.Value, CurrentClientID, tran),
                    });

                    #endregion

                    await _dbContext.SaveSubItemListAsync(journalEntries, "JournalMasterID", jmjournalMasterID.Value, tran);

                    #endregion
                }

                await _dbContext.DeleteAsync<BookingSection>("BookingCourtID=@bookingCourtID", new { bookingCourtID }, tran);


                bookingCourt.IsCancelled = true;
                bookingCourt.CancelJournalMasterID = jmjournalMasterID;
                bookingCourt.CancelledOn = DateTime.UtcNow;
                bookingCourt.CancelledBy = CurrentEntityID;
                await _dbContext.SaveAsync(bookingCourt, tran);

                tran.Commit();
                await SendBookingSignalRPush("slot cancelled");
                return Ok(new Success());
            }
            catch (Exception err)
            {
                tran.Rollback();
                return BadRequest(new Error());
            }
        }

        [HttpPost("get-court-booking-list")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetCourtBookingList(BookingPagedListPostModelWithFilter model)
        {
            PagedListQueryModel query = model;

            query.Select = $@"SELECT B.BookingID, BookingNo,VE.Name AS CustomerName,Status,B.Date as BookedOn,BB.Name as BookedBy,Remarks,VE.Phone as CustomerPhone
                FROM Booking B
                LEFT JOIN viEntity VE ON VE.EntityID = B.CustomerEntityID
                LEFT JOIN viEntity BB ON BB.EntityID = B.BookedBy
                Join AccJournalMaster J on J.JournalMasterID=B.JournalMasterID and J.IsDeleted=0 and J.IsSuccess=1";

            query.WhereCondition = $" B.IsDeleted=0 and B.BranchID={CurrentBranchID} and B.Status in({(int)CourtBookingStatus.Booked},{(int)CourtBookingStatus.Cancelled})";
            //query.WhereCondition += _common.GeneratePagedListFilterWhereCondition(model, "BookingNo");
            query.SearchLikeColumnNames=new() { "VE.Name","BookingNo" };
            if (!string.IsNullOrEmpty(model.CustomerPhone))
                query.WhereCondition+=$" and VE.Phone like '%{model.CustomerPhone}%'";
            query.OrderByFieldName = model.OrderByFieldName;
            var res = await _dbContext.GetPagedList<BookingViewModel>(query, null);
            return Ok(res);
        }

        [HttpPost("get-customer-ledger-balance")]
        public async Task<IActionResult> GetCustomerLedgerBalance(PagedListPostModelWithFilter model)
        {
            PagedListQueryModel query = model;

            query.Select = $@"Select LedgerID,E.Name,E.Phone,E.EmailAddress,Balance
                From
                (Select L.LedgerID,L.EntityID,ISNULL(Sum(Debit)-Sum(Credit),0) Balance
                from AccJournalEntry J
                JOIN AccJournalMaster JM on JM.JournalMasterID=J.JournalMasterID
                JOIN AccLedger L on L.LedgerID=J.LedgerID and L.IsDeleted=0
                Where L.IsDeleted=0 and JM.IsDeleted=0 and IsSuccess=1 and JM.BranchID={CurrentBranchID}
                Group by L.LedgerID,L.EntityID
                Having ISNULL(Sum(Debit)-Sum(Credit),0)<>0) As A
                JOIN viEntity E on E.EntityID=A.EntityID and EntityTypeID={(int)EntityType.Customer}";
            query.WhereCondition = "1=1 ";
            query.WhereCondition += _common.GeneratePagedListFilterWhereCondition(model, "E.Name");
            query.OrderByFieldName = model.OrderByFieldName;
            var res = await _dbContext.GetPagedList<CustomerLedgerBalanceModel>(query, null);
            return Ok(res);
        }

        [HttpPost("bulk-booking-search")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> BulkBookingSearch(BulkBookingSearchModel model)
        {
            model.ToHourID -= 1;
            BulkbookingSearchResultModel res = new();

            var dates = new List<DateTime>();

            foreach (DateTime day in EachDay(model.FromDate.Value, model.ToDate.Value))
            {
                if (model.DayList.Where(s => s.IsChecked == true && s.DayID == (int)day.DayOfWeek).Count() > 0)
                {
                    dates.Add(day);
                }
            }

            string selectedCourtIds = "";
            string unselectedCourtIds = "";
            List<UnavailableSlotModel> unavailableSlotes = new();

            if (model.CourtList.Where(s => s.IsChecked == true).Count() > 0)
            {
                foreach (var court in model.CourtList.Where(s => s.IsChecked == true))
                {
                    selectedCourtIds += court.CourtID + ",";
                }
                selectedCourtIds = selectedCourtIds.TrimEnd(',');
            }

            if (model.CourtList.Where(s => s.IsChecked == false).Count() > 0)
            {
                foreach (var court in model.CourtList.Where(s => s.IsChecked == false))
                {
                    unselectedCourtIds += court.CourtID + ",";
                }
                unselectedCourtIds = unselectedCourtIds.TrimEnd(',');
            }

            foreach (var date in dates)
            {
                List<CourtListModel> dailyAvailableCourts = new();
                if (!string.IsNullOrEmpty(selectedCourtIds))
                {
                    dailyAvailableCourts = await _dbContext.GetListByQueryAsync<CourtListModel>($@"SELECT Top {model.CourtCount} C.CourtID,C.CourtName
                                                                        FROM Court C
                                                                        JOIN Hall H on H.HallID=C.HallID
                                                                        JOIN HallTiming HT on HT.HallID=H.HallID and HT.DayID={(int)date.DayOfWeek} and HT.IsDeleted=0 and HT.StartHourID<=@FromHourID and HT.EndHourID>=@ToHourID
                                                                        WHERE C.CourtID NOT IN (
                                                                            SELECT Distinct BC.CourtID
                                                                            FROM BookingCourt BC
                                                                            JOIN Booking B ON B.BookingID = BC.BookingID
                                                                            JOIN BookingSection BS ON BS.BookingCourtID = BC.BookingCourtID 
                                                                            Left Join AccJournalMaster J on J.JournalMasterID=B.JournalMasterID and J.IsDeleted=0
                                                                            WHERE BC.IsDeleted=0 and B.IsDeleted=0 and BS.IsDeleted=0
                                                                            and BC.Date = Convert(date,@Date) AND (BS.HourID BETWEEN @FromHourID AND @ToHourID) and BC.IsCancelled=0
                                                                            and ((Status={(int)CourtBookingStatus.Booked} and IsSuccess=1) or (Status={(int)CourtBookingStatus.Carted} and DATEADD(MINUTE,{PDV.CourtCartMinute},B.Date)>GETUTCDATE()))
                                                                        ) AND C.IsDeleted=0 and CourtID in({selectedCourtIds})", new { Date = date, model.FromHourID, model.ToHourID });
                }

                if ((dailyAvailableCourts == null || dailyAvailableCourts.Count < model.CourtCount) && !string.IsNullOrEmpty(unselectedCourtIds))
                {
                    int cnt = dailyAvailableCourts == null ? model.CourtCount : model.CourtCount - dailyAvailableCourts.Count;

                    dailyAvailableCourts.AddRange(await _dbContext.GetListByQueryAsync<CourtListModel>($@"SELECT Top {cnt} C.CourtID,C.CourtName
                                                                        FROM Court C
                                                                        JOIN Hall H on H.HallID=C.HallID
                                                                        JOIN HallTiming HT on HT.HallID=H.HallID and HT.DayID={(int)date.DayOfWeek} and HT.IsDeleted=0 and HT.StartHourID<=@FromHourID and HT.EndHourID>=@ToHourID
                                                                        WHERE C.CourtID NOT IN (
                                                                            SELECT Distinct BC.CourtID
                                                                            FROM BookingCourt BC
                                                                            JOIN Booking B ON B.BookingID = BC.BookingID
                                                                            JOIN BookingSection BS ON BS.BookingCourtID = BC.BookingCourtID 
                                                                            Left Join AccJournalMaster J on J.JournalMasterID=B.JournalMasterID and J.IsDeleted=0
                                                                            WHERE BC.IsDeleted=0 and B.IsDeleted=0 and BS.IsDeleted=0
                                                                            and BC.Date = Convert(date,@Date) AND (BS.HourID BETWEEN @FromHourID AND @ToHourID) and BC.IsCancelled=0
                                                                            and ((Status={(int)CourtBookingStatus.Booked}  and IsSuccess=1) or (Status={(int)CourtBookingStatus.Carted} and DATEADD(MINUTE,{PDV.CourtCartMinute},B.Date)>GETUTCDATE()))
                                                                        ) AND C.IsDeleted=0 and CourtID in({unselectedCourtIds})", new { Date = date, model.FromHourID, model.ToHourID }));


                }

                var fromhour = await _dbContext.GetAsync<HourMaster>(model.FromHourID.Value);
                var tohour = await _dbContext.GetAsync<HourMaster>(model.ToHourID.Value + 1);

                if (dailyAvailableCourts == null || dailyAvailableCourts.Count < model.CourtCount)
                {

                    res.UnavailableSlots.Add(new()
                    {
                        Date = date,
                        UnavailableCount = (tohour.HourID - fromhour.HourID) * (dailyAvailableCourts == null ? model.CourtCount : model.CourtCount - dailyAvailableCourts.Count),
                        Time = fromhour.HourName + "-" + tohour.HourName
                    });
                }

                if (dailyAvailableCourts != null)
                {
                    foreach (var court in dailyAvailableCourts)
                    {
                        res.AvailableSlots.Add(new()
                        {
                            Date = date,
                            CourtID = court.CourtID,
                            CourtName = court.CourtName,
                            FromHourID = fromhour.HourID,
                            FromHourName = fromhour.HourName,
                            ToHourID = model.ToHourID.Value,
                            ToHourName = tohour.HourName
                        });
                    }
                }
            }
            return Ok(res);
        }

        private IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
                yield return day;
        }

        [HttpPost("bulk-booking")]
        public async Task<IActionResult> BulkBooking(List<AvailableSlotModel> model)
        {
            BulkBookingResultModel res = new();
            _cn.Open();
            using var tran = _cn.BeginTransaction();
            try
            {
                Booking booking = new()
                {
                    Date = DateTime.UtcNow,
                    Status = (int)CourtBookingStatus.Carted,
                    BookingNo = await _dbContext.GetByQueryAsync<int>($"Select ISNULL(Max(BookingNo),10000)+1 from Booking Where BranchID={CurrentBranchID}", null, tran),
                    BranchID = CurrentBranchID
                };
                res.BookingID = await _dbContext.SaveAsync(booking, transaction: tran);


                foreach (var item in model)
                {
                    var alreadyCarted = await _dbContext.GetByQueryAsync<int>($@"Select Count(BC.BookingCourtID)
                        From Booking B
                        JOIN BookingCourt BC on BC.BookingID=B.BookingID
                        Where B.IsDeleted=0 and BC.IsDeleted=0 and BC.IsCancelled=0 and BC.Date=Convert(Date,@Date)
                        and (@FromHourID between BC.FromHourID and BC.ToHourID  or @ToHourID between BC.FromHourID and BC.ToHourID)
                        and BC.CourtID=@CourtID
                        and (Status={(int)CourtBookingStatus.Booked} or (Status={(int)CourtBookingStatus.Carted} and DATEADD(MINUTE,{PDV.CourtCartMinute},B.Date)>GETUTCDATE()))", item, tran);

                    if (alreadyCarted > 0)
                    {
                        res.UnavailableCourts.Add(item.CourtName + " " + item.Date.ToString("dd/MM/yyyy" + " | " + item.FromHourName + " - " + item.ToHourName));
                    }
                    else
                    {
                        var priceGroup= await _dbContext.GetByQueryAsync<CourtPriceGroup>($@"Select CurrencyID,IncTax,TaxCategoryID
                                                                                                            From Court C
                                                                                                            Join CourtPriceGroup G on G.PriceGroupID=C.PriceGroupID and G.IsDeleted=0
                                                                                                            where CourtID=@CourtID", item,tran);
                        if (priceGroup==null)
                        {
                            var courtName = await _dbContext.GetFieldsAsync<Court, string>("CourtName", "CourtId=@CourtID", item,tran);
                            return BadRequest(new Error() { ResponseMessage=$"Price group is not assigned for {courtName}" });
                        }
                        decimal Rate = 0;
                        int tempFromHourID = 0,tempToHourID= item.ToHourID+1;
                        CourtPriceGroupItem courtPriceItem = new();
                        do
                        {
                            int tempQuantity = 0;
                            tempFromHourID=tempFromHourID==0 ? item.FromHourID : tempFromHourID;
                            courtPriceItem = await _dbContext.GetAsync<CourtPriceGroupItem>($"@HourID between StartHourID and EndHourID-1 and DayID={Convert.ToInt32(item.Date.DayOfWeek)} and IsDeleted=0", new { HourID=tempFromHourID},tran);
                            if (courtPriceItem==null)
                            {
                                string HourName = await _dbContext.GetFieldsAsync<HourMaster, string>("HourName", "HourID=@tempFromHourID", new { tempFromHourID }, tran);
                                return BadRequest(new Error() { ResponseMessage=$"{HourName} is not included in any price group item of {item.Date.DayOfWeek.ToString()}" });
                            }
                            if (tempToHourID<=courtPriceItem.EndHourID)
                                tempQuantity=tempToHourID - tempFromHourID;
                            else
                            {
                                tempQuantity=courtPriceItem.EndHourID.Value - tempFromHourID;
                                tempFromHourID=courtPriceItem.EndHourID.Value;
                            }  
                            Rate+=(courtPriceItem.Rate*tempQuantity);
                        } while (tempToHourID>courtPriceItem.EndHourID);

                        decimal price = 0, tax = 0, taxPercentage = 0;
                        //int quantity = item.ToHourID - item.FromHourID + 1;
                        if (priceGroup.TaxCategoryID != null)
                        {
                            taxPercentage = await _dbContext.GetByQueryAsync<decimal>($@"Select TaxPercentage from viTaxCategory Where TaxCategoryID={priceGroup.TaxCategoryID}", null, tran);
                            if (priceGroup.IncTax)
                            {
                                price = Rate / ((taxPercentage / 100) + 1);
                                tax = Rate - price;
                            }
                            else
                            {
                                price = Rate;
                                tax = price * taxPercentage / 100;
                            }
                        }
                        else
                        {
                            price = Rate;
                        }

                        //price *= quantity;
                        //tax *= quantity;

                        BookingCourt bookingCourt = new()
                        {
                            Date = item.Date,
                            BookingID = res.BookingID,
                            CourtID = item.CourtID,
                            FromHourID = item.FromHourID,
                            ToHourID = item.ToHourID,
                            // Rate = Rate * quantity,
                            Rate = Rate,
                            CurrencyID = priceGroup.CurrencyID,
                            TaxCategoryID = priceGroup.TaxCategoryID,
                            Price = price,
                            Discount = 0,
                            NetPrice = price,
                            TaxPercentage = taxPercentage,
                            Tax = tax,
                            TotalPrice = price + tax
                        };
                        bookingCourt.BookingCourtID = await _dbContext.SaveAsync(bookingCourt, transaction: tran);


                        var allHourIds = await _dbContext.GetListByQueryAsync<int>($@"SELECT HourID
                                                                            FROM HourMaster
                                                                            WHERE HourID between {item.FromHourID} and {item.ToHourID}", null, transaction: tran);

                        List<CourtSectionMap> sectionList = await _dbContext.GetListByQueryAsync<CourtSectionMap>($@"Select * FROM CourtSectionMap WHERE IsDeleted =0 and CourtID={item.CourtID}",null, transaction: tran);

                        List<BookingSection> bookingSections = new();

                        foreach (var section in sectionList)
                        {
                            foreach (var hourId in allHourIds)
                            {
                                bookingSections.Add(new BookingSection()
                                {
                                    SectionID = section.SectionID,
                                    HourID = hourId
                                });
                            }
                        }
                        await _dbContext.SaveSubItemListAsync(bookingSections, "BookingCourtID", bookingCourt.BookingCourtID, transaction: tran);
                    }
                }

                tran.Commit();
                await SendBookingSignalRPush("bulk carted");
            }
            catch (Exception err)
            {
                tran.Rollback();
                return BadRequest(new Error());
            }

            res.BookedCourts = await _dbContext.GetListByQueryAsync<CourtBookingViewModelNew>($@"Select BC.BookingID,BC.BookingCourtID,CourtName,CurrencyName as Currency,BC.Rate,BC.Date,H1.HourName+'-'+H2.HourName as Time,B.Date as AddedOn,BC.Price,BC.Discount,BC.NetPrice,BC.Tax,BC.TotalPrice
                        From BookingCourt BC
                        JOIN Booking B on B.BookingID=BC.BookingID
                        JOIN Court C on C.CourtID=BC.CourtID
                        JOIN CourtPriceGroup CG on CG.PriceGroupID=C.PriceGroupID
						JOIN Currency CR on CR.CurrencyID=CG.CurrencyID
                        JOIN HourMaster H1 on H1.HourID=BC.FromHourID
                        JOIN HourMaster H2 on H2.HourID=BC.ToHourID+1
                        Where B.BookingID=@bookingID", new { res.BookingID });
            return Ok(res);
        }

        [HttpGet("lock-court/{bookingID}")]
        public async Task<IActionResult> LockCourts(int bookingID)
        {
            var booking =await _dbContext.GetAsync<Booking>(bookingID);
            if(booking!=null && booking.Status==(int)CourtBookingStatus.Carted && booking.Date.Value.AddMinutes(PDV.CourtCartMinute)>DateTime.UtcNow)
            {
                booking.Status = (int)CourtBookingStatus.Carted;
                booking.Date = DateTime.UtcNow;
                booking.BookedBy = CurrentEntityID;
                booking.IsLocked = true;
                await _dbContext.SaveAsync(booking);

                await _dbContext.ExecuteAsync($@"Update BookingCourt Set Rate=0,Price=0,Discount=0,NetPrice=0,TaxCategoryID=null,
                    TaxPercentage=0,Tax=0,TotalPrice=0
                    Where IsDeleted=0 and BookingID=@bookingID", new { bookingID });
                await SendBookingSignalRPush("court locked");
                return Ok(new Success());
            }
            return BadRequest(new Error("Booking not found"));
        }

        #region Court Price Group

        [HttpPost("get-all-price-groups")]
        public async Task<IActionResult> GetAll(PagedListPostModelWithFilter model)
        {
            PagedListQueryModel query = model;
            query.Select = $@"Select PriceGroupID as ID, PriceGroupName as Value
                                From CourtPriceGroup";
            query.OrderByFieldName = model.OrderByFieldName;
            query.WhereCondition=$"IsDeleted=0 and ClientID={CurrentClientID}";
            query.WhereCondition += _common.GeneratePagedListFilterWhereCondition(model, "PriceGroupName");
            var res = await _dbContext.GetPagedList<IdnValuePair>(query, null);
            return Ok(res ?? new());
        }

        [HttpGet("get-court-price-group-details/{priceGroupId}")]
        public async Task<IActionResult> GetPriceGroupDetails(int priceGroupId)
        {
            var res = await _dbContext.GetByQueryAsync<CourtPriceGroupModel>($@"Select G.*,CurrencyName 
                                                                                    From CourtPriceGroup G
                                                                                    Join Currency C on C.CurrencyID=G.CurrencyID and C.IsDeleted=0
                                                                                    Where G.IsDeleted=0 and PriceGroupId=@priceGroupId", new { priceGroupId });
            res.Items=await _dbContext.GetListByQueryAsync<CourtPriceGroupItemModel>($@"Select I.*,S.HourName as StartHourName,E.HourName as EndHourName 
                                                                                    From CourtPriceGroupItem I
                                                                                    Join HourMaster S on S.HourID=I.StartHourID and S.IsDeleted=0
                                                                                    Join HourMaster E on E.HourID=I.EndHourID and E.IsDeleted=0
                                                                                    Where I.IsDeleted=0 and PriceGroupId=@priceGroupId", new { priceGroupId });
            return Ok(res);
        }

        [HttpPost("save-court-price-group")]
        public async Task<IActionResult> GetPriceGroupDetails(CourtPriceGroupModel model)
        {
            _cn.Open();
            using var tran = _cn.BeginTransaction();
            try
            {
                var priceGroup= _mapper.Map<CourtPriceGroup>(model);
                priceGroup.ClientID=CurrentClientID;
                priceGroup.PriceGroupID=await _dbContext.SaveAsync(priceGroup,tran);
                var groupItems = _mapper.Map<List<CourtPriceGroupItem>>(model.Items);
                await _dbContext.SaveSubItemListAsync(groupItems, "PriceGroupID",priceGroup.PriceGroupID,tran);
                tran.Commit();
                return Ok(new Success());
            }
            catch (Exception err)
            {
                tran.Rollback();
                return BadRequest(new Error());
            }
        }

        #endregion

        #region Court Counter Code


        [HttpPost("get-all-counter-code")]
        public async Task<IActionResult> GetAllCounterCode(PagedListPostModel model)
        {
            PagedListQueryModel query = model;
            query.Select = $@"SELECT * FROM CourtCounterCode";
            query.WhereCondition = $"IsDeleted=0";
            query.SearchLikeColumnNames = new() { "CounterCode" };
            var res = await _dbContext.GetPagedList<CourtCounterCode>(query, null);
            return Ok(res);
        }

        [HttpGet("get-counter-code/{counterCodeId}")]
        public async Task<IActionResult> GetCounterCode(int counterCodeId)
        {
            var res = await _dbContext.GetAsync<CourtCounterCode>(counterCodeId);
            return Ok(res);
        }

        [HttpPost("save-counter-code")]
        public async Task<IActionResult> Save(CourtCounterCode model)
        {
            await _dbContext.SaveAsync(model);
            return Ok(new Success());
        }

        [HttpGet("delete-counter-code")]
        public async Task<IActionResult> DeleteCounterCode(int id)
        {
            await _dbContext.DeleteAsync<CourtCounterCode>(id);
            return Ok(new Success());
        }

        #endregion

        [HttpGet("get-customer-balance/{entityId}")]
        public async Task<IActionResult> GetCustomerBalance(int entityId)
        {
            var customer = await _dbContext.GetByQueryAsync<decimal?>($@"Select ISNULL(Sum(Debit)-Sum(Credit),0)
                    from AccJournalEntry J
                    JOIN AccJournalMaster JM on JM.JournalMasterID=J.JournalMasterID
                    JOIN AccLedger L on L.LedgerID=J.LedgerID and L.IsDeleted=0
                    Where L.EntityID=@EntityID and L.IsDeleted=0 and JM.IsDeleted=0 and IsSuccess=1", new { EntityID = entityId });
            return Ok(customer);
        }

        [HttpPost("save-customer-payment")]
        public async Task<IActionResult> SaveCustomerPayment(CustomerPaymentPostModel model)
        {
            _cn.Open();
            using var tran = _cn.BeginTransaction();
            try
            {
                int? voucherTypeID = await _dbContext.GetFieldsAsync<AccVoucherType, int?>("VoucherTypeID", $"VoucherTypeNatureID={(int)VoucherTypeNatures.Receipt} and ClientID={CurrentClientID}", null,tran);
                var voucherNumber = await _accounts.GetVoucherNumber(voucherTypeID.Value, CurrentBranchID,tran);
                AccJournalMaster jm = new()
                {
                    VoucherTypeID = voucherTypeID,
                    BranchID = CurrentBranchID,
                    JournalNo = voucherNumber.JournalNo,
                    JournalNoPrefix = voucherNumber.JournalNoPrefix,
                    Date = DateTime.Now.Date,
                    EntityID = model.EntityID,
                    IsSuccess = true,
                    NarrationType = (int)VoucherNarrationType.Single,
                    Particular = "",
                    Remarks = "",
                };
                jm.JournalMasterID = await _dbContext.SaveAsync(jm,tran);
                List<AccJournalEntry> journalEntries = new();
                int debitLedgerType = model.PaymentType==(int)PaymentTypes.Bank ? (int)LedgerTypes.Bank : (int)LedgerTypes.Cash;

                journalEntries.Add(new()
                {
                    Debit = model.Amount,
                    Credit = 0,
                    LedgerID = await _dbContext.GetByQueryAsync<int>($@"Select LedgerID from AccLedger Where ClientID={CurrentClientID} and IsDeleted=0 and LedgerTypeID={debitLedgerType}", null,tran),
                });
                journalEntries.Add(new()
                {
                    Debit = 0,
                    Credit = model.Amount,
                    LedgerID = await _accounts.GetEntityLedgerID(model.EntityID.Value, CurrentClientID,tran),
                });
                await _dbContext.SaveSubItemListAsync(journalEntries, "JournalMasterID", jm.JournalMasterID,tran);
                tran.Commit();
                return Ok(new Success());
            }
            catch (Exception err)
            {
                tran.Rollback();
                return BadRequest(new Error());
            }
        }

        [AllowAnonymous]
        [HttpGet("send-booking-signalr-push/{message}")]
        public async Task<IActionResult> SendSignalRPush(string message)
        {
            await SendBookingSignalRPush(message);
            return Ok(new Success());
        }

        private async Task SendBookingSignalRPush(string message)
        {
            var entityIds = await _dbContext.GetListByQueryAsync<int>($@"Select EntityID
									from Users U
									Left Join UserTypeRole UT on (UT.UserID=U.UserID or UT.UserTypeID=U.UserTypeID) and RoleID={(int)Roles.CourtBooking} and IsNull(UT.HasAccess,0)=1 and UT.IsDeleted=0
									Where (UT.ID!=0 or U.UserTypeID in({(int)UserTypes.SuperAdmin},{(int)UserTypes.Developer},{(int)UserTypes.Client})) and U.IsDeleted=0", null);
            foreach (var entity in entityIds)
            {
                await _notification.SendSignalRPush(entity, message, null, "booking");
            }
        }

    }
}
