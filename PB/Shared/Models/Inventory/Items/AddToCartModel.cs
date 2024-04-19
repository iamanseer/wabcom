using PB.Model;
using PB.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Inventory.Items
{
    public class AddToCartModel
    {
        public int EnquiryID { get; set; }
        public int? CustomerEntityID { get; set; }
        public string? Description { get; set; }
        [RequiredIfNot("IsInCart", true, ErrorMessage = "Please provide customer name")]
        public string? CustomerName { get; set; }
        [RequiredIfNot("IsInCart", true, ErrorMessage = "Please provide customer phone")]
        [RegularExpression("^[0-9]+$", ErrorMessage = "Phone number contain only digits")]
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public bool IsInCart { get; set; } = true;
        public List<AddToCartItemModel> CartItems { get; set; } = new();
    }

    public class AddToCartItemModel
    {
        public int EnquiryItemID { get; set; }
        public int? EnquiryID { get; set; }
        public int? ItemVariantID { get; set; }
        public string? Description { get; set; }
        public int? Quantity { get; set; } = 1;
        public string? ItemName { get; set; }
    }


}
