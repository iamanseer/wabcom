using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.eCommerce.Products
{
    public class EC_ItemReviewImages : Table
    {
        [PrimaryKey]
        public int ID { get; set; }
        public int? ReviewID { get; set; }
        public int? MediaID { get; set; }
    }
}
