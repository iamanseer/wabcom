using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.eCommerce.Cart
{
    public class EC_CartModel
    {
        public int CartID { get; set; }
        public int? CustomerEntityID { get; set; }
        public int? ItemVariantID { get; set; }
        public DateTime? AddedOn { get; set; }
        public string? ItemName { get; set; }
    }
}
