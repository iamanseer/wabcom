using AutoMapper;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NPOI.SS.Formula.Functions;
using PB.Client.Pages.Settings;
using PB.Client.Pages.SuperAdmin.Client;
using PB.DatabaseFramework;
using PB.EntityFramework;
using PB.Model;
using PB.Model.Models;
using PB.Server.Repository;
using PB.Server.Repository.eCommerce;
using PB.Shared;
using PB.Shared.Enum;
using PB.Shared.Helpers;
using PB.Shared.Models;
using PB.Shared.Models.CRM.Customer;
using PB.Shared.Models.eCommerce.Cart;
using PB.Shared.Models.eCommerce.Common;
using PB.Shared.Models.eCommerce.Customers;
using PB.Shared.Models.eCommerce.Product;
using PB.Shared.Models.eCommerce.WishList;
using PB.Shared.Models.Inventory.Items;
using PB.Shared.Tables;
using PB.Shared.Tables.Accounts.JournalMaster;
using PB.Shared.Tables.CourtClient;
using PB.Shared.Tables.eCommerce.Customer;
using PB.Shared.Tables.eCommerce.Entity;
using PB.Shared.Tables.eCommerce.Products;
using PB.Shared.Tables.eCommerce.Users;
using System.Data;
using System.Reflection.Metadata.Ecma335;
using static NPOI.HSSF.Util.HSSFColor;

