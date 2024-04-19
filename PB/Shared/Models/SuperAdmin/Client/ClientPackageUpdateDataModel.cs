using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.SuperAdmin.Client
{
    public class ClientPackageUpdateDataModel
    {
        public int ClientID { get; set; }
        public int ClientEntityID { get; set; }
        public int InvoiceID { get; set; }
        public int OldPackageID { get; set; } 
    }
}
