using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.eCommerce.WishList
{
    public class EC_WishListViewModel
    {
        public int ID { get; set; }
        public int? CustomerEntityID { get; set; }
        public int? ItemVariantID { get; set; }
        public DateTime? AddedOn { get; set; }
        public int? MediaID { get; set; }
        public string? ItemName { get; set; }
        public string? FileName { get; set; }
        public string? FirstName { get; set; }
    }

}
