using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.eCommerce.Product
{
    public class EC_ItemCategoryViewModel
    {
        public int CategoryID { get; set; }
        public string? CategoryName { get; set; }
        public int? ParentID { get; set; }
        public string? ParentCategoryName { get; set; }
        public int? MediaID { get; set; }
        public string? FileName { get; set; }
    }
}
