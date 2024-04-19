using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables
{
	public class MembershipPackageFeature:Table
	{
		[PrimaryKey]
		public int ID { get; set; }
		public int? PackageID { get; set; }
		public int? FeatureID { get; set; }
	}
}
