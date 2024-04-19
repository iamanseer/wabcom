using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Inventory.Invoices
{
    public class InvoiceMenuModel
    {
        public int InvoiceID { get; set; }
        public string? Name { get; set; }
        public string? Prefix { get; set; }
        public int InvoiceNumber { get; set; }  
    }
}
