using PB.Shared.Tables.Whatsapp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models
{
    public class ChatbotMultiReplyModel
    {
        public ChatbotMultiReply Reply { get; set; } = new();
        public ChatbotMultiReplyLocation Location { get; set; } = new();
    }
}
