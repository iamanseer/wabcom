using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.eCommerce.Customers
{
    public class EC_CustomerListModel
    {
        public int CustomerID { get; set; }
        public int EntityID { get; set; }
        public string? FirstName { get; set; }
        public string? Phone { get; set; }
        public string? EmailAddress { get; set; }
    }
}
