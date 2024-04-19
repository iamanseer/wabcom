using PB.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Court
{
    public class CourtPriceGroupItem:Table
    {
        [PrimaryKey]
        public int? PriceGroupItemID { get; set; }
        public int? PriceGroupID { get; set; }
        [Required(ErrorMessage ="Please choose day")]
        [Range(0,int.MaxValue,ErrorMessage ="Please choose valid day")]
        public int? DayID { get; set; }
        [Required(ErrorMessage = "Please choose starting hour")]
        [Range(1, int.MaxValue, ErrorMessage = "Please choose valid starting hour")]
        public int? StartHourID { get; set; }
        [Required(ErrorMessage = "Please choose ending hour")]
        [Range(1, int.MaxValue, ErrorMessage = "Please choose valid ending hour")]
        public int? EndHourID { get; set; }
        public decimal Rate { get; set; }
    }
}
