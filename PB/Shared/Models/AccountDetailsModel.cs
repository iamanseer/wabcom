using PB.Model;
using PB.Shared.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Model.Models
{
	public class AccountDetailsModel
	{
		public int? EntityID { get; set; }
		public int? UserID { get; set; }
		public int? ClientID { get; set; }
        [Required(ErrorMessage = "Please provide name")]
        public string? CompanyName { get; set; }
        [Required(ErrorMessage = "Please provide email address")]
        public string? EmailAddress { get; set; }
        [Required(ErrorMessage = "Please provide phone")]
        public string? Phone { get; set; }
		public int? MediaID { get; set; }
        public string? FileName { get; set; }
        [Required(ErrorMessage = "Please provide user name")]
        public string? UserName { get; set; }
        [RequiredIf("UserTypeID",(int)UserTypes.Client,ErrorMessage = "Please provide address Line 1")]
        public string? AddressLine1 { get; set; }
		public string? AddressLine2 { get; set; }
		public string? AddressLine3 { get; set; }
        [RequiredIf("UserTypeID",(int)UserTypes.Client,ErrorMessage ="Please Provide a state")]
        public int? StateID { get; set; }
        [RequiredIf("UserTypeID",(int)UserTypes.Client, ErrorMessage = "Please choose country")]
        public int? CountryID { get; set; }
		public string? PinCode { get; set; }
        public string? ContactPersonName { get; set; }
        public string? ContactPersonPhone { get; set; }
        public string? ContactPersonEmail { get; set; }
		public string? CountryName { get; set; }
		public string? StateName { get; set; }
		public int? EntityInstituteInfoID { get; set; }
		public int? UserTypeID { get; set; }
		public int? EntityPersonalInfoID { get; set; }
		public int? AddressID { get; set; }
        public string? GSTNo { get; set; }
       // public int CityID { get; set; }

    }
}
