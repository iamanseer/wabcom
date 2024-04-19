using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PB.DatabaseFramework;
using PB.EntityFramework;
using PB.Model;
using PB.Model.Models;
using PB.Shared.Helpers;
using PB.Shared.Models;
using PB.Shared.Models.eCommerce.Common;
using PB.Shared.Models.eCommerce.Customers;
using PB.Shared.Tables;
using PB.Shared.Tables.eCommerce.Users;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PB.Server.Repository.eCommerce
{
    public interface IEC_CommonRepository
    {
        Task<OtpViewModel> GenerateOtp(EC_CustomerPhoneVerifyPostModel model);
        Task<List<EC_CustomerAddressViewModel>> GetEntityAddress(int entityID, IDbTransaction? tran = null);
        Task<int> SaveUser(EC_Users user, IDbTransaction tran = null, string brandName = "", string emailAddress = "");
        Task<LoginResponseModel> GetToken(int userId, int clientId, int branchId);
        string GeneratePagedListFilterWhereCondition(PagedListPostModelWithFilter searchModel, string searchLikeFieldName = "");


    }
    public class EC_CommonRepository : IEC_CommonRepository
    {
        private readonly IHostEnvironment _env;
        private readonly IDbContext _dbContext;
        private readonly IEmailSender _email;
        private readonly INotificationRepository _notification;
        private readonly IConfiguration _configuration;
        private readonly IAuthenticationRepository _auth;

        public EC_CommonRepository(IHostEnvironment env, IDbContext dbContext, IEmailSender email, INotificationRepository notification, IConfiguration configuration, IAuthenticationRepository auth)
        {
            _env = env;
            _dbContext = dbContext;
            _email = email;
            _notification = notification;
            _configuration = configuration;
            _auth = auth;
        }

        #region Generate Otp 
        public async Task<OtpViewModel> GenerateOtp(EC_CustomerPhoneVerifyPostModel model)
        {
            OtpViewModel viewModel = new();
            int _min = 1000;
            int _max = 9999;
            Random _rdm = new();
            viewModel.OTP = (_rdm.Next(_min, _max)).ToString();
            viewModel.OtpGeneratedOn = DateTime.UtcNow;
            return viewModel;
        }

        #endregion

        #region Customer address
        public async Task<List<EC_CustomerAddressViewModel>> GetEntityAddress(int entityID, IDbTransaction? tran = null)
        {
            List<EC_CustomerAddressViewModel> res = new();
            var customerAddressList = await _dbContext.GetListByQueryAsync<EC_CustomerAddressModel>($@"Select *
                                                                                                    From EC_EntityAddress 
                                                                                                    Where EntityID={entityID} and IsDeleted=0", null);
            foreach (var address in customerAddressList)
            {
                EC_CustomerAddressViewModel customerAddressView = new EC_CustomerAddressViewModel() { AddressID = address.AddressID };
                customerAddressView.CompleteAddress = address.AddressLine1 + ',';
                if (!string.IsNullOrEmpty(address.AddressLine2))
                    customerAddressView.CompleteAddress += address.AddressLine2 + ',';
                if (!string.IsNullOrEmpty(address.City))
                    customerAddressView.CompleteAddress += address.City + ',';
                if (!string.IsNullOrEmpty(address.State))
                    customerAddressView.CompleteAddress += address.State + ',';
                if (!string.IsNullOrEmpty(address.PinCode))
                    customerAddressView.CompleteAddress += address.PinCode + ',';
                customerAddressView.CompleteAddress = customerAddressView.CompleteAddress.TrimEnd(',');

                res.Add(customerAddressView);
            }

            return res ?? new();
        }

        #endregion

        #region Users


        public async Task<int> SaveUser(EC_Users user, IDbTransaction tran = null, string brandName = "", string emailAddress = "")
        {
            var u = await _dbContext.GetAsync<EC_Users>($@"(UserName='{user.UserName}') and UserID<>{Convert.ToInt32(user.UserID)}", null, tran);
            if (u != null)
            {
                if (u.UserName == user.UserName)
                    throw new PBException("UserNameExist");
            }

            if (user.UserID == 0 && string.IsNullOrEmpty(user.Password))
            {
                user.Password = Guid.NewGuid().ToString("n").Substring(0, 8);
            }

            if (!string.IsNullOrEmpty(user.Password))
            {
                user.PasswordHash = Guid.NewGuid().ToString("n").Substring(0, 8);
                user.Password = GetHashPassword(user.Password, user.PasswordHash);
            }

            int userId = await _dbContext.SaveAsync(user, tran);

            return userId;
        }
        public static string GetHashPassword(string password, string salt)
        {
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: Encoding.ASCII.GetBytes(salt),
            prf: KeyDerivationPrf.HMACSHA1,
            iterationCount: 10000,
            numBytesRequested: 256 / 8));
            return hashed;
        }



        #endregion

        #region Get Tocken

        public async Task<LoginResponseModel> GetToken(int userId, int clientId, int branchId)
        {
            var user = await _dbContext.GetByQueryAsync<EC_CustomerTockenDataModel>($@"Select U.UserID, EP.FirstName as Name, UserName, Password, PasswordHash, U.UserTypeID,U.EntityID,
            ISNULL(EmailAddress,'') as EmailAddress,ISNULL(Phone,'') as MobileNo, T.DisplayName as UserTypeName
			from EC_Users U
			LEFT JOIN EC_Entity E on E.EntityID=U.EntityID and E.IsDeleted=0
			Left Join EC_EntityPersonalInfo EP on EP.EntityID=E.EntityID and EP.IsDeleted=0
            LEFT JOIN UserType T on T.UserTypeID=U.UserTypeID 
			where ISNULL(U.IsDeleted,0)=0 and  U.UserID={userId}", null);

            var branchName = await _dbContext.GetFieldsAsync<ViBranch, string>("BranchName", $"BranchID={branchId}", null);
            if (string.IsNullOrEmpty(branchName))
                branchName = "";

            var claims = new List<Claim>()
            {
               new Claim(ClaimTypes.Name, user.Name),
               new Claim("UserName", user.UserName.ToString()),
               new Claim("UserID", user.UserID.ToString()),
               new Claim("UserTypeID", user.UserTypeID.ToString()),
               new Claim("EntityID", user.EntityID.ToString()),
               new Claim("ClientID", clientId.ToString()),
               new Claim("BranchID", branchId.ToString()),
               new Claim("PhoneNo", user.MobileNo.ToString()),
               new Claim("UserTypeName", user.UserTypeName),
               new Claim("BranchName",branchName),
               new Claim(ClaimTypes.Role, user.UserTypeName.Replace(" ",""))
            };

            return await GetJWTToken(userId, user.UserTypeID, clientId, claims);

        }

        #region JWT Token creation - Same for all project. Dont edit

        private async Task<LoginResponseModel> GetJWTToken(int userId, int userTyeId, int clientId, List<Claim> claims)
        {
            LoginResponseModel result = new()
            {
                AccessToken = await CreateAccessToken(userId, userTyeId, clientId, claims),
                RefreshToken = await _auth.GetRefreshToken(userId)
            };
            return result;
        }

        private async Task<string> CreateAccessToken(int userId, int userTyeId, int clientId, List<Claim> claims, int? expiryDays = null)
        {
            expiryDays ??= Convert.ToInt32(_configuration["Jwt:ExpiryInDays"]);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecurityKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry = DateTime.Now.AddDays(expiryDays.Value);

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: expiry,
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        #endregion

        #endregion

        #region Filter
        public string GeneratePagedListFilterWhereCondition(PagedListPostModelWithFilter searchModel, string searchLikeFieldName = "")
        {
            string dateWhereCondition = "";
            string enumWhereCondition = "";
            string booleanWhereCondition = "";
            string fieldWhereCondition = "";
            string query = "";

            //Generating date filter where condition
            if (searchModel.FilterByDateOptions != null && searchModel.FilterByDateOptions.Count > 0)
            {
                dateWhereCondition = "";
                for (short i = 0; i < searchModel.FilterByDateOptions.Count; i++)
                {
                    var dateFilterItem = searchModel.FilterByDateOptions[i];
                    if (dateFilterItem.StartDate != null && dateFilterItem.EndDate != null)
                    {
                        string StartDate = dateFilterItem.StartDate.Value.ToString("yyyy-MM-dd");
                        string EndDate = dateFilterItem.EndDate.Value.ToString("yyyy-MM-dd");
                        switch (i)
                        {
                            case 0:

                                dateWhereCondition = $" and (Convert(nvarchar,{dateFilterItem.FieldName},23) Between '{StartDate}' and '{EndDate}')";
                                break;

                            case > 0:

                                dateWhereCondition += $" and (Convert(nvarchar,{dateFilterItem.FieldName},23) Between '{StartDate}' and '{EndDate}')";

                                break;
                        }
                    }
                }
            }

            //Generating IDnValue filter where condition
            if (searchModel.FilterByIdOptions != null && searchModel.FilterByIdOptions.Count > 0)
            {
                enumWhereCondition = "";
                for (short i = 0; i < searchModel.FilterByIdOptions.Count; i++)
                {
                    var enumFilterItem = searchModel.FilterByIdOptions[i];
                    switch (i)
                    {
                        case 0:

                            if (enumFilterItem.SelectedEnumValues != null && enumFilterItem.SelectedEnumValues.Count > 0)
                            {
                                if (enumFilterItem.SelectedEnumValues.Count == 1)
                                {
                                    enumWhereCondition = $" and {enumFilterItem.FieldName}={enumFilterItem.SelectedEnumValues[0]}";
                                }
                                else
                                {
                                    string commaSeparatedEnumValues = string.Join(",", enumFilterItem.SelectedEnumValues);
                                    enumWhereCondition = $" and {enumFilterItem.FieldName} in ({commaSeparatedEnumValues})";
                                }
                            }

                            break;

                        case > 0:

                            if (enumFilterItem.SelectedEnumValues != null && enumFilterItem.SelectedEnumValues.Count > 0)
                            {
                                if (enumFilterItem.SelectedEnumValues.Count == 1)
                                {
                                    enumWhereCondition += $" and {enumFilterItem.FieldName}={enumFilterItem.SelectedEnumValues[0]}";
                                }
                                else
                                {
                                    string commaSeparatedEnumValues = string.Join(",", enumFilterItem.SelectedEnumValues);
                                    enumWhereCondition += $" and {enumFilterItem.FieldName} in ({commaSeparatedEnumValues})";
                                }
                            }

                            break;
                    }
                }
            }

            //Generating boolean filter where condition
            if (searchModel.FilterByBooleanOptions != null && searchModel.FilterByBooleanOptions.Count > 0)
            {
                booleanWhereCondition = "";
                for (short i = 0; i < searchModel.FilterByBooleanOptions.Count; i++)
                {

                    var booleanFilterItem = searchModel.FilterByBooleanOptions[i];
                    switch (i)
                    {
                        case 0:

                            booleanWhereCondition = $" AND {booleanFilterItem.FieldName}={booleanFilterItem.Value}";

                            break;

                        case > 0:

                            booleanWhereCondition += $" AND {booleanFilterItem.FieldName}={booleanFilterItem.Value}";

                            break;
                    }
                }
            }


            //Generating Field filter where condition
            if (searchModel.FilterByFieldOptions != null && searchModel.FilterByFieldOptions.Count > 0)
            {
                fieldWhereCondition = "";
                for (short i = 0; i < searchModel.FilterByFieldOptions.Count; i++)
                {

                    var fieldFilterItem = searchModel.FilterByFieldOptions[i];
                    switch (i)
                    {
                        case 0:

                            fieldWhereCondition = $" AND {fieldFilterItem.FieldName} IS NOT NULL";

                            break;

                        case > 0:

                            fieldWhereCondition += $" AND {fieldFilterItem.FieldName} IS NOT NULL";

                            break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(dateWhereCondition))
            {
                query += dateWhereCondition;
                dateWhereCondition = string.Empty;
            }

            if (!string.IsNullOrEmpty(enumWhereCondition))
            {
                query += enumWhereCondition;
                enumWhereCondition = string.Empty;
            }

            if (!string.IsNullOrEmpty(booleanWhereCondition))
            {
                query += booleanWhereCondition;
                booleanWhereCondition = string.Empty;
            }


            if (!string.IsNullOrEmpty(fieldWhereCondition))
            {
                query += fieldWhereCondition;
                fieldWhereCondition = string.Empty;
            }


            if (!string.IsNullOrEmpty(searchModel.SearchString) && !string.IsNullOrEmpty(searchLikeFieldName))
            {
                query += $" and {searchLikeFieldName} like '%{searchModel.SearchString}%'";
            }

            return query;
        }

        #endregion

    }
}
