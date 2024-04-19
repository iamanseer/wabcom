using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.eCommerce.Common
{
    public class EC_CustomerTockenDataModel
    {
        public int UserID { get; set; }
        public string? Name { get; set; }
        public int UserTypeID { get; set; }
        public int EntityID { get; set; }
        public string? UserTypeName { get; set; }
        public int ClientID { get; set; }
        public string? EmailAddress { get; set; }
        public string? UserName { get; set; }
        public string? MobileNo { get; set; }
    }
}
