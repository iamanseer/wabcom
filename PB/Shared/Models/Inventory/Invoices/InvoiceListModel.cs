using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Inventory.Invoices
{
    public class InvoiceListModel
    {
        public int InvoiceID { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? AccountsDate { get; set; } 
        public int InvoiceNumber { get; set; }
        public string? Prefix { get; set; }
        public string? Name { get; set; }
        public string? InvoiceTypeName { get; set; }
        public string? Username { get; set; } 
    }
}
