using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Inventory.Invoices
{
    public class InvoicePaymentTermSlab:Table
    {
        [PrimaryKey]
        public int ID { get; set; }
        public int? SlabID { get; set; }
        public int? InvoiceID { get; set; }
        public decimal Percentage { get; set; }
        public int Days { get; set; }
        public decimal Amount { get; set; }

    }
}
