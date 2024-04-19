using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.eCommerce.Customer
{
    public class EC_WishList:Table
    {
        [PrimaryKey]
        public int ID { get; set; }
        public int? CustomerEntityID { get; set; }
        public int? ItemVariantID { get; set; }
        public DateTime? AddedOn { get; set; }
    }
}
