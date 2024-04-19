using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.eCommerce.Products
{
    public class EC_ItemCategory:Table
    {
        [PrimaryKey]
        public int CategoryID { get; set;}
        public string?CategoryName { get; set;}
        public int? BranchID { get; set; }
        public int? ParentID { get; set; }
        public int? MediaID { get; set; }
    }
}
