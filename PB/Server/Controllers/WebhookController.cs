using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PB.Client.Pages;
using PB.DatabaseFramework;
using PB.EntityFramework;
using PB.IdentityServer;
using PB.Model;
using PB.Server.Repository;
using PB.Shared.Enum;
using PB.Shared.Models;
using PB.Shared.Tables;
using System.Reflection.Emit;

namespace PB.Server.Controllers
{
    [Route("webhook")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        private readonly IDbContext _dbContext;
        private readonly IWhatsappRepository _whatsapp;

        public WebhookController(IDbContext dbContext,IWhatsappRepository whatsapp)
        {
            _dbContext = dbContext;
            _whatsapp = whatsapp;
        }

        [HttpGet]
        public ActionResult<string> SetupWebHook()
        {
            string mode = Request.Query["hub.mode"];
            if (mode == "subscribe")
            {
                string verifyToken = Request.Query["hub.verify_token"];
                if (verifyToken == "Progbiz@2030")
                {
                    var challenge = Request.Query["hub.challenge"];
                    return Ok(Convert.ToInt32(challenge));
                }
            }   
            return Ok();
        }

        [HttpPost]
        public async Task ReceiveMessage()
        {
            #region Save IncomingData
            var requestBody = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var reqBody = JsonConvert.SerializeObject(requestBody);
            var model = JsonConvert.DeserializeObject<ReceiveMessageModel>(requestBody);
            if (!reqBody.Contains("919821937148"))
            {
                await _whatsapp.ReceiveMessage(model, reqBody);
            }

            #endregion
        }
    }
}
