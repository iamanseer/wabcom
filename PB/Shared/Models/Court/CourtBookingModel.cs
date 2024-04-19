using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Court
{
    public  class CourtBookingModel
    {
        public int BookingID { get;set; }
        public int GameID { get;set; }
        public string? GameName { get; set; }
        public int? HallID { get; set; }
        public string? HallName { get; set; }
        public int CourtID { get; set; }
        public string?CourtName { get; set; }   
        public DateTime? Date { get; set; }
        public int FromHourID { get; set; }
        public int ToHourID { get; set; }
        public string? FromHourName { get; set; }
        public string? ToHourName { get; set; }
        public int CustomerEntityID { get; set; }
        public string? CustomerName { get; set; }
        public int BookingCourtID { get; set; }

    }
    public class CourtBookingViewModel:CourtBookingModel
    {
        public string? BookingNo { get; set; }
        public string? Phone { get; set; }
        public string? EmailAddress { get; set; }
        public bool? IsCancelled { get; set; }
        public string? Timing { get; set; }
    }
}
