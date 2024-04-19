using PB.Shared.Models.CRM.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Inventory.Supplier
{
    public class SupplierDataModel 
    {
        public List<AddressView> SupplierAddresses { get; set; } = new();
        public List<SupplierMailRecipientsModel> MailReceipients { get; set; } = new(); 
        public string? TaxNumber { get; set; }
        public string? ISDCode { get; set; }
        public int? CountryID { get; set; }
        public int? CurrencyID { get; set; }

    }
    public class SupplierMailRecipientsModel 
    {
        public int ContactPersonEntityID { get; set; }

        private string? _EmailAddress;
        public string? EmailAddress
        {
            get { return EmailAddress is not null ? "< " + EmailAddress + " >" : null; }
            set { _EmailAddress = value; }
        }
    }
}
