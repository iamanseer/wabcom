using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PB.DatabaseFramework;
using PB.EntityFramework;
using PB.Model;
using PB.Shared;
using PB.Shared.Enum;
using PB.Shared.Helpers;
using PB.Shared.Models;
using PB.Shared.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PB.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class NotificationController : BaseController
    {
        private readonly IDbContext _dbContext;
        private readonly INotificationRepository _notification;
        private readonly IHttpContextAccessor _accessor;

        public NotificationController(IDbContext dbContext, INotificationRepository notification, IHttpContextAccessor accessor)
        {
            _dbContext = dbContext;
            _notification = notification;
            _accessor = accessor;
        }

        [HttpGet("get-notification-count")]
        public async Task<ActionResult> GetNotificationCount()
        {
            var result = await _dbContext.GetByQueryAsync<int>($@"Select Count(*) from viNotification where EntityID={CurrentEntityID} and IsRead=0", null);
            return Ok(result);
        }

        [HttpGet("get-unread-message-count")]
        public async Task<ActionResult> GetUnreadMessageCount()
        {
            var result = await _dbContext.GetByQueryAsync<int>($@"Select Count(Distinct ContactID) from WhatsappChat Where IsIncoming=1 and SeenOn is null and WhatsappAccountID in(Select WhatsappAccountID from WhatsappAccount Where ClientID={CurrentClientID})", null);
            return Ok(result);
        }

        [HttpPost("get-notifications")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> GetAllNotifications(NotificationSearchModel model)
        {
            if (model.TimeOffset == 0)
                model.TimeOffset = TimeOffset;
            PagedListQueryModel searchData = model;
            searchData.Select = $@"Select NotificationID, NotificationText, NotificationText2, NotificationTypeID,RefID, EntityID, IsRead,DATEADD(MI,{model.TimeOffset}, AddedOn) as AddedOn from viNotification";
            searchData.WhereCondition = $"EntityID={CurrentEntityID}";
            searchData.SearchLikeColumnNames = new List<string>() { "NotificationText" };
            var result = await _dbContext.GetPagedList<ViNotificationCustom>(searchData, null);
            await _dbContext.ExecuteAsync($"Update Notification set IsRead=1,ReadOn=@Date where EntityID={CurrentEntityID} and ReadOn is null", new { Date = DateTime.UtcNow });
            await _dbContext.ExecuteAsync(@"Update Users set NotificationReadOn=@Date Where UserID=@UserID", new { Date = DateTime.UtcNow, UserID = CurrentUserID });
            return Ok(result);
        }


        [HttpGet("add-signalR-id")]
        public async Task<IActionResult> AddSignalRId(string connectionId, int isApp, string deviceId)
        {
            var ipAddress = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();
            if(string.IsNullOrEmpty(deviceId))
                await _dbContext.ExecuteAsync($"Delete from UserMachine Where EntityID={CurrentEntityID} and IsSignalR=1 and IsApp={isApp} and IPAddress=@IPAddress", new { IPAddress = ipAddress });
            else
                await _dbContext.ExecuteAsync($"Delete from UserMachine Where EntityID={CurrentEntityID} and DeviceID=@DeviceID", new { DeviceID = deviceId });

            await _dbContext.SaveAsync(new UserMachineCustom() { EntityID = CurrentEntityID, IsSignalR = true, MachineID = connectionId, IsApp = Convert.ToBoolean(isApp), IPAddress = ipAddress,DeviceID=deviceId }, needLog: false);
            return Ok(new Success());
        }


        [HttpGet("test-notification")]
        [AllowAnonymous]
        public async Task<IActionResult> TestNotification(int entityId)
        {
            await _notification.SendNotification(entityId, $"Notification test @{DateTime.Now}", (int)NotificationTypes.Enquiry, 1, $"Notification msg2 test @{DateTime.Now}");
            await _notification.SendSignalRPush(entityId, $"Test @{DateTime.Now}", null, "chat");
            return Ok(new Success());
        }

        [HttpGet("get-app-latest-version")]
        public async Task<IActionResult> GetAppVersion()
        {
            var res=await _dbContext.GetByQueryAsync<AppVersionModel>("Select AppVersion From GeneralSetting", null);
            return Ok(res);
        }
    }
}
