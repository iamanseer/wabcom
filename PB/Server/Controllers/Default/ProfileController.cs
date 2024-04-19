using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NPOI.SS.Formula.Functions;
using PB.Client.Pages.SuperAdmin.Client;
using PB.DatabaseFramework;
using PB.EntityFramework;
using PB.Model;
using PB.Shared;
using PB.Shared.Enum;
using PB.Shared.Models;
using PB.Shared.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace PB.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ProfileController : BaseController
    {
        private readonly IDbContext _dbContext;
        private readonly IAuthenticationRepository _auth;

        public ProfileController(IDbContext dbContext, IAuthenticationRepository auth)
        {
            _dbContext = dbContext;
            _auth = auth;
        }

        #region Profile

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            await _auth.ChangePassword(CurrentUserID, model.CurrentPassword, model.NewPassword);
            return Ok(new Success());
        }

        [HttpGet("get-profile-details")]
        [HttpPost("get-profile-details")]
        public async Task<IActionResult> GetProfileDetails()
        {
            //var res = await _auth.GetProfileDetails(CurrentEntityID);
            //return Ok(res);

            var result = await _dbContext.GetByQueryAsync<ProfileModelCustom>(@$"
                                       	                Select Name As FirstName,Phone,EmailAddress,M.MediaID,M.FileName AS MediaURL,U.Username
                                                        From viEntity E
                                                        Left Join Media M ON E.MediaID=M.MediaID and M.IsDeleted=0
                                                        Left Join Users U ON U.EntityID=E.EntityID and U.IsDeleted=0
                                                        Where E.EntityID={CurrentEntityID}
            ", null);
            return Ok(result);
        }


        [HttpPost("update-profile")]
        public async Task<IActionResult> UpdateProfile(ProfileModel model)
        {
            await _auth.UpdateProfile(CurrentEntityID, model);
            return Ok(new ProfileUpdateResultModel()
            {
                ResponseTitle = ResponseMessage.ResourceManager.GetString("SuccessTitle"),
                ResponseMessage = ResponseMessage.ResourceManager.GetString("ProfileUpdated"),
                ProfileURL = await _dbContext.GetFieldsAsync<ViEntity, string>("ProfileImage", $"EntityID={CurrentEntityID}", null)
            });
        }

        //[HttpPost("update-profile-photo")]
        //public async Task<IActionResult> UpdateProfilePhoto(ProfilePhotoModel model)
        //{
        //    await _dbContext.UpdateAsync<EntityCustom>($"MediaID=@MediaID", $"EntityID={CurrentEntityID}", model);
        //    return Ok(new Success());
        //}

        #endregion

        [HttpPost("app-logout")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> AppLogout(AppLogoutModel model)
        {
            var data = await _dbContext.GetAsync<UserMachine>($"EntityID={CurrentEntityID} and MachineID=@MachineID", model);
            if (data != null)
                await _dbContext.DeleteAsync<UserMachine>(data.ID);

            return Ok(new Success());
        }

        [HttpPost("udate-profile-for-ordering-app")]
        public async Task<IActionResult> UpdateProfileNew(ProfileModel model)
        {
            var email = await _dbContext.GetByQueryAsync<string>($"Select EmailAddress From Entity Where EmailAddress=@EmailAddress And EntityID<>@EntityID And ClientID=@ClientID And (EntityTypeID={(int)EntityType.Staff} OR EntityTypeID={(int)EntityType.Client})", new { EmailAddress = model.EmailAddress, EntityID = CurrentEntityID, ClientID=CurrentClientID });
            if (email is not null)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ResponseMessage = "EmailExist"
                });
            }
            var phone = await _dbContext.GetByQueryAsync<string>($"Select Phone From Entity Where Phone=@Phone And EntityID<>@EntityID And ClientID=@ClientID And (EntityTypeID={(int)EntityType.Staff} OR EntityTypeID={(int)EntityType.Client})", new { Phone = model.Phone, EntityID = CurrentEntityID, ClientID= CurrentClientID });
            if (phone is not null)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ResponseMessage = "PhoneExist"
                });
            }

            if (CurrentUserTypeID == (int)UserTypes.Client)
            {
                await _dbContext.ExecuteAsync("Update Entity Set Phone=@Phone,EmailAddress=@EmailAddress Where EntityID=@EntityID", new { Phone = model.Phone, EmailAddress = model.EmailAddress, EntityID = CurrentEntityID });
                await _dbContext.ExecuteAsync("Update EntityInstituteInfo Set Name=@Name Where EntityID=@EntityID", new { Name = model.FirstName, EntityID = CurrentEntityID });
            }

            if (CurrentUserTypeID == (int)UserTypes.Staff)
            {
                await _dbContext.ExecuteAsync("Update Entity Set Phone=@Phone,EmailAddress=@EmailAddress,MediaID=@MediaID Where EntityID=@EntityID", new { Phone = model.Phone, EmailAddress = model.EmailAddress, MediaID = model.MediaID, EntityID = CurrentEntityID });
                await _dbContext.ExecuteAsync("Update EntityPersonalInfo Set FirstName=@FirstName,MiddleName=@MiddleName,LastName=@LastName Where EntityID=@EntityID", new { FirstName = model.FirstName, MiddleName = model.MiddleName, LastName = model.LastName, EntityID = CurrentEntityID });
            }

            return Ok(new ProfileUpdateResultModel()
            {
                ResponseTitle = ResponseMessage.ResourceManager.GetString("SuccessTitle"),
                ResponseMessage = ResponseMessage.ResourceManager.GetString("ProfileUpdated"),
                ProfileURL = await _dbContext.GetFieldsAsync<ViEntity, string>("ProfileImage", $"EntityID={CurrentEntityID}", null)
            });
        }
    }
}
