using PB.Shared.Tables;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models
{
    public class ClientListViewModel
    {
        public int? ClientID { get; set; }
        public int? EntityId { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? EmailAddress { get; set; }
        public bool LoginStatus { get; set; }
        public int? PaymentStatus { get; set; }
        public bool IsBlock { get; set; }
        public string? BlockReason { get; set; }
    }


    public class ClientViewModel
    {
        public int? ClientID { get; set; }
        public int? EntityID { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? EmailAddress { get; set; }
        public bool LoginStatus { get; set; }
        public int? PaymentStatus { get; set; }
        public int? InvoiceID { get; set; }
        public ClientMembershipPlanViewModel PlanList { get; set; } = new();
        public List<ClientInvoiceListModel> InvoiceData { get; set; } = new();
        public bool IsBlock { get; set; }
        public string? BlockReason { get; set; }

    }

    public class ClientMembershipPlanViewModel
    {
        public int? ClientID { get; set; }
        public int? PackageID { get; set; }
        public int? PlanID { get; set; }
        public int? CapacityID { get; set; }
        public int? InvoiceID { get; set; }
        public int? InvoiceJournalMasterID { get; set; }
        public string? PackageName { get; set; }
        public string? PlanName { get; set; }
        public int? Capacity { get; set; }
        public decimal? Fee { get; set; }
        public DateTime? DueDate { get; set; }
        public int? MonthCount { get; set; }
        public string? PackageDescription { get; set; }
        public List<MembershipFeature> membershipFeatures { get; set; } = new();
        public string? FeaturesName { get; set; }
        public int? PaidStatus { get; set; }
    }


    public class ClientInvoiceListModel
    {
        public int? InvoiceID { get; set; }
        public string? PackageName { get; set; }
        public decimal? Fee { get; set; }
        public int? PaidStatus { get; set; }
        public DateTime? DisconnectionDate { get; set; }
        public int? ClientID { get; set; }
        public int? InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
    }

    public class AccountActiveModel
    {
        public int? EntityID { get; set; }
        public decimal? Fee { get; set; }
        public string? PackageName { get; set; }
        public string? Email { get; set; }
        public int? InvoiceID { get; set; }
        public int? MonthCount { get; set; }
        public int? PackageID { get; set; }
        public int? ClientID { get; set; }
        public int? InvoiceJournalMasterID { get; set; }
        public decimal Discount { get; set; }
    }


    public class PaymentVerificationViewModel
    {
        public int InvoiceID { get; set; }
        public int? MediaID { get; set; }
        public int? ClientID { get; set; }
        public string? FileName { get; set; }
        public string? PaymentRefNo { get; set; }
        [Required(ErrorMessage = "PLease choose a receipt voucher from the list")]
        public int? ReceiptVoucherTypeID { get; set; }
    }


    public class ClientPaymentListModel
    {
        public int? ClientID { get; set; }
        public string? Name { get; set; }
        public string? PackageName { get; set; }
        public decimal? Fee { get; set; }
        public int? PaidStatus { get; set; }
        public int? InvoiceID { get; set; }
        public DateTime? DisconnectionDate { get; set; }
    }

    public class GenerateDefaultTaxCategoryModel
    {
        [Required]
        public int? ClientID { get; set; }
        [Required]
        public int? CountryID { get; set; }
        public string? CountryName { get; set; }
    }
}
