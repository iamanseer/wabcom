using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared
{
    public class Success : BaseSuccessResponse
    {
        public Success(string message = "Success", string title = "SuccessTitle")
        {
            ResponseMessage = Resources.ResponseMessage.ResourceManager.GetString(message);
            if (!string.IsNullOrEmpty(title))
                ResponseTitle = Resources.ResponseMessage.ResourceManager.GetString(title);

            ResponseMessage ??= message;
            ResponseTitle ??= title;
        }
    }
}
