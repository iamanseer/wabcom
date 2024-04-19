using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Inventory.Items
{
    public class ItemGroupData : Table
    {
        [PrimaryKey]
        public int ID { get; set; }
        public string? ItemCode { get; set; }
        public string? GroupName { get; set; } 
    }
}
