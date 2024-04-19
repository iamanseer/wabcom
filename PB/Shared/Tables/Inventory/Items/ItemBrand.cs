using PB.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Inventory.Items
{
    public class ItemBrand : Table
    {
        [PrimaryKey]
        public int BrandID { get; set; }
        public string? BrandName { get; set; }
        public int? MediaID { get; set; }
        public int? ClientID { get; set; } 
    }
}
