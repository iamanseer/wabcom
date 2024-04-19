using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.eCommerce.Products
{
    public class EC_ItemRating : Table
    {
        [PrimaryKey]
        public int RatingID { get; set; }
        public int Rating { get; set; }
        public int? ItemID { get; set; }
        public int? CustomerEntityID { get; set; }
    }
}
