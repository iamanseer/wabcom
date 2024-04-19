using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PB.DatabaseFramework;
using PB.EntityFramework;
using PB.Model;
using PB.Shared.Models;
using PB.Shared.Tables;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PB.DatabaseFramework
{

    public class AuthenticationRepository2 : IAuthenticationRepository
    {
        private readonly IDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender;
        public AuthenticationRepository2(IDbContext dbContext, IConfiguration configuration, IEmailSender emailSender, IConfiguration config)
        {
            this._dbContext = dbContext;
            this._configuration = configuration;
            this._emailSender = emailSender;
        }

        public async Task<AuthenticationResultModel> VerifyUser(LoginRequestModel model, bool checkIsLoggedIn = false, bool checkEmailConfirmed = true, bool checkLoginStatus = true, bool checkApprovalPending = true)
        {
            var user = await _dbContext.GetByQueryAsync<AuthenticationDataModel>($@"Select UserID, Password, PasswordHash, UserTypeID,
			        LoginStatus, EmailConfirmed, ISNULL(ClientID,0) as ClientID, IsApprovalPending, IsLoggedIn, EntityID
			        from Users
                    Where UserName=@UserName and IsDeleted=0", new { UserName = model.Username});

            if (user == null)
                throw new PBException("InvalidUserName");

            string encpassword = GetHashPassword(model.Password, user.PasswordHash);
            if (encpassword != user.Password)
                throw new PBException("InvalidPassword");

            if (checkIsLoggedIn && !Convert.ToBoolean(user.IsLoggedIn))
                throw new PBException("AlreadyLoggedIn");

            if (checkEmailConfirmed && !Convert.ToBoolean(user.EmailConfirmed))
                throw new PBException("EmailNotVerified");

            if (checkLoginStatus && !Convert.ToBoolean(user.LoginStatus))
                throw new PBException("LoginInactive");

            if (checkApprovalPending && Convert.ToBoolean(user.IsApprovalPending))
                throw new PBException("ApprovalPending");

            bool canAccess = false;
            if (model.AccessibleUserTypes != null)
            {
                foreach (var userTypeId in model.AccessibleUserTypes)
                {
                    if (user.UserTypeID == userTypeId)
                    {
                        canAccess = true;
                        break;
                    }
                }
                if (!canAccess)
                    throw new PBException("ProhibittedApp");
            }

            return new AuthenticationResultModel()
            {
                UserID = user.UserID,
                ClientID = user.ClientID,
                UserTypeID = user.UserTypeID,
                EmailConfirmed=user.EmailConfirmed,
                EntityID=user.EntityID,
                IsApprovalPending=user.IsApprovalPending,
                IsLoggedIn=user.IsLoggedIn,
                LoginStatus =user.LoginStatus,
            };
        }

        public async Task<List<IdnValuePair>> GetClients()
        {
            var res = await _dbContext.GetListByQueryAsync<IdnValuePair>(@"SELECT  C.ClientID as ID, E.Name as Value
                        FROM Client C
                        LEFT JOIN viEntity E on E.EntityID = C.EntityID", null);
            res.Insert(0, new IdnValuePair() { ID = 0, Value = "--NA--" });
            return res;
        }

        public async Task<List<IdnValueParentPair>> GetBranches(int userTypeID, int clientId, int userId)
        {
            switch ((DefaultUserTypes)userTypeID)
            {
                case DefaultUserTypes.SuperAdmin:
                case DefaultUserTypes.Developer:
                case DefaultUserTypes.Support:
                    var res = await _dbContext.GetListOfIdValueParentPairAsync<ViBranch>("BranchName", "ClientID","",null);
                    return res;
                case DefaultUserTypes.Client:
                    return await _dbContext.GetListOfIdValueParentPairAsync<ViBranch>("BranchName", "ClientID", $"ClientID={clientId}", null);
                default:
                    return await _dbContext.GetListByQueryAsync<IdnValueParentPair>($@"Select B.BranchID as ID, BranchName as Value
                        From viBranch B
                        JOIN BranchUser U on U.BranchID = B.BranchID
                        Where U.UserID = {userId} and U.CanAccess = 1", null);
            }

        }

        public async Task VerifyBranchAccess(int userId, int branchId, int clientId, int userTypeId)
        {
            int branch;
            switch(userTypeId)
            {
                case (int)DefaultUserTypes.SuperAdmin:
                case (int)DefaultUserTypes.Developer:
                case (int)DefaultUserTypes.Support:
                    if (branchId == 0)
                        return;
                    branch = await _dbContext.GetFieldsAsync<Branch, int>("BranchID", $"BranchID={branchId}", null);
                    break;
                case (int)DefaultUserTypes.Client:
                    branch = await _dbContext.GetFieldsAsync<Branch, int>("BranchID", $"BranchID={branchId} and ClientID={clientId}", null);
                    break;
                default:
                    branch = await _dbContext.GetByQueryAsync<int>($@"Select B.BranchID
                            From viBranch B
                            JOIN BranchUser U on U.BranchID = B.BranchID
                            Where U.UserID = {userId} and U.CanAccess = 1", null);
                    break;
            }
            
            if (branch == 0)
            {
                throw new PBException("UnPermittedBranch");
            }
        }

        public async Task<string>GetRefreshToken(int userId)
        {
            RefreshToken refreshToken = GenerateRefreshToken();
            refreshToken.UserID = userId;
            await _dbContext.SaveAsync(refreshToken,needLog:false);

            return refreshToken.Token;
        }

        public async Task<bool> ValidateRefreshToken(int userId, string refreshToken)
        {

            RefreshToken refreshTokenUser = await _dbContext.GetByQueryAsync<RefreshToken>($@"Select * from RefreshToken where UserID={userId} and Token='{refreshToken}' and ExpiryDate>@Date order by 1 desc",new { Date=System.DateTime.UtcNow });

            if (refreshTokenUser != null)
            {
                return true;
            }
            return false;
        }

        public JwtSecurityToken GetUserFromAccessToken(string accessToken)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecurityKey"]));

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = false,
                    ValidateAudience = false
                };

                //var principle = tokenHandler.ValidateToken(accessToken, tokenValidationParameters, out SecurityToken securityToken);

                var jwtSecurityToken = tokenHandler.ReadJwtToken(accessToken);

                if (jwtSecurityToken != null && jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return jwtSecurityToken;
                }
            }
            catch //(Exception err)
            {
                return null;
            }

            return null;
        }

        private static RefreshToken GenerateRefreshToken()
        {
            RefreshToken refreshToken = new();

            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                refreshToken.Token = Convert.ToBase64String(randomNumber);
            }
            refreshToken.ExpiryDate = DateTime.UtcNow.AddMonths(12);

            return refreshToken;
        }

        public async Task<List<string>> GetRoles(int userId, int clientId, int userTypeId,int branchId=0)
        {
            if (userTypeId == (int)DefaultUserTypes.SuperAdmin || userTypeId == (int)DefaultUserTypes.Developer)
            {
                if (clientId == 0 && branchId == 0)
                {
                    var roles = await _dbContext.GetListByQueryAsync<string>($@"SELECT  RoleName FROM Role Where IsForSupport=1", null);
                    roles.Add("SuperAdmin");
                    return roles;
                }
                else
                    return await _dbContext.GetListByQueryAsync<string>($@"SELECT  RoleName FROM Role Where IsForSupport=0", null);
            }
            else
            {
                if (userTypeId == (int)DefaultUserTypes.Support && clientId != 0 && branchId != 0)
                {
                    return await _dbContext.GetListByQueryAsync<string>($@"SELECT  RoleName FROM Role Where IsForSupport=0", null);
                }



                return await _dbContext.GetListByQueryAsync<string>($@"SELECT  Distinct RoleName
                FROM UserTypeRole PP
                JOIN Role P on P.RoleID=PP.RoleID
                LEFT JOIN UserType T on T.UserTypeID=PP.UserTypeID
                LEFT JOIN Users U on U.UserTypeID=T.UserTypeID or U.UserID=PP.UserID
			    Where U.UserID={userId} and ISNULL(PP.HasAccess,0)=1 and (PP.ClientID={clientId} or PP.ClientID is null) and PP.IsDeleted=0", null);
            }
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


        #region Error have to solve
        //public async Task<LoginResponseModel> GetJWTToken(int userId, int userTyeId, int clientId, List<Claim> claims)
        //{
        //    LoginResponseModel result = new()
        //    {
        //        AccessToken = await CreateAccessToken(userId, userTyeId, clientId, claims),
        //        RefreshToken = await GetRefreshToken(userId)
        //    };
        //    return result;
        //}
        //private async Task<string> CreateAccessToken(int userId, int userTyeId, int clientId, List<Claim> claims, int? expiryDays = null)
        //{
        //    expiryDays ??= Convert.ToInt32(_configuration["Jwt:ExpiryInDays"]);

        //    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecurityKey"]));
        //    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        //    var expiry = DateTime.Now.AddDays(expiryDays.Value);

        //    List<string> roles = await GetRoles(userId, clientId, userTyeId);
        //    foreach (var role in roles)
        //    {
        //        claims.Add(new Claim(ClaimTypes.Role, role));
        //    }

        //    var token = new JwtSecurityToken(
        //        _configuration["Jwt:Issuer"],
        //        _configuration["Jwt:Audience"],
        //        claims,
        //        expires: expiry,
        //        signingCredentials: creds
        //    );
        //    return new JwtSecurityTokenHandler().WriteToken(token);
        //}

        #endregion

        public async Task<int> SaveUser(User user, bool needEmailConfirmation, IDbTransaction tran = null, string brandName = "",string emailAddress="")
        {
            var u = await _dbContext.GetAsync<User>($@"UserName=@UserName and UserID<>@UserID", user, tran);
            if (u != null)
            {
                if (u.UserName.ToLower() == user.UserName.ToLower())
                    throw new PBException("UserNameExist");
            }

            if (user.UserID == 0 && string.IsNullOrEmpty(user.Password))
            {
                user.Password = Guid.NewGuid().ToString("n").Substring(0, 8);
            }

            if (user.UserID == 0 && needEmailConfirmation)
            {
                int _min = 1000;
                int _max = 9999;
                Random _rdm = new();
                user.OTP = (_rdm.Next(_min, _max)).ToString();
            }

            if (!string.IsNullOrEmpty(user.Password))
            {
                user.PasswordHash = Guid.NewGuid().ToString("n").Substring(0, 8);
                user.Password = GetHashPassword(user.Password, user.PasswordHash);
            }

            int userId = await _dbContext.SaveAsync(user, tran);

            if (user.UserID == 0 && needEmailConfirmation && !user.EmailConfirmed)
            {
                var message = $@"
                        <div style = ""font-family: Helvetica,Arial,sans-serif;min-width:1000px;overflow:auto;line-height:2"">
                            <div style = ""margin:50px auto;width:70%;padding:20px 0"">
                                <div style = ""border-bottom:1px solid #eee"" >
                                    <a href = """" style = ""font-size:1.4em;color: #00466a;text-decoration:none;font-weight:600""> {brandName} </a>
                                </div>
                                <p style = ""font-size:1.1em""> Hi,</p>
                                <p> Thank you for choosing {brandName}.Use the following OTP to complete your Sign Up procedures. </p>
                                <h2 style = ""background: #00466a;margin: 0 auto;width: max-content;padding: 0 10px;color: #fff;border-radius: 4px;""> {user.OTP} </h2>       
                                <hr style = ""border:none;border-top:1px solid #eee"" />
                                <div style=""float:right; padding: 8px 0; color:#aaa;font-size:0.8em;line-height:1;font-weight:300"">
                                    <p> Regards,</p>
                                    <p> team {brandName}</p>
                                </div>
                            </div>
                        </div>";

                if (string.IsNullOrEmpty(emailAddress))
                    emailAddress = user.UserName;
                await _emailSender.SendHtmlEmailAsync(emailAddress, "Verify your e-mail address of your account", message, tran);
            }
            return userId;
        }


        public async Task ForgotPassword(ForgotPasswordModel model,string brandName)
        {
            var user = await _dbContext.GetAsync<UserCustom>($"UserName=@EmailAddress",model);
            if (user == null)
            {
                int userId = await _dbContext.GetByQueryAsync<int>($@"Select UserID
                    from viEntity
                    Where UserID is not null and (EmailAddress=@EmailAddress or Phone=@EmailAddress)",model);

                if (userId == 0)
                    throw new PBException("EmailNotFound");
                else
                    user = await _dbContext.GetAsync<UserCustom>(userId);
            }

            int _min = 1000;
            int _max = 9999;
            Random _rdm = new();
            user.OTP = (_rdm.Next(_min, _max)).ToString();
            user.OTPGeneratedAt = DateTime.UtcNow;
            await _dbContext.SaveAsync(user);

            var message = $@"
                <div style = ""font-family: Helvetica,Arial,sans-serif;min-width:1000px;overflow:auto;line-height:2"">
                    <div style = ""margin:50px auto;width:70%;padding:20px 0"">
                        <div style = ""border-bottom:1px solid #eee"" >
                            <a href = """" style = ""font-size:1.4em;color: #00466a;text-decoration:none;font-weight:600""> {brandName} </a>
                        </div>
                        <p style = ""font-size:1.1em""> Hi,</p>
                        <h3>Reset your password?</h3>
						 <p>If you requested a password reset for your account,Use the following OTP.If you didn't make this request,ignore this email<br>	</p>
                        <h2 style = ""background: #00466a;margin: 0 auto;width: max-content;padding: 0 10px;color: #fff;border-radius: 4px;""> {user.OTP} </h2>       
                        <hr style = ""border:none;border-top:1px solid #eee"" />
                        <div style=""float:right; padding: 8px 0; color:#aaa;font-size:0.8em;line-height:1;font-weight:300"">
                            <p> Regards,</p>
                            <p> team {brandName}</p>
                        </div>
                    </div>
                </div>";

            string emailAddress = await _dbContext.GetFieldsAsync<ViEntity, string>("EmailAddress", $"EntityID={user.EntityID}", null);
            await _emailSender.SendHtmlEmailAsync(emailAddress, "Password Reset Request", message);

        }

        public async Task ResetPassword(ResetPasswordModel model)
        {
            var user = await _dbContext.GetAsync<User>($"UserName=@EmailAddress",model);
            if (user == null)
            {
                int userId = await _dbContext.GetByQueryAsync<int>($@"Select UserID
                    from viEntity
                    Where UserID is not null and (EmailAddress=@EmailAddress or Phone=@EmailAddress)", model);

                if (userId == 0)
                    throw new PBException("EmailNotFound");
                else
                    user = await _dbContext.GetAsync<User>(userId);
               
            }

            //if(user.OTP!=model.OTP)
            //    throw new PBException("InvalidOTP");

            user.PasswordHash = Guid.NewGuid().ToString("n").Substring(0, 8);
            user.Password = GetHashPassword(model.Password, user.PasswordHash);
            user.OTP = "";
            user.EmailConfirmed = true;
            await _dbContext.SaveAsync(user);
        }

        public async Task ChangePassword(int userId, string currentPassword, string password, IDbTransaction tran = null)
        {
            var user = await _dbContext.GetAsync<User>(userId);

            if (user != null)
            {
                var cp = GetHashPassword(currentPassword, user.PasswordHash);
                if (cp != user.Password)
                    throw new PBException("InvalidCurrentPassword");

                user.PasswordHash = Guid.NewGuid().ToString("n").Substring(0, 8);
                user.Password = GetHashPassword(password, user.PasswordHash);
                await _dbContext.SaveAsync(user, tran);
            }
        }

        public async Task ChangePasswordWithoutCurrentPassword(int userId, string password, IDbTransaction tran = null)
        {
            var user = await _dbContext.GetAsync<User>(userId);
            if (user != null)
            {
                user.PasswordHash = Guid.NewGuid().ToString("n").Substring(0, 8);
                user.Password = GetHashPassword(password, user.PasswordHash);
                await _dbContext.SaveAsync(user, tran);
            }
        }

        public async Task<ProfileModel> GetProfileDetails(int entityID)
        {
            return await _dbContext.GetByQueryAsync<ProfileModel>($@"Select FirstName,MiddleName,LastName,EmailAddress,Phone,MediaID,QRCode 
                from viEntity
                Where EntityID={entityID}", null);
        }

        public async Task UpdateProfile(int entityID,ProfileModel model)
        {
            await _dbContext.UpdateAsync<Entity>("Phone=@Phone,EmailAddress=@EmailAddress,MediaID=@MediaID", entityID, model);
            await _dbContext.UpdateAsync<EntityPersonalInfo>("FirstName=@FirstName,MiddleName=@MiddleName,LastName=@LastName", entityID, model);
        }

    }
}
