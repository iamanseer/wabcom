using PB.Shared.Enum;
using PB.Shared.Tables.Whatsapp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models
{
    public class ChatbotReplyListModel
    {
        public int? WhatsappAccountID { get; set; }
        public int ReplyID { get; set; }
        public string? ReplyHeader { get; set; }
        public string? ReplyBody { get; set; }
        public string? ReplyFooter { get; set; }
        public string? ListButtonText { get; set; }
        public int ParentReplyID { get; set; }
        public int ReplyTypeID { get; set; }
        public List<ChatbotReplyListItemModel>? ReplyList { get; set; }
        public List<ChatbotMultiReplyListModel>? MultiReplyList { get; set; }

    }
    public class ChatbotReplyListItemModel
    {
        public int WhatsappAccountID { get; set; }
        public int ReplyID { get; set; }
        public int ParentReplyID { get; set; }
        public int Code { get; set; }
        public int ReplyTypeID { get; set; }
        public string? Description { get; set; }
    }

    public class ChatbotReplyHeaderUpdateModel
    {
        public int ReplyID { get; set; }
        public string? ReplyHeader { get; set; }
        public string? ReplyBody { get; set; }
        public string? ReplyFooter { get; set; }
        public string? ListButtonText { get; set; }
    }

    public class ChatbotReplyModel: ChatbotReply
    {
        public List<ChatbotReplyOptionModel> Options { get; set; } = new();
        public List<ChatbotSubmitAssigneeModel>? Assignees { get; set; }
        public string? ItemName { get; set; }


        public int PurchaseReplyID { get; set; }
        public string? QuantityLabel { get; set; } = "Please enter quantity";
        public string? PaymentLabel { get; set; } = "Please pay @Amount though this link";
        public string? ValidationFailedMessage { get; set; } = "Please enter valid quantity";
        public string? PaymentSuccessMessage { get; set; } = "Your payment is success";
        public string? PaymentFailedMessage { get; set; } = "Your payment is failed";
    }

    public class ChatbotReplyOptionModel
    {
        public int ReplyID { get; set; }
        public string? Code { get; set; }
        public string? Short { get; set; }
        public string? Detailed { get; set; }
        public string? SmallDescription { get; set; }
        public string? Header { get; set; }
        public string? Footer { get; set; }
    }

    public class ChatbotMultiReplyListModel
    {
        public int MultiReplyID { get; set; }
        public int SlNo { get; set; }
        public int MediaTypeID { get; set; }
        private string _MediaType;

        public string MediaType
        {
            get {
                _MediaType=((MessageTypes)MediaTypeID).ToString();
                return _MediaType; 
            }
        }

        public string? FileName { get; set; }
        public string? Caption { get; set; }
        public string? WMediaID { get; set; }
        public string? DocumentName { get; set; }
        public int? MediaID { get; set; }
    }

}
