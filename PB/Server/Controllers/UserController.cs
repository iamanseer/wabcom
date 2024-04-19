using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PB.DatabaseFramework;
using PB.Model;
using PB.Server.Repository;
using PB.Shared.Enum;
using PB.Shared.Models;
using PB.Shared;
using System.Data;
using PB.Shared.Tables;
using PB.Shared.Helpers;
using static NPOI.HSSF.Util.HSSFColor;
using PB.Shared.Models.Common;
using PB.Model.Models;
using PB.EntityFramework;

namespace PB.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UserController : BaseController
    {
        private readonly IDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IDbConnection _cn;
        private readonly IAuthenticationRepository _auth;
        private readonly IUserRepository _user;

        public UserController(IDbContext dbContext, IMapper mapper, IDbConnection cn, IAuthenticationRepository auth, IUserRepository user)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _cn = cn;
            _auth = auth;
            _user = user;
        }

        [HttpPost("save-user")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "UsersAdd,UsersEdit")]
        public async Task<IActionResult> SaveUser(UserSingleModel model)
        {

            var userName = await _dbContext.GetByQueryAsync<string>($@"
                                            Select UserName 
                                            From Users 
                                            Where UserID<>@UserID and UserName=@UserName and UserTypeID in({(int)UserTypes.Client},{(int)UserTypes.Staff}) and ClientID={CurrentClientID} and IsDeleted=0
            ",model);
            if (!string.IsNullOrEmpty(userName))
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ResponseMessage = "Username already exist, try different username",
                    ResponseTitle = "Invalid Submission"
                });
            }

            var email = await _dbContext.GetByQueryAsync<string>(@$"Select EmailAddress 
                                                                            from Entity
                                                                            where EmailAddress=@EmailAddress and EntityID<>{Convert.ToInt32(model.EntityID)} and EntityTypeID in({(int)EntityType.Client},{(int)EntityType.Branch},{(int)EntityType.User},{(int)EntityType.Customer}) and ClientID={CurrentClientID} and IsDeleted=0
            ", model);

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

                    var logSummaryID = await _dbContext.InsertAddEditLogSummary(model.UserID, "User : " + model.UserName, tran);

                    model.ClientID = CurrentClientID;
                    var entity = _mapper.Map<Entity>(model);
                    entity.EntityTypeID = (int)EntityType.Staff;

                    model.EntityID = await _dbContext.SaveAsync(entity, tran, logSummaryID: logSummaryID);

                    var entityPersonalInfo = _mapper.Map<EntityPersonalInfo>(model);
                    await _dbContext.SaveAsync(entityPersonalInfo, tran, logSummaryID: logSummaryID);

                    var entityAddress = _mapper.Map<EntityAddress>(model);
                    await _dbContext.SaveAsync(entityAddress, tran, logSummaryID: logSummaryID);
                    if(model.UserID== 0)
                    {
                        var user = _mapper.Map<UserCustom>(model);
                        user.EmailConfirmed = true;
                        user.UserTypeID = (int)UserTypes.Staff;
                        model.UserID = await _user.SaveUser(user, false, tran);
                    }
                    else 
                    {
                        await _dbContext.ExecuteAsync($"UPDATE Users Set UserName=@UserName,LoginStatus='{model.LoginStatus}' Where UserID={model.UserID}", new { UserName = model.UserName }, tran);
                    }
                    
                    //Branches
                    var branchAccessList = _mapper.Map<List<BranchUser>>(model.Branches);
                    await _dbContext.SaveSubItemListAsync(branchAccessList, "UserID", model.UserID, tran, logSummaryID: logSummaryID);

                    //Roles
                    var roleAccessList = model.Roles.Where(r => r.HasAccess).ToList();
                    if (roleAccessList.Count > 0)
                    {
                        foreach (var roleAccess in roleAccessList)
                        {
                            model.Roles.Where(r => r.RoleID == roleAccess.RoleID).First().HasAccess = true;
                        }
                    }
                    var rolesAccessMappedList = _mapper.Map<List<UserTypeRoleCustom>>(model.Roles);
                    await _dbContext.SaveSubItemListAsync<UserTypeRoleCustom>(rolesAccessMappedList, "UserID", model.UserID, tran, logSummaryID: logSummaryID);

                    //Reports
                    //var reportAccessList = _mapper.Map<List<UserReportAccess>>(model.Reports);
                    //await _dbContext.SaveSubItemListAsync<UserReportAccess>(reportAccessList, "UserID", model.UserID, tran, logSummaryID: logSummaryID);


                    tran.Commit();

                    var res = new UserAddResultModel() { UserID = model.UserID, UserName = model.UserName };
                    return Ok(res);
                }
                catch (PBException err)
                {
                    tran.Rollback();
                    return BadRequest(new Error(err.Response));
                }
                catch (Exception err)
                {
                    tran.Rollback();
                    return BadRequest(new DbError(err.Message));
                }
            }
        }

        [HttpPost("get-all-users")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Users")]
        public async Task<IActionResult> GetAll(PagedListPostModelWithFilter model)
        {
            var userTypePriority = await _dbContext.GetByQueryAsync<int>($"SELECT PriorityOrder FROM UserType Where UserTypeID = {CurrentUserTypeID}",null);

            PagedListQueryModel searchData = model;
            searchData.Select = $@"Select Distinct E.Name,U.UserID,E.Phone,T.UserTypeName as UserType,EmailAddress as Email
                                From viEntity E
						        LEFT JOIN UserType T on T.UserTypeID=E.UserTypeID
								LEFT JOIN Users U on U.EntityID=E.EntityID
								LEFT JOIN BranchUser BU on U.UserID=BU.UserID  and (BranchID={CurrentBranchID} or BranchID=0)";
            searchData.OrderByFieldName = "E.Name";
            searchData.WhereCondition = $"T.PriorityOrder>{userTypePriority} and U.IsDeleted=0 and U.ClientID={CurrentClientID}";
            searchData.SearchLikeColumnNames = new() { "E.Name" };
            var result = await _dbContext.GetPagedList<UserListViewModel>(searchData, null);
            return Ok(result);
        }

        [HttpGet("delete-user/{userID}")]
       // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "UsersDelete")]
        public async Task<IActionResult> Delete(int userID)
        {

            var user = await _dbContext.GetAsync<User>($"UserID={userID}", null);
            
            _cn.Open();
            using (var tran = _cn.BeginTransaction())
            {
                try
                {
                    var logSummary = await _dbContext.InsertDeleteLogSummary(userID, "User Deleted by : " + CurrentUserID, tran);
                    await _dbContext.DeleteAsync<Entity>(user.EntityID.Value, tran, logSummary);
                    await _dbContext.DeleteSubItemsAsync<EntityAddress>("EntityID",user.EntityID.Value, tran, logSummary);
              
                    await _dbContext.DeleteSubItemsAsync<BranchUser>("UserID",user.UserID, tran, logSummary); 
                    await _dbContext.DeleteAsync<User>(userID, tran, logSummary);
                    await _dbContext.DeleteSubItemsAsync<UserTypeRoleCustom>("UserID", user.UserID, tran, logSummary);


                    tran.Commit();
                    return Ok(true);
                }
                catch (PBException err)
                {
                    tran.Rollback();
                    return BadRequest(new Error(err.Response));
                }
                catch (Exception err)
                {
                    tran.Rollback();
                    return BadRequest(new DbError(err.Message));
                }
            }
        }

        [HttpGet("get-user/{userId}")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Users")]
        public async Task<IActionResult> Get(int userId)
        {
            var userTypePriority = await _dbContext.GetByQueryAsync<int>($"SELECT PriorityOrder FROM UserType Where UserTypeID = {CurrentUserTypeID}", null);

            var result = await _dbContext.GetByQueryAsync<UserSingleModel>($@"Select U.UserID, E.EntityID, E.FirstName,
                                                                                    U.UserName,E.Phone,U.UserTypeID,T.UserTypeName as UserType,
                                                                                    E.EmailAddress,UserName,LoginStatus,EntityPersonalInfoID
                                                                                    From Users U
																	                JOIN UserType T on T.UserTypeID=U.UserTypeID
                                                                                    LEFT JOIN viEntity E on E.EntityID= U.EntityID
																	                LEFT JOIN EntityAddress EA on EA.EntityID= E.EntityID
																	                LEFT JOIN  Entity G on U.EntityID=G.EntityID
                                                                                    Where T.PriorityOrder>={userTypePriority} and U.UserID={userId}", null);

            result.Branches = await _dbContext.GetListByQueryAsync<BranchUserModel>($@"Select B.BranchID,B.BranchName,BU.BranchUserID,BU.CanAccess
                                                                                    From vibranch B
                                                                                    Left Join BranchUser BU on BU.BranchID=B.BranchID and BU.IsDeleted=0 and BU.UserID={userId}
                                                                                    Where B.ClientID={CurrentClientID}", null);

            //result.Roles = await _dbContext.GetListByQueryAsync<UserTypeAccessModel>($@"Select PR.RoleID,R.DisplayName as RoleName,UR.ID,UR.CanAdd,UR.CanEdit,UR.CanDelete,UR.CanMail,UR.CanWhatsapp,UR.HasAccess
            //                                                                            From Client C
            //                                                                            Left Join MembershipPackageRole PR ON PR.PackageID=C.PackageID AND PR.IsDeleted=0
            //                                                                            Left Join Role R ON R.RoleID=PR.RoleID AND R.IsDeleted=0
            //                                                                            Left Join UserTypeRole UR ON PR.RoleID=UR.RoleID AND UR.IsDeleted=0 AND UR.UserID={userId}
            //                                                                            Where C.ClientID={CurrentClientID} ");

            result.Roles = await _dbContext.GetListByQueryAsync<UserTypeAccessModel>($@"Select PR.RoleID,R.DisplayName as RoleName,UR.ID,UR.CanAdd,UR.CanEdit,UR.CanDelete,UR.CanMail,UR.CanWhatsapp,UR.HasAccess
                                                                                        From Client C
                                                                                        Left Join MembershipPackageRole PR ON PR.PackageID=C.PackageID AND PR.IsDeleted=0
                                                                                        Left Join Role R ON R.RoleID=PR.RoleID AND R.IsDeleted=0
                                                                                        Left Join UserTypeRole UR ON PR.RoleID=UR.RoleID AND UR.IsDeleted=0 AND UR.UserID={userId}
                                                                                        Where C.ClientID={CurrentClientID}
                                                                                        UNION
                                                                                        Select R.RoleID,R.DisplayName as RoleName,UR.ID,UR.CanAdd,UR.CanEdit,UR.CanDelete,UR.CanMail,UR.CanWhatsapp,UR.HasAccess
                                                                                        From Role R
                                                                                        LEFT Join UserTypeRole UR  on R.RoleID=UR.RoleID and UR.IsDeleted=0 AND UR.UserID={userId}
                                                                                        Where {PDV.ProgbizClientID}={CurrentClientID}  AND RoleGroupID={(int)RoleGroups.SupportRoles} and R.Isdeleted=0", null);

            result.Roles.Where(role => role.CanWhatsapp && role.CanMail && role.CanEdit && role.CanAdd && role.CanDelete).Select(r => { r.IsRowWise = true; return r; }).ToList();

            var rolesHasAccessCount = result.Roles.Where(r => r.HasAccess).Count();

            if (rolesHasAccessCount == result.Roles.Count)
                result.IsAccessOnAllRole = true;
            else
                result.IsAccessOnAllRole = false;

            //result.Reports = await _dbContext.GetListByQueryAsync<ReportsModel>($@"Select R.ReportID,R.ReportName,UA.UserReportAccessID,UA.HasAccess
            //                                                                        From Report R
            //                                                                        Left Join UserReportAccess UA on R.ReportID=UA.ReportID and UA.IsDeleted=0 and UA.UserID={userId}
            //                                                                        Where R.IsDeleted=0");

            //var reportHasAccessCount = result.Reports.Where(re => re.HasAccess).Count();

            //if (reportHasAccessCount == result.Reports.Count)
            //    result.IsAllReports = true;
            //else
            //    result.IsAllReports = false;

            return Ok(result??new());
        }

        [HttpGet("get-roles")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Users")]
        public async Task<IActionResult> GetAllRoles(int userId)
        {
            //var result = await _dbContext.GetListByQueryAsync<UserTypeAccessModel>($@"Select 0 AS ID,NULL AS UserID,R.RoleID,R.DisplayName as RoleName,0 AS HasAccess,0 AS CanAdd,0 AS CanEdit,0 AS CanDelete,0 AS CanMail,0 AS CanWhatsapp
            //                                                                            From Client C
            //                                                                            Left Join MembershipPackageRole PR ON PR.PackageID=C.PackageID AND PR.IsDeleted=0
            //                                                                            Left Join Role R ON R.RoleID=PR.RoleID AND R.IsDeleted=0
            //                                                                            Where C.ClientID={CurrentClientID} AND C.IsDeleted=0");

            var result = await _dbContext.GetListByQueryAsync<UserTypeAccessModel>($@"Select 0 AS ID,NULL AS UserID,R.RoleID,R.DisplayName as RoleName,0 AS HasAccess,0 AS CanAdd,0 AS CanEdit,
                                                                                        0 AS CanDelete,0 AS CanMail,0 AS CanWhatsapp
                                                                                        From Client C
                                                                                        Left Join MembershipPackageRole PR ON PR.PackageID=C.PackageID AND PR.IsDeleted=0
                                                                                        Left Join Role R ON R.RoleID=PR.RoleID AND R.IsDeleted=0
                                                                                        Where C.ClientID={CurrentClientID} AND C.IsDeleted=0
                                                                                        UNION
                                                                                        Select 0 AS ID,NULL AS UserID,R.RoleID,R.DisplayName as RoleName,0 AS HasAccess,0 AS CanAdd,0 AS CanEdit,
                                                                                        0 AS CanDelete,0 AS CanMail,0 AS CanWhatsapp
                                                                                        From Role R 
                                                                                        Where {PDV.ProgbizClientID}={CurrentClientID} AND RoleGroupID={(int)RoleGroups.SupportRoles} and R.Isdeleted=0", null);
            //Order by HasAccess desc, R.DisplayNameOrder by HasAccess desc, R.DisplayName
            return Ok(result);
        }





        [HttpGet("get-roles/{userId}")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Users")]
        public async Task<IActionResult> GetRoles(int userId)
        {
            var result = await _dbContext.GetListByQueryAsync<UserTypeAccessModel>($@"Select UR.ID,R.RoleID,R.DisplayName as RoleName,HasAccess,RG.RoleGroupName,RG.RoleGroupID,CanAdd,CanEdit,CanDelete,CanMail,CanWhatsapp
                    From Role R
                    LEFT JOIN RoleGroup RG on RG.RoleGroupID=R.RoleGroupID
                    LEFT JOIN UserTypeRole UR on UR.RoleID=R.RoleID and UR.UserID={userId} and UR.IsDeleted=0
                    Where R.RoleID in(Select RoleID from UserTypeRole Where (UserTypeID={CurrentUserTypeID} or UserID={CurrentUserID}) and HasAccess=1) or {CurrentUserTypeID}<={(int)UserTypes.Client} and R.IsForSupport=0
                    ", null); 
            //Order by HasAccess desc, R.DisplayNameOrder by HasAccess desc, R.DisplayName
            return Ok(result);
        }

        [HttpGet("get-user-menu-list")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Users")]
        public async Task<IActionResult> GetMenuList()
        {
            var result = await _dbContext.GetListByQueryAsync<ViewPageMenuModel>($@"Select U.UserID as ID, V.Name as MenuName 
                                                                                    From Users  U
                                                                                    Join viEntity V on V.EntityID=U.EntityID	
                                                                                    Where U.IsDeleted=0  and U.UserTypeID={(int)UserTypes.Staff} and V.ClientID={CurrentClientID}", null);
            return Ok(result);
        }

        [HttpGet("get-branches/{userId}")]
        public async Task<IActionResult> GetBranches(int userId)
        {
            var result = await _dbContext.GetListByQueryAsync<BranchUserModel>($@"Select Distinct BranchUserID,B.BranchName,B.BranchID,CanAccess
                From viBranch B
                LEFT JOIN BranchUser BU on BU.UserID ={userId} and BU.BranchID = B.BranchID and  BU.IsDeleted=0 
                Where ClientID={CurrentClientID}", null);

            return Ok(result);
        }

        #region User Reports

        //[HttpGet("get-user-reports")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        //public async Task<IActionResult> ReadUserReports()
        //{
        //    string whereCondition = "";
        //    if (CurrentUserTypeID > (int)UserTypes.Client)
        //    {
        //        whereCondition = $@" and ReportID in (Select ReportID 
								//						From UserReportAccess
								//						Where UserID={CurrentUserID} and IsDeleted=0)";
        //    }
        //    var reports = await _dbContext.GetListByQueryAsync<ReportsModel>($@"Select R.ReportID,R.ReportName
							 //                                                   From Report R
        //                                                                        Where R.IsDeleted = 0 {whereCondition}");
        //    return Ok(reports);
        //}


        [HttpGet("get-branches-as-idvaluepair")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetBranchesAsIdnValuePair()
        {
            var branches = await _dbContext.GetListByQueryAsync<IdnValuePair>($@"Select B.BranchID as ID,B.BranchName as Value
			                                                                    From viBranch B
			                                                                    Left Join BranchUser BU on BU.BranchID=B.BranchID and BU.IsDeleted=0
			                                                                    Where ClientID={CurrentClientID}", null);

            return Ok(branches);
        }

        #endregion


        #region Client User Account Edit

        [HttpGet("get-account-details")]
        public async Task<IActionResult> GetUserEditDetails()
        {
            var result = await _dbContext.GetByQueryAsync<AccountDetailsModel>($@"Select E.EntityID,U.UserID,U.ClientID,EntityTypeID,Phone,EmailAddress,UserName,AddressLine1,AddressLine2,AddressLine3,CountryName,StateName,PinCode,
                                                                                    ContactPerson as ContactPersonName,ContactNo as ContactPersonPhone,ContactEmail as ContactPersonEmail,M.MediaID,
                                                                                    FileName,EntityPersonalInfoID,UserTypeID,EntityInstituteInfoID,CL.GSTNo,
                                                                                    Case When UserTypeID=4 then EI.Name else EP.FirstName end as CompanyName,EA.AddressID,EA.CountryID,EA.StateID
                                                                                    From Entity E
                                                                                    Left Join Users U on U.EntityID=E.EntityID and U.IsDeleted=0
                                                                                    Left Join Client CL on CL.EntityID=U.EntityID and CL.IsDeleted=0
                                                                                    Left Join EntityInstituteInfo EI on EI.EntityID=U.EntityID and EI.IsDeleted=0
                                                                                    Left Join EntityPersonalInfo EP on EP.EntityID=U.EntityID and EP.IsDeleted=0
                                                                                    Left Join EntityAddress EA on EA.EntityID=EI.EntityID and EA.IsDeleted=0
                                                                                    Left Join Country C on C.CountryID=EA.CountryID and C.IsDeleted=0
                                                                                    Left Join CountryState S on S.StateID=EA.StateID and S.IsDeleted=0
                                                                                    Left Join Media M on M.MediaID=E.MediaID
                                                                                    Where E.EntityID={CurrentEntityID} and E.IsDeleted=0", null);
            return Ok(result??new());
        }

        [HttpPost("save-account-details")]
        public async Task<IActionResult> UpdateClientUserAccount(AccountDetailsModel model)
        {
            _cn.Open();
            using (var tran = _cn.BeginTransaction())
            {
                try
                {
                    if (model.UserTypeID == (int)UserTypes.Client)
                    {
                        var EntityAddress = new EntityAddressCustom()
                        {
                            EntityID = model.EntityID,
                            AddressID = Convert.ToInt32(model.AddressID),
                            AddressLine1 = model.AddressLine1,
                            AddressLine2 = model.AddressLine2,
                            AddressLine3 = model.AddressLine3,
                            AddressType = (int)AddressTypes.Office,
                            CountryID=model.CountryID,
                            StateID=model.StateID,
                            Pincode=model.PinCode,
                            
                        };
                        model.AddressID = await _dbContext.SaveAsync(EntityAddress, tran);


                        await _dbContext.ExecuteAsync($"Update Entity Set Phone=@Phone,EmailAddress=@EmailAddress,MediaID=@MediaID Where EntityID=@EntityID", model, tran);
                        await _dbContext.ExecuteAsync($"Update Users Set UserName=@UserName Where EntityID=@EntityID", model, tran);
                        await _dbContext.ExecuteAsync($"Update EntityInstituteInfo Set Name=@CompanyName,ContactPerson=@ContactPersonName,ContactNo=@ContactPersonPhone,ContactEmail=@ContactPersonEmail Where EntityID=@EntityID", model, tran);
                        await _dbContext.ExecuteAsync($"Update Client Set GSTNo=@GSTNo Where EntityID=@EntityID", model, tran);

                    }
                    else
                    {

                        await _dbContext.ExecuteAsync($"Update Entity Set Phone=@Phone,EmailAddress=@EmailAddress,MediaID=@MediaID Where EntityID=@EntityID", model, tran);
                        await _dbContext.ExecuteAsync($"Update Users Set UserName=@UserName Where EntityID=@EntityID", model, tran);
                        await _dbContext.ExecuteAsync($"Update EntityPersonalInfo Set FirstName=@CompanyName Where EntityID=@EntityID", model, tran);

                    }

                    tran.Commit();
                    return Ok(new Success());
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
    }
}
