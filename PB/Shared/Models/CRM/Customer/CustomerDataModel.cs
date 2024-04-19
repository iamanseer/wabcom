using PB.Model.Models;
using PB.Shared.Models.Inventory.Invoices;
using PB.Shared.Models.WhatsaApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.CRM.Customer
{
    public class CustomerOrSupplierDataModel
    {
        public List<AddressView> CustomerAddresses { get; set; } = new();
        public List<MailRecipentsModel> MailReceipients { get; set; } = new();
        public List<BillToBillAgainstReferenceModel> Advances { get; set; } = new();
        public string? TaxNumber { get; set; }
        public string? ISDCode { get; set; }
        public int? CountryID { get; set; }
        public int? CurrencyID { get; set; }
    }
    public class AddressView
    {
        public int AddressID { get; set; }
        public string? CompleteAddress { get; set; }
    }
    public class MailRecipentsModel
    {
        public int EntityID { get; set; }
        public string? EmailAddress { get; set; }
        //private string? _EmailAddress;
        //public string? EmailAddress
        //{
        //    get { return EmailAddress is not null ? "< " + Email + " >" : null; }
        //    set { _EmailAddress = value; }
        //}
    }
}
