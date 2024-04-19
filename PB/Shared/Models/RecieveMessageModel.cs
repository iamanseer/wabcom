using PB.Shared.Tables.Whatsapp;

namespace PB.Shared.Models
{
    public class ReceiveMessageModel
    {
        public string? Object { get; set; }
        public List<ReceiveMessagEntryModel>? Entry { get; set; }
    }


    public class MessageTemplateStatusUpdate
    {
        public string? Event { get; set; }
        public long? message_template_id { get; set; }
        public string? message_template_name { get; set; }
        public string? message_template_language { get; set; }
        public string? reason { get; set; }
    }


    public class ReceiveMessagEntryModel
    {
        public string? Id { get; set; }
        public List<ReceiveMessagChangesModel>? Changes { get; set; }
    }
    public class ReceiveMessagChangesModel
    {
        public ReceiveMessagValueModel? Value { get; set; }
        public string? Field { get; set; }
    }
    public class ReceiveMessagValueModel : MessageTemplateStatusUpdate
    {
        public string? Messaging_product { get; set; }
        public ReceiveMessagMetaDataModel? Metadata { get; set; }
        public List<ReceiveMessagContactModel>? Contacts { get; set; }
        public List<ReceiveMessageMessageModel>? Messages { get; set; }
        public List<MessageStatusStatusesModel>? Statuses { get; set; }
    }

    public class ReceiveMessageMessageModel
    {
        public string? From { get; set; }
        public string? Id { get; set; }
        public string? Timestamp { get; set; }
        public ReceiveMessageTextModel? Text { get; set; }
        public ReceiveMediaModel? image { get; set; }
        public ReceiveMediaModel? audio { get; set; }
        public ReceiveMediaModel? voice { get; set; }
        public ReceiveMediaModel? video { get; set; }
        public ReceiveMediaModel? document { get; set; }
        public List<ReceiveContactsModel>? contacts { get; set; }
        public WhatsappChatLocation? location { get; set; }
        public ReceiveMediaModel? sticker { get; set; }
        public ReceiveInteractiveModel? Interactive { get; set; }
        public string? Type { get; set; }
    }
    public class ReceiveMessageTextModel
    {
        public string? Body { get; set; }
    }
    public class ReceiveMediaModel
    {
        public string? caption { get; set; }
        public string? file { get; set; }
        public string? id { get; set; }
        public string? mime_type { get; set; }
        public string? sha256 { get; set; }
        public bool voice { get; set; }
        public bool animated { get; set; }
    }
    public class ReceiveContactsModel
    {
        public ReceiveContactNameModel? name { get; set; }
        public List<ReceiveContactPhonesModel>? phones { get; set; }
        public ReceiveContactAddressesModel? addresses { get; set; }
        public DateOnly? birthday { get; set; }
        public ReceiveContactEmailsModel? emails { get; set; }
        public ReceiveContactImsModel? ims { get; set; }
        public ReceiveContactOrgModel? org { get; set; }
        public ReceiveContactUrlsModel? urls { get; set; }

    }

    //public class ReceiveImageModel
    //{
    //    public string? Caption { get; set; }
    //    public string? Mime_type { get; set; }
    //    public string? Sha256 { get; set; }
    //    public string? Id { get; set; }
    //}
    //public class ReceiveAudioModel
    //{
    //    public string? Mime_type { get; set; }
    //    public string? Id { get; set; }
    //}    
    //public class ReceiveDocumentModel
    //{
    //    public string? Caption { get; set; }
    //    public string? Filename { get; set; }
    //    public string? Mime_type { get; set; }
    //    public string? Sha256 { get; set; }
    //    public string? Id { get; set; }
    //}
    //public class ReceiveStickerModel
    //{
    //    public bool? Animated { get; set; }
    //    public string? Mime_type { get; set; }
    //    public string? Sha256 { get; set; }
    //    public string? Id { get; set; }
    //}
    //public class ReceiveContactModel
    //{
    //    public ReceiveContactAddressesModel? addresses { get; set; }
    //    public DateOnly? birthday { get; set; }
    //    public ReceiveContactEmailsModel? emails { get; set; }
    //    public ReceiveContactImsModel? ims { get; set; }
    //    public ReceiveContactNameModel? name { get; set; }
    //    public ReceiveContactOrgModel? org { get; set; }
    //    public ReceiveContactPhonesModel? phones { get; set; }
    //    public ReceiveContactUrlsModel? urls { get; set; }
    //}

