using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Inventory.Items
{
    public class ItemBrandModel
    {
        public int BrandID { get; set; }
        [Required(ErrorMessage = "Please provide brand name")]
        public string? BrandName { get; set; }
        [Required(ErrorMessage = "Please provide brand logo")]
        public int? MediaID { get; set; }
        public string? FileName { get; set; }
        public int? ClientID { get; set; } 
    }
}
