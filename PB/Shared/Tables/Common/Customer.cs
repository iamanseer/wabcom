using PB.Model;
using PB.Shared.Enum;
using System.ComponentModel.DataAnnotations;

namespace PB.Shared.Tables.Common
{
    public class Customer : Table
    {
        [PrimaryKey]
        public int CustomerID { get; set; }
        public int? EntityID { get; set; }
        public int Status { get; set; } = 1;
        [Required(ErrorMessage = "Please choose the type of customer")]
        public int Type { get; set; } = (int)CustomerTypes.Individual;
        public string? Remarks { get; set; }
        public int? ClientID { get; set; }
        public string? TaxNumber { get; set; }



        public DateTime? JoinedOn { get; set; }
        public int? CategoryID { get; set; }
        public int? BusinessTypeID { get; set; }
        public string? TallyEmailID { get; set; }
        public int? CustomerPriority { get; set; }
        public int? OwnedBy { get; set; }
        public string? BusinessGivenInValue { get; set; }
        public int? BusinessGivenInNos { get; set; }
        public DateTime? LastBusinessDate { get; set; }
        public decimal? LastBusinessAmount { get; set; }
        public int? LastBusinessType { get; set; }
        public int? AMCStatus { get; set; }
        public DateTime? AMCExpiry { get; set; }
        public int? SubscriptionID { get; set; }
        public DateTime? SubscriptionExpiry { get; set; }
        public decimal? Lat { get; set; }
        public decimal? Long { get; set; }
        public string? AccountantContactNo { get; set; }
        public string? AccountantEmailID { get; set; }

    }
}
