using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models
{
    public class ClientPackageUpdateModel
    {
        public int? PackageID { get; set; }
        public string? PackageName { get; set; }
        public int? ClientID { get; set; }
        public string? Name { get; set; }
        public decimal Discount { get; set; }
    }

    public class ClientExistingPackageModel
    {
        public int? PackageID { get; set; }
        public string? PackageName { get; set; }
        public decimal? Fee { get; set; }
        public decimal? BalanceAmount { get; set; }
        public int? TotalDays { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? StartDate { get; set; }
    }
}
