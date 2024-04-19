using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.CRM
{
    public class EnquiryAssignee : Table
    {
        [PrimaryKey]
        public int EnquiryAssigneeID { get; set; }
        public int EnquiryID { get; set; }
        public int EntityID { get; set; }
    }
}
