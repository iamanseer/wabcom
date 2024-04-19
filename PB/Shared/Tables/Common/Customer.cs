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
    }
}
