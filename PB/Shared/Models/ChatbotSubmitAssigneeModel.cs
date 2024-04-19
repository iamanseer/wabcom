using PB.Shared.Tables.Whatsapp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models
{
    public class ChatbotSubmitAssigneeModel : ChatbotSubmitAssignee
    {
        public bool IsAssigned { get; set; } = false;
        public string? Name { get; set; }
    }
}
