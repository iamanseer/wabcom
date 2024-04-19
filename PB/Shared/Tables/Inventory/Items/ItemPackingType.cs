using PB.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Inventory.Items
{
    public class ItemPackingType : Table
    {
        [PrimaryKey]
        public int PackingTypeID { get; set; }
        [Required]
        public string? PackingTypeCode { get; set; }
        [Required]
        public string? PackingTypeName { get; set; }
        public int? ClientID { get; set; }
    }
}
