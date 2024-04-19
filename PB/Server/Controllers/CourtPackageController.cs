using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PB.Shared.Models;
using PB.Model;
using PB.Server.Repository;
using PB.Shared;
using PB.Shared.Models.Court;
using PB.Shared.Tables.CourtClient;
using System.Data;
using PB.Model.Models;
using Microsoft.IdentityModel.Tokens;
using PB.EntityFramework;

namespace PB.Server.Controllers
{
    [Route("api/court-package")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CourtPackageController : BaseController
    {
        private readonly IDbContext _dbContext;
        private readonly IDbConnection _cn;
        private readonly IMapper _mapper;
        private readonly ICourtPackageRepository _court;

        public CourtPackageController(IDbContext dbContext, IDbConnection cn, IMapper mapper, ICourtPackageRepository court)
        {

            _dbContext = dbContext;
            _cn = cn;
            _mapper = mapper;
            _court = court;
        }

        #region Court Package
        [HttpPost("save-court-package")]
        public async Task<IActionResult> SaveCourtPackage(CourtPackageModel model)
        {
            var Count = await _dbContext.GetByQueryAsync<int>(@$"Select Count(*) 
                                                                    from CourtPackage
                                                                    where LOWER(TRIM(PackageName))=LOWER(TRIM(@PackageName)) and CourtPackageID<>@CourtPackageID  and IsDeleted=0", model);
            if (Count != 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "Package Name already exist",
                    ResponseMessage = "The PackageName already exist,try different one"
                });
            }

            CourtPackage Package = new()
            {
                CourtPackageID = model.CourtPackageID,
                PackageName = model.PackageName,
                ValidityMonth = model.ValidityMonth,
                TotalHours = model.TotalHours,
                Fee = model.Fee,
                TaxCategoryID = model.TaxCategoryID,
                IncTax = model.IncTax,
                ClientID = CurrentClientID,
            };
            await _dbContext.SaveAsync(Package);
            return Ok(new Success());

        }


        [HttpGet("get-court-package/{PackageId}")]
        public async Task<IActionResult> GetPackage(int PackageId)
        {
            var res = await _dbContext.GetByQueryAsync<CourtPackageModel>($@"Select CP.*,TaxCategoryName 
                                                                                From CourtPackage CP
                                                                                Left Join TaxCategory TC on TC.TaxCategoryID=CP.TaxCategoryID and TC.IsDeleted=0
                                                                                Where CourtPackageID={PackageId} and CP.IsDeleted=0", null);

            return Ok(res ?? new());
        }


        [HttpPost("get-all-court-packages")]
        public async Task<IActionResult> GetPackageList(PagedListPostModelWithFilter model)
        {
            PagedListQueryModel query = model;

            query.Select = $@"Select * From CourtPackage";

            query.WhereCondition = $"IsDeleted=0";
            query.OrderByFieldName = model.OrderByFieldName;
            if (!model.SearchString.IsNullOrEmpty())
            {
                query.WhereCondition += $" and PackageName like '%{model.SearchString}%'";
            }

            var res = await _dbContext.GetPagedList<CourtPackageModel>(query,null);
            return Ok(res);
        }


        [HttpGet("get-court-package-view/{PackageID}")]
        public async Task<IActionResult> GetCourtPackageView(int PackageID)
        {
            var res = await _dbContext.GetByQueryAsync<CourtPackageViewModel>($@"Select CP.*,TaxCategoryName
                                                                                From CourtPackage CP
                                                                                LEFT JOIN TaxCategory TC ON TC.TaxCategoryID=CP.TaxCategoryID and TC.IsDeleted=0
                                                                                Where CP.CourtPackageID={PackageID} and CP.IsDeleted=0", null);
            return Ok(res);
        }

        [HttpGet("get-court-package-menu-list")]
        public async Task<IActionResult> GetMenuList()
        {
            var result = await _dbContext.GetListByQueryAsync<ViewPageMenuModel>($@"Select MP.CourtPackageID as ID,MP.PackageName as MenuName
                                                                                    From CourtPackage MP
                                                                                    Where MP.IsDeleted=0", null);
            return Ok(result);
        }

        [HttpGet("delete-court-package")]
        public async Task<IActionResult> DeletePackage(int Id)
        {
            await _dbContext.DeleteAsync<CourtPackage>(Id);
            return Ok(true);
        }
        #endregion

        #region Court package purchase

        [HttpGet("check-purchase-code-availability/{purchaseCode}")]
        public async Task<IActionResult> CheckPurchaseCodeAvailability(string purchaseCode)
        {
            var res = await _dbContext.GetAsync<CourtPackagePurchase>($"PurchaseCode=@purchaseCode and IsDeleted=0",new { purchaseCode});
            return Ok((res!=null)?false:true);
        }

        [HttpPost("save-court-customer-package")]
        public async Task<IActionResult> SaveCourtCustomerPackage(CourtPackagePurchaseModel model)
        {
            CustomerCourtPackageDetailsModel InsertModel = new()
            {
                ClientID = CurrentClientID,
                CustomerEntityID = model.EntityID.Value,
                PackageID = model.CourtPackageID.Value,
                StartDate=model.StartDate,
                EndDate=model.EndDate,
                PaymentTypeID= model.PaymentTypeID.Value,
                PurchaseCode=model.PurchaseCode
            };

            await _court.HandleCustomerCourtPackageAccountsPurchaseEntries(InsertModel);
            return Ok(new Success());
        }

        [HttpGet("get-court-customer-existing-package/{EntityID}")]
        public async Task<IActionResult> GetExisting(int EntityID)
        {
            var res = await _dbContext.GetByQueryAsync<ExistingCourtPackagePurchaseModel>($@"Select PackageName as ExistingPackageName,EndDate as ExistingEndDate,Fee as ExistingFee
                                                                                From CourtPackagePurchase CPP
                                                                                Join CourtPackage CP on CP.CourtPackageID=CPP.CourtPackageID and CP.IsDeleted=0
                                                                                JOIN Customer C on C.EntityID=CPP.EntityID and C.IsDeleted=0
                                                                                Join viEntity E on E.EntityID=C.EntityID
                                                                                Where CPP.EntityID={EntityID} and EndDate>=GETUTCDATE() and CPP.IsDeleted=0", null);
            return Ok(res??new());
        }

        [HttpPost("get-package-validity")]
        public async Task<IActionResult> GetValidity(IntModel model)
        {
            var res = await _dbContext.GetByQueryAsync<IntModel>($@"Select ValidityMonth as Id From CourtPackage
                                                                                            Where CourtPackageID={model.Id} and IsDeleted=0", null);
            return Ok(res ?? new());
        }
        #endregion

    }
}
