using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Inventory.Items
{
    public class ItemImportResponsePushModel
    {
        public string? DeviceID { get; set; }
        public List<ItemImportSkippedModel> SkippedItems { get; set; } = new();
    }
}
