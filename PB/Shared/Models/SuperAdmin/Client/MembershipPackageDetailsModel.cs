using PB.Shared.Tables.Tax;

namespace PB.Shared.Models.SuperAdmin.Client
{
    public class MembershipPackageDetailsModel
    {
        public PB.Shared.Tables.MembershipPackage Package { get; set; } = new();
        public List<TaxCategoryItem> TaxCategoryItems { get; set; } = new();
        public decimal Discount { get; set; }
        public decimal NetFee { get; set; }
        public decimal TaxPercentage { get; set; }
        public decimal Tax { get; set; }
        public decimal GrossFee { get; set; }
    }
}
