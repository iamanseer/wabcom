using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables
{
    public class MembershipPlan:Table
    {
        [PrimaryKey]
        public int PlanID { get; set; }
        public string?   PlanName { get; set; }
        public  int MonthCount { get; set; }
    }
}
