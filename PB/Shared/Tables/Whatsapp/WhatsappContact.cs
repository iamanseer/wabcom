using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Whatsapp
{
    public class WhatsappContact : Table
    {
        [PrimaryKey]
        public int ContactID { get; set; }
        public string? Phone { get; set; }
        public string? Name { get; set; }
        public int? ClientID { get; set; }
        public int? EntityID { get; set; }
        public string? OTP { get; set; }
    }
}
