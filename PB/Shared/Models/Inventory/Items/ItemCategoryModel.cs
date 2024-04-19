using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Inventory.Items
{
    public class ItemCategoryModel
    {
        public int CategoryID { get; set; }
        [Required(ErrorMessage = "Please provide category name")]
        public string? CategoryName { get; set; }
        public int? ParentID { get; set; }
        public string? ParentCategoryName { get; set; } 
    }
}
