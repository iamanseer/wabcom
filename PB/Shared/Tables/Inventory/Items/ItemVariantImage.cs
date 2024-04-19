using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Inventory.Items
{
    public class ItemVariantImage : Table
    {
        [PrimaryKey]
        public int ImageID { get; set; }  
        public int? ItemVariantID { get; set; }
        public int? MediaID { get; set; } 
    }
}
