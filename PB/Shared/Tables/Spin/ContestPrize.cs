using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables
{
    public class ContestPrize:Table
    {
        [PrimaryKey] 
        public int PrizeID { get; set; }
        public int? GiftID { get; set; }
        public int? ContactID { get; set; }
        public DateTime? ReceivedOn { get; set; }
        public bool IsSent { get; set; }
        public DateTime? SentOn { get; set; }
    }
}
