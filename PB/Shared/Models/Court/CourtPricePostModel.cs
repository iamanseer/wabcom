using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Court
{
    public class CourtPricePostModel
    {
        public int CourtID { get; set; }
        public DateTime? Date { get; set; }
        public int? HourID { get; set; }
    }
}
