using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models
{
    public class MessageStatusModel
    {
        public string? Object { get; set; }
        public List<MessageStatusEntryModel>? Entry { get; set; }

    }
    public class MessageStatusEntryModel
    {
        public string? Id { get; set; }
        public List<MessageStatusChangesModel>? Changes { get; set; }

    }
    public class MessageStatusChangesModel
    {
        public MessageStatusValueModel? Value { get; set; }
        public string? Field { get; set; }
    }
    public class MessageStatusValueModel
    {
        public string? Messaging_product { get; set; }
        public MessageStatusMetadataModel? Metadata { get; set; }
        public List<MessageStatusStatusesModel>? Status { get; set; }

    }
    public class MessageStatusMetadataModel
    {
        public string? Display_phone_number { get; set; }
        public string? Phone_number_id { get; set; }
    }
    public class MessageStatusStatusesModel
    {
        public string? Id { get; set; }
        public string? Recipient_id { get; set; }
        public string? Status { get; set; } //sent, delivered
        public string? Timestamp { get; set; }
        public ConversationModel? Conversation { get; set; }
        public ConversationPricingModel? Pricing { get; set; }
        public List<MessageStatusErrorModel> Errors { get; set; }

    }

    public class ConversationModel
    {
        public string? Id { get; set; }
    }


    public class ConversationPricingModel
    {
        public bool Billable { get; set; }
        public string? pricing_model { get; set; }
        public string? Category { get; set; }

        private bool _typeId;

        public bool IsUserInitiated
        {
            get
            {
                switch (Category)
                {
                    case "user_initiated":
                        _typeId = true;
                        break;
                    default:
                        _typeId = false;
                        break;
                }
                return _typeId;
            }
        }
    }

    public class MessageStatusConversationModel
    {
        public string? Id { get; set; }//CONVERSATION_ID
        public string? Expiration_timestamp { get; set; }
        public MessageStatusOriginModel? Origin { get; set; }
    }
    public class MessageStatusOriginModel
    {
        public string? Type { get; set; } //user_initiated or business_initated  or referral_conversion
    }
    public class MessageStatusPricingModel
    {
        public string? Pricing_model { get; set; }
        public bool Billable { get; set; }
        public string? Category { get; set; }//user_initiated or business_initated  or referral_conversion
    }

    public class MessageStatusErrorModel
    {
        public string code { get; set; }
        public string title { get; set; }
        public MessageStatusErrorDataModel error_data { get; set; }
    }

    public class MessageStatusErrorDataModel
    {
        public string? details { get; set; }
    }
}