namespace PB.Server.Controllers.eCommerce
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class EC_CustomerController : BaseController
    {
        private readonly IDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IDbConnection _cn;
        private readonly IConfiguration _config;
        private readonly IEmailSender _email;
        private readonly IUserRepository _user;
        private readonly IEC_CommonRepository _ecommon;
        public EC_CustomerController(IDbContext dbContext, IMapper mapper, IDbConnection cn, IConfiguration config, IEmailSender email, IUserRepository user, IEC_CommonRepository ecommon)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _cn = cn;
            _config = config;
            _email = email;
            _user = user;
            _ecommon = ecommon;
        }

        #region Login
        [AllowAnonymous]
        [HttpPost("customer-login")]
        public async Task<IActionResult> Generate(EC_CustomerPhoneVerifyPostModel model)
        {
            var userData = await _dbContext.GetByQueryAsync<EC_Users>($@"Select U.* 
                                                                        From EC_Users U
                                                                        Left Join EC_Entity E on E.EntityID=U.EntityID and E.Isdeleted=0
                                                                        Where E.Phone=@Phone and U.IsDeleted=0 and  E.EntityTypeID={(int)EntityType.Customer}", new { Phone = model.PhoneNo });

            var getOtp = await _ecommon.GenerateOtp(model);
            var res = await _dbContext.ExecuteAsync($@"Update EC_users Set OTP=@OTP,OTPGeneratedAt=@AddedOn Where UserID={userData.UserID}", new { OTP = getOtp.OTP, AddedOn = getOtp.OtpGeneratedOn });
            return Ok(new Success());
        }
        #endregion

        #region Customer

        [AllowAnonymous]
        [HttpPost("verify-ec-customer-phone")]
        public async Task<IActionResult> VerifyPhone(EC_CustomerPhoneVerifyPostModel model)
        {
            PhoneNoVerificationResultModel returnObject = new();

            //1. Fetch user from User table that match the phone number in the entity table.
            //2. if not exist insert entries entity, customer and user (otp generation and sending)
            //3. if exist then genrateotp and send
            //4. return DateTime otpgeneratedat

            var userData = await _dbContext.GetByQueryAsync<EC_Users>($@"Select U.* 
                                                                        From EC_Users U
                                                                        Left Join EC_Entity E on E.EntityID=U.EntityID and E.Isdeleted=0
                                                                        Where E.Phone=@Phone and U.IsDeleted=0 and  E.EntityTypeID={(int)EntityType.Customer}", new { Phone = model.PhoneNo });
            if (userData == null)
            {
                EC_Entity entity = new()
                {
                    EntityTypeID = (int)EntityType.Customer,
                    Phone = model.PhoneNo,
                };
                entity.EntityID = await _dbContext.SaveAsync(entity);
                var getOtp = await _ecommon.GenerateOtp(model);
                EC_Customer customer = new()
                {
                    EntityID = entity.EntityID,
                    HasLogin = false,
                };
                customer.CustomerID = await _dbContext.SaveAsync(customer);
                EC_Users users = new()
                {
                    EntityID = entity.EntityID,
                    UserTypeID = (int)UserTypes.Customer,
                    UserName = model.PhoneNo,
                    Password = "12345",
                    BranchID = CurrentBranchID,
                    LoginStatus = false,
                    OTP = getOtp.OTP,
                    OTPGeneratedAt = getOtp.OtpGeneratedOn,
                    IsVerified = false,

                };
                users.UserID = await _ecommon.SaveUser(users, null, null, null);
                returnObject = new()
                {
                    OtpAddedOn = getOtp.OtpGeneratedOn,
                    ResponseTitle = "Done",
                    ResponseMessage = "Otp generated successfully"

                };
            }
            else
            {

                var getOtp = await _ecommon.GenerateOtp(model);
                var res = await _dbContext.ExecuteAsync($@"Update EC_Users Set OTP=@OTP,OTPGeneratedAt=@AddedOn Where UserID={userData.UserID}", new { OTP = getOtp.OTP, AddedOn = getOtp.OtpGeneratedOn });
                returnObject = new()
                {
                    OtpAddedOn = getOtp.OtpGeneratedOn,
                    ResponseTitle = "Done",
                    ResponseMessage = "Customer otp is updated"

                };
            }
            return Ok(returnObject);
        }

        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp(EC_CustomerPhoneVerifyPostModel model)
        {
            var userData = await _dbContext.GetByQueryAsync<EC_Users>($@"Select U.* 
                                                                        From EC_Users U
                                                                        Left Join EC_Entity E on E.EntityID=U.EntityID and E.Isdeleted=0
                                                                        Where E.Phone=@Phone and U.IsDeleted=0 and  E.EntityTypeID={(int)EntityType.Customer}", new { Phone = model.PhoneNo });

            var getOtp = await _ecommon.GenerateOtp(model);
            var res = await _dbContext.ExecuteAsync($@"Update EC_Users Set OTP=@OTP,OTPGeneratedAt=@AddedOn Where UserID={userData.UserID}", new { OTP = getOtp.OTP, AddedOn = getOtp.OtpGeneratedOn });

            return Ok(new Success());
        }


        [AllowAnonymous]
        [HttpPost("verify-ec-customer-otp")]
        public async Task<IActionResult> VerifyCustomerOtp(EC_CustomerOtpPostModel model)
        {
            //1. Fetch user from User table that match the phone number in the entity table.
            //2. Compare current time and user otpgenratedat greater than 5 minutes > return badrequest
            //3. otp does not match > return badrequest
            //4. if ok set otp,otpgenratedat=null and accesstoken,refreshtoken as response

            var userData = await _dbContext.GetByQueryAsync<EC_Users>($@"Select U.* 
                                                                        From EC_Users U
                                                                        Left Join EC_Entity E on E.EntityID=U.EntityID and E.Isdeleted=0
                                                                        Where E.Phone=@Phone and U.IsDeleted=0 and  E.EntityTypeID={(int)EntityType.Customer}", new { Phone = model.PhoneNo });
            DateTime? otpTimes = userData.OTPGeneratedAt.Value.AddMinutes(5);
            DateTime? currentTime = DateTime.UtcNow;

            if (currentTime > otpTimes)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "Invalid submission",
                    ResponseMessage = "Your otp is expired, please resend otp and try again"
                });
            }
            if (model.Otp != userData.OTP)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "Invalid submission",
                    ResponseMessage = "Please check the otp"
                });
            }

            var res = await _dbContext.ExecuteAsync($@"Update EC_Users Set OTP=@OTP,OTPGeneratedAt=@AddedOn,LoginStatus=1 Where UserID={userData.UserID}", new { OTP = "", AddedOn = "" });
            var response = await _ecommon.GetToken(userData.UserID, 0, 0);
            return Ok(response??new());
        }

        [HttpPost("save-ec-customer")]
        public async Task<IActionResult> SaveCustomer(EC_CustomerModel model)
        {
            //email verification
            string emailAddress = await _dbContext.GetByQueryAsync<string>(@$"Select EmailAddress 
                                                                        From EC_Entity
                                                                        Where IsDeleted=0 And EntityID<>@EntityID And EmailAddress=@EmailAddress", new { EntityID = Convert.ToInt32(model.EntityID), EmailAddress = model.EmailAddress });
            if (!string.IsNullOrEmpty(emailAddress))
            {
                return BadRequest("EmailExist");
            }

            #region Entity Personal Info

            //saving or updating personal info 
            EC_EntityPersonalInfo personalInfo = new()
            {
                EntityPersonalInfoID = model.EntityPersonalInfoID,
                EntityID = model.EntityID,
                FirstName = model.FirstName,
                LastName = model.LastName,
            };
            model.EntityPersonalInfoID = await _dbContext.SaveAsync(personalInfo);

            #endregion

            #region Entity

            await _dbContext.ExecuteAsync($@"Update EC_Entity Set EmailAddress=@EmailAddress Where EntityID={model.EntityID}", new { EmailAddress = model.EmailAddress });

            #endregion

            return Ok(new Success());
        }

        [HttpGet("get-ec-customer-details")]
        public async Task<IActionResult> CustomerDetails()
        {
            var customerAccountDetails = await _dbContext.GetByQueryAsync<EC_CustomerDetailsModel>($@"Select CustomerID,E.EntityID,EntityTypeID,EmailAddress,Phone,Phone2,EntityPersonalInfoID,FirstName,LastName 
                                                                                                            From EC_Customer C
                                                                                                            Left Join EC_Entity E on C.EntityID=E.EntityID and E.IsDeleted=0
                                                                                                            Left Join EC_EntityPersonalInfo EP on EP.EntityID=E.EntityID and EP.IsDeleted=0
                                                                                                            Where E.EntityID={CurrentEntityID} and C.IsDeleted=0", null);
            customerAccountDetails.customerAddress = await _ecommon.GetEntityAddress(CurrentEntityID);
            customerAccountDetails.ratingAndReview = await _dbContext.GetListByQueryAsync<EC_ItemRatingAndReviewModel>($@"Select RatingID,Rating,C.EntityID,ReviewID,Review,ItemName,FirstName as Name
                                                                                                                            From EC_Customer C
                                                                                                                            Left Join  EC_ItemRating R on R.CustomerEntityID=C.EntityID and R.IsDeleted=0
                                                                                                                            Left Join EC_ItemReview  V on V.CustomerEntityID=C.EntityID and V.isDeleted=0
                                                                                                                            Left Join EC_Item I on I.ItemID=R.ItemID and I.ItemID=V.ItemID and I.IsDeleted=0
                                                                                                                            Left Join EC_EntitypersonalInfo E on E.EntityID=R.CustomerEntityID and E.EntityID=V.CustomerEntityID and E.IsDeleted=0
                                                                                                                            Where R.IsDeleted=0 and C.EntityID={CurrentEntityID}", null);
            foreach (var images in customerAccountDetails.ratingAndReview.Where(s=>s.ReviewID!=null).ToList())
            {
                images.Reviewimage = await _dbContext.GetListByQueryAsync<EC_ItemReviewImageModel>($@"Select R.* 
                                                                                From EC_ReviewImages R
                                                                                Left Join Media m on M.MediaID=R.mediaID and IsDeleted=0
                                                                                Where R.IsDeleted=0 and ReviewID={images.ReviewID}", null);
            }
            customerAccountDetails.wishLists = await _dbContext.GetListByQueryAsync<EC_WishListViewModel>($@"Select W.*,ItemName,I.Price
                                                                                                            From EC_WishList W
                                                                                                            Left Join EC_Entity E on E.EntityID=W.CustomerEntityID and E.IsDeleted=0
                                                                                                            left Join EC_EntityPersonalInfo EP on EP.EntityID=E.EntityID and EP.IsDeleted=0
                                                                                                            Left Join EC_Item I on I.ItemVariantID=W.ItemVariantID and I.IsDeleted=0
                                                                                                            Where W.IsDeleted=0 and CustomerEntityID={CurrentEntityID}", null);
           


            return Ok(customerAccountDetails ?? new());
        }


        [HttpPost("get-ec-customers-paged-list")]
        public async Task<IActionResult> GetCustomerPagedList(PagedListPostModelWithFilter pagedListPostModel)
        {
            PagedListQueryModel query = pagedListPostModel;
            query.Select = $@"Select CustomerID,E.EntityID,FirstName,Phone,EmailAddress 
                                From EC_Customer C
                                Left Join EC_Entity E on E.EntityID=C.EntityID and E.IsDeleted=0
                                Left Join EC_EntityPersonalInfo EP on EP.EntityID=E.EntityID and EP.IsDeleted=0";

            query.WhereCondition = $"C.IsDeleted=0";
            query.OrderByFieldName = pagedListPostModel.OrderByFieldName;
            query.WhereCondition += !string.IsNullOrEmpty(pagedListPostModel.SearchString) ? $" and EP.FirstName like '%{pagedListPostModel.SearchString}%'" : "";
            query.WhereCondition += _ecommon.GeneratePagedListFilterWhereCondition(pagedListPostModel);
            var result = await _dbContext.GetPagedList<EC_CustomerListModel>(query, null);
            return Ok(result);
        }
        #endregion

        #region Customer Address

        [HttpPost("save-ec-customer-address")]
        public async Task<IActionResult> SaveCustomerAddress(EC_CustomerAddressModel model)
        {
            var customerAddress = _mapper.Map<EC_EntityAddress>(model);
            customerAddress.EntityID = CurrentEntityID;
            customerAddress.AddressID = await _dbContext.SaveAsync(customerAddress);
            return Ok(new Success());
        }

        [HttpGet("get-ec-customer-address/{AddressID}")]
        public async Task<IActionResult> GetCustomerAddress(int AddressID)
        {
            var customerAddress = await _dbContext.GetByQueryAsync<EC_CustomerAddressModel>($@"Select *
                                                                        From EC_EntityAddress 
                                                                        Where AddressID={AddressID} and IsDeleted=0", null);
            return Ok(customerAddress ?? new());
        }

        [HttpGet("get-list-of-ec-customer-address")]
        public async Task<IActionResult> GetListOfEntityAddress()
        {
            var AddressList = await _ecommon.GetEntityAddress(CurrentEntityID);
            return Ok(AddressList ?? new());
        }

        [HttpGet("delete-ec-customer-address/{addressID}")]
        public async Task<IActionResult> DeleteCustomerAddress(int addressID)
        {
            try
            {
                await _dbContext.ExecuteAsync($@"Update EC_EntityAddress Set IsDeleted=1 Where AddressID={addressID}");
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

        #region Wishlist

        [HttpPost("add-to-wishlist")]
        public async Task<IActionResult>AddToWishList(EC_WishListModel model)
        {
            var wishList = _mapper.Map<EC_WishList>(model);
            wishList.CustomerEntityID = CurrentEntityID;
            wishList.AddedOn = DateTime.UtcNow;
            wishList.ID = await _dbContext.SaveAsync(wishList);
            return Ok(new Success());
        }


        [HttpGet("remove-wishlist")]
        public async Task<IActionResult> RemoveWishList(int Id)
        {
            await _dbContext.DeleteAsync<EC_WishList>(Id);
            return Ok(new Success());
        }

        [HttpPost("get-wishlist-paged-list")]
        public async Task<IActionResult> GetWishListPagedList(PagedListPostModelWithFilter pagedListPostModel)
        {
            PagedListQueryModel query = pagedListPostModel;
            query.Select = $@"Select W.*,ItemName,I.Price,FirstName
                                From EC_WishList W
                                Left Join EC_Entity E on E.EntityID=W.CustomerEntityID and E.IsDeleted=0
                                left Join EC_EntityPersonalInfo EP on EP.EntityID=E.EntityID and EP.IsDeleted=0
                                Left Join EC_Itemvariant I on I.ItemVariantID=W.ItemVariantID and I.IsDeleted=0
                                Left Join EC_Item EI on EI.ItemID=I.ItemID and EI.IsDeleted=0";
            query.WhereCondition = $"W.IsDeleted=0";
            query.OrderByFieldName = pagedListPostModel.OrderByFieldName;
            query.WhereCondition += !string.IsNullOrEmpty(pagedListPostModel.SearchString) ? $" and EP.FirstName like '%{pagedListPostModel.SearchString}%'" : "";
            query.WhereCondition += _ecommon.GeneratePagedListFilterWhereCondition(pagedListPostModel, "EP.FirstName");
            var result = await _dbContext.GetPagedList<EC_WishListViewModel>(query, null);
            return Ok(result);
        }

        [HttpPost("get-wishlist-data-paged-list")]
        public async Task<IActionResult> GetWishListViewPagedList(PagedListPostModelWithFilter pagedListPostModel)
        {
            PagedListQueryModel query = pagedListPostModel;
            query.Select = $@"Select W.*,ItemName,I.Price
                                From EC_WishList W
                                Left Join EC_Entity E on E.EntityID=W.CustomerEntityID and E.IsDeleted=0
                                left Join EC_EntityPersonalInfo EP on EP.EntityID=E.EntityID and EP.IsDeleted=0
                                Left Join EC_Itemvariant I on I.ItemVariantID=W.ItemVariantID and I.IsDeleted=0
                                Left Join EC_Item EI on EI.ItemID=I.ItemID and EI.IsDeleted=0";
            query.WhereCondition = $"W.IsDeleted=0 and W.CustomerEntityID={CurrentEntityID}";
            query.OrderByFieldName = pagedListPostModel.OrderByFieldName;
            query.WhereCondition += !string.IsNullOrEmpty(pagedListPostModel.SearchString) ? $" and ItemName like '%{pagedListPostModel.SearchString}%'" : "";
            query.WhereCondition += _ecommon.GeneratePagedListFilterWhereCondition(pagedListPostModel, "ItemName");
            var result = await _dbContext.GetPagedList<EC_WishListViewModel>(query, null);
            return Ok(result);
        }

        [HttpGet("get-wishlist-by-id/{id}")]
        public async Task<IActionResult>GetWishlist(int id)
        {
            var wishList = await _dbContext.GetByQueryAsync<EC_WishListViewModel>($@"Select W.*,ItemName,I.Price
                                                                                        From EC_WishList W
                                                                                        Left Join EC_Entity E on E.EntityID=W.CustomerEntityID and E.IsDeleted=0
                                                                                        left Join EC_EntityPersonalInfo EP on EP.EntityID=E.EntityID and EP.IsDeleted=0
                                                                                        Left Join EC_Itemvariant I on I.ItemVariantID=W.ItemVariantID and I.IsDeleted=0
                                                                                        Left Join EC_Item EI on EI.ItemID=I.ItemID and EI.IsDeleted=0
                                                                                        Where W.IsDeleted=0 and ID={id}", null);
            return Ok(wishList ?? new());
        }

        #endregion

        #region Cart

        [HttpPost("add-to-cart")]
        public async Task<IActionResult> AddToCart(EC_CartModel model)
        {
            var cart = _mapper.Map<EC_Cart>(model);
            cart.CustomerEntityID = CurrentEntityID;
            cart.AddedOn = DateTime.UtcNow;
            cart.CartID = await _dbContext.SaveAsync(cart);
            return Ok(new Success());
        }


        [HttpGet("remove-cart")]
        public async Task<IActionResult> RemoveCart(int Id)
        {
            await _dbContext.DeleteAsync<EC_Cart>(Id);
            return Ok(new Success());
        }

        [HttpGet("get-cart/{itemVariantID}")]
        public async Task<IActionResult> GetAddCart(int itemVariantID)
        {
            var cart = await _dbContext.GetByQueryAsync<EC_CartModel>($@"Select CartID,CustomerEntityID,C.itemVariantID,ItemName,AddedOn 
                                                                        From EC_Cart C
                                                                        Left Join EC_Itemvariant I on I.ItemVariantID=C.ItemVariantID and I.IsDeleted=0
                                                                        Left Join EC_Item EI on EI.ItemID=I.ItemID and EI.IsDeleted=0
                                                                        Where C.ItemVariantID={itemVariantID} and C.IsDeleted=0 and C.CustomerEntityID={CurrentEntityID}",null);
            
            return Ok(cart??new());
        }
        [HttpGet("get-cart/{cartID}")]
        public async Task<IActionResult> GetAddCartByCartID(int cartID)
        {
            var cart = await _dbContext.GetByQueryAsync<EC_CartModel>($@"Select CartID,CustomerEntityID,C.itemVariantID,ItemName,AddedOn 
                                                                        From EC_Cart C
                                                                        Left Join EC_Itemvariant I on I.ItemVariantID=C.ItemVariantID and I.IsDeleted=0
                                                                        Left Join EC_Item EI on EI.ItemID=I.ItemID  and EI.IsDeleted=0
                                                                        Where C.CartID={cartID} and C.IsDeleted=0 and C.CustomerEntityID={CurrentEntityID}",null);

            return Ok(cart ?? new());
        }


        [HttpPost("get-cart-paged-list")]
        public async Task<IActionResult> GetCartPagedList(PagedListPostModelWithFilter pagedListPostModel)
        {
            PagedListQueryModel query = pagedListPostModel;
            query.Select = $@"Select CartID,CustomerEntityID,C.itemVariantID,ItemName,AddedOn,FirstName as Name 
                                From EC_Cart C
                                Left Join EC_Entity E on E.EntityID=C.CustomerEntityID and E.IsDeleted=0
                                left Join EC_EntityPersonalInfo EP on EP.EntityID=E.EntityID and EP.IsDeleted=0
                                Left Join EC_Itemvariant I on I.ItemVariantID=C.ItemVariantID and I.IsDeleted=0
                                Left Join EC_Item EI on EI.ItemID=I.ItemID and EI.IsDeleted=0";
            query.WhereCondition = $"C.IsDeleted=0 ";
            query.OrderByFieldName = pagedListPostModel.OrderByFieldName;
            query.WhereCondition += !string.IsNullOrEmpty(pagedListPostModel.SearchString) ? $" and FirstName like '%{pagedListPostModel.SearchString}%'" : "";
            query.WhereCondition += _ecommon.GeneratePagedListFilterWhereCondition(pagedListPostModel, "FirstName");
            var cartResult = await _dbContext.GetPagedList<EC_CartListModel>(query, null);
            return Ok(cartResult??new());
        }

        [HttpPost("get-cart-details-paged-list")]
        public async Task<IActionResult> GetCartDetailsPagedList(PagedListPostModelWithFilter pagedListPostModel)
        {
            PagedListQueryModel query = pagedListPostModel;
            query.Select = $@"Select CartID,CustomerEntityID,C.itemVariantID,ItemName,AddedOn 
                                From EC_Cart C
                                Left Join EC_Entity E on E.EntityID=C.CustomerEntityID and E.IsDeleted=0
                                Left Join EC_Itemvariant I on I.ItemVariantID=C.ItemVariantID and I.IsDeleted=0
                                Left Join EC_Item EI on EI.ItemID=I.ItemID and EI.IsDeleted=0";
            query.WhereCondition = $"C.IsDeleted=0 and CustomerEntityID={CurrentEntityID} ";
            query.OrderByFieldName = pagedListPostModel.OrderByFieldName;
            query.WhereCondition += !string.IsNullOrEmpty(pagedListPostModel.SearchString) ? $" and ItemName like '%{pagedListPostModel.SearchString}%'" : "";
            query.WhereCondition += _ecommon.GeneratePagedListFilterWhereCondition(pagedListPostModel, "ItemName");
            var cartResult = await _dbContext.GetPagedList<EC_CartListModel>(query, null);
            return Ok(cartResult??new());
        }

        #endregion

        #region Item rating and Review

        [HttpPost("add-item-rating")]
        public async Task<IActionResult>AddReview(EC_ItemRatingReviewPostModel model)
        {
            var itemRating = new EC_ItemRating();
            if (model.Rating!=null)
            {
                itemRating = _mapper.Map<EC_ItemRating>(model);
                itemRating.CustomerEntityID = CurrentEntityID;
                itemRating.RatingID = await _dbContext.SaveAsync(itemRating);
            }

            if (model.Review != null || (model.ReviewImages != null && model.ReviewImages.Count > 0))
            {
                EC_ItemReview itemReview = new()
                {
                    ReviewID=model.ReviewID,
                    Review=model.Review,
                    ItemID=model.ItemID,
                    CustomerEntityID=CurrentEntityID
                };
                itemReview.ReviewID = await _dbContext.SaveAsync(itemReview);

                if (model.ReviewImages != null && model.ReviewImages.Count > 0)
                {
                    List<EC_ItemReviewImages> imageList = new();
                    foreach (var images in model.ReviewImages)
                    {
                        EC_ItemReviewImages reviewImage = new()
                        {
                            ID = images.ID,
                            MediaID = images.MediaID
                        };
                        imageList.Add(reviewImage);
                    }
                    await _dbContext.SaveListAsync(imageList, $"ReviewID={itemReview.ReviewID}", needToDelete: false);
                }
            }
            return Ok(new Success());
        }


        [HttpPost("get-item-rating-and-review-paged-list")]
        public async Task<IActionResult> GetItemRatingPagedList(PagedListPostModelWithFilter pagedListPostModel)
        {
            PagedListQueryModel query = pagedListPostModel;
            query.Select = $@"Select RatingID,Rating,C.EntityID,ReviewID,Review,ItemName,FirstName as Name
                            From EC_Customer C
                            Left Join  EC_ItemRating R on R.CustomerEntityID=C.EntityID and R.IsDeleted=0
                            Left Join EC_ItemReview  V on V.CustomerEntityID=C.EntityID and V.isDeleted=0
                            Left Join EC_Item I on I.ItemID=R.ItemID and I.ItemID=V.ItemID and I.IsDeleted=0
                            Left Join EC_EntitypersonalInfo E on E.EntityID=R.CustomerEntityID and E.EntityID=V.CustomerEntityID and E.IsDeleted=0
                            ";
            query.WhereCondition = $" C.IsDeleted=0";
            query.OrderByFieldName = pagedListPostModel.OrderByFieldName;
            query.WhereCondition += !string.IsNullOrEmpty(pagedListPostModel.SearchString) ? $" and ItemName like '%{pagedListPostModel.SearchString}%'" : "";
            query.WhereCondition += _ecommon.GeneratePagedListFilterWhereCondition(pagedListPostModel, "ItemName");
            var cartResult = await _dbContext.GetPagedList<EC_ItemReviewAndRatingViewModel>(query, null);
            return Ok(cartResult ?? new());
        }

        [HttpGet("get-customer-rating-and-review")]
        public async Task<IActionResult>GetCustomerRating()
        {
            var ratingList = await _dbContext.GetListByQueryAsync<EC_ItemRatingAndReviewModel>($@"Select RatingID,Rating,C.EntityID,ReviewID,Review,ItemName,FirstName as Name
                                                                                            From EC_Customer C
                                                                                            Left Join  EC_ItemRating R on R.CustomerEntityID=C.EntityID and R.IsDeleted=0
                                                                                            Left Join EC_ItemReview  V on V.CustomerEntityID=C.EntityID and V.isDeleted=0
                                                                                            Left Join EC_Item I on I.ItemID=R.ItemID and I.ItemID=V.ItemID and I.IsDeleted=0
                                                                                            Left Join EC_EntitypersonalInfo E on E.EntityID=R.CustomerEntityID and E.EntityID=V.CustomerEntityID and E.IsDeleted=0
                                                                                            Where R.IsDeleted=0 and C.EntityID={CurrentEntityID}", null);
            return Ok(ratingList??new());
        }


        [HttpPost("Delete-rating-and-review")]
        public async Task<IActionResult>DeleteReviewAndRating(EC_ItemReviewRatingDeleteModel model)
        {
            await _dbContext.ExecuteAsync($"UPDATE EC_ItemRating SET IsDeleted=1 Where ItemID={model.ItemID} and CustomerEntityID={model.EntityID}", null);
            await _dbContext.ExecuteAsync($"UPDATE EC_ItemReview SET IsDeleted=1 Where ItemID={model.ItemID} and CustomerEntityID={model.EntityID}", null);
            if(model.ReviewID!=0)
            {
                await _dbContext.ExecuteAsync($"UPDATE EC_ReviewImages SET IsDeleted=1 Where ReviewID={model.ReviewID}", null);
            }
            return Ok(new Success());
        }
        #endregion



    }
}
