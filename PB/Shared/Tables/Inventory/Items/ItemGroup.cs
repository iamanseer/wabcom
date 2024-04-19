using PB.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Inventory.Items
{
    public class ItemGroup : Table
    {
        [PrimaryKey]
        public int GroupID { get; set; }
        [Required(ErrorMessage = "Please provide group name")]
        public string? GroupName { get; set; }
        public int? ClientID { get; set; } 
    } 
}
