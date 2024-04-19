using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PB.DatabaseFramework;
using PB.EntityFramework;
using PB.Model;
using PB.Server.Repository;
using PB.Shared;
using PB.Shared.Models;
using PB.Shared.Tables;
using System.Net.Mail;
using System.Reflection;
using System.Xml.Linq;

namespace PB.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class EmailController : BaseController
    {
        private readonly IDbContext _dbContext;
        private readonly IHostEnvironment _env;
        private readonly IBackgroundJobClient _job;

        public EmailController(IDbContext dbContext, IHostEnvironment env,  IBackgroundJobClient job)
        {
            _dbContext = dbContext;
            _env = env;
            _job = job;
        }

        [HttpPost("save-mail-settings")]
        public async Task<IActionResult> SaveMailSetting(MailSettingsPostModel model)
        {
            MailSettings mailSettings = new()
            {
                MailSettingsID = model.MailSettingsID,
                Name = model.Name,
                SMTPHost = model.SMTPHost,
                EmailAddress = model.EmailAddress,
                Password = model.Password,
                Port = Convert.ToInt32(model.Port),
                EnableSSL = model.EnableSSL,
                ClientID = CurrentClientID,
            };
            await _dbContext.SaveAsync(mailSettings);
            return Ok(new Success());
        }

        [HttpPost("get-all-mail-settings")]
        public async Task<IActionResult> GetAllMailSettings(PagedListPostModel model)
        {
            PagedListQueryModel searchData = model;
            searchData.Select = "select MailSettingsID,SMTPHost,Port,Name from MailSettings";
            searchData.WhereCondition = $"IsDeleted=0 and ClientID={CurrentClientID}";
            var result = await _dbContext.GetPagedList<MailSettings>(searchData,null);
            return Ok(result);
        }

        [HttpGet("get-mail-settings/{Id}")]
        public async Task<IActionResult> GetMailSettings(int Id)
        {
            var result = await _dbContext.GetByQueryAsync<MailSettingsPostModel>($@"Select MailSettingsID,SMTPHost,Port,Name,EmailAddress,EnableSSL,Password,UserName
                                                                            from MailSettings
                                                                            where MailSettingsID={Id}", null);
            return Ok(result);
        }

        [HttpGet("get-client-mail-setting")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetMail()
        {
              var result = await _dbContext.GetByQueryAsync<MailSettingsPostModel>($@"Select MailSettingsID,SMTPHost,Port,Name,EmailAddress,EnableSSL,Password,UserName
                                                                            from MailSettings
                                                                            where ClientID={CurrentClientID}", null);
              result=result?? new();
                return Ok(result);
           
        }

        [HttpGet("delete-mail-settings")]
        public async Task<IActionResult> DeleteMailSettings(int Id)
        {
            var result = await _dbContext.DeleteAsync<MailSettings>(Id);
            return Ok(new Success());
        }
    }
}