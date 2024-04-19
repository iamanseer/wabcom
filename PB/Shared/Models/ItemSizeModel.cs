using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models
{
    public class ItemSizeModel
    {
        public int SizeID { get; set; }
        [Required (ErrorMessage ="Please provide item size name")]
        public string? Size { get; set; }
        public int? ClientID { get; set; }
    }
}
