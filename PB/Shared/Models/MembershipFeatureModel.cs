using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models
{
    public class MembershipFeatureModel
    {
        public int FeatureID { get; set; }
        [Required]
        public string? FeatureName { get; set; }
        [Required]
        public string? Description { get; set; }
        public int? MediaID { get; set; }
    }
}
