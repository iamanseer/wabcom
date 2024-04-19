using PB.Shared.Tables;
using PB.Shared.Tables.Tax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models
{
    public class ClientInvoicePdfViewModel
    {
        
        public string? ClientName { get; set; }
        public string? ClientAddress1 { get; set; }
        public string? ClientEmail { get; set; }
        public string? ClientPhone { get; set;}
        public int? ClientMediaID { get; set; }
        public string? ClientGSTNo { get; set; }
        public string? ClientCity { get; set; }
        public string? ClientState { get; set; }
        public string? ClientCountry { get; set; }
        public string? ClientPincode { get; set; }
        public string? ClientAddress2 { get; set; }
        public string? ClientAddress3 { get; set; }


        public int? PackageID { get; set; }
        public string? PackageName { get; set; }
        public List<MembershipFeature> Features { get; set; } = new();
        public string? AvailableFeatures { get; set; }
        public int? Capacity { get; set; }
        public string? PlanName { get; set; }
        public decimal Fee { get; set; }
        public decimal Discount { get; set; }
        public string? PackageDescription { get; set; }

        public int? InvoiceID { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? DisconnectionDate { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public decimal TaxPercentage { get; set; }
        public decimal Tax { get; set; }
        public int? TaxCategoryID { get; set; }
        public int? InvoiceNo { get; set; }
        public List<TaxCategoryItem> Taxcategoryitem { get; set; } = new();
        public CompanyDetailsViewmodel companyDetails { get; set; } = new();

    }


    public class CompanyDetailsViewmodel
    {
        public string? CompanyName { get; set; }
        public string? CompanyAddress1 { get; set; }
        public string? CompanyEmail { get; set; }
        public string? CompanyPhone { get; set; }
        public int? CompanyMediaID { get; set; }
        public string? CompanyGSTNo { get; set; }
        public string? CompanyProfileImage { get; set; }
        public string? CompanyCity { get; set; }
        public string? CompanyState { get; set; }
        public string? CompanyCountry { get; set; }
        public string? CompanyPincode { get; set; }
        public string? CompanyAddress2 { get; set; }
        public string? CompanyAddress3 { get; set; }

    }
}
