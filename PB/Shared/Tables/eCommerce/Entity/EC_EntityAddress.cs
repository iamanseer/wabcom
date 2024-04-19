using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.eCommerce.Entity
{
    public class EC_EntityAddress : Table
    {
        [PrimaryKey]
        public int AddressID { get; set; }
        public int? EntityID { get; set; }
        public int? AddressTypeID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? PinCode { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
    }
}
