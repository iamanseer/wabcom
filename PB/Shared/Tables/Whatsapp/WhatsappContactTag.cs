using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Whatsapp
{
    public class WhatsappContactTag : Table
    {
        [PrimaryKey]
        public int ContactTagID { get; set; }
        public int? ContactID { get; set; }
        public int? TagID { get; set; }
    }
}
