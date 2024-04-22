using AutoMapper;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PB.Client.Pages.SuperAdmin;
using PB.DatabaseFramework;
using PB.EntityFramework;
using PB.Model;
using PB.Model.Models;
using PB.Server.Repository;
using PB.Shared;
using PB.Shared.Enum;
using PB.Shared.Helpers;
using PB.Shared.Models;
using PB.Shared.Models.Common;
using PB.Shared.Models.SuperAdmin.Client;
using PB.Shared.Models.SuperAdmin.ClientInvoice;
using PB.Shared.Tables;
using System.Data;
using System.Runtime.InteropServices;

namespace PB.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class SupportController : BaseController
    {
        private readonly IDbContext _dbContext;
        private readonly IDbConnection _cn;
        private readonly IMapper _mapper;
        private readonly IUserRepository _user;
        private readonly IAccountRepository _accounts;
        private readonly ICRMRepository _crm;
        private readonly IBackgroundJobClient _job;
        private readonly ISuperAdminRepository _supperAdmin;
        private readonly IPDFRepository _pdf;
        private readonly ICommonRepository _common;
        private readonly IInventoryRepository _inventory; 
        public SupportController(IDbContext dbContext, IDbConnection cn, IMapper mapper, IUserRepository user, IAccountRepository account, ICRMRepository crm, IBackgroundJobClient job, ISuperAdminRepository superAdmin, IPDFRepository pdf, ICommonRepository common, IInventoryRepository inventory)
        {
            _dbContext = dbContext;
            _cn = cn;
            _mapper = mapper;
            _user = user;
            _accounts = account;
            _crm = crm;
            _job = job;
            _supperAdmin = superAdmin;
            _pdf = pdf;
            _common = common;
            _inventory = inventory;
        }

        #region MembershiFeature

        [HttpPost("save-feature")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "MembershipManagment")]
        public async Task<IActionResult> SaveFeature(MembershipFeatureModel model)
        {
            var FeatureName = await _dbContext.GetByQueryAsync<string?>(@$"Select FeatureName 
                                                                    from MembershipFeature
                                                                    where LOWER(FeatureName) =LOWER(@FeatureName) and FeatureID<>@FeatureID  and IsDeleted=0", model);
            if (FeatureName != null)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "feature name already exist",
                    ResponseMessage = "The feature name already exist,try different feature name"
                });
            }
            var feature = _mapper.Map<Shared.Tables.MembershipFeature>(model);
            await _dbContext.SaveAsync(feature);
            return Ok(new Success());
        }

        [HttpPost("get-all-feature")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "MembershipManagment")]
        public async Task<IActionResult> GetFeatureList(PagedListPostModelWithFilter model)
        {
            PagedListQueryModel query = model;

            query.Select = $@"Select FeatureID,FeatureName
                                    From MembershipFeature ";

            query.WhereCondition = $"IsDeleted=0 ";
            query.OrderByFieldName = model.OrderByFieldName;
            if (!model.SearchString.IsNullOrEmpty())
            {
                query.WhereCondition += $" and FeatureName like '%{model.SearchString}%'";
            }
            var res = await _dbContext.GetPagedList<MembershipFeatureModel>(query, null);
            return Ok(res);
        }

        [HttpGet("get-feature/{FeatureId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "MembershipManagment")]
        public async Task<IActionResult> GetFeature(int FeatureId)
        {
            var res = await _dbContext.GetByQueryAsync<Shared.Tables.MembershipFeature>($@"Select FeatureID,FeatureName,Description,MF.MediaID
                                                                                From MembershipFeature MF
                                                                                LEFT JOIN Media M ON MF.MediaID = M.MediaID
                                                                                Where MF.FeatureID={FeatureId} and MF.IsDeleted=0", null);
            return Ok(res ?? new());
        }

        [HttpGet("delete-feature")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "MembershipManagment")]
        public async Task<IActionResult> DeleteFeature(int Id)
        {
            int? mediaID = await _dbContext.GetByQueryAsync<int?>($@"Select MediaID
                                                                                From MembershipFeature                                                                            
                                                                                Where FeatureID ={Id} and IsDeleted = 0", null);
            if (mediaID != null)
            {
                await _dbContext.DeleteAsync<Media>(mediaID.Value);
            }

            await _dbContext.DeleteAsync<Shared.Tables.MembershipFeature>(Id);
            return Ok(true);
        }
        #endregion

        #region MembershipCapacity
        [HttpPost("save-capacity")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "MembershipManagment")]
        public async Task<IActionResult> SaveCapacity(MembershipUserCapacityModel model)
        {
            var PlanName = await _dbContext.GetByQueryAsync<string?>(@$"Select Capacity 
                                                                    from MembershipUserCapacity
                                                                    where LOWER(Capacity) =LOWER(@Capacity) and CapacityID<>@CapacityID  and IsDeleted=0", model);
            if (PlanName != null)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "Capacity already exist",
                    ResponseMessage = "The Capacity already exist"
                });
            }
            var capacity = _mapper.Map<MembershipUserCapacity>(model);
            capacity.CapacityID = await _dbContext.SaveAsync(capacity);

            return Ok(new UserCapacityAddResultModel() { CapacityID = capacity.CapacityID, Capacity = capacity.Capacity });
        }

        [HttpPost("get-all-capacity")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "MembershipManagment")]
        public async Task<IActionResult> GetCapacityList(PagedListPostModelWithFilter model)
        {
            PagedListQueryModel query = model;

            query.Select = $@"Select CapacityID,Capacity
                                        From MembershipUserCapacity ";

            query.WhereCondition = $"IsDeleted=0 ";
            query.OrderByFieldName = model.OrderByFieldName;
            if (!model.SearchString.IsNullOrEmpty())
            {
                query.WhereCondition += $" and Capacity like '%{model.SearchString}%'";
            }
            var res = await _dbContext.GetPagedList<MembershipUserCapacityModel>(query, null);
            return Ok(res);
        }

        [HttpGet("get-capacity/{CapacityId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "MembershipManagment")]
        public async Task<IActionResult> GetCapacity(int CapacityId)
        {
            var res = await _dbContext.GetByQueryAsync<MembershipUserCapacity>($@"Select CapacityID,Capacity
                                                                                    From MembershipUserCapacity
                                                                                    Where CapacityID={CapacityId} and IsDeleted=0", null);
            return Ok(res ?? new());
        }

        [HttpGet("delete-capacity")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "MembershipManagment")]
        public async Task<IActionResult> DeleteCapacity(int Id)
        {
            await _dbContext.DeleteAsync<MembershipUserCapacity>(Id);
            return Ok(true);
        }

        #endregion

        #region MembershipPlan

        [HttpPost("save-plan")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "MembershipManagment")]
        public async Task<IActionResult> SavePlan(MembershipPlanModel model)
        {
            var PlanName = await _dbContext.GetByQueryAsync<string?>(@$"Select PlanName 
                                                                    from MembershipPlan
                                                                    where LOWER(PlanName) =LOWER(@PlanName) and PlanID<>@PlanID  and IsDeleted=0", model);
            if (PlanName != null)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "Plan name already exist",
                    ResponseMessage = "The plan name already exist,try different plan name"
                });
            }
            var plan = _mapper.Map<Shared.Tables.MembershipPlan>(model);
            plan.PlanID = await _dbContext.SaveAsync(plan);
            return Ok(new MembershipPlanAddResultModel() { PlanID = plan.PlanID, PlanName = plan.PlanName });
        }

        [HttpPost("get-all-plan")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "MembershipManagment")]
        public async Task<IActionResult> GetPlanList(PagedListPostModelWithFilter model)
        {
            PagedListQueryModel query = model;

            query.Select = $@"Select PlanID,PlanName,MonthCount
                                    From MembershipPlan";

            query.WhereCondition = $"IsDeleted=0 ";
            query.OrderByFieldName = model.OrderByFieldName;
            if (!model.SearchString.IsNullOrEmpty())
            {
                query.WhereCondition += $" and PlanName like '%{model.SearchString}%'";
            }
            var res = await _dbContext.GetPagedList<MembershipPlanModel>(query, null);
            return Ok(res);
        }

        [HttpGet("get-plan/{PlanId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "MembershipManagment")]
        public async Task<IActionResult> GetPlan(int PlanId)
        {
            var res = await _dbContext.GetByQueryAsync<Shared.Tables.MembershipPlan>($@"Select PlanID,PlanName,MonthCount
                                                                                        From MembershipPlan
                                                                                        Where PlanID={PlanId} and IsDeleted=0", null);
            return Ok(res ?? new());
        }

        [HttpGet("delete-plan")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "MembershipManagment")]
        public async Task<IActionResult> DeletePlan(int Id)
        {
            await _dbContext.DeleteAsync<Shared.Tables.MembershipPlan>(Id);
            return Ok(true);
        }
        #endregion

        #region MembershipFee

        [HttpPost("save-fee")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "MembershipManagment")]
        public async Task<IActionResult> SaveFee(MembershipFeeModel model)
        {
            var fee = _mapper.Map<MembershipFee>(model);
            await _dbContext.SaveAsync(fee);
            return Ok(new Success());
        }
        [HttpPost("get-all-fee")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "MembershipManagment")]
        public async Task<IActionResult> GetFeeList(PagedListPostModelWithFilter model)
        {
            PagedListQueryModel query = model;

            query.Select = $@"Select F.FeeID,F.Fee,F.ComboFee,P.PlanID,P.PlanName,MF.FeatureID,MF.FeatureName,C.CapacityID,C.Capacity
                                    From MembershipFee F 
                                    LEFT JOIN MembershipFeature MF ON MF.FeatureID = F.FeatureID
                                    LEFT JOIN MembershipUserCapacity C ON C.CapacityID = F.CapacityID
                                    LEFT JOIN MembershipPlan P ON P.PlanID= F.PlanID";

            query.WhereCondition = $"F.IsDeleted=0 ";
            query.OrderByFieldName = model.OrderByFieldName;
            var res = await _dbContext.GetPagedList<MembershipFeeModel>(query, null);
            return Ok(res);
        }

        [HttpGet("get-fee/{FeeId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "MembershipManagment")]
        public async Task<IActionResult> GetFee(int FeeId)
        {
            var res = await _dbContext.GetByQueryAsync<MembershipFeeModel>($@"Select F.FeeID,F.Fee,F.ComboFee,P.PlanID,P.PlanName,MF.FeatureID,MF.FeatureName,C.CapacityID,C.Capacity
                                                                                        From MembershipFee F 
                                                                                        LEFT JOIN MembershipFeature MF ON MF.FeatureID = F.FeatureID
                                                                                        LEFT JOIN MembershipUserCapacity C ON C.CapacityID = F.CapacityID
                                                                                        LEFT JOIN MembershipPlan P ON P.PlanID= F.PlanID
                                                                                        Where F.FeeID={FeeId} and F.IsDeleted=0", null);
            return Ok(res ?? new());
        }

        [HttpGet("delete-fee")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "MembershipManagment")]
        public async Task<IActionResult> Deletefee(int Id)
        {
            await _dbContext.DeleteAsync<MembershipFee>(Id);
            return Ok(true);
        }
        #endregion

        #region Membership Packages

        [HttpPost("get-all-packages")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "MembershipManagment")]
        public async Task<IActionResult> GetPackageList(PagedListPostModelWithFilter model)
        {
            PagedListQueryModel query = model;

            query.Select = $@"Select MP.*,Capacity,PlanName,TaxCategoryName
                                From MembershipPackage MP
                                LEFT JOIN MembershipUserCapacity C ON C.CapacityID = MP.CapacityID and C.IsDeleted=0
                                LEFT JOIN MembershipPlan P ON P.PlanID= MP.PlanID and P.IsDeleted=0
                                LEFT JOIN TaxCategory TC ON TC.TaxCategoryID=MP.TaxCategoryID and TC.IsDeleted=0";

            query.WhereCondition = $"MP.IsDeleted=0 ";
            query.OrderByFieldName = model.OrderByFieldName;
            if (!model.SearchString.IsNullOrEmpty())
            {
                query.WhereCondition += $" and PackageName like '%{model.SearchString}%'";
            }

            var res = await _dbContext.GetPagedList<MembershipPackageModel>(query, null);
            return Ok(res);
        }


        [HttpGet("get-package/{PackageId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "MembershipManagment")]
        public async Task<IActionResult> GetPackage(int PackageId)
        {
            var res = await _dbContext.GetByQueryAsync<MembershipPackageModel>($@"Select MP.*,Capacity,PlanName,TaxCategoryName,FileName
                                                                            From MembershipPackage MP
                                                                            LEFT JOIN MembershipUserCapacity C ON C.CapacityID = MP.CapacityID and C.IsDeleted=0
                                                                            LEFT JOIN MembershipPlan P ON P.PlanID= MP.PlanID and P.IsDeleted=0
                                                                            LEFT JOIN TaxCategory TC ON TC.TaxCategoryID=MP.TaxCategoryID and TC.IsDeleted=0
                                                                            LEFT JOIN Media M on M.MediaID=MP.MediaID and M.IsDeleted=0
                                                                            Where MP.PackageID={PackageId} and MP.IsDeleted=0", null);
            res.feature = await _dbContext.GetListByQueryAsync<IdnValuePair>($@"Select MF.FeatureID as ID,FeatureName as Value
                                                                                From MembershipPackageFeature MPF
                                                                                Left Join MembershipFeature MF on MF.FeatureID=MPF.FeatureID and MF.IsDeleted=0
                                                                                Where MPF.PackageID={PackageId} and MPF.IsDeleted=0", null);
            res.PackageRoleList = await _dbContext.GetListByQueryAsync<PackageRoleModel>($@"Select R.RoleID, RoleGroupID,RoleName,PackageRoleID
                                                                                            From Role R
                                                                                            Left Join MembershipPackageRole PR on PR.RoleID=R.RoleID and PR.IsDeleted=0
                                                                                            Where PackageID={PackageId} and R.IsDeleted=0", null);


            return Ok(res ?? new());
        }


        [HttpGet("delete-package")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "MembershipManagment")]
        public async Task<IActionResult> DeletePackage(int Id)
        {
            await _dbContext.DeleteAsync<MembershipPackage>(Id);
            return Ok(true);
        }

        [HttpGet("get-all-features")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetFeatures()
        {
            var res = new List<IdnValuePair>();
            res = await _dbContext.GetListByQueryAsync<IdnValuePair>($@"Select FeatureID as ID,FeatureName as Value From MembershipFeature
                                                                                    Where IsDeleted=0", null);
            return Ok(res ?? new());
        }


        [HttpPost("save-package")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "MembershipManagment")]
        public async Task<IActionResult> Savepackage(MembershipPackageModel packageModel)
        {
            var PackageName = await _dbContext.GetByQueryAsync<string?>(@$"Select PackageName 
                                                                    from MembershipPackage
                                                                    where LOWER(PackageName) =LOWER(@PackageName) and PackageID<>@PackageID  and IsDeleted=0", packageModel);
            if (PackageName != null)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "package name already exist",
                    ResponseMessage = "The package name already exist,try different package name"
                });
            }

            _cn.Open();
            using(var tran = _cn.BeginTransaction())
            {
                try
                {
                    //Checking package updation and updation related works
                    if (packageModel.PackageID > 0)
                    {
                        var oldPackageRoleGroups = await _supperAdmin.FetchPackageRoleGroups(packageModel.PackageID , tran);
                        var updatedPackageRoleGroups = packageModel.PackageRoleList.Select(item => item.RoleGroupID).ToList();
                        if (oldPackageRoleGroups is not null && updatedPackageRoleGroups is not null)
                        {
                            if (!oldPackageRoleGroups.Contains((int)RoleGroups.AccountsManagement) && updatedPackageRoleGroups.Contains((int)RoleGroups.AccountsManagement))
                            {
                                //fetch all clients having this package and add accounts related entries
                                var clients = await _dbContext.GetListAsync<ClientCustom>($"PackageID={packageModel.PackageID} AND IsDeleted=0", null, tran);
                                foreach (var client in clients)
                                {
                                    await _accounts.InsertClientDefaultAccountsRelatedEntries(client.ClientID, tran);
                                }
                            }

                            if (!oldPackageRoleGroups.Contains((int)RoleGroups.InventoryManagement) && updatedPackageRoleGroups.Contains((int)RoleGroups.InventoryManagement))
                            {
                                //fetch all clients having this package and add inventory related entries
                                var clients = await _dbContext.GetListAsync<ClientCustom>($"PackageID={packageModel.PackageID} AND IsDeleted=0", null, tran);
                                foreach (var client in clients)
                                {
                                    await _inventory.InsertClientInventoryDefaultEntries(client.ClientID, tran);
                                }
                            }
                        }
                    }

                    //Membership package
                    var package = _mapper.Map<MembershipPackage>(packageModel);
                    package.PackageID = await _dbContext.SaveAsync(package, tran);

                    //Membership package features
                    List<MembershipPackageFeature> packageFeature = new();
                    foreach (var feature in packageModel.feature)
                    {
                        MembershipPackageFeature Features = new()
                        {
                            FeatureID = feature.ID,
                        };
                        packageFeature.Add(Features);

                    }
                    await _dbContext.SaveSubItemListAsync(packageFeature, "PackageID", package.PackageID, tran);

                    //Membership package roles
                    List<MembershipPackageRole> packageRoleList = new();
                    foreach (var roles in packageModel.PackageRoleList)
                    {
                        MembershipPackageRole role = new()
                        {
                            PackageRoleID = roles.PackageRoleID,
                            PackageID = package.PackageID,
                            RoleID = roles.RoleID,
                        };
                        packageRoleList.Add(role);
                    }
                    await _dbContext.SaveSubItemListAsync(packageRoleList, "PackageID", package.PackageID, tran);
                    tran.Commit();
                    return Ok(new Success());
                }
                catch(Exception e)
                {
                    tran.Rollback();
                    return BadRequest(new BaseErrorResponse() { ResponseMessage=e.Message });
                }
            }
        }


        [HttpGet("get-package-view/{PackageID}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "MembershipManagment")]
        public async Task<IActionResult> GetPackageView(int PackageID)
        {
            var res = await _dbContext.GetByQueryAsync<MembershipPackageViewModel>($@"Select MP.*,Capacity,PlanName,TaxCategoryName
                                                                                    From MembershipPackage MP
                                                                                    LEFT JOIN MembershipUserCapacity C ON C.CapacityID = MP.CapacityID and C.IsDeleted=0
                                                                                    LEFT JOIN MembershipPlan P ON P.PlanID= MP.PlanID and P.IsDeleted=0
                                                                                    LEFT JOIN TaxCategory TC ON TC.TaxCategoryID=MP.TaxCategoryID and TC.IsDeleted=0
                                                                                    Where MP.PackageID={PackageID} and MP.IsDeleted=0", null);
            res.Features = await _dbContext.GetListByQueryAsync<Shared.Tables.MembershipFeature>($@"Select MF.*
                                                                                From MembershipPackageFeature MPF
                                                                                Left Join MembershipFeature MF on MF.FeatureID=MPF.FeatureID and MF.IsDeleted=0
                                                                                Where MPF.PackageID={PackageID} and MPF.IsDeleted=0", null);

            string feature = "";
            foreach (var item in res.Features)
            {
                feature += item.FeatureName + ",";
            }
            feature = feature.TrimEnd(',');
            res.AvailableFeatures = feature;
            return Ok(res ?? new());
        }



        [HttpGet("get-package-menu-list")]
        public async Task<IActionResult> GetMenuList()
        {
            var result = await _dbContext.GetListByQueryAsync<ViewPageMenuModel>($@"Select MP.PackageID as ID,MP.PackageName as MenuName
                                                                                    From MembershipPackage MP
                                                                                    Where MP.IsDeleted=0", null);
            return Ok(result);
        }
        #endregion

        #region Admin Client

        [HttpPost("get-admin-client-paged-list")]
        public async Task<IActionResult> GetItems(PagedListPostModelWithFilter model)
        {
            PagedListQueryModel query = model;
            query.Select = $@"Select Name,Phone,EmailAddress,LoginStatus,B.PaymentStatus,B.InvoiceID,C.ClientID,C.EntityID,IsNull(IsBlock,0)IsBlock,BlockReason 
                            From Client C
			                Left Join (Select PaidStatus as PaymentStatus,A.InvoiceID,I.ClientID From(
		                    Select ClientID,Max(InvoiceID)as InvoiceID From ClientInvoice
		                    Where isDeleted=0 
		                    Group By ClientID) as A 
		                    Left Join Clientinvoice I on I.InvoiceID=A.InvoiceID 
		                    ) as B on B.ClientID=C.ClientID
		                    Left Join viEntity vE on vE.EntityID=C.EntityID 
			                Left Join Users U on U.EntityID=vE.EntityID and U.IsDeleted=0";
            query.WhereCondition = $"C.IsDeleted=0";

            query.OrderByFieldName = "C.ClientID desc";

            if (!model.SearchString.IsNullOrEmpty())
            {
                query.WhereCondition += $" and Name like '%{model.SearchString}%'";
            }

            var res = await _dbContext.GetPagedList<ClientListViewModel>(query, null);
            return Ok(res);
        }

        [HttpGet("get-client-menu-list")]
        public async Task<IActionResult> GetCustomerMenuList()
        {

            var result = await _dbContext.GetListByQueryAsync<ViewPageMenuModel>($@"Select C.ClientID as ID,E.Name as MenuName
                                                                                    From Client C
                                                                                    Join viEntity E ON E.EntityID=C.EntityID
                                                                                    Where C.IsDeleted=0
                                                                                    Order By 1 Desc
            ", null);

            return Ok(result);
        }

        [HttpGet("get-clients/{clientId}")]
        public async Task<IActionResult> GetClients(int clientId)
        {

            var client = await _dbContext.GetByQueryAsync<ClientViewModel>($@"Select C.ClientID,C.EntityID,Name,Phone,EmailAddress,LoginStatus,IsNull(IsBlock,0)IsBlock,BlockReason 
                                                                            From Client C 
                                                                            Join Users u on U.ClientID=C.ClientID
                                                                            Left Join viEntity E ON E.EntityID=C.EntityID
                                                                            Where C.IsDeleted=0 and C.ClientID={clientId}", null);
            //var client = await _dbContext.GetByQueryAsync<ClientViewModel>($@"Select Name,Phone,EmailAddress,LoginStatus,B.*,C.EntityID 
            //                                           From Client C
            //                                                       Left Join (Select PaidStatus as PaymentStatus,A.InvoiceID,I.ClientID From(
            //                                                          Select ClientID,Max(InvoiceID)as InvoiceID From ClientInvoice
            //                                                          Where isDeleted=0 
            //                                                          Group By ClientID) as A 
            //                                                          Left Join Clientinvoice I on I.InvoiceID=A.InvoiceID 
            //                                                          ) as B on B.ClientID=C.ClientID
            //                                                          Left Join viEntity vE on vE.EntityID=C.EntityID 
            //                                                       Left Join Users U on U.EntityID=vE.EntityID and U.IsDeleted=0
            //                                                                Where C.IsDeleted=0 and C.ClientID={clientId}");
            client.PlanList = await _dbContext.GetByQueryAsync<ClientMembershipPlanViewModel>($@"Select PackageName,PackageDescription,PlanName,MonthCount,Capacity,MP.Fee,MP.PackageID,ClientID
                                                                                                    InvoiceJournalMasterID,MP.PlanID,MP.CapacityID
                                                                                                    From Client CI
                                                                                                    Join MembershipPackage MP on MP.PackageID=CI.PackageID and MP.IsDeleted=0
                                                                                                    Join MembershipPlan MSP on MP.PlanID=MSP.PlanID and MSP.IsDeleted=0
                                                                                                    Join MembershipUserCapacity MC on MC.CapacityID=MP.CapacityID and MC.IsDeleted=0
                                                                                                    Where ClientID={clientId} and CI.IsDeleted=0", null);
            //foreach (var feature in client.PlanList)
            //{
            client.PlanList.membershipFeatures = await _dbContext.GetListByQueryAsync<Shared.Tables.MembershipFeature>($@"Select FeatureName,MF.FeatureID
                                                                                                        From Client CI
                                                                                                        Join MembershipPackage MP on MP.PackageID=CI.PackageID and MP.IsDeleted=0
                                                                                                        Join MembershipPackageFeature MPF on MPF.PackageID=MP.PackageID and MPF.IsDeleted=0
                                                                                                        Join MembershipFeature MF on MF.FeatureID=MPF.FeatureID  and MF.IsDeleted=0
                                                                                                        Where ClientID={clientId} and MP.PackageID={client.PlanList.PackageID} and CI.IsDeleted=0", null);
            string features = "";
            foreach (var item in client.PlanList.membershipFeatures)
            {
                features += item.FeatureName + ",";
            }
            features = features.TrimEnd(',');
            client.PlanList.FeaturesName = features;

            // }

            client.InvoiceData = await _dbContext.GetListByQueryAsync<ClientInvoiceListModel>($@"Select PackageName,MP.Fee,PaidStatus,DisconnectionDate,C.ClientID,InvoiceID,InvoiceNo,InvoiceDate
                                                                                                From Client C
                                                                                                Left Join ClientInvoice CI on C.ClientID=CI.ClientID and CI.IsDeleted=0 
                                                                                                Left Join MembershipPackage MP on CI.packageID=MP.PackageID and MP.IsDeleted=0
                                                                                                Where  C.Isdeleted=0 and C.ClientID={clientId}
                                                                                                Order by InvoiceID desc", null);


            return Ok(client ?? new());
        }

        [HttpGet("get-payment-details/{invoiceID}")]
        public async Task<IActionResult> GetPaymentDetails(int invoiceID)
        {
            var res = await _dbContext.GetByQueryAsync<PaymentVerificationViewModel>(@$"Select PaymentRefNo,C.MediaID,FileName,C.ClientID,InvoiceID
                                                                                        From ClientInvoice C
                                                                                        Left Join Media M on M.MediaID=C.MediaID and M.Isdeleted=0
                                                                                        Where C.InvoiceID={invoiceID} and C.IsDeleted=0", null);
            return Ok(res ?? new());
        }



        [HttpPost("block-client")]
        public async Task<IActionResult> BlockClient(IdnValuePair model)
        {
            var res = await _dbContext.ExecuteAsync($@"UPDATE Client SET IsBlock=1,BlockReason=@BlockReason Where ClientID=@ClientID", new { BlockReason = model.Value , ClientID  = model.ID });
            return Ok(new Success());
        }

        [HttpGet("unblock-client/{clientID}")]
        public async Task<IActionResult> BlockClient(int clientID)
        {
            var res = await _dbContext.ExecuteAsync($@"UPDATE Client SET IsBlock=0,BlockReason=NULL Where ClientID={clientID}");
            return Ok(new Success());
        }

        #endregion

        #region Client

        [HttpPost("add-client")]
        public async Task<IActionResult> ClientRegister(ClientRegisterModel model)
        {
            string mobileNumber = await _dbContext.GetByQueryAsync<string>(@$"Select EntityID 
                                                                    From Entity
                                                                    Where IsDeleted=0 And EntityID<>@EntityID And Phone=@Phone And EntityTypeID={(int)EntityType.Client}", new { EntityID = Convert.ToInt32(model.EntityID), Phone = model.Phone });
            if(!string.IsNullOrEmpty(mobileNumber))
            {
                return BadRequest(new Error("MobileExist"));
            }

            string emailAddress = await _dbContext.GetByQueryAsync<string>(@$"Select EntityID 
                                                                    From Entity
                                                                    Where IsDeleted=0 And EntityID<>@EntityID And EmailAddress=@EmailAddress And EntityTypeID={(int)EntityType.Client}", new { EntityID = Convert.ToInt32(model.EntityID), EmailAddress = model.EmailAddress });
            if (!string.IsNullOrEmpty(emailAddress))
            {
                return BadRequest(new Error("EmailExist"));
            }

            string userName = await _dbContext.GetByQueryAsync<string>(@$"Select Username 
                                                                    From Users
                                                                    Where IsDeleted=0 And Username=@Username", new { Username = model.EmailAddress });
            if (!string.IsNullOrEmpty(userName))
            {
                return BadRequest(new Error("UsernameExist"));
            }

            _cn.Open();
            using var tran = _cn.BeginTransaction();
            {
                try
                {
                    #region Client

                    var clientEntity = new EntityCustom
                    {
                        Phone = model.Phone,
                        EntityTypeID = (int)EntityType.Client,
                        EmailAddress = model.EmailAddress,
                        CountryID = model.CountryID,

                    };
                    model.EntityID = clientEntity.EntityID =  await _dbContext.SaveAsync(clientEntity, tran);

                    var client = new ClientCustom
                    {
                        EntityID = model.EntityID.Value,
                        PackageID = model.PackageID,
                        IsPaid = model.IsPaid,

                    };
                    model.ClientID = client.ClientID = await _dbContext.SaveAsync(client, tran);

                    Model.Branch branch = new()
                    {
                        ClientID = client.ClientID,
                        EntityID = model.EntityID.Value,
                    };
                    branch.BranchID = await _dbContext.SaveAsync(branch, tran);

                    EntityInstituteInfo instituteInfo = new()
                    {
                        EntityID = model.EntityID,
                        Name = model.CompanyName,
                        ContactPerson = model.Name,
                    };
                    await _dbContext.SaveAsync(instituteInfo, tran);

                    #endregion

                    #region User

                    UserCustom newuser = new()
                    {
                        EntityID = model.EntityID,
                        LoginStatus = true,
                        UserName = model.EmailAddress,
                        UserTypeID = (int)UserTypes.Client,
                        Password = model.Password,
                        ClientID = client.ClientID,
                        EmailConfirmed = true,
                    };
                    newuser.UserID = await _user.SaveUser(newuser, true, tran, PDV.BrandName, model.EmailAddress);

                    #endregion

                    ClientRegistrationPackageDetailsModel InsertModel = new()
                    {
                        ClientID = model.ClientID.Value,
                        ClientEntityID = client.EntityID,
                        PackageID = model.PackageID.Value,
                        PaymentStatus = (int)PaymentStatus.Pending
                    };

                    tran.Commit();
                 
                    return Ok(new Success());
                    // var result = await _supperAdmin.HandleClientPackageAccountsInvoiceEntries(InsertModel, tran);

                    //if (result is not null)
                    //{
                    //    var packageRoleGroups = await _supperAdmin.FetchPackageRoleGroups(model.PackageID.Value, tran);
                    //    if (packageRoleGroups is not null && packageRoleGroups.Contains((int)RoleGroups.AccountsManagement))
                    //    {
                    //        await _accounts.InsertClientDefaultAccountsRelatedEntries(model.ClientID.Value, tran);
                    //    }

                    //    if (packageRoleGroups is not null && packageRoleGroups.Contains((int)RoleGroups.InventoryManagement))
                    //    {
                    //        await _inventory.InsertClientInventoryDefaultEntries(model.ClientID.Value, tran);
                    //    }

                    //    if (model.IsPaid == true)   
                    //    {
                    //        PaymentVerificationPostModel verificationModel = new() 
                    //        {
                    //            InvoiceID = result.InvoiceID,
                    //            RecieptVoucherTypeID = model.VoucherTypeID.Value
                    //        };
                    //        await _supperAdmin.InsertPaymentVerificationEntries(verificationModel, CurrentUserID, tran);

                    //        ClientInvoiceMailDetailsModel sendModel = new();

                    //        sendModel.MailRecipients = model.EmailAddress;

                    //        sendModel.Subject = "Tank you";

                    //        sendModel.Message = $@"
                    //                       <div style = 'font-family: Helvetica,Arial,sans-serif;min-width:1000px;overflow:auto;line-height:2;direction:ltr;'>
                    //                            <div style = 'margin:50px auto;width:70%;padding:20px 0'>
                    //                                <div style = 'border-bottom:1px solid #eee' >
                    //                                    <a href = ' style = 'font-size:1.4em;color: #00466a;text-decoration:none;font-weight:600'> {PDV.BrandName} </a>
                    //                                </div>
                    //                                <p style = 'font-size:1.1em'>    Hi {model.CompanyName},</p>
                    //                                <p>Your company registration is completed successfully and your account is activated.Now you can login your account with:
                    //                                <br>
                    //                                UserName: {model.EmailAddress}
                    //                                <br>
                    //                                Password:{model.Password}
                    //                                <br>Wish you have a successfull journey with us.</p>

                    //                                <h2 style = 'background: #00466a;text-align:right;width: max-content;padding: 0 10px;color: #fff;border-radius: 4px;'> </h2> 
                    //                               <hr style = 'border: none; border - top:1px solid #eee' />
                    //                            <div style='float:right; padding: 8px 0; color:#aaa;font-size:0.8em;line-height:1;font-weight:300'>
                    //                                <p> Regards,</p>
                    //                                    < p>  {PDV.BrandName} team</p>
                    //                                </div>
                    //                            </div>
                    //                        </div>
                    //                    ";

                    //        _job.Enqueue(() => _common.SendInvoiceEmailToClient(sendModel));
                    //    }

                    //    tran.Commit();

                    //    string packageName = await _dbContext.GetFieldsAsync<MembershipPackage, string>("PackageName", $"PackageID={model.PackageID.Value}", null);
                    //    string pdfName = "pdf_invoice_" + packageName.ToLower().Replace(' ', '_');
                    //    int pdfMediaID = await _pdf.CreateClientInvoicePdf(result.InvoiceID, pdfName);
                    //    if (pdfMediaID > 0)
                    //    {
                    //        var mailDetails = await _common.GetInvoiceEMailDetailsForClient(result.InvoiceID);
                    //        if (mailDetails is not null)
                    //            await _common.SendInvoiceEmailToClient(mailDetails);
                    //    }
                    //    return Ok(new Success());
                    //}
                    //else
                    //{
                    //    tran.Rollback();
                    //    return BadRequest(new BaseErrorResponse() { ResponseMessage = "SomethingWentWrong" });
                    //}
                }
                catch (Exception e)
                {
                    tran.Rollback();
                    return BadRequest(new BaseErrorResponse()
                    {
                        ResponseTitle = "Something went wrong",
                        ResponseMessage = e.Message
                    });
                }
            }
        }

        [HttpPost("approve-invoice-payment")]
        public async Task<IActionResult> ActveAccount(PaymentVerificationPostModel verificationModel) 
        {

            var result = await _supperAdmin.InsertPaymentVerificationEntries(verificationModel, CurrentUserID);
            if (result is not null)
            {
                var sendModel = await _common.GetInvoiceEMailDetailsForClient(result.InvoiceID);
                if (sendModel is not null)
                    await _common.SendInvoiceEmailToClient(sendModel);
            }

            return Ok(new Success());
        }

        [HttpGet("reject-invoice-payment/{invoiceId}")]
        public async Task<IActionResult> RejectAccount(int invoiceId)
        {

            var result = await _supperAdmin.InsertClientPaymentRejectionEntries(invoiceId, CurrentUserID);
            if (result is not null)
            {
                var packageModel = await _dbContext.GetByQueryAsync<PacageIDnNameModel>(@$"
                                                                Select MP.PackageID,MP.PackageName
                                                                From ClientInvoice CI
                                                                Left Join MembershipPackage MP ON MP.PackageID=CI.PackageID AND MP.IsDeleted=0
                                                                Where CI.InvoiceID={result.InvoiceID} AND CI.IsDeleted=0
                ",null);

                if (packageModel is not null)
                {
                    string pdfName = "pdf_invoice_" + packageModel.PackageName?.ToLower().Replace(' ', '_');

                    int pdfMediaID = await _pdf.CreateClientInvoicePdf(result.InvoiceID, pdfName);

                    if (pdfMediaID > 0)
                    {
                        var mailDetails = await _common.GetInvoiceEMailDetailsForClient(result.InvoiceID);
                        if (mailDetails is not null)
                            await _common.SendInvoiceEmailToClient(mailDetails);
                    }
                }
            }

            return Ok(new Success());
        }

        [HttpPost("update-client-package")]
        public async Task<IActionResult> UpdateClientPackage(ClientPackageUpdateModel updatePackageModel)
        {
            ClientInvoiceSaveReturnModel? result = null;

            _cn.Open();
            using (var tran = _cn.BeginTransaction())
            {
                try
                {
                    var clientOldPackageDetails = await _dbContext.GetByQueryAsync<ClientPackageUpdateDataModel>(@$"
                                                            Select C.EntityID As ClientEntityID,C.ClientID,B.InvoiceID,C.PackageID As OldPackageID
                                                            From Client C
                                                            Left Join (
	                                                            Select A.InvoiceID,I.ClientID 
	                                                            From(
		                                                            Select ClientID,Max(InvoiceID)as InvoiceID From ClientInvoice
		                                                            Where isDeleted=0 
		                                                            Group By ClientID
		                                                            ) as A 
	                                                            Left Join Clientinvoice I on I.InvoiceID=A.InvoiceID 
	                                                            ) as B on B.ClientID=C.ClientID
                                                            Where C.ClientID ={updatePackageModel.ClientID.Value} and C.IsDeleted=0
                    ", null, tran);

                    ClientRegistrationPackageDetailsModel newPackageEntryModel = new()
                    {
                        ClientID = clientOldPackageDetails.ClientID,
                        ClientEntityID = clientOldPackageDetails.ClientEntityID,
                        InvoiceID = clientOldPackageDetails.InvoiceID,
                        PackageID = updatePackageModel.PackageID.Value,
                        Discount = updatePackageModel.Discount,
                    };

                    result = await _supperAdmin.HandleUpdateClientPackage(newPackageEntryModel, tran);

                    if(result is not null)
                    {
                        var oldPackageFeatures = await _supperAdmin.FetchPackageRoleGroups(clientOldPackageDetails.OldPackageID, tran);
                        var newPackageFeatures = await _supperAdmin.FetchPackageRoleGroups(updatePackageModel.PackageID.Value, tran);
                        
                        //Checking old package doesn't contain 'Accounts' and new package contains 'Accounts' feature
                        if (oldPackageFeatures is not null && newPackageFeatures is not null && !oldPackageFeatures.Contains((int)RoleGroups.AccountsManagement) && newPackageFeatures.Contains((int)RoleGroups.AccountsManagement))
                        {
                            await _accounts.InsertClientDefaultAccountsRelatedEntries(updatePackageModel.ClientID.Value, tran);
                        }

                        //Checking old package doesn't contain 'Inventory' and new package contains 'Inventory' feature
                        if (oldPackageFeatures is not null && newPackageFeatures is not null && !oldPackageFeatures.Contains((int)RoleGroups.InventoryManagement) && newPackageFeatures.Contains((int)RoleGroups.InventoryManagement))
                        {
                            await _inventory.InsertClientInventoryDefaultEntries(updatePackageModel.ClientID.Value, tran);
                        }

                        tran.Commit();

                        string packageName = await _dbContext.GetFieldsAsync<MembershipPackage, string>("PackageName", $"PackageID={updatePackageModel.PackageID.Value}", null);
                        string pdfName = "pdf_invoice_" + packageName.ToLower().Replace(' ', '_');
                        int pdfMediaID = await _pdf.CreateClientInvoicePdf(result.InvoiceID, pdfName);
                        if (pdfMediaID > 0)
                        {
                            var mailDetails = await _common.GetInvoiceEMailDetailsForClient(result.InvoiceID);
                            if (mailDetails is not null)
                                await _common.SendInvoiceEmailToClient(mailDetails);
                        }
                        return Ok(new Success());
                    }
                    else
                    {
                        tran.Rollback();
                        return BadRequest(new BaseErrorResponse() { ResponseMessage = "SomethingWentWrong"});
                    }
                }
                catch (Exception e)
                {
                    tran.Rollback();
                    return BadRequest(new BaseErrorResponse()
                    {
                        ResponseTitle = "Something went wrong",
                        ResponseMessage = e.Message
                    });
                }
            }
        }

        [HttpPost("get-all-payment-details")]
        public async Task<IActionResult> GetAllClientPaymentDetails(PagedListPostModelWithFilter model)
        {
            PagedListQueryModel query = model;

            query.Select = $@"Select Name,PackageName,CI.ClientID,CI.InvoiceID,CI.Fee,DisconnectionDate,PaidStatus From ClientInvoice CI
                            Join MembershipPackage MP on MP.PackageID=CI.PackageID and MP.IsDeleted=0
                            Join viEntity V on V.ClientID=CI.ClientID and UserTypeID={(int)UserTypes.Client}";

            query.WhereCondition = $"CI.IsDeleted=0 and (PaidStatus={(int)PaymentStatus.Pending} or PaidStatus={(int)PaymentStatus.Paid})";
            query.OrderByFieldName = "PaidStatus desc";
            if (!model.SearchString.IsNullOrEmpty())
            {
                query.WhereCondition += $" and Name like '%{model.SearchString}%'";
            }
            query.WhereCondition += _common.GeneratePagedListFilterWhereCondition(model, "Name");
            var res = await _dbContext.GetPagedList<ClientPaymentListModel>(query, null);
            return Ok(res);
        }


        [HttpGet("get-client-existing-package/{ClientID}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "MembershipManagment")]
        public async Task<IActionResult> GetExistingPackage(int ClientID)
        {
            //var res = await _dbContext.GetByQueryAsync<ClientExistingPackageModel>($@"Select C.PackageID,PackageName,Fee
            //                                                                        From Client C
            //                                                                        Join MembershipPackage MP on MP.PackageID=C.PackageID and MP.IsDeleted=0
            //                                                                        Where C.ClientID={ClientID} and C.IsDeleted=0");

            var res = await _dbContext.GetByQueryAsync<ClientExistingPackageModel>($@"Select C.PackageID,PackageName,B.Fee,DATEDIFF(DAY,StartDate,EndDate) TotalDays,EndDate,StartDate
                                                                                    From Client C
                                                                                    Left Join (Select I.ClientID,EndDate,StartDate,I.GrossFee as Fee From(
                                                                                    Select ClientID,Max(InvoiceID)as InvoiceID From ClientInvoice
                                                                                    Where isDeleted=0 
                                                                                    Group By ClientID) as A 
                                                                                    Left Join Clientinvoice I on I.InvoiceID=A.InvoiceID 
                                                                                    ) as B on B.ClientID=C.ClientID
                                                                                    Join MembershipPackage MP on MP.PackageID=C.PackageID and MP.IsDeleted=0
                                                                                    Where C.ClientID={ClientID} and C.IsDeleted=0", null);

            if (DateTime.UtcNow <= res.EndDate)
            {
                DateTime currentDate = DateTime.UtcNow;
                DateTime targetDate = res.StartDate.Value;
                TimeSpan timeUntilTarget = currentDate - targetDate;
                int daysUntilTarget = timeUntilTarget.Days;
                decimal? dailyCost = res.Fee / res.TotalDays;
                int remainingDays = res.TotalDays.Value - daysUntilTarget;
                decimal? Balance = dailyCost * remainingDays;
                res.BalanceAmount = Math.Round(Balance.Value);
            }

            return Ok(res ?? new());
        }

        #endregion

        #region Daily Pacakge expire checkup

        [HttpGet("regenerate-expired-invoice")]
        public async Task<IActionResult> ExpiredInvoiceGeneration()
        {
            //_job.Enqueue(() => _common.SetInvoiceGeneration());

            // _job.Enqueue(() => _supperAdmin.DaywisePackageExpireCheckup());

            //await _common.SetInvoiceGeneration();

            var result = await _supperAdmin.DaywisePackageExpireCheckup();

            return Ok(result ?? new());
        }

        #endregion

        #region Roles

        [HttpGet("get-all-roles")]
        public async Task<IActionResult> GetAllRoles()
        {
            PackageRoleListModel res = new();
            res.RoleGroups = await _dbContext.GetListByQueryAsync<RoleGroupModel>($@"Select * 
                                                                                    From RoleGroup
                                                                                    Where IsDeleted=0", null);
            res.Roles = await _dbContext.GetListByQueryAsync<RoleModel>($@"Select * 
                                                                            From Role
                                                                            Where IsDeleted=0", null);
            return Ok(res ?? new());
        }


        [HttpGet("get-all-rolegroups")]
        public async Task<IActionResult> GetAllRolesGroups()
        {
            List<RoleGroupsModel> res = new();
            res = await _dbContext.GetListByQueryAsync<RoleGroupsModel>($@"Select * 
                                                                        From RoleGroup
                                                                        Where IsDeleted=0 and RoleGroupID Not in ({(int)Shared.Enum.RoleGroups.SupportRoles})", null);
            foreach (var roleGroup in res)
            {
                roleGroup.Roles = await _dbContext.GetListByQueryAsync<RoleModel>($@"Select * 
                                                                            From Role
                                                                            Where IsDeleted=0 and RoleGroupID={roleGroup.RoleGroupID}", null);
            }

            return Ok(res ?? new());
        }

        #endregion
    }
}
