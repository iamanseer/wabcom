using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.CRM.Customer
{
    public class CustomerListModel
    {
        public int EntityID { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? EmailAddress { get; set; }
        public int Status { get; set; }
        public int Type { get; set; }
    }
}
