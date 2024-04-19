using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PB.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {

        protected virtual string CurrentUserName { get { return User.Claims.FirstOrDefault(c => c.Type == "UserName").Value.ToString();  } }
        protected virtual int CurrentUserID { get { return Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserID").Value); } }
        protected virtual int CurrentUserTypeID { get { return Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserTypeID").Value); } }
        protected virtual int CurrentEntityID { get { return Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "EntityID").Value); } }
        protected virtual int CurrentClientID { get { return Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "ClientID").Value); } }
        protected virtual int CurrentBranchID { get { return Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "BranchID").Value); } }
        protected virtual int TimeOffset { get { return User.Claims.FirstOrDefault(c => c.Type == "TimeOffset") == null ? 0 : Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "TimeOffset").Value); } }

    }


}
