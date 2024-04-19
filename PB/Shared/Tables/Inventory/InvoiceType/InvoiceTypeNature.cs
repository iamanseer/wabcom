using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Inventory.InvoiceType
{
    public class InvoiceTypeNature : Table
    {
        [PrimaryKey]
        public int InvoiceTypeNatureID { get; set; }
        public string? InvoiceTypeNatureName { get; set; }
    }
}
