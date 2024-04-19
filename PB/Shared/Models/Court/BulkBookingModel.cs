using PB.Shared.Models.Court;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Court
{
    public class BulkBookingSearchModel
    {
        [Required(ErrorMessage ="Please choose From Date")]
        public DateTime? FromDate { get; set; }
        [Required(ErrorMessage = "Please choose To Date")]
        public DateTime? ToDate { get; set; }
        [Required(ErrorMessage = "Please choose start hour")]
        public int? FromHourID { get; set; }
        [Required(ErrorMessage = "Please choose end hour")]
        public int? ToHourID { get; set; }
        public List<DayListModel> DayList { get; set; } = new();
        [Range(1,int.MaxValue,ErrorMessage ="Please choose court count")]
        public int CourtCount { get; set; }
        public List<CourtListModel> CourtList { get; set; } = new();
        public bool? IsAvoidUnavailableSlots { get; set; } = false;
    }

    public class DayListModel
    {
        public int? DayID { get;set; }
        public string? DayName { get; set; }
        public bool? IsChecked { get; set; } = false;

    }
    public class BulkbookingSearchResultModel
    {
        public List<AvailableSlotModel> AvailableSlots { get; set; } = new();
        public List<UnavailableSlotModel> UnavailableSlots { get; set; } = new();
    }
    public class AvailableSlotModel
    {
        public DateTime Date { get; set; }
        public int? CourtID { get; set; }
        public string? CourtName { get; set; }
        public int FromHourID { get; set; }
        public string? FromHourName { get; set; }
        public int ToHourID { get; set; }
        public string? ToHourName { get; set; }
    } 
    public class UnavailableSlotModel
    {
        public DateTime Date { get; set; }
        public int UnavailableCount { get; set; }
        public string Time { get; set; }
    }

    public class BulkBookingResultModel
    {
        public int BookingID { get; set; }
        public List<string> UnavailableCourts { get; set; } = new();
        public List<CourtBookingViewModelNew> BookedCourts { get; set; } = new();
    }
}
