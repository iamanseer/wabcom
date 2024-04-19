using PB.Shared.Tables.CourtClient;
using PB.Shared.Tables.Tax;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Court
{
    public class CourtPackageModel
    {
        public int CourtPackageID { get; set; }
        [Required]
        public string? PackageName { get; set; }
        [Required]
        public int ValidityMonth { get; set; }
        [Required]
        public decimal TotalHours { get; set; }
        [Required]
        public decimal Fee { get; set; }
        public int? TaxCategoryID { get; set; }
        public bool IncTax { get; set; }
        public int? ClientID { get; set; }
        public string? TaxCategoryName { get; set; }
    }


    public class CourtPackageViewModel
    {
        public decimal Fee { get; set; }
        public int CourtPackageID { set; get; }
        public string? PackageName { set; get; }
        public int ValidityMonth { get; set;}
        public decimal TotalHours { get; set; }
        public string? TaxCategoryName { set; get; }
        public bool IncTax { get; set; }
    }

    public class CourtPackagePurchaseModel
    {
        [Required (ErrorMessage ="Please choose one package")]
        public int? CourtPackageID { get; set; }
        [Required(ErrorMessage = "Please choose one customer")]
        public int? EntityID { get; set; }
        public string? CustomerName { get; set; }
        public string? PackageName { get; set; }
        public string? ExistingPackageName { get; set; }
        public DateTime? ExistingEndDate { get; set; }
        public decimal ExistingFee { get; set; }
        public DateTime? StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }
        [Required(ErrorMessage = "Please choose one payment type")]
        public int? PaymentTypeID { get; set; }
        public string? PurchaseCode { get; set; }

    }

    public class CustomerCourtPackageDetailsModel
    {
        public int ClientID { get; set; }
        public int CustomerEntityID { get; set; }
        public int PackageID { get; set; }
        public decimal Discount { get; set; } = 0;
        public DateTime? StartDate { get; set; } 
        public DateTime? EndDate { get; set; }
        public int PaymentTypeID { get; set; }
        public string? PurchaseCode { get; set; }
    }

    public class CourtPackageDetailsModel
    {
        public CourtPackage Package { get; set; } = new();
        public List<TaxCategoryItem> TaxCategoryItems { get; set; } = new();
        public decimal Discount { get; set; }
        public decimal NetFee { get; set; }
        public decimal TaxPercentage { get; set; }
        public decimal Tax { get; set; }
        public decimal GrossFee { get; set; }
    }

    public class ExistingCourtPackagePurchaseModel
    {
       public string? ExistingPackageName { get; set; }
        public DateTime? ExistingEndDate { get; set; }
        public decimal ExistingFee { get; set; }
    }
}
