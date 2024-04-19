using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Court
{
    public class CourtAvailPostModel
    {
        public int GameID { get; set; }
        public int HallID { get; set; }
        public DateTime? Date { get; set; }
      
    }
    public class CourtBookingDataModel
    {
        public int BookingID { get; set; }
        public string? Name { get; set; }
        public int? HallID { get; set; }
        public string? HallName { get; set; }
        public int? BookingStatus { get; set; }
        public int HourID { get; set; }
        public string? Phone { get; set; }
        public int CourtID { get; set; }
        public int JournelMasterID { get; set; }

    }
}
