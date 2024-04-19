using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.WhatsaApp
{
    public class BroadcastReceipientHistoryExcelModel
    {
        public string? Phone { get; set; }
        public string? Name { get; set; }
        public int MessageStatus { get; set; }
        public string? SentOn { get; set; }
        public string? DeliveredOn { get; set; }
        public string? SeenOn { get; set; }
        public string? FailedReason { get; set; }
        public string? Message { get; set; }
    }
}
