using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Inventory.Suppliers
{
    public class Supplier : Table
    {
        [PrimaryKey]
        public int SupplierID { get; set; }
        public int? EntityID { get; set; }
        public int? Status { get; set; }
        public string? Remarks { get; set; }
        public string? TaxNumber { get; set; }
        public int? ClientID { get; set; } 
    }
}
