using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.eCommerce.Customers
{
    public class EC_CustomerAddressModel
    {
        public int AddressID { get; set; }
        public int? EntityID { get; set; }
        public int AddressTypeID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? PinCode { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
    }

    public class EC_CustomerAddressViewModel
    {
        public int? AddressID { get; set; }
        public string? CompleteAddress { get; set; }
    }
}
