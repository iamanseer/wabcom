using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Inventory.InvoiceType
{
    public class InvoiceType : Table
    {
        [PrimaryKey] public int InvoiceTypeID { get; set; }
        public string? InvoiceTypeName { get; set; }
        public int? InvoiceTypeNatureID { get; set; }
        public string? Prefix { get; set; }
        public int? ClientID { get; set; }
        public int? LedgerID { get; set; }
        public int? VoucherTypeID { get; set; }
    }
}
