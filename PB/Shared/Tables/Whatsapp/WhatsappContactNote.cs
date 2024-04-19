using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Whatsapp
{
    public class WhatsappContactNote : Table
    {
        [PrimaryKey]
        public int NoteID { get; set; }
        public string? Note { get; set; }
        public int? ContactID { get; set; }
        public int? AddedBy { get; set; }
        public DateTime? AddedOn { get; set; }
    }
}
