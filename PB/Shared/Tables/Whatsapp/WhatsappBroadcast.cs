using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Whatsapp
{
    public class WhatsappBroadcast : Table
    {
        [PrimaryKey]
        public int BroadcastID { get; set; }
        public string? BroadcastName { get; set; }
        public int? TemplateID { get; set; }
        public DateTime? ScheduleTime { get; set; }
        public int PeriodicDay { get; set; }
        public DateTime? AddedOn { get; set; }
        public int? HeaderMediaID { get; set; }
        public bool IsSent { get; set; }
    }
}
