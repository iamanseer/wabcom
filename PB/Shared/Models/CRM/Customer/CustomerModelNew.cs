﻿using PB.Shared.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.CRM.Customer
{
    public class CustomerModelNew
    {
        public int CustomerID { get; set; }
        public int? EntityID { get; set; }
        public int Status { get; set; } = 1;
        [Required(ErrorMessage = "Please choose the type of customer")]
        public int Type { get; set; } = (int)CustomerTypes.Individual;
        public string? Remarks { get; set; }
        public int? ClientID { get; set; }
        public string? TaxNumber { get; set; }
        [Required(ErrorMessage = "Please provide customer name")]
        public string? Name { get; set; }
        public int EntityPersonalInfoID { get; set; }
        public int EntityInstituteInfoID { get; set; }
        public int EntityTypeID { get; set; }
        [Required(ErrorMessage = "Please provide customer phone")]
        [RegularExpression("^[0-9]+$", ErrorMessage = "Only numbers are allowed")]
        public string? Phone { get; set; }
        //[EmailAddress(ErrorMessage = "Please povide vallid e-mail address")]
        [RegularExpression("^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$", ErrorMessage = "Please provide a valid Email like example@test.com")]
        public string? EmailAddress { get; set; }
        public int? MediaID { get; set; }
        public bool? AllowType { get; set; }
        public int? ContactID { get; set; }
        [Required(ErrorMessage = "Please choose a country")]
        public int? CountryID { get; set; }
        public string? CountryName { get; set; }
        public string? ISDCode { get; set; }
        public List<AddressView> Addresses { get; set; } = new();
        public List<CustomerContactPersonModel> ContactPersons { get; set; } = new();
    }
}
