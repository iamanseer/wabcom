using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.CRM
{
    public class BusinessType:Table
    {
        [PrimaryKey]
        public int BusinessTypeID { get; set; }
        public string? BusinessTypeName { get; set; }
        public int? ClientID { get; set; }
    }
}
