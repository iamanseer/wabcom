using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables
{
    public class MembershipFeature:Table
    {
        [PrimaryKey]
        public int  FeatureID { get; set; }
        public string? FeatureName { get; set; }
        public string? Description { get; set; }
        public  int? MediaID { get; set; }
    }
}
