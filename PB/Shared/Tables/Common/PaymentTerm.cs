using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Common
{
    public class PaymentTerm:Table
    {
        [PrimaryKey]
        public int PaymentTermID { get; set; }
        public int? ClientID { get; set; }
        public string? PaymentTermName { get; set; }
        public string? Description { get; set; }
        
    }
}
