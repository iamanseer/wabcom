using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PB.DatabaseFramework;
using PB.EntityFramework;
using PB.Server.Repository.eCommerce;
using PB.Server.Repository;
using System.Data;
using PB.Model.Models;
using PB.Model;

namespace PB.Server.Controllers.eCommerce
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class EC_CommonController : BaseController
    {
        private readonly IDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IDbConnection _cn;
        private readonly IConfiguration _config;
        private readonly IEmailSender _email;
        public EC_CommonController(IDbContext dbContext, IMapper mapper, IDbConnection cn, IConfiguration config, IEmailSender email)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _cn = cn;
            _config = config;
            _email = email;
        }


        [HttpPost("get-list-of-ec-item-varients")]
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
                    From EC_ItemVariant IM
                    Left Join EC_Item I on I.ItemID=IM.ItemID and I.IsDeleted=0" :
            select += @" ItemVariantID as ID,ItemName as Value
                    From EC_ItemVariant IM
                    Left Join EC_Item I on I.ItemID=IM.ItemID and I.IsDeleted=0";

            whereCondition = !string.IsNullOrEmpty(model.SearchString) ?
                $"Where IM.IsDeleted=0 AND I.ItemName like '{model.SearchString}%'" :
                $"Where IM.IsDeleted=0 ";

            string query = select + " " + whereCondition;

            var result = await _dbContext.GetListByQueryAsync<IdnValuePair>(query, null);
            return Ok(result ?? new());
        }

        [HttpPost("get-list-of-ec-items")]
        public async Task<IActionResult> GetListOfItems(CommonSearchModel model)
        {
            string select = "";
            string whereCondition = "";

            if (model.ReadDataOnSearch && string.IsNullOrEmpty(model.SearchString))
            {
                select = "SELECT TOP 20 ";
            }

            select = string.IsNullOrEmpty(select) ?
                @" Select ItemID as ID,ItemName as Value
                    From EC_Item " :
            select += @" ItemID as ID,ItemName as Value
                    From EC_Item";

            whereCondition = !string.IsNullOrEmpty(model.SearchString) ?
                $"Where IsDeleted=0 AND ItemName like '{model.SearchString}%'" :
                $"Where IsDeleted=0 ";

            string query = select + " " + whereCondition;

            var result = await _dbContext.GetListByQueryAsync<IdnValuePair>(query, null);
            return Ok(result ?? new());
        }


        [HttpPost("get-list-of-ec-item-category")]
        public async Task<IActionResult> GetListOfItemCategory(CommonSearchModel model)
        {
            string select = "";
            string whereCondition = "";

            if (model.ReadDataOnSearch && string.IsNullOrEmpty(model.SearchString))
            {
                select = "SELECT TOP 20 ";
            }

            select = string.IsNullOrEmpty(select) ?
                @" Select CategoryID as ID,CategoryName as Value
                    From EC_ItemCategory " :
            select += @" CategoryID as ID,CategoryName as Value
                    From EC_ItemCategory";

            whereCondition = !string.IsNullOrEmpty(model.SearchString) ?
                $"Where IsDeleted=0 AND CategoryName like '{model.SearchString}%'" :
                $"Where IsDeleted=0 ";

            string query = select + " " + whereCondition;

            var result = await _dbContext.GetListByQueryAsync<IdnValuePair>(query, null);
            return Ok(result ?? new());
        }




    }
}
