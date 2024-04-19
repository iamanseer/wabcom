using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.CourtClient
{
    public class BookingCourt:Table
    {
        [PrimaryKey]
        public int BookingCourtID { get; set; }
        public int? BookingID { get; set; }
        public DateTime? Date { get; set; }
        public int? CourtID { get; set; }
        public int? FromHourID { get; set; }
        public int? ToHourID { get; set; }
        public decimal? Rate { get; set; }
        public int? CurrencyID { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public decimal NetPrice { get; set; }
        public int? TaxCategoryID { get; set; }
        public decimal TaxPercentage { get; set; }
        public decimal Tax { get; set; }
        public decimal TotalPrice { get; set; }
        public bool IsCancelled { get; set; }
        public int? CancelledBy { get; set; }
        public DateTime? CancelledOn { get; set; }
        public int? CancelJournalMasterID { get; set; }
    }
}
