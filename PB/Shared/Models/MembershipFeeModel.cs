using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models
{
    public class MembershipFeeModel
    {
        public int FeeID { get; set; }
        [Required]
        public int? FeatureID { get; set; }
        [Required]
        public int? CapacityID { get; set; }
        [Required]
        public int? PlanID { get; set; }
        [Required]
        public decimal Fee { get; set; }
        [Required]
        public int ComboFee { get; set; }
        [Required]
        public string FeatureName { get; set; }
        
        public string Capacity { get; set; }    
        public string PlanName { get; set; }
    }
}
