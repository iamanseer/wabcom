using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Inventory.Invoices
{
    public class InvoiceTaxItem : Table
    {
        [PrimaryKey] public int InvoiceTaxItemID { get; set; }
        public int InvoiceID { get; set; }
        public int TaxCategoryItemID { get; set; }
        public decimal TaxAmount { get; set; } 
    }
}
