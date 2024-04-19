using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Whatsapp
{
    public class WhatsappBroadcastParameter : Table
    {
        [PrimaryKey]
        public int ParameterID { get; set; }
        public int? ReceipientID { get; set; }
        public int? VariableID { get; set; }
        public string? Value { get; set; }
    }
}
