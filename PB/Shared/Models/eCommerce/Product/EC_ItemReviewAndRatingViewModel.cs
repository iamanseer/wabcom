using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.eCommerce.Product
{
    public class EC_ItemReviewAndRatingViewModel
    {
        public int RatingID { get; set; }
        public decimal? Rating { get; set; }
        public string? Name { get; set; }
        public int? ItemID { get; set; }
        public int? EntityID { get; set; }
        public string? ItemName { get; set; }
        public string? Review { get; set; }
        public string? ReviewID { get; set; }
    }
}
