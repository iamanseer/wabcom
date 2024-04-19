using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models
{
    public class MessageResponseModel
    {
        public string? messaging_product { get; set; }
        public List<MessageResponseMessageModel>? messages { get; set; }
    }

    public class MessageResponseMessageModel
    {
        public string? id { get; set; }
    }
}
