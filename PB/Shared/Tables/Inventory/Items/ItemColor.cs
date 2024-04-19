using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Inventory.Items
{
    public class ItemColor : Table
    {
        [PrimaryKey]
        public int ColorID { get; set; }
        public string? ColorName { get; set; }
        public string? ColorCode { get; set;}
        public int? ClientID { get; set; }
    }
}
