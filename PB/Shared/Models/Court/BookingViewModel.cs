using PB.Shared.Enum.Court;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Court
{
    public class BookingViewModel
    {
        public int BookingID { get; set; }
        public string? BookingNo { get; set; }
        public DateTime BookedOn { get; set; }
        public string? BookedBy { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public CourtBookingStatus Status { get;set; }
        public string? Remarks { get; set; }
    }
}
