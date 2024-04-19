using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.CRM
{
    public class QuotationMailRecipient : Table
    {
        [PrimaryKey]
        public int MailRecipientID { get; set; }
        public int? EntityID { get; set; }
        public int? QuotationID { get; set; }
    }
}
