using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables
{
	public class MembershipPackage : Table
	{
		[PrimaryKey]
		public int PackageID { get; set; }
		public string? PackageName { get; set; }
		public bool IsCustom { get; set; }
		public int? CapacityID { get; set; }
		public decimal Fee { get; set; }
		public int? PlanID { get; set; }
		public int? TaxCategoryID { get; set; }
		public string? PackageDescription { get; set; }
        public string? PackageFeatures { get; set; }
		public int? MediaID { get; set; }


    }
}
