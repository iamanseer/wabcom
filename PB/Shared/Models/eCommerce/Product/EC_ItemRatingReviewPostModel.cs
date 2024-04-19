using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.eCommerce.Product
{
    public class EC_ItemRatingReviewPostModel
    {
        public int RatingID { get; set; }
        public decimal? Rating { get; set; }
        public int? ItemID { get; set; }
        public string? ItemName { get; set; }
        public int ReviewID { get; set; }
        public string? Review { get; set; }
        public List<EC_ItemReviewImageModel> ReviewImages { get; set; } = new();
    }
}
