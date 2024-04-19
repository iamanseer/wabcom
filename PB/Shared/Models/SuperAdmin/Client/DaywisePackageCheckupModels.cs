using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.SuperAdmin.Client
{

    public class InvoiceGenerationListModel
    {
        public int? EntityID { get; set; }
        public int? ClientID { get; set; }
        public decimal? Fee { get; set; }
        public int? PackageID { get; set; }
        public DateTime? EndDate { get; set; }
        public int? MonthCount { get; set; }
        public int? InvoiceID { get; set; }
        public string? PackageName { get; set; }
        public decimal? Discount { get; set; }
    }


    public class ClientPaymentDateModel
    {
        public DateTime? InvoiceDate { get; set; }
        public DateTime? DisconnectionDate { get; set; }
        public DateTime? DueDate { get; set; }
    }

    public class ClientInvoiceSendDetailsModel
    {
        public string? FileName { get; set; }
        public string? PackageName { get; set; }
        public string? EmailAddress { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public int? InvoiceID { get; set; }
        public DateTime? DisconnectionDate { get; set; }
        public string? Name { get; set; }
        public int? InvoiceNo { get; set; }
    }


    public class ClientUpdatedPackageDetails
    {
        public int? PackageID { get; set; }
        public int? MonthCount { get; set; }
        public int? TaxcategoryID { get; set; }
        public decimal? Fee { get; set; }
    }
}