    public class ReceiveLocationModel
    {
        public double? latitude { get; set; }
        public double? longitude { get; set; }
        public string? address { get; set; }
        public string? name { get; set; }
        //public string? url { get; set; }
    }
    public class ReceiveMessagContactModel
    {
        public ReceiveMessagProfileModel? Profile { get; set; }
        public string? Wa_id { get; set; }
    }
    public class ReceiveMessagProfileModel
    {
        public string? Name { get; set; }
    }
    public class ReceiveContactAddressesModel
    {
        public string? street { get; set; }
        public string? city { get; set; }
        public string? state { get; set; }
        public string? zip { get; set; }
        public string? country { get; set; }
        public int? country_code { get; set; }
        public string? type { get; set; }
    }
    public class ReceiveContactEmailsModel
    {
        public string? email { get; set; }
        public string? type { get; set; }
    }
    public class ReceiveContactImsModel
    {
        public string? service { get; set; }
        public string? user_id { get; set; }
    }
    public class ReceiveContactNameModel
    {
        public string? first_name { get; set; }
        public string? middle_name { get; set; }
        public string? last_name { get; set; }
        public string? formatted_name { get; set; }
        public string? name_suffix { get; set; }
        public string? name_prefix { get; set; }
    }
    public class ReceiveContactOrgModel
    {
        public string? company { get; set; }
        public string? department { get; set; }
        public string? title { get; set; }
    }
    public class ReceiveContactPhonesModel
    {
        public string? phone { get; set; }
        public string? wa_id { get; set; }
        public string? type { get; set; }
    }
    public class ReceiveContactUrlsModel
    {
        public string? url { get; set; }
        public string? type { get; set; }
    }
    public class ReceiveMessagMetaDataModel
    {
        public string? Display_phone_number { get; set; }
        public string? Phone_number_id { get; set; }
    }
    public class MarkasReadMessageMode
    {
        public string? messaging_product { get; set; } = "whatsapp";
        public string? status { get; set; } = "read";
        public string? message_id { get; set; }
    }
    public class SendModel
    {
        public string messaging_product { get; set; } = "whatsapp";
        public string recipient_type { get; set; } = "individual";
        public string? type { get; set; }
        public string? to { get; set; }
    }
    public class SendTextMessageModel : SendModel
    {
        public SendTextMessageMessageModel? text { get; set; }
    }
    public class SendTextMessageMessageModel
    {
        public bool preview_url { get; set; }
        public string? body { get; set; }
    }

    public class SendTemplateModel : SendModel
    {
        public TemplateModel template { get; set; } = new();
        public object? buttons { get; set; } = new();
    }



    public class SendMediaModel : SendModel
    {
        public SendMediaImageModel? image { get; set; }
        public SendMediaAudioModel? audio { get; set; }
        public SendMediaVideoModel? video { get; set; }
        public SendMediaDocumentModel? document { get; set; }
        public List<SendMediaContactsModel>? contacts { get; set; }
        public SendMediaLocationModel? location { get; set; }
        public SendMediaStickerModel? sticker { get; set; }
    }
    public class SendMediaImageModel
    {
        public string? link { get; set; }
        public string? id { get; set; }
        public string? caption { get; set; }
    }

    public class SendInteractiveMediaModel
    {
        public string? link { get; set; }
        public string? id { get; set; }
    }

    public class SendInteractiveDocumentModel
    {
        public string? link { get; set; }
        public string? id { get; set; }
        public string? filename { get; set; }
    }


