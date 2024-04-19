using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Spin
{
    public class WinnersListModel
    {
        public int PrizeID { get; set; }
        public int? GiftID { get; set; }
        public int? ContactID { get; set; }
        public string? Name { get; set; }
        public string? GiftName { get; set; }
        public string? ProfileName { get; set; }
    }
}
