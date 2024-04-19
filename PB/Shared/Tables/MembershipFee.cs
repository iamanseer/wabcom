using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables
{
    public class MembershipFee:Table
    {
        [PrimaryKey]
        public int FeeID { get; set; }
        public int FeatureID { get; set; }
        public int CapacityID { get; set; }
        public int PlanID { get; set; }
        public decimal Fee { get; set; }
        public int ComboFee { get; set; }
    }
}
