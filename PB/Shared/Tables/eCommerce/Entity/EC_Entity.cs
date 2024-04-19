using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.eCommerce.Entity
{
    public class EC_Entity : Table
    {
        [PrimaryKey]
        public int EntityID { get; set; }
        public int? EntityTypeID { get; set; }
        public string? EmailAddress { get; set; }
        public string? Phone { get; set; }
        public string? Phone2 { get; set; }
    }
}
