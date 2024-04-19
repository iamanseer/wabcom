using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PB.DatabaseFramework;
using PB.EntityFramework;
using PB.Server.Repository.eCommerce;
using PB.Server.Repository;
using System.Data;
using PB.Model;
using PB.Shared.Models.Common;
using PB.Shared.Models.Inventory.Items;
using PB.Shared.Tables.Inventory.Items;
using PB.Shared.Models.eCommerce.Product;
using PB.Shared.Tables.eCommerce.Products;
using PB.Shared.Models.eCommerce.Common;
using PB.Shared.Models.Court;
using PB.Shared.Models;

namespace PB.Server.Controllers.eCommerce
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ProductController : BaseController
    {
        private readonly IDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IDbConnection _cn;
        private readonly IConfiguration _config;
        private readonly IEmailSender _email;
        private readonly IUserRepository _user;
        private readonly IEC_CommonRepository _ecommon;
        public ProductController(IDbContext dbContext, IMapper mapper, IDbConnection cn, IConfiguration config, IEmailSender email, IUserRepository user, IEC_CommonRepository ecommon)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _cn = cn;
            _config = config;
            _email = email;
            _user = user;
            _ecommon = ecommon;
        }

        #region Item Category

        [HttpPost("save-ec-item-category")]
        public async Task<IActionResult> SaveECItemCategory(EC_ItemCategoryModel model)
        {
            var itemCategoryCount = await _dbContext.GetByQueryAsync<int>(@$"Select Count(*) 
                                                                            from EC_ItemCategory
                                                                            where LOWER(CategoryName)=LOWER(@CategoryName) and CategoryID<>@CategoryID and IsDeleted=0", model);
            if (itemCategoryCount != 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "InvalidSubmission",
                    ResponseMessage = "ItemCategoryExist"
                });
            }

            EC_ItemCategory itemGroup = _mapper.Map<EC_ItemCategory>(model);
            itemGroup.CategoryID = await _dbContext.SaveAsync(itemGroup);
            EC_ItemCategorySuccessModel itemCategorySuccessModel = new()
            {
                CategoryID = itemGroup.CategoryID,
                CategoryName = itemGroup.CategoryName
            };
            return Ok(itemCategorySuccessModel);
        }

        [HttpGet("delete-ec-item-category/{categoryID}")]
        public async Task<IActionResult> DeleteItemCategory(int categoryID)
        {
            await _dbContext.ExecuteAsync($"Update EC_ItemCategory Set IsDeleted=1 Where CategoryID={categoryID}", null);
            return Ok(true);
        }

        [HttpGet("get-ec-item-category/{categoryID}")]
        public async Task<IActionResult> GetItemCategory(int categoryID)
        {
            return Ok(await _dbContext.GetByQueryAsync<EC_ItemCategoryModel>($"Select * From ItemCategory Where CategoryID={categoryID}", null));
        }

        [HttpPost("get-ec-item-category-paged-list")]
        public async Task<IActionResult> GetItemCategoryPagedList(PagedListPostModelWithFilter pagedListPostModel)
        {
            PagedListQueryModel queryModel = new();
            queryModel.Select = @"Select C.CategoryID, C.CategoryName, C.ParentID, P.CategoryName As ParentCategoryName,C.MediaID,FileName
                                From EC_ItemCategory C
                                Left Join ItemCategory P ON P.CategoryID=C.ParentID
                                Left Join Media M on M.MediaID=C.MediaID and M.IsDeleted=0";
            queryModel.WhereCondition = $"C.IsDeleted=0";
            queryModel.OrderByFieldName = pagedListPostModel.OrderByFieldName;
            queryModel.SearchLikeColumnNames = new() { "C.CategoryName" };
            queryModel.SearchString = pagedListPostModel.SearchString;
            queryModel.WhereCondition += _ecommon.GeneratePagedListFilterWhereCondition(pagedListPostModel, "C.CategoryName");
            var itemCategoryList = await _dbContext.GetPagedList<EC_ItemCategoryViewModel>(queryModel, null);
            return Ok(itemCategoryList ?? new());
        }
        #endregion

    }
}
