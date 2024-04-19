using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Inventory.Items
{
    public class ItemImage:Table
    {
        [PrimaryKey]
        public int ID { get; set; }
        public int? ItemID { get; set; }
        public int? MediaID { get; set; }
    }
}
