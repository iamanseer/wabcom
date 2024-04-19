using PB.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.CRM
{
    public class EnquiryItem : Table
    {
        [PrimaryKey]
        public int EnquiryItemID { get; set; }
        public int? EnquiryID { get; set; }
        [Required(ErrorMessage = "Please choose an item")]
        public int? ItemVariantID { get; set; }
        public string? Description { get; set; }
        [Required(ErrorMessage = "Please provide quantity for the item")]
        [Range(1,int.MaxValue,ErrorMessage = "Please add a minimum of 1 quantity")]
        public int Quantity { get; set; } = 1;
    }
}
 