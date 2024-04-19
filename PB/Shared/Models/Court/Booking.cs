using PB.Model;
using PB.Shared.Enum.Court;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Court
{
    public class GetCourtDaySchedulePostModel
    {
        public int HallID { get; set; }
        public string? HallName { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
    }

    public class GetCourtDayScheduleResultModel
    {
        public List<IdnValuePair>? Hours { get; set; }
        public List<IdnValuePair>? Courts { get; set; }
        public List<BookingDetailsModel> Bookings { get; set; }
    }

    public class BookingDetailsModel
    {
        public int BookingID { get; set; }
        public int BookingCourtID { get; set; }
        public int HourID { get; set; }
        public int CourtID { get; set; }
        public int BookingNo { get; set; }
        public CourtBookingStatus Status { get; set; }
        public string? Name { get; set; }
        public string? Remarks { get; set; }
        public DateTime? BookedOn { get; set; }
        public bool IsOnlinePayment { get; set; }
        public bool IsPanelBooking { get; set; }
        public string? CounterCode { get; set; }
    }

    public class CourtBookingModelNew
    {
        public int BookingID { get; set; }
        public DateTime? AddedOn { get; set; }
        public List<CourtBookingViewModelNew> BookingItems { get; set; } = new();
    }
    public class CourtBookingViewModelNew
    {
        public int BookingCourtID { get; set; }
        public int BookingID { get; set; }
        public string? CourtName { get; set; }
        public DateTime Date { get; set; }
        public string? Time { get; set; }
        public decimal Rate { get; set; }
        public string? Currency { get; set; }
        public DateTime? AddedOn { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public decimal NetPrice { get; set; }
        public decimal Tax { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class CartItemModel
    {
        public int GameID { get; set; } = 2;
        public int HallID { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public int HourID { get; set; }
        public string? HourName { get; set; }
        public int? CourtID { get; set; }
        public string? CourtName { get; set; }
        public int BookingID { get; set; }
        public decimal Rate { get; set; }
        public string? CurrencyName { get; set; }
    }

    public class CourtPriceDetailsModel
    {
        public decimal Rate { get; set; }
        public string? CurrencyName { get; set; }
        public int? TaxCategoryID { get; set; }
        public bool IncTax { get; set; }
        public int? CurrencyID { get; set; }
        public int? PriceGroupID { get; set; }
        public int? PriceGroupItemID { get; set; }
    }

    public class ConfirmBookingModel
    {
        public int? CountryID { get; set; } = 231;
        [Required(ErrorMessage ="Enter Phone Number")]
        public string? Phone { get; set; }
        public int? EntityID { get; set; }
        public string? EmailAddress { get; set; }
        [Required(ErrorMessage = "Enter Name")]
        public string? Name { get; set; }
        public string? Remarks { get; set; }
        public decimal Cash { get; set; }
        public int? BookingID { get; set; }
        public int DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal Balance { get; set; }
    }

    public class ConfirmBookingResultModel
    {
        public int BookingID { get; set; }
        public int JournalMasterID { get; set; }
    }

    public class SearchCustomerWithPhoneModel
    {
        public int? CountryID { get; set; }
        public string? Phone { get; set; }
    }

    public class SearchCustomerWithPhoneResultModel
    {
        public int? EntityID { get; set; }
        public string? Name { get; set; }
        public string? EmailAddress { get; set; }
        public decimal Balance { get; set; }
    }

    public class CourtPaymentSummaryModel
    {
        public string? CurrencyName { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public decimal NetPrice { get; set; }
        public decimal Tax { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class TaxSummaryModel
    {
        public int? LedgerID { get; set; }
        public decimal TaxAmount { get; set; }
    }


    public class BookingReceiptModel
    {
        public int BookingNo { get; set; }
        public string? BookedBy { get; set; }
        public DateTime BookedOn { get; set; }
        public string? Name { get; set; }
        public string? EmailAddress { get; set; }
        public string? Phone { get; set; }
        public string? Remarks { get; set; }
        public int DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal CashCollected { get; set; }
        public decimal Credit { get; set; }
        public List<BookingReceiptCourtModel> Courts { get; set; } = new();
        public decimal Total { get; set; }
        public decimal Discount { get; set; }
        public CourtBookingStatus Status { get; set; }
        public string? CancelledBy { get; set; }
        public DateTime? CancelledOn { get; set; }
    }

    public class BookingReceiptCourtModel
    {
        public int BookingCourtID { get; set; }
        public string? CourtName { get; set; }
        public int Slots { get; set; }
        public string? Slot { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public decimal NetPrice { get; set; }
        public string? TaxCategoryName { get; set; }
        public decimal Tax { get; set; }
        public decimal TotalPrice { get; set; }
        public bool IsCancelled { get; set; }
        public string? CancelledBy { get; set; }
        public DateTime? CancelledOn { get; set; }

    }

    public class  CustomerLedgerBalanceModel
    {
        public int LedgerID { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? EmailAddress { get; set; }
        public decimal? Balance { get; set; }
    }

    
}
