using PB.Model;
using PB.Shared.Enum;
using PB.Shared.Tables;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models
{
	public class MembershipPackageModel
	{
		public int PackageID { get; set; }
		[Required]
		public string? PackageName { get; set; }
		public bool IsCustom { get; set; }
		[Required(ErrorMessage = "Please choose Capacity")]
		public int? CapacityID { get; set; }
		[Required]
		public decimal Fee { get; set; }
		[Required(ErrorMessage = "Please choose plan")]
		public int? PlanID { get; set; }
		public int? TaxCategoryID { get; set; }
		public int Capacity { get; set; }
		public string? PlanName { get; set; }
        public string? TaxCategoryName { get; set; }
		public List<IdnValuePair> feature { get; set; } = new();
		public int ID { get; set; }
        [RequiredIf("IsCustom", false, ErrorMessage = "Please provide Package Description")]
        public string? PackageDescription { get; set; }
        [RequiredIf("IsCustom", false, ErrorMessage = "Please provide Package Features")]
        public string? PackageFeatures { get; set; }
		public int? MediaID { get; set; }
		public string? FileName { get; set; }
		//public List<RoleGroupModel> RoleGroups { get; set; } = new();
		//public List<RoleModel> Roles { get; set; } = new();

		public List<PackageRoleModel> PackageRoleList { get; set; } = new();


	}



	public class MembershipPackageViewModel
	{
        public int PackageID { get; set; }
        public string? PackageName { get; set; }
        public bool IsCustom { get; set; }
        public int? CapacityID { get; set; }
        public decimal Fee { get; set; }
        public int? PlanID { get; set; }
        public int? TaxCategoryID { get; set; }
        public string? Capacity { get; set; }
        public string? PlanName { get; set; }
        public string? TaxCategoryName { get; set; }
        public List<MembershipFeature> Features { get; set; } = new();
		public string? AvailableFeatures { get; set; }
    }

	public class FeatureIDModel
	{
		public int? ID { get; set; }
	}
}
