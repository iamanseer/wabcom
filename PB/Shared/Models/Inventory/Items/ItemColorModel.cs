using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Inventory.Items
{
    public class ItemColorModel
    {
        public int ColorID { get; set; }
        [Required(ErrorMessage = "Please provide item color name")]
        public string? ColorName { get; set; }
        [Required(ErrorMessage = "Please provide item color code")]
        public string? ColorCode { get; set; }
        public int? ClientID { get; set; } 
    }
}