    public class SendMediaAudioModel
    {
        public string? link { get; set; }
        public string? id { get; set; }
    }
    public class SendMediaVideoModel
    {
        public string? link { get; set; }
        public string? id { get; set; }
        public string? caption { get; set; }
    }
    public class SendMediaDocumentModel
    {
        public string? link { get; set; }
        public string? id { get; set; }
        public string? caption { get; set; }
        public string? filename { get; set; }
    }
    public class SendMediaStickerModel
    {
        public string? link { get; set; }
        public string? id { get; set; }
    }
    public class SendMediaLocationModel
    {
        public double? latitude { get; set; }
        public double? longitude { get; set; }
        public string? name { get; set; }
        public string? address { get; set; }
    }
    public class SendMediaContactsModel
    {
        // public SendContactsAddressesModel? addresses { get; set; }
        // public SendContactsEmailsModel? emails { get; set; }
        public SendContactsNameModel? name { get; set; }
        //  public SendContactsOrgModel? org { get; set; }
        public List<SendContactsPhonesModel>? phones { get; set; }
        //  public SendContactsUrlsModel? urls { get; set; }
        // public string? birthday { get; set; }
    }
    public class SendContactsAddressesModel
    {
        public string? street { get; set; }
        public string? city { get; set; }
        public string? state { get; set; }
        public string? zip { get; set; }
        public string? country { get; set; }
        public string? country_code { get; set; }
        public string? type { get; set; }
    }
    public class SendContactsEmailsModel
    {
        public string? email { get; set; }
        public string? type { get; set; }
    }
    public class SendContactsNameModel
    {
        public string? formatted_name { get; set; }
        public string? first_name { get; set; }
        //public string? middle_name { get; set; }
        //public string? last_name { get; set; }
        //public string? prefix { get; set; }
        //public string? suffix { get; set; }
    }
    public class SendContactsOrgModel
    {
        public string? company { get; set; }
        public string? department { get; set; }
        public string? title { get; set; }
    }
    public class SendContactsPhonesModel
    {
        public string? phone { get; set; }
        //public string? type { get; set; }
        //public string? wa_id { get; set; }
    }
    public class SendContactsUrlsModel
    {
        public string? url { get; set; }
        public string? type { get; set; }
    }

    public class ReceiveInteractiveModel
    {
        public string? type { get; set; }
        public ReceiveInteractiveListModel? list_reply { get; set; }
        public ReceiveInteractiveButtonModel? button_reply { get; set; }
        public ReceiveInteractiveNfmReplyModel? nfm_reply { get; set; }

    }
    public class ReceiveInteractiveListModel
    {
        public string? Id { get; set; }
        public string? title { get; set; }
        public string? description { get; set; }
    }
    public class ReceiveInteractiveButtonModel
    {
        public string? Id { get; set; }
        public string? title { get; set; }
    }

    public class SendGetAddressModel : SendModel
    {
        public SendGetAddressInteractiveModel? interactive { get; set; }
    }

    public class SendGetAddressInteractiveModel
    {
        public string? type { get; set; } = "address_message";
        public SendGetAddressBodyModel body { get; set; }
        public SendGetAddressActionModel action { get; set; }
    }

    public class SendGetAddressBodyModel
    {
        public string? text { get; set; }
    }

    public class SendGetAddressActionModel
    {
        public string name { get; set; } = "address_message";
        public SendGetAddressParameterModel parameters { get; set; }
    }

    public class SendGetAddressParameterModel
    {
        public string country { get; set; } = "IN";
        public SendGetAddressValueModel values { get; set; }
    }

    public class SendGetAddressValueModel
    {
        public string? name { get; set; }
        public string? phone_number { get; set; }
    }


    public class SendGetLocationModel : SendModel
    {
        public SendGetLocationInteractiveModel? interactive { get; set; }
    }

    public class SendGetLocationInteractiveModel
    {
        public string type { get; set; } = "location_request_message";
        public SendGetLocationBodyModel body { get; set; }
        public SendGetLocationActionModel action { get; set; }
    }

    public class SendGetLocationBodyModel
    {
        public string? type { get; set; } = "text";
        public string? text { get; set; }
    }

    public class SendGetLocationActionModel
    {
        public string name { get; set; } = "send_location";
    }


    #region Address

    public class ReceiveInteractiveNfmReplyModel
    {
        public ReceiveInteractiveResponseModel? response_json { get; set; }
        public string? body { get; set; }
        public string? name { get; set; }
    }

    public class ReceiveInteractiveResponseModel
    {
        public ReceiveInteractiveValuesModel? values { get; set; }
    }
    public class ReceiveInteractiveValuesModel
    {
        public string? in_pin_code { get; set; }
        public string? landmark_area { get; set; }
        public string? address { get; set; }
        public string? city { get; set; }
        public string? name { get; set; }
        public string? phone_number { get; set; }
        public string? state { get; set; }
        public string? house_number { get; set; }
        public string? floor_number { get; set; }
        public string? tower_number { get; set; }
        public string? building_name { get; set; }
    }

    #endregion

}



