using PB.Server.Repository;
using PB.Shared;
using PB.Shared.Enum;
using PB.Shared.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PB.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using PB.DatabaseFramework;
using PB.Shared.Models;
using System.Net;
using PB.Shared.Tables;
using System.Net.Mail;
using Microsoft.VisualBasic;
using NPOI.SS.Formula.Functions;
using PB.Client.Pages.Settings;
using static NPOI.HSSF.Util.HSSFColor;
using PB.Client.Pages.SuperAdmin;
using PB.Model.Tables;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Hangfire;
using System.Security.Principal;
using NPOI.HPSF;
using NPOI.OpenXmlFormats.Dml;
using PB.Shared.Models.SuperAdmin.Client;
using PB.Shared.Models.Common;
using PB.Model.Models;
using PB.Shared.Tables.CRM;
using PB.EntityFramework;
using static System.Net.WebRequestMethods;
using System.Reflection.PortableExecutable;

namespace PB.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationRepository _auth;
        private readonly IDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IDbConnection _cn;
        private readonly IUserRepository _user;
        private readonly IEmailSender _email;
        private readonly IAccountRepository _accounts;
        private readonly IBackgroundJobClient _job;
        private readonly IHostEnvironment _env;
        private readonly ICommonRepository _common;
        private readonly ISuperAdminRepository _supperAdmin;
        private readonly IPDFRepository _pdf;
        private readonly IInventoryRepository _inventory;
        public AuthController(IAuthenticationRepository auth, IDbContext dbContext, IConfiguration configuration, IDbConnection cn, IUserRepository user, IEmailSender email, IAccountRepository account, ICommonRepository common, IBackgroundJobClient job, IHostEnvironment env, ISuperAdminRepository supperAdmin, IPDFRepository newPDF, ICommonRepository newCommon, IInventoryRepository inventory)
        {
            _auth = auth;
            _dbContext = dbContext;
            _configuration = configuration;
            _cn = cn;
            _user = user;
            _email = email;
            _accounts = account;
            _common = common;
            _job = job;
            _env = env;
            _supperAdmin = supperAdmin;
            _pdf = newPDF;
            _common = newCommon;
            _inventory = inventory;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestModel model)
        {
            var user = await _auth.VerifyUser(model, checkEmailConfirmed: false);

            LoginResponseModelCustom response = new();


            response.EmailConfirmed = user.EmailConfirmed;

            if (!response.EmailConfirmed)
                return Ok(response);

            if (model.BranchID == -1)
            {
                if (user.UserTypeID == (int)DefaultUserTypes.Developer || user.UserTypeID == (int)DefaultUserTypes.SuperAdmin)
                {
                    response.Clients = await _auth.GetClients();
                }
                response.Branches = await _auth.GetBranches(user.UserTypeID, user.ClientID, user.UserID);

                if (response.Branches.Count == 0)
                {
                    if (user.UserTypeID == (int)DefaultUserTypes.Developer || user.UserTypeID == (int)DefaultUserTypes.SuperAdmin)
                        response = await GetToken(user.UserID, 0, 0, model.TimeOffset);
                    else
                        throw new PBException("DontHaveBranch");
                }
                else if (response.Branches.Count == 1)
                    response = await GetToken(user.UserID, user.ClientID, response.Branches[0].ID, model.TimeOffset);
            }
            else
            {
                int clientId;
                if (user.UserTypeID == (int)DefaultUserTypes.Developer || user.UserTypeID == (int)DefaultUserTypes.SuperAdmin)
                    clientId = model.ClientID;
                else
                    clientId = user.ClientID;

                await _auth.VerifyBranchAccess(user.UserID, model.BranchID, clientId, user.UserTypeID);
                response = await GetToken(user.UserID, clientId, model.BranchID, model.TimeOffset); 
            }

            if (model.MachineID != null)
            {
                var machineID = await _dbContext.GetFieldsAsync<UserMachine, string>("MachineID", $"EntityID={user.EntityID} And MachineID=@MachineID", new { MachineID = model.MachineID });
                if(string.IsNullOrEmpty(machineID))
                {
                    UserMachine userMachine = new()
                    {
                        EntityID = user.EntityID,
                        MachineID = model.MachineID
                    };
                    await _dbContext.SaveAsync(userMachine);
                }
            }
            return Ok(response);
        }

        private async Task<LoginResponseModelCustom> GetToken(int userId, int clientId, int branchId, int timeoffset)
        {
            var user = await _dbContext.GetByQueryAsync<UserTokenDataModelCustom>($@"Select U.UserID, ISNULL(E.Name,UserName) as Name, UserName, Password, PasswordHash, U.UserTypeID,
			                                                                            Isnull(ProfileImage,'') as ProfileImage,U.EntityID,
                                                                                        ISNULL(EmailAddress,'') as EmailAddress,ISNULL(Phone,'') as MobileNumber, T.DisplayName as UserTypeName,
                                                                                        ISNULL(U.ClientID,0) as ClientID
			                                                                            from Users U
			                                                                            LEFT JOIN viEntity E on E.EntityID=U.EntityID
                                                                                        LEFT JOIN UserType T on T.UserTypeID=U.UserTypeID
			                                                                            where ISNULL(U.IsDeleted,0)=0 and U.UserID={userId}", null);

            var tokenBranchModel = await _dbContext.GetFieldsAsync<ViBranch, UserTokenBranchDetailsModel>("BranchName,GMTOffset", $"BranchID={branchId}", null);

            int? paymentStatus = null;
            if (user.UserTypeID == (int)UserTypes.Client)
            {
                var BlockStatus = await _dbContext.GetByQueryAsync<bool>($@"Select IsNull(IsBlock,0)IsBlock From Client Where ClientID={clientId} and IsDeleted=0", null);
                if (BlockStatus)
                {
                    LoginResponseModelCustom result = new()
                    {
                        ClientID = clientId,
                        RenewalStatus = (int)RenewalStatus.Blocked,
                        UserTypeID = user.UserTypeID
                    };
                    return result;
                }
                paymentStatus = await _supperAdmin.GeneratePaymentStatus(clientId);
                if (paymentStatus == (int)RenewalStatus.Disconnected)
                {
                    LoginResponseModelCustom result = new()
                    {
                        ClientID = clientId,
                        RenewalStatus = (int)RenewalStatus.Disconnected,
                        UserTypeID = user.UserTypeID
                    };
                    return result;
                }
                if (paymentStatus == (int)RenewalStatus.NotCompleted)
                {
                    LoginResponseModelCustom result = new()
                    {
                        ClientID = clientId,
                        RenewalStatus = (int)RenewalStatus.NotCompleted,
                        UserTypeID = user.UserTypeID
                    };
                    return result;
                }
            }

            if (user.UserTypeID == (int)UserTypes.Staff)
            {
                var BlockStatus = await _dbContext.GetByQueryAsync<bool>($@"Select IsNull(IsBlock,0)IsBlock From Client Where ClientID={user.ClientID} and IsDeleted=0", null);
                if (BlockStatus)
                {
                    LoginResponseModelCustom result = new()
                    {
                        RenewalStatus = (int)RenewalStatus.Blocked,
                        UserTypeID = user.UserTypeID
                    };
                    return result;
                }
                int? LoginStatus = await _supperAdmin.GetClientPaymentStatus(userId, user.ClientID, null);
                if (LoginStatus == (int)RenewalStatus.Disconnected)
                {
                    LoginResponseModelCustom result = new()
                    {
                        RenewalStatus = (int)RenewalStatus.Disconnected,
                        UserTypeID = user.UserTypeID
                    };
                    return result;
                }
            }

            if (timeoffset == 0)
            {
                timeoffset = tokenBranchModel != null ? tokenBranchModel.GMTOffset : 0;
            }

            var claims = new List<Claim>()
            {
               new Claim(ClaimTypes.Name, user.UserID.ToString()),
               new Claim("ProfileName", user.Name),
               new Claim("UserName", user.UserName.ToString()),
               new Claim("UserID", user.UserID.ToString()),
               new Claim("UserTypeID", user.UserTypeID.ToString()),
               new Claim("ProfileURL", user.ProfileImage.ToString()),
               new Claim("EntityID", user.EntityID.ToString()),
               new Claim("EmailAddress", user.EmailAddress),
               new Claim("ClientID", clientId.ToString()),
               new Claim("BranchID", branchId.ToString()),
               new Claim("UserTypeName", user.UserTypeName),
               new Claim("BranchName",tokenBranchModel != null && !string.IsNullOrEmpty(tokenBranchModel.BranchName) ? tokenBranchModel.BranchName : ""),
               new Claim("TimeOffset",timeoffset.ToString()),
               new Claim(ClaimTypes.Role, user.UserTypeName)
            };

            var res = await GetJWTToken(userId, user.UserTypeID, clientId, claims);
            if (user.UserTypeID == (int)UserTypes.Client)
            {
                res.RenewalStatus = paymentStatus;
                res.ClientID = clientId;
                res.UserTypeID = user.UserTypeID;
            }
            return res;
        }

        #region JWT Token creation - Same for all project. Dont edit

        private async Task<LoginResponseModelCustom> GetJWTToken(int userId, int userTyeId, int clientId, List<Claim> claims)
        {
            LoginResponseModelCustom result = new()
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

            List<string> roles = await _common.GetRoles(userId, clientId, userTyeId);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: expiry,
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult> RefreshToken(LoginResponseModel refreshRequest)
        {
            var token = _auth.GetUserFromAccessToken(refreshRequest.AccessToken);

            if (token != null)
            {
                var userId = Convert.ToInt32(token.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);

                var user = await _dbContext.GetAsync<PB.Model.User>(userId);
                if (!user.LoginStatus)
                    throw new PBException("LoginInactive");


                if (await _auth.ValidateRefreshToken(userId, refreshRequest.RefreshToken))
                {
                    var userTypeId = Convert.ToInt32(token.Claims.FirstOrDefault(c => c.Type == "UserTypeID")?.Value);
                    var clientId = Convert.ToInt32(token.Claims.FirstOrDefault(c => c.Type == "ClientID")?.Value);
                    var branchId = Convert.ToInt32(token.Claims.FirstOrDefault(c => c.Type == "BranchID")?.Value);
                    return Ok(await GetToken(userId, clientId, branchId, refreshRequest.TimeOffset));
                }
            }
            return Ok(null);
        }

        #endregion

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            await _auth.ForgotPassword(model, PDV.BrandName);
            OtpSendSuccessModel successModel = new();
            successModel.UserID = await _dbContext.GetByQueryAsync<int>($@"Select UserID
                    from viEntity
                    Where UserID is not null and (EmailAddress=@EmailAddress or Phone=@EmailAddress)", model);

            return Ok(successModel);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            await _auth.ResetPassword(model);
            return Ok(new Success("PasswordChangeSuccess"));
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegistrationModel model)
        {
            int branchID = 0;
            var user = await _dbContext.GetAsync<Entity>($"EntityTypeID in({(int)EntityType.Client},{(int)EntityType.Branch}) and Phone=@MobileNo", model);
            if (user != null)
            {
                return BadRequest(new Error("MobileExist"));
            }
            int userId;
            var euser = await _dbContext.GetAsync<UserCustom>($"UserName=@Email", model);
            if (euser != null)
                return BadRequest(new Error("EmailExist"));
            var CompanyName = await _dbContext.GetByQueryAsync<string?>(@$"Select Name
                                                                            From Client C
                                                                            Left Join EntityInstituteInfo EI ON EI.EntityID=C.EntityID AND EI.IsDeleted=0
                                                                            Where LOWER(EI.Name)=LOWER(@CompanyName) and C.IsDeleted=0
            ", model);

            if (CompanyName != null)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "Company name already exist",
                    ResponseMessage = "The company name already exist,Please try different company name."
                });
            }

            if (model.PackageID == 0)
            {
                return BadRequest(new Error("Please choose one membership package"));
            }
            else
            {
                _cn.Open();
                using (var tran = _cn.BeginTransaction())
                {
                    try
                    {
                        #region Client

                        var clientEntity = new EntityCustom
                        {
                            Phone = model.MobileNo,
                            EntityTypeID = (int)EntityType.Client,
                            EmailAddress = model.Email,
                            CountryID = model.CountryID,

                        };
                        model.EntityID = await _dbContext.SaveAsync(clientEntity, tran);

                        var client = new ClientCustom
                        {
                            EntityID = model.EntityID,
                            PackageID = model.PackageID,

                        };
                        model.ClientID = client.ClientID = await _dbContext.SaveAsync(client, tran);

                        Model.Branch branch = new()
                        {
                            ClientID = client.ClientID,
                            EntityID = model.EntityID,
                        };
                        branchID = branch.BranchID = await _dbContext.SaveAsync(branch, tran);

                        EntityInstituteInfo instituteInfo = new()
                        {
                            EntityID = model.EntityID,
                            Name = model.CompanyName,
                            ContactPerson = model.ClientName,
                        };
                        await _dbContext.SaveAsync(instituteInfo, tran);

                        #endregion

                        #region User

                        UserCustom newuser = new()
                        {
                            EntityID = model.EntityID,
                            LoginStatus = true,
                            UserName = model.Email,
                            UserTypeID = (int)UserTypes.Client,
                            Password = model.Password,
                            ClientID = client.ClientID,
                            EmailConfirmed = false,
                        };
                        userId = await _user.SaveUser(newuser, true, tran, PDV.BrandName, model.Email);

                        #endregion

                        ClientRegistrationPackageDetailsModel InsertModel = new()
                        {
                            ClientID = model.ClientID,
                            ClientEntityID = client.EntityID,
                            PackageID = model.PackageID.Value,
                            PaymentStatus = (int)PaymentStatus.CheckoutNotComplete
                        };

                        var result = await _supperAdmin.HandleClientPackageAccountsInvoiceEntries(InsertModel, tran);

                        if (result is not null)
                        {
                            var packageRoleGroups = await _supperAdmin.FetchPackageRoleGroups(model.PackageID.Value, tran);

                            if (packageRoleGroups is not null && packageRoleGroups.Contains((int)RoleGroups.AccountsManagement))
                            {
                                await _accounts.InsertClientDefaultAccountsRelatedEntries(model.ClientID, tran);
                            }
                            if (packageRoleGroups is not null && packageRoleGroups.Contains((int)RoleGroups.InventoryManagement))
                            {
                                await _inventory.InsertClientInventoryDefaultEntries(model.ClientID, tran);
                            }

                            tran.Commit();
                            string packageName = await _dbContext.GetFieldsAsync<MembershipPackage, string>("PackageName", $"PackageID={model.PackageID.Value}", null);
                            string pdfName = "pdf_invoice_" + packageName.ToLower().Replace(' ', '_');
                            int pdfMediaID = await _pdf.CreateClientInvoicePdf(result.InvoiceID, pdfName);
                            if (pdfMediaID > 0)
                            {
                                var mailDetails = await _common.GetInvoiceEMailDetailsForClient(result.InvoiceID);
                                if (mailDetails is not null)
                                    await _common.SendInvoiceEmailToClient(mailDetails);
                            }

                            return Ok(new RegistraionAddResultModel() { ClientID = model.ClientID, UserID = userId });
                        }
                        else
                        {
                            tran.Rollback();
                            return BadRequest(new BaseErrorResponse() { ResponseMessage = "SomethingWentWrong" });
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
        }


        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOTP(VerifyOTPPostModel model)
        {
            //var user = await _dbContext.GetAsync<UserCustom>("UserName=@UserName and OTP=@OTP", model);
            var user = await _dbContext.GetByQueryAsync<UserCustom>($@"
                                                        Select U.* 
                                                        From Users U
                                                        Join Entity E ON E.EntityID=U.EntityID
                                                        Where (U.Username=@Username OR E.EmailAddress=@Username) And U.OTP=@OTP", new { Username = model.UserName, OTP = model.OTP });

            if (user == null)
                return BadRequest(new Error("InvalidOTP"));
            else
            {

                TimeSpan duration = DateTime.UtcNow.Subtract(Convert.ToDateTime(user.OTPGeneratedAt));

                if (duration.Minutes > 5)
                    return BadRequest(new BaseErrorResponse()
                    {
                        ResponseTitle = "OTP Timeout",
                        ResponseMessage = "Seems like you've ran out time,Please regenerate OTP and try again"
                    });
                user.OTPGeneratedAt = null;
                user.EmailConfirmed = true;
                user.OTP = "";
                await _dbContext.SaveAsync(user);


                //var branch = await _dbContext.GetByQueryAsync<Model.Branch>($@"Select C.ClientID,B.BranchID
                //    from Users U
                //    JOIN Client C on U.ClientID = C.ClientID
                //    JOIN Branch B on B.ClientID = C.ClientID
                //    Where UserID={user.UserID}");
                //return Ok(await GetToken(user.UserID, branch.ClientID.Value, branch.BranchID));
                return Ok(new Success());
            }
        }

        [HttpPost("verify-mail")]
        public async Task<IActionResult> VerifyMail(VerifyOTPPostModel model)
        {
            var user = await _dbContext.GetAsync<UserCustom>("UserID=@UserID and OTP=@OTP", model);
            if (user == null)
                return BadRequest(new Error("InvalidOTP"));
            else
            {

                TimeSpan duration = DateTime.UtcNow.Subtract(Convert.ToDateTime(user.OTPGeneratedAt));

                if (duration.Minutes > 5)
                    return BadRequest(new BaseErrorResponse()
                    {
                        ResponseTitle = "OTP Timeout",
                        ResponseMessage = "Seems like you've ran out time,Please regenerate OTP and try again"
                    });
                user.OTPGeneratedAt = null;
                user.EmailConfirmed = true;
                user.OTP = "";
                await _dbContext.SaveAsync(user);


                var branch = await _dbContext.GetByQueryAsync<Model.Branch>($@"Select C.ClientID,B.BranchID
                    from Users U
                    JOIN Client C on U.ClientID = C.ClientID
                    JOIN Branch B on B.ClientID = C.ClientID
                    Where UserID={user.UserID}", null);
                return Ok(await GetToken(user.UserID, branch.ClientID.Value, branch.BranchID, model.TimeOffset));
            }
        }

        [HttpGet("regenerate-otp/{userID}")]
        public async Task<IActionResult> RegenerateOTP(int userID)
        {
            var user = await _dbContext.GetAsync<UserCustom>(userID);
            if (user != null)
            {
                int _min = 1000;
                int _max = 9999;
                Random _rdm = new();
                user.OTP = (_rdm.Next(_min, _max)).ToString();
                user.OTPGeneratedAt = DateTime.UtcNow;
                await _dbContext.SaveAsync(user);
                string emailAddress = "";
                var message = $@"
                       <div style = 'font-family: Helvetica,Arial,sans-serif;min-width:1000px;overflow:auto;line-height:2;direction:rtl;'>
                            <div style = 'margin:50px auto;width:70%;padding:20px 0'>
                                <div style = 'border-bottom:1px solid #eee' >
                                    <a href ='' style = 'font-size:1.4em;color: #00466a;text-decoration:none;font-weight:600'> {PDV.BrandName} </a>
                                </div>
                                <p style = 'font-size:1.1em"">    Hi, Thank you for choosing {PDV.BrandName}.</p>
                                <p>Use the following OTP to complete your Sign Up procedures. </p> </p>
                              
                                <h2 style = 'background: #00466a;text-align:right;width: max-content;padding: 0 10px;color: #fff;border-radius: 4px;'> {user.OTP} </h2> 
                               <hr style = 'border: none; border - top:1px solid #eee' />
                            <div style='float:right; padding: 8px 0; color:#aaa;font-size:0.8em;line-height:1;font-weight:300'>
                                <p> Regards,</p>
                                    < p>  {PDV.BrandName} team</p>
                                </div>
                            </div>
                        </div>
                ";

                if (!string.IsNullOrEmpty(user.UserName) && IsValidEmail(user.UserName))
                {
                    emailAddress = user.UserName;
                }
                else
                {
                    emailAddress = await _dbContext.GetFieldsAsync<EntityCustom, string>("EmailAddress", $"EntityID={user.EntityID.Value}", null);
                }

                //  await _email.SendHtmlEmailAsync(emailAddress, "Verify your e-mail address of your account", message, tran);
                await _email.SendHtmlEmailAsync(emailAddress, "Verify your e-mail address of your account", message);
                return Ok(user.OTPGeneratedAt);
            }
            else
                return Ok(null);
        }

        static bool IsValidEmail(string email)
        {
            try
            {
                var mailAddress = new System.Net.Mail.MailAddress(email);
                return true;
            }
            catch
            {
                return false;
            }
        }

        [HttpPost("get-user")]
        public async Task<IActionResult> GetUserID(StringModel model)
        {
            var user = await _dbContext.GetByQueryAsync<UserCustom>($@"select VE.EntityID,U.UserID,VE.EmailAddress from ViEntity VE
                                                                LEFT JOIN users U ON U.EntityID = VE.EntityID
                                                                WHERE U.IsDeleted=0 and (VE.EmailAddress=@Value or U.UserName=@Value)", model);

            return Ok(user.UserID);
        }


        #region Country,Register

        [HttpGet("get-all-country-list")]
        public async Task<IActionResult> GetISD(int countryId)
        {
            var res = new List<IdnValuePair>();
            res = await _dbContext.GetListByQueryAsync<IdnValuePair>(@$"Select CountryID as ID,ISDCode as Value From Country
                                                                            Where  IsDeleted=0", null);

            return Ok(res ?? new());
        }

        [HttpGet("get-membership-packages-list")]
        public async Task<IActionResult> GetPlan()
        {
            var res = new List<MembershipPackageListModel>();
            res = await _dbContext.GetListByQueryAsync<MembershipPackageListModel>(@$"Select PackageName,Fee,PackageID
                                                                                    From MembershipPackage 
                                                                                    Where IsDeleted=0 and IsCustom=0
                                                                                    Order By PackageID ASC", null);


            return Ok(res ?? new());
        }

        [HttpGet("get-membership-plans-list")]
        public async Task<IActionResult> GetPackagePlan()
        {
            var res = new List<MembershipPlanModel>();
            res = await _dbContext.GetListByQueryAsync<MembershipPlanModel>(@$"Select PlanID,PlanName,MonthCount,Case When MonthCount=1 then 1 else 0 end as IsSelected 
                                                                                From MembershipPlan
                                                                                Where IsDeleted=0
                                                                                Order By MonthCount ASC", null);


            return Ok(res ?? new());
        }

        [HttpGet("get-packages-list/{PlanID}")]
        public async Task<IActionResult> GetPackageMonth(int PlanID)
        {
            var res = new List<MembershipPackageListModel>();
            res = await _dbContext.GetListByQueryAsync<MembershipPackageListModel>(@$"Select PackageName,Fee,PackageID,MP.PlanID,MP.MediaID,FileName
                                                                                        From MembershipPackage MP
                                                                                        Join MembershipPlan M on M.PlanID=MP.PlanID and M.IsDeleted=0
                                                                                        Left Join Media MM on MM.MediaID=MP.MediaID and MM.IsDeleted=0
                                                                                        Where MP.IsDeleted=0 and M.PlanID={PlanID} and IsCustom=0
                                                                                        Order By PackageID ASC", null);


            return Ok(res ?? new());
        }

        [HttpGet("get-membership-package-details/{packageId}")]
        public async Task<IActionResult> GetPackageDescription(int packageId)
        {
            var res = await _dbContext.GetByQueryAsync<MembershipPackageListModel>(@$"Select PackageID,PackageName,PackageFeatures,PackageDescription,IsCustom,Fee,MonthCount
                                                                                    From MembershipPackage MP
                                                                                    Join MembershipPlan M on M.PlanId=MP.PlanID and M.IsDeleted=0
                                                                                    Where PackageID={packageId} and MP.IsDeleted=0 and IsCustom=0", null);

            res.featureList = res.PackageFeatures.Split('.').ToList();

            return Ok(res ?? new());
        }

        #endregion


        #region Payment Checkout

        [HttpGet("get-check-out-details/{ClientId}")]
        public async Task<IActionResult> GetCheckOut(int ClientId)
        {
            var res = await _dbContext.GetByQueryAsync<PlanCheckOutModel>(@$"Select CI.InvoiceID,CI.PackageID,C.ClientID,Packagename,CI.Fee,PackageFeatures,PackageDescription,
                                                                            U.UserID,V.Name as CompanyName,VE.EmailAddress,C.EntityID,MonthCount,E.CountryID,
                                                                            CO.CountryName,U.EntityID as UserEntityID,CI.InvoiceJournalMasterID,EII.ContactPerson as FirstName,B.BranchID,
                                                                            EA.StateID,StateName as State,EA.CityID,CityName as City,B.ZoneID,ZoneName,EA.Pincode,EA.AddressID,EA.AddressLine1
                                                                            From ClientInvoice CI
                                                                            Join MembershipPackage Mp on MP.PackageID=CI.PackageID and Mp.IsDeleted=0
                                                                            Join MembershipPlan M on M.PlanID=MP.PlanID
                                                                            Join Client C on C.ClientID=CI.ClientID and C.IsDeleted=0
                                                                            Join Users U on U.ClientID=C.ClientID and U.IsDeleted=0
                                                                            Left Join viEntity V on V.EntityID=U.EntityID 
                                                                            Left Join viEntity VE on VE.EntityID=C.EntityID
                                                                            Left Join Entity E on E.EntityID=VE.EntityID and E.IsDeleted=0
                                                                            Left Join EntityInstituteInfo EII on EII.EntityID=VE.EntityID and EII.Isdeleted=0
                                                                            Left Join Country CO on CO.CountryID=E.CountryID
                                                                            Left Join Branch B on B.EntityID=C.EntityID
                                                                            Left Join EntityAddress EA on EA.EntityID=C.EntityID and EA.IsDeleted=0
                                                                            Left Join CountryState SC on SC.StateID=EA.StateID and SC.IsDeleted=0
                                                                            Left Join CountryCity CC on CC.CityID=EA.CityID and CC.IsDeleted=0
                                                                            Left Join CountryZone CZ on CZ.ZoneID=B.ZoneID and CZ.IsDeleted=0
                                                                            Where C.ClientID={ClientId} and CI.IsDeleted=0", null);

            res.features = res.PackageFeatures.Split('.').ToList();
            return Ok(res ?? new());
        }


        [HttpPost("save-check-out")]
        public async Task<IActionResult> SaveCheckOut(PlanCheckOutModel model)
        {
            ClientInvoiceSaveReturnModel? result = null;

            _cn.Open();
            using (var tran = _cn.BeginTransaction())
            {
                try
                {

                    int existingPackageID = await _dbContext.GetByQueryAsync<int>(@$"Select PackageID From Client
                                                                            Where ClientID={model.ClientID} and IsDeleted=0 
                    ", null, tran);


                    await _dbContext.ExecuteAsync($"UPDATE Entity SET EmailAddress='{model.EmailAddress}',CountryID={model.CountryID} Where EntityID={model.EntityID}", null, tran);
                    await _dbContext.ExecuteAsync($"UPDATE EntityInstituteInfo SET Name='{model.CompanyName}',ContactPerson='{model.FirstName}' Where EntityID={model.EntityID}", null, tran);
                    await _dbContext.ExecuteAsync($"UPDATE Branch Set ZoneID={model.ZoneID} Where BranchID={model.BranchID}", null, tran);

                    EntityAddressCustom address = new()
                    {
                        AddressID = model.AddressID,
                        EntityID = model.EntityID,
                        AddressLine1 = model.AddressLine1,
                        CountryID = model.CountryID,
                        StateID = model.StateID,
                        State = model.State,
                        Pincode = model.PinCode,
                        AddressType = (int)AddressTypes.Billing,
                        CityID = model.CityID
                    };
                    await _dbContext.SaveAsync(address, tran);

                    await _dbContext.ExecuteAsync($@"UPDATE ClientInvoice
                                                            SET PaidStatus={(int)PaymentStatus.Pending}
                                                            Where InvoiceID={model.InvoiceID} and ClientID={model.ClientID}
                    ", null, tran);


                    if (existingPackageID != model.PackageID)
                    {
                        ClientRegistrationPackageDetailsModel UpdateModel = new()
                        {
                            ClientID = model.ClientID.Value,
                            ClientEntityID = model.EntityID.Value,
                            PackageID = model.PackageID.Value,
                            PaymentStatus = (int)PaymentStatus.Pending,
                            InvoiceJournalMasterID = model.InvoiceJournalMasterID.Value,
                            InvoiceID = model.InvoiceID
                        };

                        result = await _supperAdmin.HandleCheckoutPackageChange(UpdateModel, tran);
                    }

                    tran.Commit();

                    if (result is not null)
                    {
                        string packageName = await _dbContext.GetFieldsAsync<MembershipPackage, string>("PackageName", $"PackageID={model.PackageID.Value}", null);

                        string pdfName = "pdf_invoice_" + packageName.ToLower().Replace(' ', '_');

                        int pdfMediaID = await _pdf.CreateClientInvoicePdf(result.InvoiceID, pdfName);

                        if (pdfMediaID > 0)
                        {
                            var mailDetails = await _common.GetInvoiceEMailDetailsForClient(result.InvoiceID);
                            if (mailDetails is not null)
                                await _common.SendInvoiceEmailToClient(mailDetails);
                        }
                    }

                    return Ok(new Success());
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

        [HttpPost("update-payment-details")]
        public async Task<IActionResult> VerifyPayment(Shared.Models.PaymentVerificationModel model)
        {
            int invoiceID = await _dbContext.GetByQueryAsync<int>($@"Select A.InvoiceID From(
                                                                Select ClientID,Max(InvoiceID)as InvoiceID From ClientInvoice
                                                                Where isDeleted=0 
                                                                Group By ClientID) as A 
                                                                Left Join Clientinvoice I on I.InvoiceID=A.InvoiceID  and I.IsDeleted=0
                                                                Where A.ClientID={model.ClientID} and ReceiptJournalMasterID Is NULL and (PaidStatus={(int)PaymentStatus.Pending})  and Isdeleted=0", null);


            await _dbContext.ExecuteAsync($"UPDATE ClientInvoice SET PaidStatus={(int)PaymentStatus.Paid},PaymentRefNo=@PaymentRefNo,MediaID=@MediaID Where InvoiceID={invoiceID}", model);

            return Ok(new Success());
        }

        [HttpPost("save-client-enquiry")]
        public async Task<IActionResult> SaveClientEnquiry(CustomDetailsModel model)
        {
            var entity = new EntityCustom
            {
                Phone = model.Phone,
                EmailAddress = model.EmailAddress,
                CountryID = model.CountryID,
                ClientID = PDV.ProgbizClientID,
                EntityTypeID = (int)EntityType.Customer
            };
            entity.EntityID = await _dbContext.SaveAsync(entity);
            var entityInfo = new EntityPersonalInfo
            {
                EntityID = entity.EntityID,
                FirstName = model.Name,
            };
            await _dbContext.SaveAsync(entityInfo);
            var entityInstituteInfo = new EntityInstituteInfo
            {
                EntityID = entity.EntityID,
                Name = model.CompanyName,
            };
            await _dbContext.SaveAsync(entityInstituteInfo);



            int BranchId = await _dbContext.GetByQueryAsync<int>(@$"Select BranchID from Client C
                                                                    Join Branch B on B.ClientID=C.ClientID and B.IsDeleted=0
                                                                    Where C.ClientID={PDV.ProgbizClientID} and C.IsDeleted=0", null);
            model.EnquiryNo = await _dbContext.GetByQueryAsync<int>($@"Select isnull(max(EnquiryNo)+1,1) AS EnquiryNo
                                                                From Enquiry
                                                                Where BranchID={BranchId} and IsDeleted=0", null);
            var enquiry = new Enquiry
            {
                Date = DateTime.UtcNow,
                EnquiryNo = model.EnquiryNo.Value,
                ClientID = PDV.ProgbizClientID,
                CustomerEntityID = entity.EntityID,
                FirstFollowUpDate = DateTime.UtcNow.Date.AddDays(1),
                Description = "PNL Enquiry : " + model.Description,
                BranchID = BranchId,

            };
            await _dbContext.SaveAsync(enquiry);
            try
            {
                var message = $@"
                       <div style = ""font-family: Helvetica,Arial,sans-serif;min-width:1000px;overflow:auto;line-height:2;direction:ltr;"">
                            <div style = ""margin:50px auto;width:70%;padding:20px 0"">
                                <div style = ""border-bottom:1px solid #eee"" >
                                    <a href = """" style = ""font-size:1.4em;color: #00466a;text-decoration:none;font-weight:600""> {PDV.BrandName} </a>
                                </div>
                                <p style = ""font-size:1.1em"">    Hi, </p>
                                <p>Thank you for choosing our custom plan. Our authorized person will contact you shortly.For any query please contact  9846982593</p>
                                
                                <h2 style = ""background: #00466a;text-align:right;width: max-content;padding: 0 10px;color: #fff;border-radius: 4px;""> </h2> 
                               <hr style = ""border: none; border - top:1px solid #eee"" />
                            <div style=""float:right; padding: 8px 0; color:#aaa;font-size:0.8em;line-height:1;font-weight:300"">
                                <p> Regards,</p>
                                    < p>  {PDV.BrandName} team</p>
                                </div>
                            </div>
                        </div>
                ";

                await _email.SendHtmlEmailAsync(model.EmailAddress, "Custom Plan", message);
            }
            catch (Exception e)
            {

            }
            return Ok(new Success());
        }

        #endregion

    }
}
