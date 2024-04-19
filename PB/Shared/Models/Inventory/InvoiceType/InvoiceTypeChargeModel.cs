using PB.Shared.Tables.Inventory.InvoiceType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Inventory.Invoice
{
    public class InvoiceTypeChargeModel:InvoiceTypeCharge
    { 
        public string? LedgerName { get; set; }
    }
}
