using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Inventory.Items
{
    public class ItemImportPostModel : FileUploadModel
    {
        public string? DevideID { get; set; } 
    }
}
