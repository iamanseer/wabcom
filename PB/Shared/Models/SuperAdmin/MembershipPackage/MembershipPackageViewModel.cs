using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.SuperAdmin.MembershipPackage
{
    public class MembershipPackageViewModel
    {
        public int PackageID { get; set; }
        public string? PackageName { get; set; }
        public string? PackageDescription { get; set; }
        public string? PackageFeatures { get; set; }
        public decimal Fee { get; set; }
        public int? PlanID { get; set; }
        public int MonthCount { get; set; }

    }
}
