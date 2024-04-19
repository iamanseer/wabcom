using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Whatsapp
{
    public class WhatsappBroadcastReceipient : Table
    {
        [PrimaryKey]
        public int ReceipientID { get; set; }
        public string? Phone { get; set; }
        public int? BroadcastID { get; set; }
    }
}
