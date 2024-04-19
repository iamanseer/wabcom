using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Inventory.Items
{
    public class ItemGroupModel
    {
        public int GroupID { get; set; }
        [Required(ErrorMessage = "Please provide item group name")]
        public string? GroupName { get; set; } 
    }
}
