using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.eCommerce.Customer
{
    public class EC_Customer:Table
    {
        [PrimaryKey]
        public int CustomerID { get; set; }
        public int? EntityID { get; set; }
        public bool HasLogin { get; set; }
        public string? OTP { get; set; }
        public DateTime? AddedOn { get; set; }
    }
}
