using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.eCommerce.Entity
{
    public class EC_EntityPersonalInfo:Table
    {
        [PrimaryKey]
        public int EntityPersonalInfoID { get; set; }

        public int? EntityID { get; set; }

        public string? Honorific { get; set; }

        public string? FirstName { get; set; }

        public string? MiddleName { get; set; }

        public string? LastName { get; set; }

        public string? NickName { get; set; }

        public int? GenderID { get; set; }

        public DateTime? DOB { get; set; }
    }
}
