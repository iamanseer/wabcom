using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models
{
    public class ClientInvoiceModel
    {
        public int InvoiceID { get; set; }
        public DateTime? Date { get; set; }
        public int ClientID { get; set; }
        public DateTime? DueDate { get; set; }
        public int InvoiceJournalMasterID { get; set; }
        public int ReceiptJournalMasterID { get; set; }
    }
}
