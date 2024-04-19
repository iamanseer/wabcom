using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.SuperAdmin.Client
{
    public class ClientRegistrationPackageDetailsModel
    {
        public int ClientID { get; set; }
        public int ClientEntityID { get; set; } 
        public int PackageID { get; set; }
        public int InvoiceID { get; set; } = 0;
        public int InvoiceJournalMasterID { get; set; } = 0;
        public int PaymentStatus { get; set; }
        public decimal Discount { get; set; } = 0;
        public bool IsPaid { get; set; } = false;


    }
}
