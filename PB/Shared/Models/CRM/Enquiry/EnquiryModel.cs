
using PB.CRM.Model.Enum;
using PB.Model.Models;
using PB.Shared.Enum.CRM;
using PB.Shared.Tables.CRM;

namespace PB.Shared.Models.CRM.Enquiry
{
    public class EnquiryModel : PB.Shared.Tables.CRM.Enquiry
    {
        public string? CustomerName { get; set; }
        public string? EnquiryName { get; set; }
        public int? QuotationID { get; set; }
        public List<EnquiryItemModel> EnquiryItem { get; set; } = new();
        public List<EnquiryAssigneeModel> FollowupAssignees { get; set; } = new();
        public List<FollowUpModel> Histories { get; set; } = new();
        public string? Username { get; set; }
        public string? MobileNumber { get; set; }
        public int? ContactID { get; set; }
        public string? Phone { get; set; }
        public string? EmailAddress { get; set; }
        public string? LeadThroughName { get; set; }
        public string? LeadMediaIconClass
        {
            get
            {
                switch (LeadThroughID)
                {
                    case (int)LeadThroughs.Whatsapp:
                        return "fa fa-brands fa-whatsapp";
                    case (int)LeadThroughs.Facebook:
                        return "fa fa-brands fa-facebook";
                    case (int)LeadThroughs.Instagram:
                        return "fa fa-brands fa-instagram";
                    case (int)LeadThroughs.Email:
                        return "fa-regular fa-envelope";
                    case (int)LeadThroughs.Office:
                        return "fa-regular fa-building";
                    case (int)LeadThroughs.Referral:
                        return "fa fa-user-plus";
                    case (int)LeadThroughs.Event:
                        return "fa-regular fa-calendar-days";
                    case (int)LeadThroughs.Advertisement:
                        return "fa-regular fa-rectangle-ad";
                    case (int)LeadThroughs.Other:
                        return "fa-solid fa-code-branch";
                    case (int)LeadThroughs.Phone:
                        return "fa fa-phone";
                    case (int)LeadThroughs.Website:
                        return "fa-regular fa-browser";
                    case null:
                        return "";
                }
                return "";
            }
        }
        public string? LeadQualityCssClass
        {
            get
            {
                switch (LeadQuality)
                {
                    case (int)LeadQualities.Hot:
                        return "text-danger";
                    case (int)LeadQualities.Warm:
                        return "text-warning";
                    case (int)LeadQualities.Cold:
                        return "text-black";
                    case null:
                        return "";
                }
                return "";
            }
        }
        public string? NatureCSSClass
        {
            get
            {
                switch (CurrentFollowupNature)
                {
                    case (int)FollowUpNatures.New:
                        return "text-black";
                    case (int)FollowUpNatures.Followup:
                        return "text-info";
                    case (int)FollowUpNatures.Dropped:
                        return "text-danger";
                    case (int)FollowUpNatures.Interested:
                        return "text-success";
                }
                return "";
            }
        }
    }

    public class EnquiryItemModel : EnquiryItem
    {
        public bool IsEditMode { get; set; } = false;
        public string? ItemName { get; set; }
        public int RowIndex { get; set; }
    }
}
