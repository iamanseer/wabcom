using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables
{
    public class EmailRecipient : Table
    {

        [PrimaryKey]
        public int EmailReceipientID { get; set; }
        public int? BroadcastID { get; set; }
        public string? Name { get; set; }
        public string? EmailAddress { get; set; }
        public bool IsSend { get; set; }
        public DateTime? SendOn { get; set; }
        public string? Response { get; set; }
    }
}
