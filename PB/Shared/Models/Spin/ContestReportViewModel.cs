using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Spin
{
    public class ContestReportViewModel
    {
        public int? ContestID { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? ContestName { get; set; }
        public string? GiftName { get; set; }
        public int? ContactID { get; set; }
        public bool IsFinished { get; set; }
        public bool IsSent { get; set; }
        public DateTime? SentOn { get; set; }
        public int? GiftId { get; set; }
    }
    public class ContestGiftSendModel
    {
        public int? ContactID { get; set; }
        public bool IsSent { get; set; }
    }

    public class ReportIDSentModel
    {
        public int? ContestID { get; set; }
        public int StatusID { get; set; }
    }
}
