using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.eCommerce.Product
{
    public class EC_ItemReviewImageModel
    {
        public int ID { get; set; }
        public int? ReviewID { get; set; }
        public int? MediaID { get; set; }
    }
}
