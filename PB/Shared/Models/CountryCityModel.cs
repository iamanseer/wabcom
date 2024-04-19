using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models
{
    public class CountryCityModel
    {
        public int CityID { get; set; }
        [Required]
        public string CityName { get; set; }
        [Required]
        public int? StateID { get; set; }
        
        public string StateName { get; set; }
       
        public decimal? Lat { get; set; }
        public decimal? Long { get; set; }
        public int ClientID { get; set; }
    }
}
