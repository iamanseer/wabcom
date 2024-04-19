using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Model.Models
{
    public class QuotationInvoiceDefaultSettingsModel
    {
        public string? Subject { get; set; }
        public string? CustomerNote { get; set; }
        public string? TermsAndConditions { get; set; }
        public bool NeedShippingAddress { get; set; }
        public int? CurrencyID { get; set; }
        public string? CurrencyName { get; set; }
        public int? PlaceOfSupplyID { get; set; }
        public string? PlaceOfSupplyName { get; set; }
        public int CountryID { get; set; }
        public string? CountryName { get; set; }
        public string? ISDCode { get; set; } 
    }
}
