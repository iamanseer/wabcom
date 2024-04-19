using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.CourtClient
{
    public class CourtPackage:Table
    {
        [PrimaryKey]
        public int CourtPackageID { get; set; }
        public string? PackageName { get; set; }
        public int ValidityMonth { get; set; }
        public decimal TotalHours { get; set; }
        public decimal Fee { get; set; }
        public int? TaxCategoryID { get; set; }
        public bool IncTax { get; set; }
        public int? ClientID { get; set; }
    }
}
