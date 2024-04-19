using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Common
{
    public class PromotionModel
    {
        public int PromotionID { get; set; }
        [Required (ErrorMessage ="Please add promotion name")]
        public string? PromotionName { get; set; }
        [Required(ErrorMessage = "Please add start date")]
        public DateTime? StartDate { get; set; }
        [Required(ErrorMessage = "Please add end date")]
        public DateTime? EndDate { get; set; }
        public int? ClientID { get; set; }
        public List<PromotionItemListViewModel> PromotionItems { get; set; } = new();

    }
    public class PromotionItemListViewModel
    {
        public int? PromotionID { get; set; }
        public int DiscountTypeID { get; set; }
        public int PromotionItemID { get; set; }
        public decimal Percentage { get; set; }
        public decimal Amount { get; set; }
        public int? ItemID { get; set; }
        public int? ItemVariantID { get; set; }
        public string? ItemName { get; set; }
        public bool IsRowInEditMode { get; set; } = false;
        public bool ByItem { get; set; }
    }

    public class PromotionItemModel
    {
        public int? PromotionID { get; set; }
        public int DiscountTypeID { get; set; }
        public bool ByItem { get; set; } = true;
        public bool ByItemModel { get; set; }
        public decimal Percentage { get; set; }
        public decimal Amount { get; set; }
        public List<PromotionItemListViewModel> ItemsList { get; set; } = new();
    }

    public class PromotionItemListModel
    {
        public int PromotionItemID { get; set; }
        public decimal Percentage { get; set; }
        public decimal Amount { get; set; }
        public int? ItemID { get; set; }
        public int? ItemVariantID { get; set; }
    }

   
    public class PromotionListModel
    {
        public int PromotionID { get; set; }
        public string? PromotionName { get; set; }
    }
}
