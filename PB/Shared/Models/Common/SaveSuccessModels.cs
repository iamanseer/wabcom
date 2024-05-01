using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Common
{
    public class ContactPersonAddResultModel : Success
    {
        public int ContactPersonID { get; set; }
        public int EntityID { get; set; }
        public int EntityPersonalInfoID { get; set; }
        public string? Name { get; set; }
        public string? EmailAddress { get; set; }
    }

    public class ItemAddResultModel : Success
    {
        public int ItemID { get; set; }
        public int ItemVariantID { get; set; }
        public string? ItemName { get; set; }
    }

    public class EnquiryAddResultModel : Success
    {
        public int EnquiryID { get; set; }
    }

    public class AddressAddResultModel : Success
    {
        public int AddressID { get; set; }
        public string? CompleteAddress { get; set; } 
    }

    public class CustomerAddResultModel : Success
    {
        public int EntityID { get; set; }
        public string? Name { get; set; }
    }

    public class BranchAddResultModel : Success
    {
        public int BranchID { get; set; }
        public string? BranchName { get; set; }
    }

    public class UserAddResultModel : Success
    {
        public int UserID { get; set; }
        public string? UserName { get; set; }
    }

    public class FollowupStatusAddResultModel : Success
    {
        public int FollowUpStatusID { get; set; }
        public string? StatusName { get; set; }
    }

    public class PackingTypeAddResultModel : Success
    {
        public int PackingTypeID { get; set; }
        public string? PackingTypeName { get; set; }
    }

    public class StateAddResultModel : Success
    {
        public int StateID { get; set; }
        public string? StateName { get; set; }
    }

    public class CityAddResultModel : Success
    {
        public int CityID { get; set; }
        public string? CityName { get; set; }
    }

    public class FollowupAddResultModel : Success
    {
        public int FollowupID { get; set; }
    }

    public class ItemVariantAddResultModel : Success
    {
        public int ItemVariantID { get; set; }
    }

    public class ItemSizeAddResultModel
    {
        public int SizeID { get; set; }
        public string? Size { get; set; }
    }

    public class LeadThroughAddResultModel : Success
    {
        public int LeadThroughID { get; set; }
        public string? LeadThroughName { get; set; }
    }

    public class UserCapacityAddResultModel : Success
    {
        public int CapacityID { get; set; }
        public int Capacity { get; set; }
    }

    public class MembershipPlanAddResultModel : Success
    {
        public int PlanID { get; set; }
        public string? PlanName { get; set; }
    }
    public class RegistraionAddResultModel : Success
    {
        public int? UserID { get; set; }
        public int? ClientID { get; set; }
    }

    public class TaxCategoryAddResultModel : Success
    {
        public int TaxCategoryID { get; set; }
        public string? TaxCategoryName { get; set; }
    }

    public class CourtAddResultModel : Success
    {
        public int CourtID { get; set; }
        public string? CourtName { get; set; }
    }

    public class AccountGroupAddResultModel : Success
    {
        public int AccountGroupID { get; set; }
        public string? AccountGroupName { get; set; }
    }

    public class LedgerAddResultModel : Success
    {
        public int LedgerID { get; set; }
        public string? LedgerName { get; set; }
    }

    public class VoucherTypeSaveResultModel : Success
    {
        public int VoucherTypeID { get; set; }
        public string? VoucherTypeName { get; set; }
    }

    public class VoucherEntrySuccessModel : Success
    {
        public int JournalMasterID { get; set; }
    }
    public class InvoiceTypeSuccessModel : Success
    {
        public int InvoiceTypeID { get; set; }
        public string? InvoiceTypeName { get; set; }
    }
    public class QuotationSuccessModel : Success
    {
        public int QuotationID { get; set; }
    }

    public class InvoiceSuccessModel : Success
    {
        public int InvoiceID { get; set; }
    }

    public class OtpSendSuccessModel : Success
    {
        public int UserID { get; set; }
    }

    public class ItemColorSuccessModel : Success
    {
        public int ColorID { get; set; }
        public string? ColorName { get; set; }
    }

    public class ItemBrandSaveSuccessModel : Success
    {
        public int BrandID { get; set; }
        public string? BrandName { get; set; }
    }

    public class ItemCategorySuccessModel : Success
    {
        public int CategoryID { get; set; }
        public string? CategoryName { get; set; }
    }

    public class ItemGroupSaveSuccessModel : Success
    {
        public int GroupID { get; set; }
        public string? GroupName { get; set; }
    }

    public class StockAdjustmentSuccessModel : Success
    {
        public int InvoieID { get; set; }
    }

    public class SupplierSaveSuccessModel : Success
    {
        public int SupplierID { get; set; }
        public int SupplierEntityID { get; set; }
    }

    public class SupplierContactPersonAddResultModel : Success
    {
        public int ContactPersonID { get; set; }
        public int EntityID { get; set; }
        public int EntityPersonalInfoID { get; set; }
        public string? Name { get; set; }
        public string? EmailAddress { get; set; }
    }

    public class PaymentTermAddResultModel : Success
    {
        public int PaymentTermID { get; set; }
        public string? PaymentTermName { get; set; }
    }
    public class PromotionAddResultModel : Success
    {
        public int PromotionID { get; set; }
        public string? PromotionName { get; set; }
    }


    public class BusinessTypeAddResultModel : Success
    {
        public int BusinessTypeID { get; set; }
        public string? BusinessTypeName { get; set; }
    }
}
