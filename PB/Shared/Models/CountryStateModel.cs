using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models
{
    public class CountryStateModel
    {
        public int StateID { get; set; }
        [Required(ErrorMessage = "Please Enter State Name")]
        public string? StateName { get; set; }
        [Required(ErrorMessage = "Please choose the country")]
        public int? CountryID { get; set; }
        public string? CountryName { get; set; }
        public string? StateCode { get; set; }
        public decimal? Lat { get; set; }
        public decimal? Long { get; set; }
        public int? ClientID { get; set; }
    }
}
