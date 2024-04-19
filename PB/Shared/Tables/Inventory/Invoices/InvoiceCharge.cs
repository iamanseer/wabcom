using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Inventory.Invoices
{

    public class InvoiceCharge : Table
    {
        [PrimaryKey]
        public int InvoiceChargeID { get; set; }
        public int? InvoiceID { get; set; }
        public int? InvoiceTypeChargeID { get; set; } 
        public decimal? Percentage { get; set; }
        public decimal Amount { get; set; } = 0;

    }
}
