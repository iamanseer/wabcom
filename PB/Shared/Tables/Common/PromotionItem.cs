using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Common
{
    public class PromotionItem:Table
    {
        [PrimaryKey]
        public int PromotionItemID { get; set; }
        public int? PromotionID { get; set; }
        public int DiscountTypeID { get; set; }
        public decimal Percentage { get; set; }
        public decimal Amount { get; set; }
        public int? ItemID { get; set; }
        public int? ItemVariantID { get; set; }
    }
}
