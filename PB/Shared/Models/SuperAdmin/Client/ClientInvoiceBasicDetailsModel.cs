using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.SuperAdmin.Client
{
    public class ClientInvoiceBasicDetailsModel 
    {
        public  int InvoiceID { get; set; } 
        public int InvoiceJournalMasterID { get; set; }
        public int ClientID { get; set; }
        public int ClientEntityID { get; set; }
        public int PackageID { get; set; }
        public string? PackageName { get; set; }
        public string? Email { get; set; }
        public decimal Fee { get; set; }
        public decimal Discount { get; set; }
        public decimal NetFee { get; set; }
        public decimal GrossFee { get; set; } 
        public int MonthCount { get; set; } 
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; } 
        public DateTime? DisconnectionDate { get; set; }
    }
}
