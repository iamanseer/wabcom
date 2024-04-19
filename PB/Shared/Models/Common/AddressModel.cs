using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Model.Models
{
    public class AddressModel
    {
        public int AddressID { get; set; }
        public int? EntityID { get; set; }
        public int AddressType { get; set; }
        [Required(ErrorMessage = "Please provide Address Line 1")]
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? AddressLine3 { get; set; }
        public string? Pincode { get; set; }
        [Required(ErrorMessage = "Please choose country")]
        public int? CountryID { get; set; }
        public string? CountryName { get; set; }
        public int? StateID { get; set; }
        public string? State { get; set; }
        public int? CityID { get; set; }
        public string? City { get; set; }
        public bool IsDeleted { get; set; }
        public int ISDCode { get; set; }
    }
}
