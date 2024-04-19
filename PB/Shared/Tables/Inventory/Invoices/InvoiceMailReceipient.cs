using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Inventory.Invoices
{
    public class InvoiceMailReceipient : Table
    {
        [PrimaryKey]
        public int MailRecipientID { get; set; }
        public int? InvoiceID { get; set; }
        public int? EntityID { get; set; } 
    }
}
