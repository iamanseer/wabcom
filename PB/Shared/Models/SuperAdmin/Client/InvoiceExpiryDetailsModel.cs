using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.SuperAdmin.Client
{
    public class InvoiceExpiryDetailsModel 
    {
        public int ClientEntityID { get; set; }
        public int ClientID { get; set; }
        public int PackageID { get; set; }
        public int InvoiceID { get; set; }
    }
}
