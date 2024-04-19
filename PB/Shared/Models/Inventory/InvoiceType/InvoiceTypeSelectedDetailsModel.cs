using PB.Shared.Models.Inventory.Invoice;
using PB.Shared.Models.Inventory.Invoices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Inventory.InvoiceType
{
    public class InvoiceTypeSelectedDetailsModel
    {
        public int InvoiceTypeID { get; set; }
        public string? InvoiceTypeName { get; set; }
        public string? Prefix  { get; set; }
        public int InvoiceTypeNatureID { get; set; }  
        public List<InvoiceChargeModel> ExtraChargeItems { get; set; } = new(); 
    } 
}
