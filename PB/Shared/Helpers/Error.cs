using PB.Model;
using PB.Shared.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared
{
    public class Error : BaseErrorResponse
    {
        public Error(string message = "Error", string title = "ErrorTitle", string description = "")
        {
            ResponseMessage = Resources.ResponseMessage.ResourceManager.GetString(message);
            ResponseTitle = Resources.ResponseMessage.ResourceManager.GetString(title);
            ResponseErrorDescription = description;

            ResponseMessage ??= message;
            ResponseTitle ??= title;
        }

        public Error(BaseErrorResponse err)
        {
            ResponseMessage = Resources.ResponseMessage.ResourceManager.GetString(err.ResponseMessage);
            ResponseTitle = Resources.ResponseMessage.ResourceManager.GetString(err.ResponseTitle);
            ResponseErrorDescription = err.ResponseErrorDescription;

            ResponseMessage ??= err.ResponseMessage;
            ResponseTitle ??= err.ResponseTitle;
        }
    }

    public class DbError : BaseErrorResponse
    {
        public DbError(string description = "", string title = "ErrorTitle")
        {
            ResponseMessage = Resources.ResponseMessage.ResourceManager.GetString("Error");
            ResponseTitle = Resources.ResponseMessage.ResourceManager.GetString(title);
           
            ResponseMessage ??= description;
            ResponseTitle ??= title;

            ResponseErrorDescription = description;
        }
    }
}
