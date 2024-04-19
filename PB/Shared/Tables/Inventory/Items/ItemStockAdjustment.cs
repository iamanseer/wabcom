using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Inventory.Items
{
    public class ItemStockAdjustment : Table
    {
        [PrimaryKey]
        public int StockAdjustmentID { get; set; }
        public int? InvoiceItemID { get; set; }
        public decimal? SystemStock { get; set; }
        public decimal? PhysicalStock { get; set; }
    }
}
