using PB.Model;
using PB.Shared.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models
{
    //public class SendMessageModel : FileUploadModel
    //{
    //    [Required]
    //    public string? Title { get; set; }
    //    [Required]
    //    [Range(1, int.MaxValue)]
    //    public int TemplateID { get; set; }
    //    [Required]
    //    [Range(1, int.MaxValue)]
    //    public int LanguageID { get; set; }
    //    [Required]
    //    [Range(1, int.MaxValue)]
    //    public int WhatsappAccountID { get; set; }
    //    public DateTime? ScheduleTime { get; set; }
    //    public string? Component { get; set; }
    //    public int MessageSendingTypes { get; set; }
    //    public int PeriodicDay { get; set; }
    //    public List<CheckboxModel> PinnedContacts { get; set; } = new();
    //}


    //public class MessageRecipientModel
    //{
    //    public string? Name { get; set; }
    //    public string? Phone { get; set; }
    //}

    //public class MessageListModel
    //{
    //    public int BroadcastID { get; set; }
    //    public string? Name { get; set; }
    //    public string? TemplateName { get; set; }
    //    public DateTime Date { get; set; }
    //    public int TotalRecipients { get; set; }
    //    public int SuccessCount { get; set; }
    //    public int Replied { get; set; }
    //}


    //public class MessageReceipientSearchModel : PagedListPostModel
    //{
    //    public int BroadcastID { get; set; }
    //    public string? Phone { get; set; }
    //    public int Status { get; set; }
    //}
    //public class MessageCountNotificationModel
    //{
    //    public int? TotalRecipients { get; set; }
    //    public int? SuccessCount { get; set; }
    //}
    //public class WhatsappMessageContentModel
    //{
    //    public string messaging_product { get; set; } = "whatsapp";
    //    public string? to { get; set; }
    //    public string? type { get; set; }
    //    public WhatsAppTemplateModel template { get; set; } = new();
    //}

    //public class WhatsAppTemplateModel
    //{
    //    public string? name { get; set; }
    //    public WhatsappContentLanguageModel language { get; set; } = new();
    //    public object components { get; set; } = new();
    //}

    //public class WhatsappContentLanguageModel
    //{
    //    public string code { get; set; } = "en_US";
    //    public string policy { get; set; } = "deterministic";
    //}

    //public class WhatsappContentComponentHeaderModel
    //{
    //    public string type { get; set; } = "header";
    //    public List<WhatsappVariableModel> parameters { get; set; }
    //}

    //public class WhatsappContentComponentBodyModel
    //{
    //    public string type { get; set; } = "body";
    //    public List<WhatsappVariableModel> parameters { get; set; }
    //}

    //public class WhatsappVariableModel
    //{
    //    public string type { get; set; }
    //    public string text { get; set; }
    //    public WhatappImageVariableItemModel image { get; set; }
    //}

    //public class WhatappImageVariableItemModel
    //{
    //    public string link { get; set; }

    //}

    public class CreateBroadcastModel : FileUploadModel
    {
        public int BroadcastID { get; set; }
        [Required]
        public string? Title { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int TemplateID { get; set; }
        public DateTime? ScheduleTime { get; set; }
        public int MessageSendingTypes { get; set; }
        public int PeriodicDay { get; set; }
        public int? MediaID { get; set; }
        public bool IsSent { get; set; }
    }

    public class WhatsappBroadcastSaveResultModel
    {
        public int BroadcastID { get; set; }
        public List<WhatsappBroadcastRecipientSaveModel>? Receipients { get; set; } = new();
    }

    public class WhatsappBroadcastRecipientSaveModel
    {
        public int ReceipientID { get; set; }
        public string? Phone { get; set; }
        public List<WhatsappBroadcastRecipientVariableModel> Parameters { get; set; } = new();
    }

    public class WhatsappVariableValueModel
    {
        public string? VariableName { get; set; }
        public string? Value { get; set; }
        public int Section { get; set; }
        public int DataType { get; set; }
    }

    public class WhatsappBroadcastRecipientVariableModel
    {
        public int? VariableID { get; set; }
        public string? Value { get; set; }
    }


    public class MessageListModel
    {
        public int BroadcastID { get; set; }
        public string? BroadcastName { get; set; }
        public string? TemplateName { get; set; }
        public DateTime? Date { get; set; }
        public int TotalRecipients { get; set; }
        public int Sent { get; set; }
        public int Delivered { get; set; }
        public int Read { get; set; }
        public int Failed { get; set; }
        public bool IsSent { get; set; }
    }

    public class MessageReceipientSearchModel : PagedListPostModelWithFilter
    {
        public int BroadcastID { get; set; }
        public string? Phone { get; set; }
        public int Status { get; set; }
    }

    public class RecreatBroadcastModel : MessageReceipientSearchModel
    {
        public CreateBroadcastModel BroadcastModel { get; set; } = new();
    }

    public class BroadcastReceipientHistory
    {
        public string? Phone { get; set; }
        public string? Name { get; set; }
        public int SentStatus { get; set; }
        public string? SentOn { get; set; }
        public string? DeliveredOn { get; set; }
        public string? SeenOn { get; set; }
        public string? FailedReason { get; set; }
        public string? MessageStatusBadgeClass
        {
            get
            {
                switch (SentStatus)
                {
                    case (int)MessageStatus.Sent:
                        return "badge bg-dark";

                    case (int)MessageStatus.Delivered:
                        return "badge bg-success";

                    case (int)MessageStatus.Read:
                        return "badge bg-info";

                    case (int)MessageStatus.Failed:
                        return "badge bg-danger";
                }
                return null;
            }
        }

    }
    public class MessageCountNotificationModel
    {
        public int? TotalRecipients { get; set; }
        public int? SuccessCount { get; set; }
    }
    

    public class TemplateModel
    {
        public string? name { get; set; }
        public WhatsappContentLanguageModel language { get; set; } = new();
        public object? components { get; set; } = new();
    }

    public class WhatsappContentLanguageModel
    {
        public string code { get; set; } = "en_US";
        public string policy { get; set; } = "deterministic";
    }

    public class WhatsappContentComponentHeaderModel
    {
        public string type { get; set; } = "header";
        public List<WhatsappVariableModel> parameters { get; set; }
    }

    public class WhatsappContentComponentBodyModel
    {
        public string type { get; set; } = "body";
        public List<WhatsappVariableModel> parameters { get; set; }
    }

    public class WhatsappVariableModel
    {
        public string type { get; set; }
        public string text { get; set; }
        public WhatappImageVariableItemModel image { get; set; }
    }

    public class WhatappImageVariableItemModel
    {
        public string link { get; set; }

    }

    public class WhatsappMediaUploadResponseModel
    {
        public int? MediaID { get; set; }
        public string? WMediaID { get; set; }
    }

    public  class SendOTPModel
    {
        public int WhatsappAccountID { get; set; }
        public int ClientID { get; set; }
        public string? To { get; set; }
        public string? ContactName { get; set; }
        public string? TemplateName { get; set; }
        public string? Lang { get; set; }
    }


}
