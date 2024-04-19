using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.CourtClient
{
    public class Booking:Table
    {
        [PrimaryKey] 
        public int BookingID { get; set; }
        public int BookingNo { get; set; }
        public DateTime? Date { get; set; }
        public int? BookedBy { get; set; }
        public int? CustomerEntityID { get; set; }
        public int? JournalMasterID { get; set; }
        public int Status { get; set; }
        public int? BranchID { get; set; }
        public int DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal CashCollected { get; set; }
        public decimal Credit { get; set; }
        public int? CancelledBy { get; set; }
        public DateTime? CancelledOn { get; set; }
        public int? CancelJournalMasterID { get; set; }
        public bool IsLocked { get; set; }
        public int? CounterCodeID { get; set; }

        //Alter Table Booking Add  int, int, datetime
    }
}
