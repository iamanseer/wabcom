using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Inventory.Suppliers
{
    public class SupplierContactPerson : Table
    {
        [PrimaryKey]
        public int ContactPersonID { get; set; }
        public int? EntityID { get; set; }
        public string? Department { get; set; }
        public string? Designation { get; set; }
        public int? SupplierEntityID { get; set; } 
    }
}
