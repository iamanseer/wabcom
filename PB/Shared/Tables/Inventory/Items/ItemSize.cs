using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Inventory.Items
{
    public class ItemSize : Table
    {
        [PrimaryKey]
        public int SizeID { get; set; }
        public string? Size { get; set; }
        public int? ClientID { get; set; }

    }
}
