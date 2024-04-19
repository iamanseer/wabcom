using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Inventory.Items
{
    public class ItemQrCodeGenerationPostModel
    {
        public string? DeviceID { get; set; }
        public bool CreateNotGeneratedQrCodes { get; set; } = true;
    }
    public class ItemQrCodeGeneratedPushModel
    {
        public string? FileName { get; set; }
        public string? DeviceID { get; set; }
        public bool IsNewQrCodeAdded { get; set; } = false;
    }
    public class ItemQrCodePdfModel
    {
        public string? ItemName { get; set; }
        public string? ItemCode { get; set; }
        public string? QrCodeImageFile { get; set; }
    }
    public class ItemGroupQrCodePdfModel 
    {
        public string? GroupName { get; set; }
        public string? QrCodeImageFile { get; set; }
    }

    public class ItemQrCodeResponseMode
    {
        public PagedList<ItemQrCodePdfModel> Result { get; set; } = new();
        public string? HtmlContent { get; set; }
    }

    public class ItemGroupQrCodeResponseMode
    {
        public PagedList<ItemGroupQrCodePdfModel> Result { get; set; } = new();
        public string? HtmlContent { get; set; }
    } 
}
