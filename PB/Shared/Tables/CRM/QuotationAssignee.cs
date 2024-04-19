using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.CRM
{
    public class QuotationAssignee : Table
    {
        [PrimaryKey]
        public int QuotationAssigneeID { get; set; }
        public int? QuotationID { get; set; }
        public int? EntityID { get; set; }
    }
}
