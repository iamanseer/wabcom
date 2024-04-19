using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.eCommerce.Products
{
    public class EC_ItemReview : Table
    {
        [PrimaryKey]
        public int ReviewID { get; set; }
        public string? Review { get; set; }
        public int? CustomerEntityID { get; set; }
        public int? ItemID { get; set; }

    }
}
