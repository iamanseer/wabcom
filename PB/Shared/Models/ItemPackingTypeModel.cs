using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace PB.Model.Models
{
    public class ItemPackingTypeModel
    {
        public int PackingTypeID { get; set; }
        [Required(ErrorMessage = "Please provide packing type name")]
        public string? PackingTypeName { get; set; }
        [Required(ErrorMessage = "Please provide packing type code")]
        public string? PackingTypeCode { get; set; }
        public int? ClientID { get; set; }
    }
}
