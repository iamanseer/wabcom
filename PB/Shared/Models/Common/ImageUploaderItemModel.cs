using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Common
{
    public class ImageUploaderItemModel
    {
        public int ImageID { get; set; } 
        public int? MediaID { get; set; } 
        public string? AltText { get; set; }
        public string? FileName { get; set; }
    }
}
