using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PB.DatabaseFramework;
using PB.Model;
using PB.Shared.Enum;
using PB.Shared;
using System.Data;
using PB.Shared.Models;
using PB.CRM.Model;
using PB.Shared.Models;
using PB.Shared.Models.Common;
using PB.EntityFramework;

namespace PB.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BranchController : BaseController
    {
        private readonly IDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IDbConnection _cn;
        public BranchController(IDbContext dbContext, IMapper mapper, IDbConnection cn)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _cn = cn;
        }

        [HttpPost("save-branch")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "BranchesAdd,BranchesEdit")]
        public async Task<IActionResult> SaveBranch(BranchModel model)
        {
            var res = await _dbContext.GetByQueryAsync<int>(@$"Select Count(*) from viBranch
            Where BranchName=@BranchName and ClientID={CurrentClientID} and BranchID<>{model.BranchID}", model);
            if (res > 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "Something went wrong",
                    ResponseMessage = "Branch Name already exist, try different name"
                }) ;
            }

            var contactPersonNumer = await _dbContext.GetByQueryAsync<string>(@$"
                                Select Phone 
                                From Entity E
                                Where Phone='{model.ContactPersonMobile}' and IsDeleted=0
            ", null);

            if (!string.IsNullOrEmpty(contactPersonNumer))
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "Invalid Submission",
                    ResponseMessage = "Provided contact person phone number is already in use, try different one"
                });
            }

            model.ClientID = CurrentClientID;
            _cn.Open();
            using (var tran = _cn.BeginTransaction())
            {
                try
                {
                    var logSummaryID = await _dbContext.InsertAddEditLogSummary(model.BranchID, "Branch " + model.BranchName, tran);

                    var entity = _mapper.Map<Entity>(model);
                    entity.EntityTypeID = (int)EntityType.Branch;
                    model.EntityID = await _dbContext.SaveAsync(entity, tran, logSummaryID: logSummaryID);

                    var entityInstituteInfo = _mapper.Map<EntityInstituteInfo>(model);
                    entityInstituteInfo.Name = model.BranchName;
                    entityInstituteInfo.ContactNo = model.ContactPersonMobile;
                    await _dbContext.SaveAsync(entityInstituteInfo, tran, logSummaryID: logSummaryID);

                    var entityAddress = _mapper.Map<EntityAddress>(model);
                    await _dbContext.SaveAsync(entityAddress, tran, logSummaryID: logSummaryID);

                    var branch = _mapper.Map<Branch>(model);
                    branch.BranchID = await _dbContext.SaveAsync(branch, tran, logSummaryID: logSummaryID);
                    tran.Commit();
                    return Ok(new BranchAddResultModel() { BranchID=branch.BranchID,BranchName= model.BranchName });
                }
                catch (Exception err)
                {
                    tran.Rollback();
                    return BadRequest(new DbError(err.Message));
                }
            }
        }

        [HttpPost("get-all-branches")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Branches")]
        public async Task<IActionResult> GetAllBranches(PagedListPostModelWithFilter searchData)
        {
            PagedListQueryModel query = searchData;
            query.Select = $@"Select B.BranchID as BranchID,V.Name as BranchName,V.Phone,V.EmailAddress
                              From Branch B
							  LEFT JOIN viEntity V on V.EntityID=B.EntityID
                              LEFT JOIN EntityInstituteInfo I on I.EntityID=V.EntityID and I.IsDeleted=0";
            query.WhereCondition = $"B.IsDeleted=0 and B.ClientID={CurrentClientID}";
            query.SearchLikeColumnNames = new() { "BranchName" };
            query.OrderByFieldName = "V.Name";
            var result = await _dbContext.GetPagedList<BranchListModel>(query, null);
            return Ok(result);
        }

        [HttpGet("get-branch-details/{branchid}")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Branches")]
        public async Task<IActionResult> GetBranch(int branchid)
        {
            var res = await _dbContext.GetByQueryAsync<BranchModel>($@"Select B.BranchID,EA.AddressID,V.EntityID,I.Name as BranchName,EA.State,
                                                                       EmailAddress,Phone,Phone2,AddressLine1 ,AddressLine2 ,ContactEmail,ContactPerson,ContactNo as ContactPersonMobile,Pincode,
                                                                       City,B.StateID,B.CountryID,EntityInstituteInfoID,C.CountryName,S.StateName
                                                                       From Branch B
                                                                       LEFT JOIN Entity V on V.EntityID=B.EntityID and V.IsDeleted=0
																	   LEFT JOIN EntityInstituteInfo I on I.EntityID=V.EntityID and I.IsDeleted=0
                                                                       Left Join EntityAddress EA on EA.EntityID=B.EntityID and EA.IsDeleted=0
                                                                       Left Join Country C ON C.CountryID=B.CountryID and C.IsDeleted=0
                                                                       Left Join State S ON S.StateID=B.StateID and S.IsDeleted=0
                                                                       Where B.BranchID={branchid} and B.IsDeleted=0", null);
            return Ok(res);
        }

        [HttpGet("delete-branch/{branchid}")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "BranchesDelete")]
        public async Task<IActionResult> Delete(int branchid)
        {

            var branch = await _dbContext.GetAsync<Branch>(branchid);

            _cn.Open();
            using (var tran = _cn.BeginTransaction())
            {
                try
                {
                    var logSummary = await _dbContext.InsertDeleteLogSummary(branchid, "Branch Deleted by : " + CurrentUserID, tran);
                    await _dbContext.DeleteAsync<Entity>(branch.EntityID, tran, logSummary);
                    await _dbContext.DeleteSubItemsAsync<EntityAddress>("EntityID", branch.EntityID, tran, logSummary);
                    await _dbContext.DeleteSubItemsAsync<EntityInstituteInfo>("EntityID", branch.EntityID, tran, logSummary);
                    await _dbContext.DeleteAsync<Branch>(branchid, tran, logSummary);

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

        [HttpGet("get-branch-menu-list")]
        public async Task<IActionResult> GetBranchMenuList()
        {
            var result = await _dbContext.GetListByQueryAsync<PB.Model.Models.ViewPageMenuModel>($@"Select BranchID as ID, V.Name as MenuName 
                                                                                    From Branch B
                                                                                    Join viEntity V on V.EntityID=B.EntityID
																					Join Client C ON C.ClientID=B.ClientID
                                                                                    Where B.IsDeleted=0 and C.ClientID={CurrentClientID}", null);
            return Ok(result);
        }


    }
}
