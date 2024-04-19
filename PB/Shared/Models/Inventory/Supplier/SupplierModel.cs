using PB.Model;
using PB.Model.Models;
using PB.Shared.Models.CRM.Customer;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Inventory.Supplier
{
    public class SupplierModel
    {
        //Entity
        public int EntityID { get; set; }
        public int EntityTypeID { get; set; }
        [Required(ErrorMessage = "Please provide supplier phone")]
        [RegularExpression("^[0-9]+$", ErrorMessage = "Only numbers are allowed")]
        public string? Phone { get; set; }
        public string? Phone2 { get; set; }
        [RegularExpression("^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$", ErrorMessage = "Please provide a valid Email like example@test.com")]
        public string? EmailAddress { get; set; }
        public int? MediaID { get; set; }

        //EntityInstituteInfo
        public int EntityInstituteInfoID { get; set; }
        [Required(ErrorMessage = "Please provide company name")]
        public string? Name { get; set; }

        //Supplier
        public int SupplierID { get; set; }
        public int? Status { get; set; } = 1;
        public string? Remarks { get; set; }
        [Required(ErrorMessage = "Please provide tax number for the supplier")]
        public string? TaxNumber { get; set; }

        //Country
        [Required(ErrorMessage = "Please choose a country")]
        public int? CountryID { get; set; }
        public string? CountryName { get; set; }
        public string? ISDCode { get; set; }

        //EntityAddress
        public List<AddressModel> Addresses { get; set; } = new();

        //Contact Persons
        public List<SupplierContactPersonModel> ContactPersons { get; set; } = new();

    }
}
