using PB.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Inventory.Items
{
    public class ItemCategory : Table
    {
        [PrimaryKey]
        public int CategoryID { get; set; }
        [Required(ErrorMessage = "Please provide category name")]
        public string? CategoryName { get; set; }
        public int? ParentID { get; set; } 
        public int? ClientID { get; set; }
    }
}
