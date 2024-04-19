using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.SuperAdmin.ClientInvoice
{
    public class ClientInvoiceMailDetailsModel
    {
        public string? MailRecipients { get; set; }
        public string? Subject { get; set; }
        public string? Message { get; set; } 
        public string? FileName { get; set; }
        public string? PackageName { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? DisconnectionDate { get; set; }
        public int InvoiceNo { get; set; }
        public string? Name { get; set; }
    }
}
