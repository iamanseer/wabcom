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
    public class WhatsappChatStatusUpdateModel
    {
        public int? ContactID { get; set; }
        public int ChatID { get; set; }
        public int StatusID { get; set; }
    }
    public class WhatsappChatListModel
    {
        public int ContactID { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Message { get; set; }
        public int UnreadMessages { get; set; }
        public string MessageOn { get; set; }
        public string? SearchString { get; set; }       
    }

    public class WhatsappChatHistoryModel
    {
        public int ContactID { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public bool IsSelected { get; set; }
        public List<WhatsappChatHistoryItemModel> History { get; set; }
    }

    public class WhatsappChatHeaderModel
    {
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? EmailAddress { get; set; }
        public string? Address { get; set; }
        public List<ContactTagModel> Tags { get; set; } = new();
        public List<NoteListModel> Notes { get; set; } = new();
        public bool SupportChatStarted { get; set; }
        public int? AddressID { get; set; }
    }

    public class NoteListModel:ProfilePhoto
    {
        public int? ContactID { get; set; }
        public int NoteID { get; set; }
        public string? AddedBy { get; set; }
        public DateTime? AddedOn { get; set; }
        public string? Note { get; set; }
    }

    public class ContactTagModel
    {
        public int ContactTagID { get; set; }
        public int? ContactID { get; set; }
        public string? Tag { get; set; }
    }

    public class ContactEmailEditModal
    {
        public int ContactID { get; set; }
        [EmailAddress]
        public string? EmailAddress { get; set; }
    }

    public class WhatsappChatHistoryItemModel
    {
        public int? ChatID { get; set; }
        public string? Message { get; set; }
        public int MessageTypeID { get; set; }
        public bool IsIncoming { get; set; }
        public string? MessageOn { get; set; }
        public string? WMediaID { get; set; }
        //public string? WMediaURL { get; set; }
        //public string? Base64Media { get; set; }
        public string? FileName { get; set; }

        public string? FormattedName { get; set; }
        public string? ContactName { get; set; }
        public string? PhoneNo { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int ContactID { get; set; }
        public MessageStatus StatusID { get; set; }
        public int? RecievedMediaID { get; set; }
        public string? FailedReason { get; set; }
        public string? FileCaption { get; set; }
        public string? DocumentName { get; set; }
        public string? AppChatID { get; set; } //Only for mobile app
    }

    public class SendWhatsappChatModel
    {
        public string? Message { get; set; }
        [Range(1, int.MaxValue)]
        public int ContactID { get; set; }
        [Range(1, int.MaxValue)]
        public int WhatsappAccountID { get; set; }
        public string? Link { get; set; }
        public string? FormattedName { get; set; }
        public string? ContactName { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? PhoneNo { get; set; }
        public int? MediaID { get; set; }
        public string? WMediaID { get; set; }
        public string? DocumentName { get; set; }
        public string? AppChatID { get; set; } //Only for mobile app
    }

    public class MediaDetailsModel
    {
        public string? Url { get; set; }
        public string? Mime_type { get; set; }
        public byte[] File { get; set; }
    }

    public class ChatHistoryPostModel : PagedListPostModel
    {
        public int? ReceiverEntityID { get; set; }
        public int TimeOffset { get; set; }
        public int ContactId { get; set; }
        public int WhatsappAccountID { get; set; }
    }

    public class ChatReinitiateListModel
    {
        public int SessionID { get; set; }
        public int WhatsappAccountID { get; set; }
        public string Phone { get; set; }
    }


    public class ChatTemplateListModel
    {
        public int TemplateID { get; set; }
        public string? TemplateName { get; set; }
        public int VariableCount { get; set; }
        public int? HeaderMediaTypeID { get; set; }
        public int? HeaderMediaID { get; set; }
        public int? HeaderTypeID { get; set; }

    }

    public class ChatTemplateVariableListModel
    {
        public int VariableID { get; set; }
        public string? VariableName { get; set; }
        public string? Value { get; set; }
        public int Section { get; set; }
        public int DataType { get; set; }

    }


    public class ChatTemplateSentModel
    {
        public int TemplateID { get; set; }
        public int? ContactID { get; set; }
        public int? HeaderMediaID { get; set; }
        //public List<ChatTemplateVariableListModel> template { get; set; } = new();

        public List<WhatsappVariableValueModel> template { get; set; } = new();
    }


    public class WhatsappTemplateMediaSentModel:WhatsappTemplateModel
    {
        public string? FileName { get; set; }
    }
}
