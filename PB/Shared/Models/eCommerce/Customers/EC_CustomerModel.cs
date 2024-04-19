using PB.Model;
using PB.Shared.Models.eCommerce.Product;
using PB.Shared.Models.eCommerce.WishList;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.eCommerce.Customers
{
    public class EC_CustomerModel
    {
        public int EntityID { get; set; }
        public int EntityPersonalInfoID { get; set; }
        [Required (ErrorMessage ="Please enter first name")]
        public string? FirstName { get; set; }
        [Required(ErrorMessage = "Please enter first name")]
        public string? LastName { get; set; }
        [Required(ErrorMessage = "Please enter first name")]
        public string? Phone { get; set; }
        [Required(ErrorMessage = "Please enter first name")]
        public string? EmailAddress { get; set; }

    }

    public class EC_CustomerDetailsModel
    {
        public int? EntityID { get; set; }
        public int? EntityPersonalInfoID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public string? EmailAddress { get; set; }
        public int? CustomerID { get; set; }
        public int? EntityTypeID { get; set; }
        public string? Phone2 { get; set; }
        public List<EC_CustomerAddressViewModel> customerAddress { get; set; } = new();
        public List<EC_ItemRatingAndReviewModel> ratingAndReview { get; set; } = new();
        public List<EC_WishListViewModel> wishLists { get; set; } = new();

    }
}
