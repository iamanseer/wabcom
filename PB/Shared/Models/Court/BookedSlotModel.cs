using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Court
{
    public class BookedSlotModel
    {
        public int? BookingID { get; set; }
        public int? BookingCourtID { get; set; }
        public string? DateTxt { get; set; }
        public string? Timing { get; set; }
        public string? CourtName { get; set; }
        public int? CourtID { get; set; }
    }
}
