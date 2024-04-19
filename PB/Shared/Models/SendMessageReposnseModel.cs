using PB.Shared.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models
{
    public class SendMessageReposnseModel
    {
        public bool IsFailed { get; set; }
        public string? MessageID { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Message { get; set; }
        public int? MediaID { get; set; }
        public int MediaTypeID { get; set; } = (int)MessageTypes.Text;
    }
}
