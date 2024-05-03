using AutoMapper;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PB.Shared.Models;
using PB.DatabaseFramework;
using PB.Model;
using PB.Model.Tables;
using PB.Server.Repository;
using PB.Shared;
using PB.Shared.Enum;
using PB.Shared.Helpers;
using PB.Shared.Models.Common;
using PB.Shared.Models.SuperAdmin.Custom;
using PB.Shared.Tables;
using System.Data;
using PB.Model.Models;
using PB.CRM.Model.Enum;
using PB.Shared.Enum.CRM;
using PB.Shared.Tables.Accounts.VoucherTypes;
using PB.Shared.Models.SuperAdmin.Client;
using Newtonsoft.Json;
using PB.Client.Pages.SuperAdmin;
using PB.Shared.Models.WhatsaApp;
using PB.Shared.Tables.Whatsapp;
using System.Net.Http.Headers;
using PB.Client.Pages.Whatsapp;
using PB.EntityFramework;
using PB.Shared.Tables.Accounts.AccountGroups;
using PB.Shared.Tables.Accounts.Ledgers;
using NPOI.SS.Formula.Functions;
using PB.Client.Pages.SuperAdmin.Client;
using PB.Shared.Models.CRM.Customer;
using PB.Shared.Models.CRM;
using PB.Shared.Tables.Tax;
using PB.Shared.Enum.Accounts;
using PB.Shared.Models.Accounts.Ledgers;

namespace PB.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CommonController : BaseController
    {
        private readonly IDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IDbConnection _cn;
        private readonly ICRMRepository _crm;
        private readonly IConfiguration _config;
        private readonly IEmailSender _email;
        private readonly ICommonRepository _common;
        private readonly IBackgroundJobClient _job;
        private readonly ISuperAdminRepository _superAdmin;
        private readonly ICommonRepository _commonRepository;
        private readonly IAccountRepository _accounts;
        private readonly IWhiteLabelRepository _whiteLabel;
        private readonly IInventoryRepository _inventory;
        private readonly IEntityRepository _entity;
        public CommonController(IDbContext dbContext, IMapper mapper, IDbConnection cn, ICRMRepository crm, IAccountRepository account, IConfiguration config, IEmailSender email, ICommonRepository common, IBackgroundJobClient job, ISuperAdminRepository superAdmin, IWhiteLabelRepository whiteLabel, ICommonRepository commonRepository, IAccountRepository accounts, IInventoryRepository inventory, IEntityRepository entity)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _cn = cn;
            _crm = crm;
            _config = config;
            _email = email;
            _common = common;
            _job = job;
            _superAdmin = superAdmin;
            _whiteLabel = whiteLabel;
            _commonRepository = commonRepository;
            _accounts = accounts;
            _inventory = inventory;
            _entity = entity;
        }

        #region State

        [HttpPost("save-state")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> SaveState(CountryStateModel model)
        {
            var stateName = await _dbContext.GetByQueryAsync<string>($@"
                                                Select StateName
                                                From CountryState
                                                Where StateName=@StateName and StateID<>StateID and IsDeleted=0 and ClientID={CurrentClientID}
            ", model);

            if (!string.IsNullOrEmpty(stateName))
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ResponseMessage = "State already exist, please check again",
                    ResponseTitle = "Invalid Submission"
                });
            }
            CountryState newState = new()
            {
                StateName = model.StateName,
                CountryID = model.CountryID,
                StateCode = model.StateCode,
                Long = Convert.ToDecimal(model.Long),
                Lat = Convert.ToDecimal(model.Lat),
                ClientID = CurrentClientID
            };
            //var state = _mapper.Map<CountryState>(model);
            newState.StateID = await _dbContext.SaveAsync(newState);
            return Ok(new StateAddResultModel() { StateID = newState.StateID, StateName = newState.StateName });

        }

        [HttpGet("delete-state/{stateID}")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "StateDelete")]
        public async Task<IActionResult> DeleteState(int stateID)
        {
            await _dbContext.DeleteAsync<CountryState>(stateID);
            return Ok(true);
        }

        [HttpGet("get-state/{stateID}")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "State")]
        public async Task<IActionResult> GetSatate(int stateID)
        {
            var state = await _dbContext.GetByQueryAsync<StateCustom>($@"
                            Select S.*,C.CountryName
                            From CountryState S
                            Left Join Country C ON C.CountryID=S.CountryID and C.IsDeleted=0
                            Where S.ClientID={CurrentClientID} and S.IsDeleted=0 and S.StateID={stateID}
            ", null);

            return Ok(state);
        }

        #endregion

        #region City

        [HttpPost("save-city")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "StateAdd,StateEdit")]
        public async Task<IActionResult> SaveCity(CountryCityModel model)
        {
            var stateName = await _dbContext.GetByQueryAsync<string>($@"
                                                Select CityName
                                                From CountryCity
                                                Where CityName=@CityName and CityID<>CityID and IsDeleted=0 and ClientID={CurrentClientID}
            ", model);

            if (!string.IsNullOrEmpty(stateName))
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ResponseMessage = "City already exist, please check again",
                    ResponseTitle = "Invalid Submission"
                });
            }
            CountryCity newCity = new()
            {
                CityName = model.CityName,
                StateID = model.StateID,
                Long = Convert.ToDecimal(model.Long),
                Lat = Convert.ToDecimal(model.Lat),
                ClientID = CurrentClientID
            };
            //var state = _mapper.Map<CountryState>(model);
            newCity.CityID = await _dbContext.SaveAsync(newCity);
            return Ok(new CityAddResultModel() { CityID = newCity.CityID, CityName = newCity.CityName });

        }

        [HttpGet("delete-city/{CityID}")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "StateDelete")]
        public async Task<IActionResult> DeleteCity(int CityID)
        {
            await _dbContext.DeleteAsync<CountryCity>(CityID);
            return Ok(true);
        }

        [HttpGet("get-city/{cityID}")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "State")]
        public async Task<IActionResult> GetCity(int cityID)
        {
            var state = await _dbContext.GetByQueryAsync<StateCustom>($@"
                            Select S.*,C.StateName
                            From CountryCity S
                            Left Join CountryState C ON C.StateID=S.StateID and C.IsDeleted=0
                            Where S.ClientID={CurrentClientID} and S.IsDeleted=0 and S.CityID={cityID}
            ", null);

            return Ok(state);
        }
        #endregion

        #region Dropdown related API

        #region Common Search

        [AllowAnonymous]
        [HttpPost("get-list-of-zones")]
        public async Task<IActionResult> GetListOfCountryZones(CommonSearchModel model)
        {
            string selectQuery = @$"Select Top 20 ZoneID AS ID,Concat(ZoneName,' (',GMTOffsetName,' )') AS Value 
                    From CountryZone
                    Where IsDeleted=0 And CountryID={model.ID}";

            if (!model.ReadDataOnSearch)
            {
                selectQuery = @$"Select ZoneID AS ID,Concat(ZoneName,' (',GMTOffsetName,' )') AS Value 
                    From CountryZone
                    Where IsDeleted=0 And CountryID={model.ID}";
            }

            if (model.ReadDataOnSearch && !string.IsNullOrEmpty(model.SearchString))
            {
                selectQuery = @$"Select ZoneID AS ID,Concat(ZoneName,' (',GMTOffsetName,' )') AS Value 
                    From CountryZone
                    Where IsDeleted=0 And CountryID={model.ID} And (ZoneName like '{model.SearchString}%' Or GMTOffsetName like '{model.SearchString}%')";
            }
            var result = await _dbContext.GetListByQueryAsync<IdnValuePair>(selectQuery, new { SearchString = model.SearchString });
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("get-list-of-countries")]
        public async Task<IActionResult> GetListOfCountries(CommonSearchModel model)
        {
            string selectQuery = @$"Select Top 20 CountryID as ID,CountryName as Value 
                    From Country
                    Where IsDeleted=0";

            if (!model.ReadDataOnSearch)
            {
                selectQuery = @$"Select CountryID as ID,CountryName as Value 
                    From Country
                    Where IsDeleted=0";
            }

            if (model.ReadDataOnSearch && !string.IsNullOrEmpty(model.SearchString))
            {
                selectQuery = @$"Select Top 20 CountryID as ID,CountryName as Value 
                    From Country
                    Where IsDeleted=0 And CountryName like '{model.SearchString}%'";
            }
            var result = await _dbContext.GetListByQueryAsync<IdnValuePair>(selectQuery, new { SearchString = model.SearchString });
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("get-list-of-states")]
        public async Task<IActionResult> GetListOfCountryStates(CommonSearchModel model)
        {
            string selectQuery = @$"Select Top 20 StateID as ID,StateName as Value 
                    From CountryState
                    Where IsDeleted=0 and CountryID={model.ID}";

            if (!model.ReadDataOnSearch)
            {
                selectQuery = @$"Select StateID as ID,StateName as Value 
                    From CountryState
                    Where IsDeleted=0 and CountryID={model.ID}";
            }

            if (model.ReadDataOnSearch && !string.IsNullOrEmpty(model.SearchString))
            {
                selectQuery = @$"Select StateID as ID,StateName as Value 
                    From CountryState
                    Where IsDeleted=0 and CountryID={model.ID} And StateName like '{model.SearchString}%'";
            }
            var result = await _dbContext.GetListByQueryAsync<IdnValuePair>(selectQuery, new { SearchString = model.SearchString });
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("get-list-of-cities")]
        public async Task<IActionResult> GetListOfCountryCities(CommonSearchModel model)
        {
            string selectQuery = @$"Select Top 20 CityID as ID,CityName as Value 
                    From CountryCity
                    Where IsDeleted=0 and StateID={model.ID}";

            if (!model.ReadDataOnSearch)
            {
                selectQuery = @$"Select CityID as ID,CityName as Value 
                    From CountryCity
                    Where IsDeleted=0 and StateID={model.ID}";
            }

            if (model.ReadDataOnSearch && !string.IsNullOrEmpty(model.SearchString))
            {
                selectQuery = @$"Select CityID as ID,CityName as Value 
                    From CountryCity
                    Where IsDeleted=0 and StateID={model.ID} And CityName like '{model.SearchString}%'";
            }
            var result = await _dbContext.GetListByQueryAsync<IdnValuePair>(selectQuery, new { SearchString = model.SearchString });
            return Ok(result);
        }

        #endregion

        #region CRM

        [AllowAnonymous]
        [HttpPost("get-list-of-customers")]
        public async Task<IActionResult> GetListOfCustomers(CommonSearchModel model)
        {
            string selectQuery = @$"Select Top 20 C.EntityID As ID,E.Name As Value
                    From Customer C
                    Left Join viEntity E ON E.EntityID=C.EntityID
                    Where C.ClientID={CurrentClientID}"; 

            if (!model.ReadDataOnSearch)
            {
                selectQuery = @$"Select C.EntityID As ID,E.Name As Value
                    From Customer C
                    Left Join viEntity E ON E.EntityID=C.EntityID
                    Where C.ClientID={CurrentClientID}";
            }

            if (model.ReadDataOnSearch && !string.IsNullOrEmpty(model.SearchString))
            {
                selectQuery = @$"Select C.EntityID As ID,E.Name As Value
                    From Customer C
                    Left Join viEntity E ON E.EntityID=C.EntityID
                    Where C.ClientID={CurrentClientID} and C.IsDeleted=0 and E.Name like @SearchString";
            }
            var result = await _dbContext.GetListByQueryAsync<IdnValuePair>(selectQuery, new { SearchString = model.SearchString + '%' });
            return Ok(result);
        }

        #endregion

        [HttpPost("get-list-of-supplier")]
        public async Task<IActionResult> GetListOfSuppliers(CommonSearchModel model)
        {
            string select = "";
            string whereCondition = "";

            if (model.ReadDataOnSearch && string.IsNullOrEmpty(model.SearchString))
            {
                select = "SELECT TOP 20 ";
            }

            select = string.IsNullOrEmpty(select) ?
                @"Select S.EntityID As ID,E.Name As Value
                    From Supplier S
                    Left Join viEntity E ON E.EntityID=S.EntityID" :
                select += @"S.EntityID As ID,E.Name As Value
                    From Supplier S
                    Left Join viEntity E ON E.EntityID=S.EntityID";

            whereCondition = !string.IsNullOrEmpty(model.SearchString) ?
                $"Where S.ClientID={CurrentClientID} and S.IsDeleted=0 and E.Name like '{model.SearchString}%'" :
                $"Where S.ClientID={CurrentClientID} and S.IsDeleted=0";

            string query = select + " " + whereCondition;
            var result = await _dbContext.GetListByQueryAsync<IdnValuePair>(query, null);
            return Ok(result);
        }

        [HttpPost("get-list-of-goods-items")]
        public async Task<IActionResult> GetListOfGoodsItems(CommonSearchModel model)
        {
            string select = "";
            string whereCondition = "";

            if (model.ReadDataOnSearch && string.IsNullOrEmpty(model.SearchString))
            {
                select = "SELECT TOP 20 ";
            }

            select = string.IsNullOrEmpty(select) ?
                @" Select vI.ItemVariantID as ID,vI.ItemName as  Value
                    From viItem vI
                    Left Join Item I ON I.ItemID=vI.ItemID" :
            select += @"vI.ItemVariantID as ID,vI.ItemName as  Value
                    From viItem vI
                    Left Join Item I ON I.ItemID=vI.ItemID";

            whereCondition = !string.IsNullOrEmpty(model.SearchString) ?
                $"Where vI.ClientID={CurrentClientID}  AND vI.ItemName like '{model.SearchString}%' And I.IsGoods=1" :
                $"Where vI.ClientID={CurrentClientID} And I.IsGoods=1";

            string query = select + " " + whereCondition;

            var result = await _dbContext.GetListByQueryAsync<IdnValuePair>(query, null);
            return Ok(result);
        }

        [HttpPost("get-list-of-items")]
        public async Task<IActionResult> GetListOfItems(CommonSearchModel model)
        {
            string select = "";
            string whereCondition = "";

            if (model.ReadDataOnSearch && string.IsNullOrEmpty(model.SearchString))
            {
                select = "SELECT TOP 20 ";
            }

            select = string.IsNullOrEmpty(select) ?
                @" Select I.ItemVariantID as ID,I.ItemName as  Value
                    From viItem I" :
            select += @"I.ItemVariantID as ID,I.ItemName as  Value
                    From viItem I";

            whereCondition = !string.IsNullOrEmpty(model.SearchString) ?
                $"Where I.ClientID={CurrentClientID}  AND I.ItemName like '{model.SearchString}%'" :
                $"Where I.ClientID={CurrentClientID} ";

            string query = select + " " + whereCondition;

            var result = await _dbContext.GetListByQueryAsync<IdnValuePair>(query, null);
            return Ok(result);
        }

        [HttpPost("get-list-of-item-units")]
        public async Task<IActionResult> GetListOfItemUnits(CommonSearchModel model)
        {
            string select = "";
            string whereCondition = "";

            if (model.ReadDataOnSearch && string.IsNullOrEmpty(model.SearchString))
            {
                select = "SELECT TOP 20 ";
            }

            select = string.IsNullOrEmpty(select) ?
                    @"Select PackingTypeID as ID,Concat(PackingTypeName,' - ','(',PackingTypeCode,')') as Value
                        From ItemPackingType" :
                    select += @"PackingTypeID as ID,Concat(PackingTypeName,' - ','(',PackingTypeCode,')') as Value
                        From ItemPackingType";
            whereCondition = $"Where IsDeleted=0 and (ClientID={CurrentClientID} OR ClientID IS NULL)";

            string query = select + " " + whereCondition;
            var result = await _dbContext.GetListByQueryAsync<IdnValuePair>(query, null);
            return Ok(result);

        }

        [HttpPost("get-list-of-membership-features")]
        public async Task<IActionResult> GetListOfMembershipFeatures(CommonSearchModel model)
        {
            string select = "";
            string whereCondition = "";

            if (model.ReadDataOnSearch && string.IsNullOrEmpty(model.SearchString))
            {
                select = "SELECT TOP 20 ";
            }

            select = string.IsNullOrEmpty(select) ?
                @"Select FeatureID as ID,FeatureName as Value 
                    From MembershipFeature" :
            select += @"FeatureID as ID,FeatureName as Value 
                        From MembershipFeature";

            whereCondition = !string.IsNullOrEmpty(model.SearchString) ?
                $"Where IsDeleted=0 and FeatureName like '{model.SearchString}%'" :
                $"Where IsDeleted=0";

            string query = select + " " + whereCondition;
            var result = await _dbContext.GetListByQueryAsync<IdnValuePair>(query, null);
            return Ok(result);
        }

        [HttpPost("get-list-of-membership-plans")]
        public async Task<IActionResult> GetListOfMembershipPlans(CommonSearchModel model)
        {
            string select = "";
            string whereCondition = "";

            if (model.ReadDataOnSearch && string.IsNullOrEmpty(model.SearchString))
            {
                select = "SELECT TOP 20 ";
            }

            select = string.IsNullOrEmpty(select) ?
                @"Select PlanID as ID,PlanName as Value 
                    From MembershipPlan" :
            select += @"PlanID as ID,PlanName as Value 
                        From MembershipPlan";

            whereCondition = !string.IsNullOrEmpty(model.SearchString) ?
                $"Where IsDeleted=0 and PlanName like '{model.SearchString}%'" :
                $"Where IsDeleted=0";

            string query = select + " " + whereCondition;
            var result = await _dbContext.GetListByQueryAsync<IdnValuePair>(query, null);
            return Ok(result);
        }

        [HttpPost("get-list-of-membership-capacities")]
        public async Task<IActionResult> GetListOfMembershipCapacities(CommonSearchModel model)
        {
            string select = "";
            string whereCondition = "";

            if (model.ReadDataOnSearch && string.IsNullOrEmpty(model.SearchString))
            {
                select = "SELECT TOP 20 ";
            }

            select = string.IsNullOrEmpty(select) ?
                @"Select CapacityID as ID,Capacity as Value 
                    From MembershipUserCapacity" :
            select += @"CapacityID as ID,Capacity as Value 
                        From MembershipUserCapacity";

            whereCondition = !string.IsNullOrEmpty(model.SearchString) ?
                $"Where IsDeleted=0 and Capacity like '{model.SearchString}%'" :
                $"Where IsDeleted=0";

            string query = select + " " + whereCondition;
            var result = await _dbContext.GetListByQueryAsync<IdnValuePair>(query, null);
            return Ok(result);
        }

        [HttpPost("get-list-of-item-sizes")]
        public async Task<IActionResult> GetListOfItemSizes(CommonSearchModel model)
        {
            string select = "";
            string whereCondition = "";

            if (model.ReadDataOnSearch && string.IsNullOrEmpty(model.SearchString))
            {
                select = "SELECT TOP 20 ";
            }

            select = string.IsNullOrEmpty(select) ?
                @"Select SizeID as ID,Size as Value 
                    From ItemSize" :
            select += @"SizeID as ID,Size as Value 
                    From ItemSize";

            whereCondition = !string.IsNullOrEmpty(model.SearchString) ?
                $"Where IsDeleted=0 and ClientID={CurrentClientID} and Size like '{model.SearchString}%'" :
                $"Where IsDeleted=0 and ClientID={CurrentClientID}";

            string query = select + " " + whereCondition;
            var result = await _dbContext.GetListByQueryAsync<IdnValuePair>(query, null);
            return Ok(result);
        }

        [HttpPost("get-list-of-item-colors")]
        public async Task<IActionResult> GetListOfItemColors(CommonSearchModel model)
        {
            string select = "";
            string whereCondition = "";

            if (model.ReadDataOnSearch && string.IsNullOrEmpty(model.SearchString))
            {
                select = "SELECT TOP 20 ";
            }

            select = string.IsNullOrEmpty(select) ?
                @"Select ColorID as ID,ColorName as Value 
                    From ItemColor" :
            select += @"ColorID as ID,ColorName as Value 
                    From ItemColor";

            whereCondition = !string.IsNullOrEmpty(model.SearchString) ?
                $"Where IsDeleted=0 and (ClientID={CurrentClientID} or IsNull(ClientID,0)=0) and ColorName like '{model.SearchString}%'" :
                $"Where IsDeleted=0 and (ClientID={CurrentClientID} or IsNull(ClientID,0)=0)";

            string query = select + " " + whereCondition;
            var result = await _dbContext.GetListByQueryAsync<IdnValuePair>(query, null);
            return Ok(result);
        }

        [HttpPost("get-list-of-item-brands")]
        public async Task<IActionResult> GetListOfItemBrands(CommonSearchModel model)
        {
            string select = "";
            string whereCondition = "";

            if (model.ReadDataOnSearch && string.IsNullOrEmpty(model.SearchString))
            {
                select = "SELECT TOP 20 ";
            }

            select = string.IsNullOrEmpty(select) ?
                @"Select BrandID as ID,BrandName as Value 
                    From ItemBrand" :
            select += @"BrandID as ID,BrandName as Value 
                    From ItemBrand";

            whereCondition = !string.IsNullOrEmpty(model.SearchString) ?
                $"Where IsDeleted=0 and ClientID={CurrentClientID} and BrandName like '{model.SearchString}%'" :
                $"Where IsDeleted=0 and ClientID={CurrentClientID}";

            string query = select + " " + whereCondition;
            var result = await _dbContext.GetListByQueryAsync<IdnValuePair>(query, null);
            return Ok(result);
        }

        [HttpPost("get-list-of-item-groups")]
        public async Task<IActionResult> GetListOfItemGroups(CommonSearchModel model)
        {
            string select = "";
            string whereCondition = "";

            if (model.ReadDataOnSearch && string.IsNullOrEmpty(model.SearchString))
            {
                select = "SELECT TOP 20 ";
            }

            select = string.IsNullOrEmpty(select) ?
                @"Select GroupID as ID,GroupName as Value 
                    From ItemGroup" :
            select += @"GroupID as ID,GroupName as Value 
                    From ItemGroup";

            whereCondition = !string.IsNullOrEmpty(model.SearchString) ?
                $"Where IsDeleted=0 and ClientID={CurrentClientID} and GroupName like '{model.SearchString}%'" :
                $"Where IsDeleted=0 and ClientID={CurrentClientID}";

            string query = select + " " + whereCondition;
            var result = await _dbContext.GetListByQueryAsync<IdnValuePair>(query, null);
            return Ok(result);
        }

        [HttpPost("get-list-of-item-categories")]
        public async Task<IActionResult> GetListOfItemCategories(CommonSearchModel model)
        {
            string select = "";
            string whereCondition = "";

            if (model.ReadDataOnSearch && string.IsNullOrEmpty(model.SearchString))
            {
                select = "SELECT TOP 20 ";
            }

            select = string.IsNullOrEmpty(select) ?
                @"Select CategoryID as ID,CategoryName as Value 
                    From ItemCategory" :
            select += @"CategoryID as ID,CategoryName as Value 
                    From ItemCategory";

            whereCondition = !string.IsNullOrEmpty(model.SearchString) ?
                $"Where IsDeleted=0 and ClientID={CurrentClientID} and Category like '{model.SearchString}%'" :
                $"Where IsDeleted=0 and ClientID={CurrentClientID}";

            string query = select + " " + whereCondition;
            var result = await _dbContext.GetListByQueryAsync<IdnValuePair>(query, null);
            return Ok(result);
        }

        [HttpPost("get-list-of-lead-throughs")]
        public async Task<IActionResult> GetListOfLeadThroughs(CommonSearchModel model)
        {
            string select = "";
            string whereCondition = "";

            if (model.ReadDataOnSearch)
            {
                select = string.IsNullOrEmpty(model.SearchString) ? "SELECT TOP 20 " : "";
            }

            select = string.IsNullOrEmpty(select) ?
                @"Select LeadThroughID as ID,Name as Value 
                    From LeadThrough" :
                select += @"LeadThroughID as ID,Name as Value 
                    From LeadThrough";

            whereCondition = !string.IsNullOrEmpty(model.SearchString) ?
                $"Where (ClientID={CurrentClientID} OR ClientID IS NULL) and IsDeleted=0 and Size like '{model.SearchString}%'" :
                $"Where (ClientID={CurrentClientID} OR ClientID IS NULL) and IsDeleted=0";

            string query = select + " " + whereCondition;
            var result = await _dbContext.GetListByQueryAsync<IdnValuePair>(query, null);
            return Ok(result);
        }

        [HttpPost("get-list-of-business-type")]
        public async Task<IActionResult> GetListOfBusinessType(CommonSearchModel model)
        {
            string select = "";
            string whereCondition = "";

            if (model.ReadDataOnSearch)
            {
                select = string.IsNullOrEmpty(model.SearchString) ? "SELECT TOP 20 " : "";
            }

            select = string.IsNullOrEmpty(select) ?
                @"Select BusinessTypeID as ID,BusinessTypeName as Value 
                    From BusinessType" :
                select += @"BusinessTypeID as ID,BusinessTypeName as Value 
                    From BusinessType";

            whereCondition = !string.IsNullOrEmpty(model.SearchString) ?
                $"Where (ClientID={CurrentClientID} OR ClientID IS NULL) and IsDeleted=0 and BusinessTypeName like '{model.SearchString}%'" :
                $"Where (ClientID={CurrentClientID} OR ClientID IS NULL) and IsDeleted=0";

            string query = select + " " + whereCondition;
            var result = await _dbContext.GetListByQueryAsync<IdnValuePair>(query, null);
            return Ok(result);
        }


        [HttpPost("get-list-of-quotation-staff")]
        public async Task<IActionResult> GetListOfCreatedFor(CommonSearchModel model)
        {
            string select = "";
            string whereCondition = "";

            if (model.ReadDataOnSearch)
            {
                select = string.IsNullOrEmpty(model.SearchString) ? "SELECT TOP 20 " : "";
            }

            select = string.IsNullOrEmpty(select) ?
                @"Select V.EntityID as ID,Name as Value
                        From Users U
                        Left Join viEntity V on V.EntityID=U.EntityID" :
                select += @" V.EntityID as ID,Name as Value
                                From Users U
                                Left Join viEntity V on V.EntityID=U.EntityID";

            whereCondition = !string.IsNullOrEmpty(model.SearchString) ?
                $"Where (V.ClientID={CurrentClientID} OR V.ClientID IS NULL) and U.IsDeleted=0 and U.UserTypeID={(int)UserTypes.Staff} and Name like '{model.SearchString}%'" :
                $"Where (V.ClientID={CurrentClientID} OR V.ClientID IS NULL) and U.IsDeleted=0 and U.UserTypeID={(int)UserTypes.Staff}";

            string query = select + " " + whereCondition;
            var result = await _dbContext.GetListByQueryAsync<IdnValuePair>(query, null);
            return Ok(result);
        }

        [HttpPost("get-list-of-followup-status")]
        public async Task<IActionResult> GetListOfFollowupStatus(CommonSearchModel model)
        {
            string select = "";
            string whereCondition = "";

            if (model.ReadDataOnSearch && string.IsNullOrEmpty(model.SearchString))
            {
                select = "SELECT TOP 20 ";
            }

            select = string.IsNullOrEmpty(select) ?
                @$"Select FS.FollowUpStatusID as ID,
                                    Case
                                        When FS.Nature={(int)FollowUpNatures.Followup} Then Concat(FS.StatusName,' (Followup)')
                                        When FS.Nature={(int)FollowUpNatures.Dropped} Then Concat(FS.StatusName,' (Dropped)')
                                        When FS.Nature={(int)FollowUpNatures.ClosedWon} Then Concat(FS.StatusName,' (Closed won)')
                                        When FS.Nature={(int)FollowUpNatures.Interested} Then Concat(FS.StatusName,' (Interested)')
                                        End As Value
                                    From FollowUpStatus FS" :
                select += @$"FS.FollowUpStatusID as ID,
                                    Case
                                        When FS.Nature={(int)FollowUpNatures.Followup} Then Concat(FS.StatusName,' (Followup)')
                                        When FS.Nature={(int)FollowUpNatures.Dropped} Then Concat(FS.StatusName,' (Dropped)')
                                        When FS.Nature={(int)FollowUpNatures.ClosedWon} Then Concat(FS.StatusName,' (Closed won)')
                                        When FS.Nature={(int)FollowUpNatures.Interested} Then Concat(FS.StatusName,' (Interested)')
                                        End As Value
                                    From FollowUpStatus FS";

            whereCondition = !string.IsNullOrEmpty(model.SearchString) ?
                $"Where (ClientID={CurrentClientID} OR ClientID IS NULL) and Type={model.ID} and IsDeleted=0 and StatusName like '{model.SearchString}%'" :
                $"Where (ClientID={CurrentClientID} OR ClientID IS NULL) and Type={model.ID} and IsDeleted=0";

            string query = select + " " + whereCondition;
            var result = await _dbContext.GetListByQueryAsync<IdnValuePair>(query, null);
            return Ok(result);
        }

        [HttpPost("get-list-of-tax-preferences")]
        public async Task<IActionResult> GetListOfTaxPreference(CommonSearchModel model)
        {
            string select = "";
            string whereCondition = "";

            if (model.ReadDataOnSearch && string.IsNullOrEmpty(model.SearchString))
            {
                select = "SELECT TOP 20 ";
            }

            select = string.IsNullOrEmpty(select) ?
                @"Select TaxPreferenceTypeID as ID,TaxPreferenceName as Value
                    From TaxPreference" :
            select += @"TaxPreferenceTypeID as ID,TaxPreferenceName as Value
                    From TaxPreference";

            whereCondition = !string.IsNullOrEmpty(model.SearchString) ?
                        $"Where IsDeleted=0 and  TaxPreferenceName like '{model.SearchString}%'" :
                        $"Where IsDeleted=0";


            string query = select + " " + whereCondition;
            var result = await _dbContext.GetListByQueryAsync<IdnValuePair>(query, null);
            return Ok(result);

        }

        [HttpPost("get-list-of-tax-categories")]
        public async Task<IActionResult> GetListOfTaxCategories(CommonSearchModel model)
        {
            string select = "";
            string whereCondition = "";

            int clientId = 0;
            if (CurrentClientID == 0)
            {
                clientId = PDV.ProgbizClientID;
            }
            else
            {
                clientId = CurrentClientID;
            }

            if (model.ReadDataOnSearch && string.IsNullOrEmpty(model.SearchString))
            {
                select = "SELECT TOP 20 ";
            }

            select = string.IsNullOrEmpty(select) ?
                "SELECT TaxCategoryID AS ID,TaxCategoryName AS Value FROM TaxCategory" :
                select += "TaxCategoryID AS ID,TaxCategoryName AS Value FROM TaxCategory";

            whereCondition = !string.IsNullOrEmpty(model.SearchString) ?
                $"WHERE IsDeleted=0 AND ClientID={clientId} AND TaxCategoryName LIKE '{model.SearchString}%'" :
                $"WHERE IsDeleted=0 AND ClientID={clientId}";

            string query = select + " " + whereCondition;
            var results = await _dbContext.GetListByQueryAsync<IdnValuePair>(query, null);
            return Ok(results);
        }

        [HttpPost("get-list-of-currencies")]
        public async Task<IActionResult> GetListOfCurrencies(CommonSearchModel model)
        {
            string select = "";
            string whereCondition = "";

            if (model.ReadDataOnSearch && string.IsNullOrEmpty(model.SearchString))
            {
                select = "SELECT TOP 20 ";
            }

            select = string.IsNullOrEmpty(select) ?
                "SELECT CurrencyID AS ID,Concat('[',Symbol,']',' - ',CurrencyName) As Value FROM Currency" :
                select += "CurrencyID AS ID,Concat('[',Symbol,']',' - ',CurrencyName) As Value FROM Currency";

            whereCondition = !string.IsNullOrEmpty(model.SearchString) ?
                $"WHERE IsDeleted=0 AND CurrencyName LIKE '{model.SearchString}%'" :
                $"WHERE IsDeleted=0";

            string query = select + " " + whereCondition;
            var results = await _dbContext.GetListByQueryAsync<IdnValuePair>(query, null);
            return Ok(results);
        }

        [HttpPost("get-list-of-game-master")]
        public async Task<IActionResult> GetListOfGameMaster(CommonSearchModel model)
        {
            string select = "";
            string whereCondition = "";

            select = string.IsNullOrEmpty(select) ?
                "SELECT GameID AS ID,GameName AS Value FROM GameMaster" :
                select += "GameID AS ID,GameName AS Value FROM GameMaster";

            whereCondition = !string.IsNullOrEmpty(model.SearchString) ?
                $"WHERE IsDeleted=0 AND GameName LIKE '{model.SearchString}%'" :
                $"WHERE IsDeleted=0";

            string query = select + " " + whereCondition;
            var results = await _dbContext.GetListByQueryAsync<IdnValuePair>(query, null);
            return Ok(results);
        }

        [HttpPost("get-list-of-place-of-supplies")]
        public async Task<IActionResult> GetListOfPlaceOfSupplies(CommonSearchModel model)
        {
            string select = "";
            string whereCondition = "";

            if (model.ReadDataOnSearch && string.IsNullOrEmpty(model.SearchString))
            {
                select = "SELECT TOP 20 ";
            }

            select = string.IsNullOrEmpty(select) ?
                "SELECT StateID AS ID,Concat('[',StateCode,']',' - ',StateName) AS Value FROM CountryState" :
                select += "StateID AS ID,Concat('[',StateCode,']',' - ',StateName) AS Value FROM CountryState";

            whereCondition = !string.IsNullOrEmpty(model.SearchString) ?
                $"WHERE IsDeleted=0 AND CountryID={model.ID} AND StateName LIKE '{model.SearchString}%'" :
                $"WHERE IsDeleted=0 AND CountryID={model.ID}";

            string query = select + " " + whereCondition;
            var results = await _dbContext.GetListByQueryAsync<IdnValuePair>(query, null);
            return Ok(results);
        }

        [HttpPost("get-list-of-hour-master")]
        public async Task<IActionResult> GetListOfHourMaster(CommonSearchModel model)
        {
            var results = await _dbContext.GetListByQueryAsync<IdnValuePair>("SELECT HourID AS ID,HourName AS Value FROM HourMaster WHERE IsDeleted=0", model);
            return Ok(results);
        }

        [HttpPost("get-list-of-membership-package")]
        public async Task<IActionResult> GetListOfMembershipPackages(CommonSearchModel model)
        {
            string select = "";
            if (model.ReadDataOnSearch && string.IsNullOrEmpty(model.SearchString))
                select = " TOP 20 ";

            string query = @$"Select {select} PackageID as ID,PackageName as Value 
                    From MembershipPackage
                    Where IsDeleted=0";

            if (!string.IsNullOrEmpty(model.SearchString))
                query += $" and Packagename like '{model.SearchString}%'";

            var result = await _dbContext.GetListByQueryAsync<IdnValuePair>(query, null);
            return Ok(result);
        }

        [HttpPost("get-list-of-court-customer")]
        public async Task<IActionResult> GetListOfHall(CommonSearchModel model)
        {
            string select = "";
            string whereCondition = "";

            if (model.ReadDataOnSearch && string.IsNullOrEmpty(model.SearchString))
            {
                select = "SELECT TOP 20 ";
            }

            select = string.IsNullOrEmpty(select) ?
                @"Select E.EntityID as ID,E.Name As Value
                    From viEntity E
                    JOIN CourtCustomer C on C.EntityID=E.EntityID" :
                select += @"E.EntityID as ID,E.Name As Value
                    From viEntity E
                    JOIN CourtCustomer C on C.EntityID=E.EntityID";

            whereCondition = !string.IsNullOrEmpty(model.SearchString) ?
                $"Where C.ClientID={CurrentClientID} and C.IsDeleted=0 and E.Name like '{model.SearchString}%'" :
                $"Where C.ClientID={CurrentClientID} and C.IsDeleted=0";

            string query = select + " " + whereCondition;
            var results = await _dbContext.GetListByQueryAsync<IdnValuePair>(query, null);
            return Ok(results);
        }

        [HttpPost("get-list-of-clients")]
        public async Task<IActionResult> GetListOfClients(CommonSearchModel model)
        {
            string select = "";
            string whereCondition = "";

            if (model.ReadDataOnSearch && string.IsNullOrEmpty(model.SearchString))
            {
                select = "SELECT TOP 20 ";
            }

            select = string.IsNullOrEmpty(select) ?
                @"Select C.ClientID as ID,Name as Value 
                From Client C
                Join viEntity V on V.ClientID=C.ClientID " :
                select += @"C.ClientID as ID,Name as Value 
                        From Client C
                        Join viEntity V on V.ClientID=C.ClientID";

            whereCondition = !string.IsNullOrEmpty(model.SearchString) ?
                $"Where C.IsDeleted=0 and Name like '{model.SearchString}%'" :
                $"Where C.IsDeleted=0";

            string query = select + " " + whereCondition;
            var results = await _dbContext.GetListByQueryAsync<IdnValuePair>(query, null);
            return Ok(results);
        }

        [HttpPost("get-list-of-account-groups")]
        public async Task<IActionResult> GetListOfAccountGroups(CommonSearchModel model)
        {
            string select = "";
            string whereCondition = "";

            if (model.ReadDataOnSearch && string.IsNullOrEmpty(model.SearchString))
            {
                select = "SELECT TOP 20 ";
            }

            select = string.IsNullOrEmpty(select) ?
                @"Select A.AccountGroupID as ID,A.AccountGroupName as Value 
                From AccAccountGroup A " :
                select += @"A.AccountGroupID as ID,A.AccountGroupName as Value 
                From AccAccountGroup A ";

            whereCondition = !string.IsNullOrEmpty(model.SearchString) ?
                $"Where A.IsDeleted=0 and A.ClientID={CurrentClientID} and (BranchID={CurrentBranchID} OR IsNull(BranchID,0)=0) and A.AccountGroupName like '{model.SearchString}%'" :
                $"Where A.IsDeleted=0 and A.ClientID={CurrentClientID} and (BranchID={CurrentBranchID} OR IsNull(BranchID,0)=0)";

            string query = select + " " + whereCondition;
            var results = await _dbContext.GetListByQueryAsync<IdnValuePair>(query, null);
            return Ok(results);
        }

        [HttpPost("get-list-of-account-group-types")]
        public async Task<IActionResult> GetListOfAccountTypes(CommonSearchModel model)
        {
            string select = "";
            string whereCondition = "";

            if (model.ReadDataOnSearch && string.IsNullOrEmpty(model.SearchString))
            {
                select = "SELECT TOP 20 ";
            }

            select = string.IsNullOrEmpty(select) ?
                @"Select A.GroupTypeID as ID,A.GroupTypeName as Value 
                From AccAccountGroupType A " :
                select += @"A.GroupTypeID as ID,A.GroupTypeName as Value 
                From AccAccountGroupType A ";

            whereCondition = !string.IsNullOrEmpty(model.SearchString) ?
                $"Where A.IsDeleted=0 and A.GroupTypeName like '{model.SearchString}%'" :
                $"Where A.IsDeleted=0";

            string query = select + " " + whereCondition;
            var results = await _dbContext.GetListByQueryAsync<IdnValuePair>(query, null);
            return Ok(results);
        }

        [HttpPost("get-list-of-court-package")]
        public async Task<IActionResult> GetListOfCourtPackage(CommonSearchModel model)
        {
            string select = "";
            string whereCondition = "";

            if (model.ReadDataOnSearch && string.IsNullOrEmpty(model.SearchString))
            {
                select = "SELECT TOP 20 ";
            }

            select = string.IsNullOrEmpty(select) ?
                @"Select CourtPackageID as ID,PackageName As Value
                    From CourtPackage" :
                select += @"CourtPackageID as ID,PackageName As Value
                            From CourtPackage";

            whereCondition = !string.IsNullOrEmpty(model.SearchString) ?
                $"Where ClientID={CurrentClientID} and IsDeleted=0 and PackageName like '{model.SearchString}%'" :
                $"Where ClientID={CurrentClientID} and IsDeleted=0";

            string query = select + " " + whereCondition;
            var result = await _dbContext.GetListByQueryAsync<IdnValuePair>(query, null);
            return Ok(result);
        }

        [HttpPost("get-list-of-voucher-type-natures")]
        public async Task<IActionResult> GetListOfVoucherTypeNatures(CommonSearchModel model)
        {
            string select = "";
            string whereCondition = "";

            if (model.ReadDataOnSearch && string.IsNullOrEmpty(model.SearchString))
            {
                select = "SELECT TOP 20 ";
            }

            select = string.IsNullOrEmpty(select) ?
                @"Select VoucherTypeNatureID as ID,VoucherTypeNatureName As Value
                    From AccVoucherTypeNature" :
                select += @"VoucherTypeNatureID as ID,VoucherTypeNatureName As Value
                    From AccVoucherTypeNature";

            whereCondition = !string.IsNullOrEmpty(model.SearchString) ?
                $"Where ShowInList=1 and IsDeleted=0 and VoucherTypeNatureName like '{model.SearchString}%'" :
                $"Where ShowInList=1 and IsDeleted=0";

            string query = select + " " + whereCondition;
            var result = await _dbContext.GetListByQueryAsync<IdnValuePair>(query, null);
            return Ok(result);
        }

        [HttpPost("get-list-of-aisc-members")]
        public async Task<IActionResult> GetAISCMembers(CommonSearchModel model)
        {
            string select = "";
            string whereCondition = "";

            if (model.ReadDataOnSearch && string.IsNullOrEmpty(model.SearchString))
            {
                select = "SELECT TOP 20 ";
            }

            select = string.IsNullOrEmpty(select) ?
                @"Select ID as ID,FirstName As Value 
                    From AISCMembership" :
                select += @"ID as ID,FirstName As Value 
                    From AISCMembership";

            whereCondition = !string.IsNullOrEmpty(model.SearchString) ?
                $"Where IsDeleted=0 and FirstName like '{model.SearchString}%'" :
                $"Where IsDeleted=0";

            string query = select + " " + whereCondition;
            var result = await _dbContext.GetListByQueryAsync<IdnValuePair>(query, null);
            return Ok(result);
        }

        [HttpPost("get-list-of-aisc-teams")]
        public async Task<IActionResult> GetAISCTeams(CommonSearchModel model)
        {
            string select = "";
            string whereCondition = "";

            if (model.ReadDataOnSearch && string.IsNullOrEmpty(model.SearchString))
            {
                select = "SELECT TOP 20 ";
            }

            select = string.IsNullOrEmpty(select) ?
                @"Select TeamID as ID,TeamName As Value 
                    From AISCTeam" :
                select += @"TeamID as ID,TeamName As Value 
                    From AISCTeam";

            whereCondition = !string.IsNullOrEmpty(model.SearchString) ?
                $"Where IsDeleted=0 and TeamName like '{model.SearchString}%'" :
                $"Where IsDeleted=0";

            string query = select + " " + whereCondition;
            var result = await _dbContext.GetListByQueryAsync<IdnValuePair>(query, null);
            return Ok(result);
        }

        [HttpPost("get-list-of-voucher-types")]
        public async Task<IActionResult> GetListOfVoucherTypes(CommonSearchModel model)
        {
            string select = "";
            string whereCondition = "";

            if (model.ReadDataOnSearch && string.IsNullOrEmpty(model.SearchString))
            {
                select = "SELECT TOP 20 ";
            }

            select = string.IsNullOrEmpty(select) ?
                @"Select V.VoucherTypeID as ID,V.VoucherTypeName As Value
                        From AccVoucherType V
                        Left Join AccVoucherTypeNature N ON N.VoucherTypeNatureID=V.VoucherTypeNatureID AND N.IsDeleted=0 " :
                select += @"V.VoucherTypeID as ID,V.VoucherTypeName As Value
                        From AccVoucherType V
                        Left Join AccVoucherTypeNature N ON N.VoucherTypeNatureID=V.VoucherTypeNatureID AND N.IsDeleted=0";

            whereCondition = !string.IsNullOrEmpty(model.SearchString) ?
                        $"Where V.ClientID=@ClientID AND N.ShowInList=1 AND V.IsDeleted=0 AND V.VoucherTypeName like @SearchString " :
                        $"Where V.ClientID=@ClientID AND N.ShowInList=1 AND V.IsDeleted=0";

            string query = select + " " + whereCondition;
            var result = await _dbContext.GetListByQueryAsync<IdnValuePair>(query, new { ClientID = (CurrentClientID == 0 ? PDV.ProgbizClientID : CurrentClientID), SearchString = '%' + model.SearchString + '%' });
            return Ok(result);
        }

        [HttpPost("get-list-of-ledgers")]
        public async Task<IActionResult> GetListOfLedgers(SearchLedgerModel model) 
        {
            string whereCondition = $"Where L.ClientID={CurrentClientID} AND (L.BranchID={CurrentBranchID} OR IsNull(L.BranchID,0)=0) AND L.IsDeleted=0 ";
            if (model.GroupTypeIdsIn.Count > 0)
            {
                string ledgerIDsInList = string.Join(',', model.GroupTypeIdsIn);
                whereCondition += $"  And T.GroupTypeID In ({ledgerIDsInList})";
            }
            if (model.GroupTypeIdsNotIn.Count > 0)
            {
                string ledgerIDsNotInList = string.Join(',', model.GroupTypeIdsNotIn);
                whereCondition += $" And T.GroupTypeID Not In ({ledgerIDsNotInList})";
            }

            string selectQuery = @$"Select Top 20 L.LedgerID as ID, L.LedgerName as Value
                                    From AccLedger L
                                    Join AccAccountGroup A ON A.AccountGroupID=L.AccountGroupID And A.IsDeleted=0
                                    Join AccAccountGroupType T ON T.GroupTypeID=A.GroupTypeID
                                    {whereCondition}";

            if (!model.ReadDataOnSearch)
            {
                selectQuery = @$"Select L.LedgerID as ID, L.LedgerName as Value
                                    From AccLedger L
                                    Join AccAccountGroup A ON A.AccountGroupID=L.AccountGroupID And A.IsDeleted=0
                                    Join AccAccountGroupType T ON T.GroupTypeID=A.GroupTypeID
                                    {whereCondition}";
            }

            if (model.ReadDataOnSearch && !string.IsNullOrEmpty(model.SearchString))
            {
                selectQuery = @$"Select L.LedgerID as ID, L.LedgerName as Value
                                    From AccLedger L
                                    Join AccAccountGroup A ON A.AccountGroupID=L.AccountGroupID And A.IsDeleted=0
                                    Join AccAccountGroupType T ON T.GroupTypeID=A.GroupTypeID
                                    {whereCondition} And (L.LedgerName like '{model.SearchString}%' Or L.LedgerCode like '{model.SearchString}%')";
            }
            var result = await _dbContext.GetListByQueryAsync<IdnValuePair>(selectQuery, null);
            return Ok(result ?? new());
        }

        [HttpPost("get-list-of-general-ledgers")]
        public async Task<IActionResult> GetListOfGeneralLedgers(LedgerSearchModel model)
        {
            string select = "";
            string? whereCondition = $"Where IsDeleted=0 And ClientID={CurrentClientID}";
            if (model.ReadDataOnSearch && string.IsNullOrEmpty(model.SearchString))
            {
                select = "SELECT TOP 20 ";
            }

            select = string.IsNullOrEmpty(select) ?
                @"Select LedgerID as ID,LedgerName As Value
                        From AccLedger" :
                select += @"LedgerID as ID,LedgerName As Value
                        From AccLedger";

            string query = select + " " + whereCondition;
            var result = await _dbContext.GetListByQueryAsync<IdnValuePair>(query, null);
            return Ok(result);
        }

        [HttpPost("get-list-of-tax-category-ledgers")]
        public async Task<IActionResult> GetListOfTaxCategoryLedgers(CommonSearchModel model)
        {
            string select = @$"Select Top 20 L.LedgerID as ID, L.LedgerName as Value
                                    From AccLedger L
                                    Join AccAccountGroup A ON A.AccountGroupID=L.AccountGroupID And A.IsDeleted=0
                                    Where L.ClientID={CurrentClientID} AND (L.BranchID={CurrentBranchID} OR IsNull(L.BranchID,0)=0) AND L.IsDeleted=0 And A.GroupTypeID={(int)AccountGroupTypes.DutiesAndTaxes}";

            if (model.ReadDataOnSearch && !string.IsNullOrEmpty(model.SearchString))
            {
                select = @$"Select L.LedgerID as ID, L.LedgerName as Value
                                    From AccLedger L
                                    Join AccAccountGroup A ON A.AccountGroupID=L.AccountGroupID And A.IsDeleted=0
                                    Where (L.LedgerName like @SearchString Or L.LedgerCode like @SearchString) And L.ClientID={CurrentClientID} AND (L.BranchID={CurrentBranchID} OR IsNull(L.BranchID,0)=0) AND IsDeleted=0 And A.GroupTypeID={(int)AccountGroupTypes.DutiesAndTaxes}";
            }
            var result = await _dbContext.GetListByQueryAsync<IdnValuePair>(select, new { SearchString = model.SearchString });
            return Ok(result);
        }

        [HttpPost("get-list-of-whatsapp-accounts")]
        public async Task<IActionResult> GetListOfWhatsappAccounts(CommonSearchModel model)
        {
            string select = "";
            string whereCondition = "";

            if (model.ReadDataOnSearch && string.IsNullOrEmpty(model.SearchString))
            {
                select = "SELECT TOP 20 ";
            }

            select = string.IsNullOrEmpty(select) ?
                @"Select WhatsappAccountID as ID,Name as Value 
                        From WhatsappAccount" :
            select += @"WhatsappAccountID as ID,Name as Value 
                        From WhatsappAccount";

            whereCondition = !string.IsNullOrEmpty(model.SearchString) ?
                $"Where IsDeleted=0 and ClientID={CurrentClientID} and Name like '{model.SearchString}%'" :
                $"Where IsDeleted=0 and ClientID={CurrentClientID}";

            string query = select + " " + whereCondition;
            var result = await _dbContext.GetListByQueryAsync<IdnValuePair>(query, null);
            return Ok(result);
        }

        [HttpPost("get-list-of-language")]
        public async Task<IActionResult> GetListOfLanguage(CommonSearchModel model)
        {
            string select = "";
            string whereCondition = "";

            if (model.ReadDataOnSearch && string.IsNullOrEmpty(model.SearchString))
            {
                select = "SELECT TOP 20 ";
            }

            select = string.IsNullOrEmpty(select) ?
                @"Select LanguageID as ID,LanguageName as Value 
                        From Language" :
            select += @"LanguageID as ID,LanguageName as Value 
                        From Language";

            whereCondition = !string.IsNullOrEmpty(model.SearchString) ?
                $"Where IsDeleted=0  and LanguageName like '{model.SearchString}%'" :
                $"Where IsDeleted=0 ";

            string query = select + " " + whereCondition;
            var result = await _dbContext.GetListByQueryAsync<IdnValuePair>(query, null);
            return Ok(result);
        }

        [HttpPost("get-list-of-branches")]
        public async Task<IActionResult> GetListOfBranches(CommonSearchModel model)
        {
            string select = "";
            string whereCondition = "";

            if (model.ReadDataOnSearch && string.IsNullOrEmpty(model.SearchString))
            {
                select = "SELECT TOP 20 ";
            }

            select = string.IsNullOrEmpty(select) ?
                @"Select BranchID as ID,BranchName as Value 
                        From viBranch" :
            select += @"BranchID as ID,BranchName as Value  
                        From viBranch";

            whereCondition = !string.IsNullOrEmpty(model.SearchString) ?
                $"Where ClientID={CurrentClientID} and BranchName like '{model.SearchString}%'" :
                $"Where ClientID={CurrentClientID}";

            string query = select + " " + whereCondition;
            var result = await _dbContext.GetListByQueryAsync<IdnValuePair>(query, null);
            return Ok(result);
        }


        [HttpPost("get-list-of-ledger")]
        public async Task<IActionResult> GetListLedger(CommonSearchModel model)
        {
            string select = "";
            string whereCondition = "";

            if (model.ReadDataOnSearch && string.IsNullOrEmpty(model.SearchString))
            {
                select = "SELECT TOP 20 ";
            }

            select = string.IsNullOrEmpty(select) ?
                @"Select LedgerID as ID,LedgerName as Value
                    From AccLedger" :
                select += @"LedgerID,LedgerName 
                    From AccLedger";

            whereCondition = !string.IsNullOrEmpty(model.SearchString) ?
                        $"Where ClientID={CurrentClientID} AND IsDeleted=0 AND LedgerName like '{model.SearchString}' " :
                        $"Where ClientID={CurrentClientID} AND IsDeleted=0";

            string query = select + " " + whereCondition;
            var result = await _dbContext.GetListByQueryAsync<IdnValuePair>(query, null);
            return Ok(result);
        }

        [HttpPost("get-list-of-invoice-types")]
        public async Task<IActionResult> GetListOfInvoiceTypes(CommonSearchModel model)
        {
            string select = "";
            string whereCondition = "";

            if (model.ReadDataOnSearch && string.IsNullOrEmpty(model.SearchString))
            {
                select = "SELECT TOP 20 ";
            }

            select = string.IsNullOrEmpty(select) ?
                @"Select InvoiceTypeID as ID,InvoiceTypeName as Value
                    From InvoiceType" :
                select += @"InvoiceTypeID as ID,InvoiceTypeName as Value
                    From InvoiceType";

            whereCondition = !string.IsNullOrEmpty(model.SearchString) ?
                        $"Where ClientID={CurrentClientID} AND IsDeleted=0 AND InvoiceTypeNatureID<>{(int)InvoiceTypeNatures.Stock_Adjustment} AND InvoiceTypeName like @InvoiceTypeName " :
                        $"Where ClientID={CurrentClientID} AND IsDeleted=0 AND InvoiceTypeNatureID<>{(int)InvoiceTypeNatures.Stock_Adjustment}";

            string query = select + " " + whereCondition;
            var result = await _dbContext.GetListByQueryAsync<IdnValuePair>(query, new { InvoiceTypeName = '%' + model.SearchString + '%' });
            return Ok(result);
        }

        [HttpPost("get-list-of-price-groups")]
        public async Task<IActionResult> GetListOfPriceGroups(CommonSearchModel model)
        {
            string select = "";
            string whereCondition = "";

            if (model.ReadDataOnSearch && string.IsNullOrEmpty(model.SearchString))
            {
                select = "SELECT TOP 20 ";
            }

            select = string.IsNullOrEmpty(select) ?
                @"Select PriceGroupID as ID,PriceGroupName as Value 
                    From CourtPriceGroup" :
            select += @"PriceGroupID as ID,PriceGroupName as Value 
                        From CourtPriceGroup";

            whereCondition = !string.IsNullOrEmpty(model.SearchString) ?
                $"Where IsDeleted=0 and ClientId={CurrentClientID} and PriceGroupName like '{model.SearchString}%'" :
                $"Where IsDeleted=0 and ClientId={CurrentClientID}";

            string query = select + " " + whereCondition;
            var result = await _dbContext.GetListByQueryAsync<IdnValuePair>(query, null);
            return Ok(result);
        }


        [HttpPost("get-list-of-payment-terms")]
        public async Task<IActionResult> GetListOfPaymentTerms(CommonSearchModel model)
        {
            string select = "";
            string whereCondition = "";

            if (model.ReadDataOnSearch && string.IsNullOrEmpty(model.SearchString))
            {
                select = "SELECT TOP 20 ";
            }

            select = string.IsNullOrEmpty(select) ?
                @"Select PaymentTermID as ID,PaymentTermName as Value 
                        From PaymentTerm" :
            select += @"PaymentTermID as ID,PaymentTermName as Value 
                        From PaymentTerm";

            whereCondition = !string.IsNullOrEmpty(model.SearchString) ?
                $"Where ClientID={CurrentClientID} and IsDeleted=0  and LanguageName like '{model.SearchString}%'" :
                $"Where ClientID={CurrentClientID} and IsDeleted=0 ";

            string query = select + " " + whereCondition;
            var result = await _dbContext.GetListByQueryAsync<IdnValuePair>(query, null);
            return Ok(result);
        }



        #endregion

        #region Common API's

        [AllowAnonymous]
        [HttpGet("get-country-isd-code/{countryID}")]
        public async Task<IActionResult> GetISD(int countryID)
        {
            var res = await _dbContext.GetByQueryAsync<StringModel>(@$"Select ISDCode as Value 
                                                                        From Country
                                                                        Where CountryID={countryID} and IsDeleted=0
            ", null);

            return Ok(res ?? new());
        }

        [AllowAnonymous]
        [HttpGet("get-default-country-details")]
        public async Task<IActionResult> GetCountry()
        {
            var res = await _dbContext.GetByQueryAsync<Country>(@$"
                                                            Select C.CountryID,C.CountryName,C.ISDCode
                                                            From viBranch B
                                                            Left Join CountryZone Z ON Z.ZoneID=B.ZoneID AND Z.IsDeleted=0
                                                            Left Join Country C ON C.CountryID=Z.CountryID AND C.IsDeleted=0
                                                            Where B.BranchID={CurrentBranchID}
            ", null);
            res = res ?? new();

            return Ok(res);
        }

        [HttpGet("get-country-details/{countryID}")]
        public async Task<IActionResult> GetCountryDetails(int countryID)
        {
            return Ok(await _dbContext.GetAsync<Country>(countryID));
        }

        [AllowAnonymous]
        [HttpGet("get-customer-country-isd-code/{customerEntityId}")]
        public async Task<IActionResult> GetCustomerISD(int customerEntityId)
        {
            var countryId = await _dbContext.GetByQueryAsync<int?>(@$"Select CountryID From Entity
															        Where EntityID={customerEntityId} and IsDeleted=0", null);
            var res = new StringModel();
            if (countryId != null)
            {
                res = await _dbContext.GetByQueryAsync<StringModel>(@$"Select ISDCode as Value 
                                                                        From Country
                                                                        Where CountryID={countryId} and IsDeleted=0", null);
            }

            return Ok(res);
        }

        [HttpGet("get-list-of-countries-with-code")]
        public async Task<IActionResult> GetListOfCountriesWithCode()
        {
            var results = await _dbContext.GetListByQueryAsync<IdnValuePair>($@"Select CountryID as ID,Code3 +' +'+ISDCode as Value 
                                                                                From Country
                                                                                Where IsDeleted=0", null);
            return Ok(results);
        }

        [HttpPost("send-pdf-document")]
        public async Task<IActionResult> SendInvoiceDocument(MailDetailsModel pdfSendModel)
        {
            pdfSendModel.ClientID = CurrentClientID;
            pdfSendModel.BranchID = CurrentBranchID;
            await _common.SendPdfDocument(pdfSendModel);
            return Ok(new Success());
        }

        [HttpGet("get-acount-group-types")]
        public async Task<IActionResult> GetAccountGroupTypes()
        {
            var result = await _dbContext.GetListOfIdValuePairAsync<AccAccountGroupType>("GroupTypeName", $"IsDeleted=0", null);
            return Ok(result ?? new());
        }

        #endregion

        [HttpGet("insert-default-client-accounts-entries/{clientID}")]
        public async Task<IActionResult> InsertClientAccountsEntries(int clientID)
        {
            await _accounts.InsertClientDefaultAccountsRelatedEntries(clientID);
            return Ok(new Success());
        }

        [HttpGet("insert-default-client-inventory-entries/{clientID}")]
        public async Task<IActionResult> InsertClientInventoryEntries(int clientID)
        {
            await _inventory.InsertClientInventoryDefaultEntries(clientID);
            return Ok(new Success());
        }

        [HttpGet("insert-all-tax-category-item-ledgers")]
        public async Task<IActionResult> InsertTaxCategoryItemLedgers()
        {
            List<TaxCategoryItem> taxCategoryItems = await _dbContext.GetListAsync<TaxCategoryItem>(null, "1 desc");
            foreach (var taxCategoryItem in taxCategoryItems)
            {
                int? clientID = await _dbContext.GetByQueryAsync<int?>($"Select ClientID From TaxCategory Where TaxCategoryID={taxCategoryItem.TaxCategoryID.Value}", null);
                int? accountGroupID = await _dbContext.GetByQueryAsync<int?>(@$"Select Top 1 AccountGroupID
                                                                                From AccAccountGroup
                                                                                Where ClientID=@ClientID And GroupTypeID=@GroupTypeID And IsDeleted=0", new { ClientID = clientID, GroupTypeID = (int)AccountGroupTypes.DutiesAndTaxes });
                string ledgerCode = "";
                string ledgerName = "";
                if (!string.IsNullOrEmpty(taxCategoryItem.TaxCategoryItemName))
                {
                    ledgerCode = taxCategoryItem.TaxCategoryItemName.Replace(" ", "-");
                    if (!ledgerCode.Contains($"{(int)Math.Floor(taxCategoryItem.Percentage)}"))
                        ledgerCode += "-" + (int)Math.Floor(taxCategoryItem.Percentage);

                    ledgerName = taxCategoryItem.TaxCategoryItemName;
                    if (!ledgerName.Contains($"{(int)Math.Floor(taxCategoryItem.Percentage)}"))
                        ledgerName += "-" + (int)Math.Floor(taxCategoryItem.Percentage);

                }

                AccLedger accLedger = new()
                {
                    AccountGroupID = accountGroupID,
                    LedgerCode = ledgerCode,
                    LedgerName = ledgerName,
                    Alias = taxCategoryItem.TaxCategoryItemName,
                    ClientID = clientID
                };
                accLedger.LedgerID = await _dbContext.SaveAsync(accLedger);
                taxCategoryItem.LedgerID = accLedger.LedgerID;
                await _dbContext.SaveAsync(taxCategoryItem);
            }
            return Ok(true);
        }

        [HttpGet("get-branch-list")]
        public async Task<IActionResult> GetBranchList()
        {

            var result = await _dbContext.GetListByQueryAsync<IdnValuePair>(@$"
                                                            Select BranchID AS ID,BranchName AS Value
                                                            From viBranch
                                                            Where ClientID={CurrentClientID}", null);

            return Ok(result);
        }

        [HttpGet("get-branch-users")]
        public async Task<IActionResult> GetBranchUsers()
        {

            var result = await _dbContext.GetListByQueryAsync<EnquiryAssigneeModel>(@$"Select E.EntityID,E.Name
                                                                                From BranchUser BU
                                                                                Join Users U ON U.UserID=BU.UserID
                                                                                Join viEntity E ON E.EntityID=U.EntityID
                                                                                Where BU.BranchID={CurrentBranchID} And BU.IsDeleted=0", null);

            return Ok(result);
        }

        [HttpPost("create-admin-client")]
        public async Task<IActionResult> CreateAdminClient(RegistrationModel model)
        {
            int count = await _dbContext.GetByQueryAsync<int>(@$"Select Count(*) From Client Where IsDeleted=0", null);
            if (count > 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ResponseErrorDescription = "The admin client is already added, you cant add it again"
                });
            }

            _cn.Open();
            using (var tran = _cn.BeginTransaction())
            {
                try
                {
                    var result = await _whiteLabel.CreateSuperAdminClient(model, tran);

                    tran.Commit();

                    return Ok(result);

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

        [HttpGet("import-all-wa-account-templates")]
        [AllowAnonymous]
        public async Task<IActionResult> ImportAllWAAccountTemplates()
        {
            try
            {
                _job.Enqueue(() => _superAdmin.ImportAllWAAccountTemplates());

                return Ok(new BaseSuccessResponse()
                {
                    ResponseMessage = "SyncBackgroundProcessStarted"
                });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("get-client-users")]
        public async Task<IActionResult> GetClientUsers()
        {
            var result = await _dbContext.GetListByQueryAsync<IdnValuePair>(@$"Select U.EntityID As ID,E.Name As Value
                                                                                From Users U
                                                                                Join viEntity E ON E.EntityID=U.EntityID
                                                                                Where U.ClientID={CurrentClientID} And U.IsDeleted=0 And U.UserTypeID>{(int)UserTypes.Client}", null);
            return Ok(result ?? new());
        }

        #region Entity 

        [HttpGet("get-entity-address/{addressID}")]
        public async Task<IActionResult> GetEntityAddress(int addressID)
        {
            return Ok(await _entity.GetEntityAddress(addressID));
        }

        [HttpPost("save-entity-address")]
        public async Task<IActionResult> SaveEntityAddress(AddressModel addressModel)
        {
            var address = _mapper.Map<EntityAddressCustom>(addressModel);
            address.AddressID = await _dbContext.SaveAsync(address);
            AddressAddResultModel returnObject = new() { AddressID = address.AddressID };
            returnObject.CompleteAddress = (await _entity.GetAddressView(returnObject.AddressID)).CompleteAddress;
            return Ok(returnObject);
        }

        [HttpGet("get-entity-address-view/{addressID}")]
        public async Task<IActionResult> GetCustomerAddressView(int addressID)
        {
            AddressView customerAddressView = await _entity.GetAddressView(addressID);
            return Ok(customerAddressView);
        }

        [HttpGet("delete-entity-address/{addressID}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteAddress(int addressID)
        {
            try
            {
                await _dbContext.ExecuteAsync($@"Update EntityAddress Set IsDeleted=1 Where AddressID={addressID}");
                return Ok(true);
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

        #region Ledger dropdown api

        [HttpPost("get-list-of-all-ledgers")]
        public async Task<IActionResult> GetListOfAllLedgers(SearchLedgerModel model)
        {
            string selectQuery = @$"Select Top 20 L.LedgerID as ID, L.LedgerName as Value
                                    From AccLedger L
                                    Join AccAccountGroup A ON A.AccountGroupID=L.AccountGroupID And A.IsDeleted=0
                                    Where L.ClientID={CurrentClientID} AND (L.BranchID={CurrentBranchID} OR IsNull(L.BranchID,0)=0) AND L.IsDeleted=0 ";

            if (!model.ReadDataOnSearch)
            {
                selectQuery = @$"Select L.LedgerID as ID, L.LedgerName as Value
                                    From AccLedger L
                                    Join AccAccountGroup A ON A.AccountGroupID=L.AccountGroupID And A.IsDeleted=0
                                    Where L.ClientID={CurrentClientID} AND (L.BranchID={CurrentBranchID} OR IsNull(L.BranchID,0)=0) AND L.IsDeleted=0 ";
            }

            if (model.ReadDataOnSearch && !string.IsNullOrEmpty(model.SearchString))
            {
                selectQuery = @$"Select L.LedgerID as ID, L.LedgerName as Value
                                    From AccLedger L
                                    Join AccAccountGroup A ON A.AccountGroupID=L.AccountGroupID And A.IsDeleted=0
                                    Where L.ClientID={CurrentClientID} AND (L.BranchID={CurrentBranchID} OR IsNull(L.BranchID,0)=0) AND L.IsDeleted=0 And (L.LedgerName like '{model.SearchString}%' Or L.LedgerCode like '{model.SearchString}%') ";
            }

            if (model.GroupTypeIdsIn.Count > 0 && model.GroupTypeIdsIn != null)
            {
                selectQuery += $" And A.GroupTypeID IN({string.Join(',', model.GroupTypeIdsIn)})";
            }
            if (model.GroupTypeIdsNotIn.Count > 0 && model.GroupTypeIdsNotIn != null)
            {
                selectQuery += $" And A.GroupTypeID NOT IN({string.Join(',', model.GroupTypeIdsNotIn)})";
            }

            var result = await _dbContext.GetListByQueryAsync<IdnValuePair>(selectQuery, null);
            return Ok(result);
        }

        #endregion

        #region Item Dropdown Api

        [HttpPost("get-list-of-all-items")]
        public async Task<IActionResult> GetListOfAllItems(CommonSearchModel model)
        {
            string select = "";
            string whereCondition = "";

            if (model.ReadDataOnSearch && string.IsNullOrEmpty(model.SearchString))
            {
                select = "SELECT TOP 20 ";
            }

            select = string.IsNullOrEmpty(select) ?
                @" Select I.ItemID as ID,I.ItemName as  Value
                    From viItem I" :
            select += @"I.ItemID as ID,I.ItemName as  Value
                    From viItem I";

            whereCondition = !string.IsNullOrEmpty(model.SearchString) ?
                $"Where I.ClientID={CurrentClientID}  AND I.ItemName like '{model.SearchString}%'" :
                $"Where I.ClientID={CurrentClientID} ";

            string query = select + " " + whereCondition;

            var result = await _dbContext.GetListByQueryAsync<IdnValuePair>(query, null);
            return Ok(result);
        }


        [HttpPost("get-list-of-item-varients")]
        public async Task<IActionResult> GetListOfItemVarient(CommonSearchModel model)
        {
            string select = "";
            string whereCondition = "";

            if (model.ReadDataOnSearch && string.IsNullOrEmpty(model.SearchString))
            {
                select = "SELECT TOP 20 ";
            }

            select = string.IsNullOrEmpty(select) ?
                @" Select ItemVariantID as ID,ItemName as Value
                    From ItemVariant IM
                    Left Join Item I on I.ItemID=IM.ItemID and I.IsDeleted=0" :
            select += @" ItemVariantID as ID,ItemName as Value
                    From ItemVariant IM
                    Left Join Item I on I.ItemID=IM.ItemID and I.IsDeleted=0";

            whereCondition = !string.IsNullOrEmpty(model.SearchString) ?
                $"Where I.ClientID={CurrentClientID}  AND I.ItemName like '{model.SearchString}%'" :
                $"Where I.ClientID={CurrentClientID} ";

            string query = select + " " + whereCondition;

            var result = await _dbContext.GetListByQueryAsync<IdnValuePair>(query, null);
            return Ok(result);
        }


        #endregion

    }
}
