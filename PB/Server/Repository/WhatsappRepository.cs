using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using NPOI.Util;
using NPOI.XWPF.UserModel;
using Org.BouncyCastle.Cms;
using PB.Client;
using PB.Client.Pages;
using PB.Client.Pages.CRM;
using PB.Client.Pages.Whatsapp;
using PB.Client.Shared;
using PB.Shared.Models;
using PB.DatabaseFramework;
using PB.Model;
using PB.Shared.Enum;
using PB.Shared.Tables;
using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using static NPOI.HSSF.Util.HSSFColor;
using PB.Shared.Tables.CRM;
using PB.CRM.Model.Enum;
using PB.Shared.Models.Spin;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using AutoMapper;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using PB.Shared.Models.WhatsaApp;
using System.Text.RegularExpressions;
using PB.Shared.Enum.WhatsApp;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using PB.Shared.Tables.Whatsapp;
using NPOI.POIFS.Crypt.Dsig;
using NPOI.HSSF.UserModel;
using PB.Shared;
using PB.Shared.Helpers;
using PB.Client.Pages.SuperAdmin;
using PB.Shared.Models.WhatsAppModels;
using Microsoft.AspNetCore.Components.Routing;
using System.Security.Policy;
using PB.EntityFramework;
using PB.Shared.Tables.Common;

namespace PB.Server.Repository
{
    public interface IWhatsappRepository
    {
        Task<SendMessageReposnseModel> SendChatReInitialisationMessage(int whatsappAccountID, string toNumber, int sessionId);
        Task ReceiveMessage(ReceiveMessageModel model, string? reqBody);
        Task<SessionResponse> UploadMediaToFb(int whatsappId, IFormFile file);
        Task<string> UploadSessionMedia(int whatsappId, IFormFile file);

        Task MarkMessageasRead(int whatsappAccountID, string incomingMessageId);
        Task<int> SendTextMessage(int whatsappAccountID, int contactId, string message);
        Task<int> SendImageMessage(int whatsappAccountID, int contactId, string message, string link, int? mediaID);
        Task<int> SendImageMessageByMediaID(int whatsappAccountID, int contactId, string message, string wmediId);
        Task<int> SendAudioMessage(int whatsappAccountID, int contactId, string message, string link, int? mediaID);
        Task<int> SendAudioMessageByMediaID(int whatsappAccountID, int contactId, string wmediId);
        Task<int> SendDocumentMessage(int whatsappAccountID, int contactId, string message, string link, int? mediaID);
        Task<int> SendDocumentMessageByMediaID(int whatsappAccountID, int contactId, string message, string wmediId, string documentName);
        Task<int> SendStickerMessageByMediaID(int whatsappAccountID, int contactId, string wmediId);
        Task SendTemplateMessage(int whatsappAccountID, int contactId, string templateName, string lang, object data, string parameter, int? recipientId);
        Task<WhatsappContact> GetContact(string phone, int clientId, string? name = "", int importBatchID = 0);
        Task<MediaDetailsModel> GetMediaURL(int whatsappAccountID, string wMediaId);
        Task<List<ChatbotSubmitAssigneeModel>> GetChatbotSumbitAssignees(int replyID, int clientID);
        Task StopSupportChat(int contactId);

        Task<int?> GetContactEntityID(int contactId, int? clientID);
        Task<string> GetEnquiryDescription(int? sessionID);
        Task<int> GetTagID(string? tag, int clientID);
        Task<WhatsappChatHistoryItemModel> GetLastChatHistoryItem(int chatId);

        #region Template

        Task<int> SaveWhatsappTemplate(WhatsappTemplatePageModel templateModel, IDbTransaction? tran = null);
        Task<WhatsappTemplateTableModel> GetTemplate(int templateID, IDbTransaction? tran = null);
        Task<bool> DeleteTemplate(int templateID, IDbTransaction? tran = null);
        Task<TemplateDetailsModel> GetTemplateDetailsForBroadcast(int templateID);
        Task<TemplateVariableListModel?> GetTemplateVariableList(int templateID);
        Task<byte[]?> GetTemplateVariableExcelData(int TempalteID, IDbTransaction? tran = null);

        #region Template Variable

        Task<int> SaveTemplateVariable(TemplateVariablePageModel variableModel, IDbTransaction? tran = null);
        Task<List<WhatsappTemplateVariableModel>?> GetTemplateVariables(int templateID, IDbTransaction? tran = null);

        #endregion

        #region Template Button

        Task<int> SaveTemplateButton(TemplateButtonPageModel buttonModel, IDbTransaction? tran = null);
        Task<List<WhatsappTemplateButtonModel>?> GetTemplateButtons(int templateID, IDbTransaction? tran = null);

        #endregion

        #region Template Cloud Api
        Task<CreateTemplateResultModel?> CreateTemplateWACloudPostRequest(int whatsappId, object content, IDbTransaction? tran = null);
        Task<EditTemplateResponseModel> EditTemplateWACloudPostRequest(int whatsappId, string templateID, object content, IDbTransaction? tran = null);
        Task<bool> DeleteTemplateWACloudRequest(int templateID, IDbTransaction? tran = null);
        Task<CreateTemplateResultModel> GenerateCloudAPICreateTemplatePostRequest(WhatsappTemplatePageModel templateModel, IDbTransaction? tran = null);
        Task<EditTemplateResponseModel> GenerateCloudAPIEditTemplatePostRequest(WhatsappTemplatePageModel templateModel, IDbTransaction? tran = null);
        Task SyncTemplatesWithFB(int clientID);

        #endregion

        #endregion

        #region Broadcast
        Task<string?> GetBroadcastReceipientExcelData(MessageReceipientSearchModel model);
        Task<WhatsappBroadcastSaveResultModel> SaveBroadcastMessage(CreateBroadcastModel createBroadcastModel, IDbTransaction? tran = null);
        Task<WhatsappBroadcastSaveResultModel?> RecreateBroadcastMessage(RecreatBroadcastModel recreateBroadcastModel, IDbTransaction? tran = null);

        #endregion

    }

    public class WhatsappRepository : IWhatsappRepository
    {
        private readonly IDbContext _dbContext;
        private readonly INotificationRepository _notification;
        private readonly IConfiguration _config;
        private readonly IMediaRepository _media;
        private readonly IHostEnvironment _env;
        private readonly IMapper _mapper;
        private readonly ICommonRepository _common;

        public WhatsappRepository(IDbContext dbContext, INotificationRepository notification, IConfiguration config, IMediaRepository media, IHostEnvironment env, IMapper mapper, ICommonRepository common)
        {
            this._dbContext = dbContext;
            this._notification = notification;
            this._config = config;
            this._media = media;
            this._env = env;
            this._mapper = mapper;
            this._common = common;
        }

        #region Message
        public async Task<SendMessageReposnseModel> SendChatReInitialisationMessage(int whatsappAccountID, string toNumber, int sessionId)
        {
            InteractiveButtonMessageModel msg = new()
            {
                type = "button"
            };
            msg.body = new InteractiveBodyModel()
            {
                text = "I haven't heard from you for a while. Do you want to continue the chat?"
            };

            msg.action.buttons.Add(new InteractiveButtonModel()
            {
                reply = new InteractiveButtonReplyModel()
                {
                    id = "re_" + sessionId.ToString(),
                    title = "Continue",
                }
            });

            return await SendInteractiveButtonMessage(whatsappAccountID, toNumber, msg);
        }



        public async Task<int> SendTextMessage(int whatsappAccountID, int contactId, string message)
        {
            var toNumber = await GetContactNumber(contactId);
            var messageRes = await SendText(whatsappAccountID, toNumber, message);
            return await SaveChat(whatsappAccountID, contactId, message, messageRes, "text", true);
        }

        public async Task<int> SendImageMessage(int whatsappAccountID, int contactId, string message, string link, int? mediaID)
        {
            var toNumber = await GetContactNumber(contactId);
            var messageRes = await SendImageByUrl(whatsappAccountID, toNumber, link, message);
            return await SaveChat(whatsappAccountID, contactId, message, messageRes, "image", true, mediaID: mediaID);
        }

        public async Task<int> SendImageMessageByMediaID(int whatsappAccountID, int contactId, string message, string wmediId)
        {
            var toNumber = await GetContactNumber(contactId);
            var messageRes = await SendImageById(whatsappAccountID, toNumber, wmediId, message);
            var chatId = await SaveChat(whatsappAccountID, contactId, message, messageRes, "image", true);

            var image = new WhatsappChatMedia()
            {
                ChatID = chatId,
                WMediaID = wmediId,
            };
            await _dbContext.SaveAsync(image);
            return chatId;
        }

        public async Task<int> SendAudioMessage(int whatsappAccountID, int contactId, string message, string link, int? mediaID)
        {
            var toNumber = await GetContactNumber(contactId);
            var messageRes = await SendAudioByUrl(whatsappAccountID, toNumber, link, message);
            return await SaveChat(whatsappAccountID, contactId, message, messageRes, "audio", true, mediaID: mediaID);
        }

        public async Task<int> SendAudioMessageByMediaID(int whatsappAccountID, int contactId, string wmediId)
        {
            var toNumber = await GetContactNumber(contactId);
            var messageRes = await SendAudioById(whatsappAccountID, toNumber, wmediId);
            var chatId = await SaveChat(whatsappAccountID, contactId, "", messageRes, "audio", true);

            var image = new WhatsappChatMedia()
            {
                ChatID = chatId,
                WMediaID = wmediId,
            };
            await _dbContext.SaveAsync(image);
            return chatId;
        }

        public async Task<int> SendDocumentMessage(int whatsappAccountID, int contactId, string message, string link, int? mediaID)
        {
            var toNumber = await GetContactNumber(contactId);
            var messageRes = await SendDocumentByUrl(whatsappAccountID, toNumber, link, message);
            return await SaveChat(whatsappAccountID, contactId, message, messageRes, "document", true, mediaID: mediaID);
        }

        public async Task<int> SendDocumentMessageByMediaID(int whatsappAccountID, int contactId, string message, string wmediId, string documentName)
        {
            var toNumber = await GetContactNumber(contactId);
            var messageRes = await SendDocumentById(whatsappAccountID, toNumber, message, wmediId, documentName);
            var chatId = await SaveChat(whatsappAccountID, contactId, message, messageRes, "document", true);
            var image = new WhatsappChatMedia()
            {
                ChatID = chatId,
                WMediaID = wmediId,
                DocumentName = documentName
            };
            await _dbContext.SaveAsync(image);
            return chatId;
        }

        public async Task<int> SendStickerMessageByMediaID(int whatsappAccountID, int contactId, string wmediId)
        {
            var toNumber = await GetContactNumber(contactId);
            var messageRes = await SendStickerById(whatsappAccountID, toNumber, wmediId);
            var chatId = await SaveChat(whatsappAccountID, contactId, "", messageRes, "sticker", true);
            var image = new WhatsappChatMedia()
            {
                ChatID = chatId,
                WMediaID = wmediId,
            };
            await _dbContext.SaveAsync(image);
            return chatId;
        }

        public async Task SendTemplateMessage(int whatsappAccountID, int contactId, string templateName, string lang, object data, string parameter, int? recipientId)
        {
            var toNumber = await GetContactNumber(contactId);
            var messageRes = await SendTemplate(whatsappAccountID, toNumber, templateName, lang, data);
            await SaveChat(whatsappAccountID, contactId, parameter, messageRes, "template", false, recipientId);
        }

        public async Task<MediaDetailsModel> GetMediaURL(int whatsappAccountID, string wMediaId)
        {
            var tempUrl = await GetTempURL<MediaDetailsModel>(whatsappAccountID, wMediaId);
            if (tempUrl != null)
            {
                tempUrl.File = await GetMediaFileFromFB(whatsappAccountID, tempUrl.Url, tempUrl.Mime_type);
                //string FileName = System.IO.Path.GetRandomFileName();
                //string base64String = Convert.ToBase64String(tempUrl.File);
                //FileUploadModel fileUpload = new()
                //{
                //    FileName = FileName,
                //    ContentType = tempUrl.Mime_type,
                //    FolderName = "incoming-media",
                //    Base64Image = base64String,
                //    Content = tempUrl.File
                //};
                //switch (tempUrl.Mime_type)
                //{
                //    case "image/png":
                //        fileUpload.Extension = "png";
                //        break;
                //    case "image/jpeg":
                //        fileUpload.Extension = "jpeg";
                //        break;
                //    case "image/jpg":
                //        fileUpload.Extension = "jpg";
                //        break;
                //    case "audio/mpeg":
                //        fileUpload.Extension = "mpeg";
                //        break;
                //    case "audio/mp4":
                //        fileUpload.Extension = "mp4";
                //        break;
                //    case "audio/ogg":
                //        fileUpload.Extension = "ogg";
                //        break;
                //    case "audio/acc":
                //        fileUpload.Extension = "acc";
                //        break;
                //    case "audio/amr":
                //        fileUpload.Extension = "amr";
                //        break;
                //    //case "application/pdf":
                //    //    fileUpload.Extension = "pdf";
                //    //    break;
                //    case "vedio/mp4":
                //        fileUpload.Extension = "mp4";
                //        break;
                //    case "vedio/3gpp":
                //        fileUpload.Extension = "3gpp";
                //        break;
                //    case "application/xls":
                //        fileUpload.Extension = "xls";
                //        break;
                //    case "application/xlsx":
                //        fileUpload.Extension = "xlsx";
                //        break;
                //    case "application/doc":
                //        fileUpload.Extension = "doc";
                //        break;
                //    case "application/docx":
                //        fileUpload.Extension = "docx";
                //        break;
                //    case "application/ppt":
                //        fileUpload.Extension = "ppt";
                //        break;
                //    case "application/pptx":
                //        fileUpload.Extension = "pptx";
                //        break;
                //    case "image/webp":
                //        fileUpload.Extension = "webp";
                //        break;
                //    default:
                //        break;
                //}
                //var media = await _media.SaveMedia(fileUpload);
                //await _dbContext.ExecuteAsync($"Update WhatsappChatMedia set MediaID={media.MediaID} where WMediaID='{wMediaId}'");

                //tempUrl.Url = await _dbContext.GetFieldsAsync<Media, string>("FileName", $"MediaID={media.MediaID}");
            }
            return tempUrl;
        }

        public async Task<List<ChatbotSubmitAssigneeModel>> GetChatbotSumbitAssignees(int replyID, int clientID)
        {
            return await _dbContext.GetListByQueryAsync<ChatbotSubmitAssigneeModel>($@"Select A.AssigneeID,E.EntityID,E.Name,Case When A.AssigneeID is null then 0 else 1 end as IsAssigned
                        From Users U 
                        JOIN viEntity E ON E.EntityID=U.EntityID
                        LEFT JOIN ChatbotSubmitAssignee A on A.EntityID=E.EntityID and ReplyID={replyID} and A.IsDeleted=0
                        Where U.IsDeleted=0 and U.ClientID={clientID} and U.UserTypeID={(int)UserTypes.Staff}", null);
        }

        public async Task StopSupportChat(int contactId)
        {
            await _dbContext.ExecuteAsync($"Update WhatsappChatSession Set IsFinished=1 Where ContactID={contactId} and HasSupportChat=1 and IsFinished=0");
        }

        #endregion

        #region Webhook
        public async Task ReceiveMessage(ReceiveMessageModel model, string? reqBody)
        {
            try
            {
                WebhookIncoming webhook = new WebhookIncoming();
                if (Convert.ToBoolean(_config["NeedWebhookLog"]))
                {
                    webhook.AddedOn = System.DateTime.UtcNow;
                    webhook.Message = reqBody;
                    webhook.ID = await _dbContext.SaveAsync(webhook);
                }

                #region Template Satus Update

                if (model.Entry[0].Changes[0].Field == "message_template_status_update")
                {
                    var templateId = model.Entry[0].Changes[0].Value.message_template_id;
                    var status = model.Entry[0].Changes[0].Value.Event;
                    var reason = model.Entry[0].Changes[0].Value.reason;
                    int? statusID = status switch
                    {
                        "APPROVED" => (int)WhatsappTemplateStatus.APPROVED,
                        "REJECTED" => (int)WhatsappTemplateStatus.REJECTED,
                        "PENDING" => (int)WhatsappTemplateStatus.PENDING,
                        "PAUSED" => (int)WhatsappTemplateStatus.PAUSED,
                        "PENDING_DELETION" => (int)WhatsappTemplateStatus.PENDING_DELETION,
                        _ => null,
                    };
                    await _dbContext.ExecuteAsync("Update WhatsappTemplate Set StatusID=@statusID,StatusReason=@reason Where WhatsappTemplateID=@templateId", new { statusID, reason, templateId });
                    return;
                }

                #endregion


                var phoneNumberId = model.Entry[0].Changes[0].Value.Metadata.Phone_number_id;
                var whatsappAccount = await GetWhatsappAccount(phoneNumberId);

                if (whatsappAccount != null )
                {
                    WhatsappContact contact = null;

                    if (model.Entry[0].Changes[0].Value.Messages != null)
                    {
                        if((whatsappAccount.IsChatbotEnabled.HasValue && whatsappAccount.IsChatbotEnabled.Value) || whatsappAccount.IsChatbotEnabled is null)
                        {
                            contact = await GetContact(model.Entry[0].Changes[0].Value.Contacts[0].Wa_id, whatsappAccount.ClientID.Value, model.Entry[0].Changes[0].Value.Contacts[0].Profile.Name);
                            await HandleIncomingMessage(whatsappAccount, model.Entry[0].Changes[0].Value.Messages[0], contact);
                        }
                        webhook.MessageType = 1;
                    }
                    else if (model.Entry[0].Changes[0].Value.Statuses != null)
                    {
                        contact = await GetContact(model.Entry[0].Changes[0].Value.Statuses[0].Recipient_id, whatsappAccount.ClientID.Value);
                        await UpdateMessageStatus(whatsappAccount, model.Entry[0].Changes[0].Value.Statuses, contact.ContactID);
                        webhook.MessageType = 2;
                    }

                    if (Convert.ToBoolean(_config["NeedWebhookLog"]))
                    {
                        webhook.ContactID = contact.ContactID;
                        webhook.WhatsappAccountID = whatsappAccount.WhatsappAccountID;
                        await _dbContext.SaveAsync(webhook,needLog:false);
                    }
                }
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        private DateTime? GetTime(string? timeStamp)
        {
            if (timeStamp != null)
                //return new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(Math.Round(Convert.ToDouble(timeStamp) / 1000d)).ToLocalTime();
                return new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(Math.Round(Convert.ToDouble(timeStamp)));
            else
                return null;
        }

        private async Task UpdateMessageStatus(WhatsappAccount whatsappAccount, List<MessageStatusStatusesModel> statuses, int contactId)
        {
            foreach (var status in statuses)
            {
                var messageStatusID = GetMessageStatusID(status.Status);
                int? conversationID = null;

                if (status.Conversation != null)
                {
                    conversationID = await _dbContext.GetByQueryAsync<int>($"Select ConversationID from WhatsappConversation Where WCID=@WCID", new { WCID = status.Conversation.Id });

                    if (conversationID == 0)
                    {
                        WhatsappConversation conversation = new()
                        {
                            Date = GetTime(status.Timestamp),
                            IsBillable = status.Pricing.Billable,
                            IsUserInitiated = status.Pricing.IsUserInitiated,
                            PricingModel = status.Pricing.pricing_model,
                            WhatsappAccountID = whatsappAccount.WhatsappAccountID,
                            WCID = status.Conversation.Id,
                            ContactID = contactId
                        };
                        conversationID = await _dbContext.SaveAsync(conversation);
                    }
                }

                WhatsappChat chat = await _dbContext.GetAsync<WhatsappChat>("MessageID= @MessageID", new { MessageID = status.Id });
                if (chat != null)
                {
                    chat.MessageStatus = messageStatusID;
                    chat.ConversationID = conversationID;

                    switch ((MessageStatus)messageStatusID)
                    {
                        case MessageStatus.Delivered:
                            chat.DeliveredOn = GetTime(status.Timestamp);
                            break;
                        case MessageStatus.Read:
                            chat.SeenOn = GetTime(status.Timestamp);
                            break;
                        case MessageStatus.Sent:
                            chat.AddedOn = GetTime(status.Timestamp);
                            break;
                        case MessageStatus.Failed:
                            chat.FailedOn = GetTime(status.Timestamp);
                            chat.FailedReason = status.Errors[0].error_data.details;
                            break;
                    }
                    await _dbContext.SaveAsync(chat);


                    var chatUpdateResponse = new WhatsappChatStatusUpdateModel()
                    {
                        ChatID = chat.ChatID,
                        ContactID = chat.ContactID.Value,
                        StatusID = (int)messageStatusID
                    };

                    var entitIds = await GetClientEntityIds(whatsappAccount.WhatsappAccountID);
                    foreach (var entityId in entitIds)
                    {
                        await _notification.SendSignalRPush(entityId, chatUpdateResponse, null, "status_update");
                    }
                }
            }
        }

        private async Task HandleIncomingMessage(WhatsappAccount whatsappAccount, ReceiveMessageMessageModel message, WhatsappContact contact)
        {
            var messageTypeID = GetMessageTypeID(message.Type);
            string messageText = "", msg = "";
            string buttonReplyId = "";
            ReceiveInteractiveValuesModel AddressModel = new();

            switch ((MessageTypes)messageTypeID)
            {
                case MessageTypes.Text:
                    msg = messageText = message.Text.Body;
                    break;
                case MessageTypes.Interactive:
                    switch (message.Interactive.type)
                    {
                        case "nfm_reply":
                            messageTypeID = (int)MessageTypes.Address;
                            AddressModel = message.Interactive.nfm_reply.response_json.values;
                            messageText = message.Interactive.nfm_reply.body;
                            break;
                        case "button_reply":
                            msg = messageText = message.Interactive.button_reply.title;
                            buttonReplyId = message.Interactive.button_reply.Id;

                            break;
                        default:
                            msg = messageText = message.Interactive.list_reply.title;
                            buttonReplyId = message.Interactive.list_reply.Id;

                            break;
                    }

                    if (buttonReplyId.StartsWith("re_"))//Chat Reinitialisation
                    {
                        var oldsessionId = Convert.ToInt32(buttonReplyId.Replace("re_", ""));
                        await _dbContext.ExecuteAsync($"Update WhatsappChatSession Set IsFinished=0,LastMessageOn=@Date,ReminderSent=0 Where SessionID={oldsessionId}", new { Date = DateTime.UtcNow });
                        return;
                    }

                    break;

            }

            int firstIncomingChatId = await _dbContext.GetByQueryAsync<int>($"Select Top(1) ChatID from WhatsappChat Where ContactID=@ContactID and IsIncoming=1 and WhatsappAccountID=@WhatsappAccountID and AddedOn>DateADD(Day,-1,GETUTCDATE())", new { ContactID = contact.ContactID , WhatsappAccountID = whatsappAccount.WhatsappAccountID });


            #region Save Chat

            WhatsappChat chat = new()
            {
                ContactID = contact.ContactID,
                AddedOn = DateTime.UtcNow,
                IsIncoming = true,
                Message = messageText,
                MessageID = message.Id,
                MessageTypeID = messageTypeID,
                WhatsappAccountID = whatsappAccount.WhatsappAccountID,
            };
            chat.ChatID = await _dbContext.SaveAsync(chat, needLog: false);


            ///Refer : https://developers.facebook.com/docs/whatsapp/on-premises/webhooks/inbound/
            switch ((MessageTypes)messageTypeID)
            {
                case MessageTypes.Image:
                    var image = new WhatsappChatMedia()
                    {
                        ChatID = chat.ChatID,
                        Caption = message.image.caption,
                        MimeType = message.image.mime_type,
                        Sha256 = message.image.sha256,
                        WMediaID = message.image.id,
                        WMediaURL = message.image.file
                    };
                    await _dbContext.SaveAsync(image, needLog: false);
                    break;

                case MessageTypes.Audio:
                    var audio = new WhatsappChatMedia()
                    {
                        ChatID = chat.ChatID,
                        Caption = message.audio.caption,
                        MimeType = message.audio.mime_type,
                        Sha256 = message.audio.sha256,
                        WMediaID = message.audio.id,
                        WMediaURL = message.audio.file,
                        IsVoice = message.audio.voice
                    };
                    await _dbContext.SaveAsync(audio, needLog: false);
                    break;
                case MessageTypes.Sticker:
                    var sticker = new WhatsappChatMedia()
                    {
                        ChatID = chat.ChatID,
                        MimeType = message.sticker.mime_type,
                        Sha256 = message.sticker.sha256,
                        WMediaID = message.sticker.id,
                        IsAnimated = message.sticker.animated
                    };
                    await _dbContext.SaveAsync(sticker, needLog: false);
                    break;
                case MessageTypes.Voice:
                    var voice = new WhatsappChatMedia()
                    {
                        ChatID = chat.ChatID,
                        Caption = message.voice.caption,
                        MimeType = message.voice.mime_type,
                        Sha256 = message.voice.sha256,
                        WMediaID = message.voice.id,
                        WMediaURL = message.voice.file,
                        IsVoice = message.audio.voice
                    };
                    await _dbContext.SaveAsync(voice, needLog: false);
                    break;
                case MessageTypes.Video:
                    var video = new WhatsappChatMedia()
                    {
                        ChatID = chat.ChatID,
                        Caption = message.video.caption,
                        MimeType = message.video.mime_type,
                        Sha256 = message.video.sha256,
                        WMediaID = message.video.id,
                        WMediaURL = message.video.file
                    };
                    await _dbContext.SaveAsync(video, needLog: false);
                    break;
                case MessageTypes.Document:
                    var document = new WhatsappChatMedia()
                    {
                        ChatID = chat.ChatID,
                        Caption = message.document.caption,
                        MimeType = message.document.mime_type,
                        Sha256 = message.document.sha256,
                        WMediaID = message.document.id,
                        WMediaURL = message.document.file
                    };
                    await _dbContext.SaveAsync(document, needLog: false);
                    break;
                case MessageTypes.Contacts:
                    foreach (var c in message.contacts)
                    {
                        var chatContact = new WhatsappChatContact()
                        {
                            ChatID = chat.ChatID,
                            FirstName = c.name.first_name,
                            LastName = c.name.last_name,
                            FormattedName = c.name.formatted_name,
                        };
                        chatContact.WhatsappChatContactID = await _dbContext.SaveAsync(chatContact, needLog: false);

                        List<WhatsappChatContactPhone> phones = new();
                        foreach (var ph in c.phones)
                        {
                            phones.Add(new WhatsappChatContactPhone()
                            {
                                Phone = ph.phone,
                                WAID = ph.wa_id,
                                Type = ph.type,
                                WhatsappChatContactID = chatContact.WhatsappChatContactID
                            });
                        }
                        if (phones != null && phones.Count > 0)
                        {
                            await _dbContext.SaveListAsync(phones, needToDelete: false);
                        }
                    }
                    break;
                case MessageTypes.Location:
                    if (message.location != null)
                    {
                        message.location.ChatID = chat.ChatID;
                        await _dbContext.SaveAsync(message.location, needLog: false);
                    }
                    break;

                case MessageTypes.Address:
                    var address = new WhatsappChatAddress()
                    {
                        ChatID = chat.ChatID,
                        Name = AddressModel.name,
                        Phone = AddressModel.phone_number,
                        Address = AddressModel.address,
                        City = AddressModel.city,
                        State = AddressModel.state,
                        PinCode = AddressModel.in_pin_code,
                        LandmarkArea = AddressModel.landmark_area,
                        HouseNo = AddressModel.house_number,
                        FloorNo = AddressModel.floor_number,
                        TowerNo = AddressModel.tower_number,
                        BuildingName = AddressModel.building_name,

                    };
                    await _dbContext.SaveAsync(address, needLog: false);
                    break;
            }

            #endregion

            #region Bot Reply

            var session = await GetSession(whatsappAccount.WhatsappAccountID, contact.ContactID);

            int sessionId = 0;
            bool isFirstChat = false;

            if (session == null)
            {
                session = new WhatsappChatSession() { ContactID = contact.ContactID, IsFinished = false, LastMessageOn = DateTime.UtcNow, WhatsappAccountID = whatsappAccount.WhatsappAccountID };
                sessionId = session.SessionID = await _dbContext.SaveAsync(session, needLog: false);
                isFirstChat = true;
            }
            else
                sessionId = session.SessionID;

            int nextChatbotReplyId = 0;
            bool? enquiryConfirmationStatus = null;
            if (!string.IsNullOrEmpty(buttonReplyId))
            {
                if (buttonReplyId.StartsWith("f"))
                    nextChatbotReplyId = Convert.ToInt32(buttonReplyId.Trim('f'));
                else if (buttonReplyId.StartsWith("b"))
                {
                    nextChatbotReplyId = Convert.ToInt32(buttonReplyId.Trim('b'));
                }
                else if (buttonReplyId.StartsWith("enq_c"))
                {
                    nextChatbotReplyId = Convert.ToInt32(buttonReplyId.Replace("enq_c", ""));
                    enquiryConfirmationStatus = true;
                }
                else if (buttonReplyId.StartsWith("enq_e"))
                {
                    nextChatbotReplyId = Convert.ToInt32(buttonReplyId.Replace("enq_e", ""));
                    enquiryConfirmationStatus = false;
                }

            }

            if (isFirstChat == true && nextChatbotReplyId == 0)//send first auto reply
            {
                string welcomeMessage = "";

                var firstReplyId = await _dbContext.GetByQueryAsync<int>($@"Select ReplyID from ChatbotReply Where @SearchString like '%'+LOWER(SearchKey)+'%' and SearchKey<>'' and WhatsappAccountID={whatsappAccount.WhatsappAccountID}", new { SearchString = messageText.ToLower() });
                if (firstReplyId == 0)
                    firstReplyId = await _dbContext.GetFieldsAsync<ChatbotReply, int>("ReplyID", $"WhatsappAccountID={whatsappAccount.WhatsappAccountID} and IsDeleted=0 and ParentReplyID is null", null);
                if (firstIncomingChatId == 0)//This is the first message after 24 hour
                {
                    var whatsappAcc = await _dbContext.GetAsync<WhatsappAccount>(whatsappAccount.WhatsappAccountID);
                    welcomeMessage = whatsappAcc.WelcomeMessage.Replace("@Name", contact.Name) + "\n";
                }

                if (firstReplyId != 0)//Send reply message if there is any reply bot
                {
                    await MarkMessageasRead(whatsappAccount.WhatsappAccountID, message.Id);

                    await SendAutoReplyMessage(whatsappAccount.WhatsappAccountID, contact, welcomeMessage, firstReplyId, sessionId, whatsappAccount.ClientID, chat.ChatID, msg ?? messageText, firstReplyId);
                }
                else
                {
                    //Send welcome message and notify the chat agent
                    await SendAutoReplyMessage(whatsappAccount.WhatsappAccountID, contact, welcomeMessage, null, null, whatsappAccount.ClientID, chat.ChatID, msg ?? messageText, 0);
                    await NotifyUser(whatsappAccount.WhatsappAccountID, messageText, contact.Name, chat.ChatID, chat.ContactID);
                }
            }
            else if (!session.HasSupportChat) //Check if there any existing support chat.No need of bot reply for such type. Else send botreply
            {
                await MarkMessageasRead(whatsappAccount.WhatsappAccountID, message.Id);


                if (nextChatbotReplyId != 0)
                {
                    var nextBotReply = await _dbContext.GetAsync<ChatbotReply>(nextChatbotReplyId);
                    if (nextBotReply != null && nextBotReply.ReplyTypeID == (int)ChatbotReplyTypes.GoToMain)
                    {
                        nextChatbotReplyId = await _dbContext.GetFieldsAsync<ChatbotReply, int>("ReplyID", $"WhatsappAccountID={whatsappAccount.WhatsappAccountID} and IsDeleted=0 and ParentReplyID is null", null);
                    }

                    await SendAutoReplyMessage(whatsappAccount.WhatsappAccountID, contact, "", nextChatbotReplyId, session.SessionID, whatsappAccount.ClientID, chat.ChatID, msg ?? messageText, nextChatbotReplyId, enquiryConfirmationStatus);
                }
                else
                {
                    var lastReplyChatID = await _dbContext.GetByQueryAsync<int?>(@$"Select Max(ChatID)
                    From WhatsappChat
                    Where SessionID={session.SessionID} and IsIncoming=0", null);

                    if (lastReplyChatID.HasValue)
                    {
                        var lastReply = await _dbContext.GetAsync<WhatsappChat>(lastReplyChatID.Value);

                        if (lastReply.ReplyID.HasValue)//last reply was auto reply
                        {
                            var lastChatbotReply = await _dbContext.GetAsync<ChatbotReply>(lastReply.ReplyID.Value);

                            switch (lastChatbotReply.ReplyTypeID)
                            {
                                case (int)ChatbotReplyTypes.Forward:
                                    if (!IsDigitsOnly(messageText))//Not a valid reply
                                        await SendAutoReplyMessage(whatsappAccount.WhatsappAccountID, contact, whatsappAccount.ChooseValidOptionLabel + "\n", lastReply.ReplyID, sessionID: session.SessionID, whatsappAccount.ClientID, chat.ChatID, msg ?? messageText, lastReply.ReplyID);
                                    else if (messageText == "0")//Go back
                                    {
                                        msg = "Back";
                                        var prevoiusChatbotReply = await _dbContext.GetAsync<ChatbotReply>(Convert.ToInt32(lastChatbotReply.ParentReplyID));
                                        if (prevoiusChatbotReply == null)
                                            await SendAutoReplyMessage(whatsappAccount.WhatsappAccountID, contact, whatsappAccount.ChooseValidOptionLabel + "\n", lastChatbotReply.ReplyID, session.SessionID, whatsappAccount.ClientID, chat.ChatID, msg ?? messageText, lastChatbotReply.ReplyID);
                                        else
                                            await SendAutoReplyMessage(whatsappAccount.WhatsappAccountID, contact, "", prevoiusChatbotReply.ReplyID, session.SessionID, whatsappAccount.ClientID, chat.ChatID, msg ?? messageText, prevoiusChatbotReply.ReplyID);
                                    }
                                    else
                                    {
                                        var nextChatbotReply = await _dbContext.GetByQueryAsync<ChatbotReply>($@"Select *
                                            from ChatbotReply
                                            Where ParentReplyID={lastChatbotReply.ReplyID} and Code=@Code and IsDeleted=0", new { Code = messageText });

                                        if (nextChatbotReply != null)
                                        {
                                            nextChatbotReplyId = nextChatbotReply.ReplyID;
                                            await SendAutoReplyMessage(whatsappAccount.WhatsappAccountID, contact, "", nextChatbotReplyId, session.SessionID, whatsappAccount.ClientID, chat.ChatID, nextChatbotReply.Short, nextChatbotReplyId);
                                        }
                                        else
                                        {
                                            await SendAutoReplyMessage(whatsappAccount.WhatsappAccountID, contact, whatsappAccount.ChooseValidOptionLabel + "\n", lastReply.ReplyID, session.SessionID, whatsappAccount.ClientID, chat.ChatID, messageText, lastReply.ReplyID);
                                        }
                                    }
                                    break;
                                case (int)ChatbotReplyTypes.ChatWithAgent:
                                    session.HasSupportChat = true;
                                    await _dbContext.SaveAsync(session, needLog: false);
                                    await SendAutoReplyMessage(whatsappAccount.WhatsappAccountID, contact, "Our executive will contact you shortly", replyId: lastReply.ReplyID, session.SessionID, whatsappAccount.ClientID, chat.ChatID, msg ?? messageText, lastReply.ReplyID);
                                    await NotifyUser(whatsappAccount.WhatsappAccountID, messageText, contact.Name, chat.ChatID, chat.ContactID);
                                    break;
                                case (int)ChatbotReplyTypes.Location:
                                case (int)ChatbotReplyTypes.Text:
                                case (int)ChatbotReplyTypes.List:
                                case (int)ChatbotReplyTypes.Address:
                                    var prevQuestion = await _dbContext.GetAsync<ChatbotReply>(lastReply.ReplyID.Value);
                                    var nextQuestion = await _dbContext.GetByQueryAsync<ChatbotReply>($@"Select Top(1) * 
                                                from ChatbotReply 
                                                where ParentReplyID={prevQuestion.ParentReplyID} and IsDeleted=0 and Code>{prevQuestion.Code}
                                                Order By Code", null);
                                    nextChatbotReplyId = lastReply.ReplyID.Value;
                                    await SendAutoReplyMessage(whatsappAccount.WhatsappAccountID, contact, "", nextQuestion.ReplyID, session.SessionID, whatsappAccount.ClientID, chat.ChatID, msg ?? messageText, nextChatbotReplyId);
                                    break;
                                case (int)ChatbotReplyTypes.ListOption:
                                    if (!IsDigitsOnly(messageText))//Not a valid reply
                                        await SendAutoReplyMessage(whatsappAccount.WhatsappAccountID, contact, whatsappAccount.ChooseValidOptionLabel + "\n", lastReply.ReplyID, session.SessionID, whatsappAccount.ClientID, chat.ChatID, msg ?? messageText, lastReply.ReplyID);
                                    else
                                    {
                                        nextChatbotReplyId = await _dbContext.GetByQueryAsync<int>($@"Select ReplyID
                                            from ChatbotReply
                                            Where ParentReplyID={lastChatbotReply.ReplyID} and Code=@Code and IsDeleted=0", new { Code = messageText });
                                        if (nextChatbotReplyId == 0)
                                            await SendAutoReplyMessage(whatsappAccount.WhatsappAccountID, contact, whatsappAccount.ChooseValidOptionLabel + "\n", lastReply.ReplyID, session.SessionID, whatsappAccount.ClientID, chat.ChatID, msg ?? messageText, lastReply.ReplyID);
                                        else
                                        {
                                            var pReplyID = await _dbContext.GetByQueryAsync<int>($"Select ParentReplyID From ChatbotReply Where ReplyID in (Select ParentReplyID From ChatbotReply Where ReplyID = {nextChatbotReplyId})",null);

                                            var nQuestion = await _dbContext.GetByQueryAsync<ChatbotReply>($@"Select * 
                                            from ChatbotReply 
                                            where ParentReplyID={pReplyID} and IsDeleted=0 and Code>{lastChatbotReply.Code}
                                            Order By Code", new { ReplyID = nextChatbotReplyId });

                                            if (nQuestion != null)
                                                await SendAutoReplyMessage(whatsappAccount.WhatsappAccountID, contact, "", nQuestion.ReplyID, session.SessionID, whatsappAccount.ClientID, chat.ChatID, msg ?? messageText, nextChatbotReplyId);
                                        }
                                    }
                                    break;
                                case (int)ChatbotReplyTypes.Purchase:

                                    break;
                            }
                        }
                    }
                    else
                    {
                        var firstReplyId = await _dbContext.GetFieldsAsync<ChatbotReply, int>("ReplyID", $"WhatsappAccountID={whatsappAccount.WhatsappAccountID} and IsDeleted=0 and ParentReplyID is null",null);
                        await SendAutoReplyMessage(whatsappAccount.WhatsappAccountID, contact, "Oops!!. Something went wrong. Can you please start from the begining.\n", firstReplyId, sessionId, whatsappAccount.ClientID, chat.ChatID, msg ?? messageText, firstReplyId);
                    }
                }


            }
            else if (session.HasSupportChat)
            {
                await NotifyUser(whatsappAccount.WhatsappAccountID, messageText, contact.Name, chat.ChatID, chat.ContactID);
            }

            await _dbContext.ExecuteAsync($"Update WhatsappChatSession Set LastMessageOn=@Date Where SessionID={sessionId}", new { Date = DateTime.UtcNow });
            #endregion

        }

        private async Task SendAutoReplyMessage(int whatsappAccountID, WhatsappContact contact, string message, int? replyId, int? sessionID, int? clientId, int? incomingChatID, string? lastChatMessage, int? lastChatReplyId, bool? enquiryConfirmationStatus = null)
        {

            if (incomingChatID > 0)
                await _dbContext.ExecuteAsync($"Update WhatsappChat Set ReplyID={lastChatReplyId},SessionID={sessionID},Message=@Message Where ChatID={incomingChatID}", new { Message = lastChatMessage });

            //string? chatHistoryMessage = "";
            SendMessageReposnseModel? messageResp = null;
            ChatbotReply firstOption = null;
            bool addEnquiry = false, isFinished = true;
            bool sendConfirmation = false;
            ChatbotReply autoReply = null;

            if (replyId != null)
            {
                autoReply = await _dbContext.GetAsync<ChatbotReply>($"ReplyID={replyId}",null);

                if (autoReply != null)
                {
                    switch ((ChatbotReplyTypes)autoReply.ReplyTypeID)
                    {
                        case ChatbotReplyTypes.Enquiry:
                            message = autoReply.Detailed ?? "";
                            messageResp = await SendEnquiryMessage(whatsappAccountID, contact.Phone, message, autoReply.ParentReplyID.Value, autoReply.ReplyID, autoReply.Header, autoReply.Footer, autoReply.ListButtonText);

                            firstOption = await _dbContext.GetByQueryAsync<ChatbotReply>($@"Select Top(1) * 
                                from ChatbotReply 
                                where ParentReplyID=@ReplyID and IsDeleted=0
                                Order By Code", new { ReplyID = replyId });
                            break;
                        case ChatbotReplyTypes.List:
                            message = autoReply.Detailed ?? "";
                            messageResp = await SendAutoReplyList(whatsappAccountID, contact.Phone, replyId, null, message, buttonText: autoReply.ListButtonText, documentName: autoReply.Short);
                            break;
                        case ChatbotReplyTypes.Text:
                            messageResp = await SendTextMediaReply(whatsappAccountID, contact.Phone, autoReply.ReplyID, autoReply.Detailed);
                            break;
                        case ChatbotReplyTypes.Location:
                            messageResp = await SendGetLocation(whatsappAccountID, contact.Phone, autoReply.Detailed);
                            break;
                        case ChatbotReplyTypes.ListOption:
                            var parentReplyID = await _dbContext.GetByQueryAsync<int>($"Select ParentReplyID From ChatbotReply Where ReplyID = {replyId}",null);
                            var grandParentReplyID = await _dbContext.GetByQueryAsync<int>($"Select ParentReplyID From ChatbotReply Where ReplyID = {parentReplyID}", null);
                            firstOption = await _dbContext.GetByQueryAsync<ChatbotReply>($@"Select Top(1) * 
                                        from ChatbotReply 
                                        where ParentReplyID={grandParentReplyID} and IsDeleted=0 and Code>(Select Code From ChatbotReply Where ReplyID = {parentReplyID})
                                        Order By Code", null);
                            break;
                        case ChatbotReplyTypes.Submit:
                            if (enquiryConfirmationStatus == false)//If it is edit, then  send
                            {
                                await _dbContext.ExecuteAsync($@"Update C
                                    Set IsDeleted=1
                                    From WhatsappChatSession S
                                    JOIN WhatsappChat C on S.SessionID=C.SessionID	
                                    JOIN ChatbotReply R on R.ReplyID=C.ReplyID
                                    Where S.SessionID={sessionID} and C.IsDeleted=0 and R.ReplyTypeID not in({(int)ChatbotReplyTypes.Forward},{(int)ChatbotReplyTypes.Enquiry})");

                                firstOption = await _dbContext.GetByQueryAsync<ChatbotReply>($@"Select Top(1) * 
                                from ChatbotReply 
                                where ParentReplyID=@ReplyID and IsDeleted=0
                                Order By Code", new { ReplyID = autoReply.ParentReplyID });
                            }
                            else
                            {
                                if (enquiryConfirmationStatus == null && autoReply.NeedConfirmation)//means new enqury submited-Send Confirmation message if its parent type is enquiry
                                {
                                    var parentQuetionType = await _dbContext.GetByQueryAsync<int>($"Select ReplyTypeID from ChatbotReply Where ReplyID =(Select ParentReplyID from ChatbotReply Where ReplyID={autoReply.ReplyID})", null);
                                    if (parentQuetionType == (int)ChatbotReplyTypes.Enquiry)
                                    {
                                        messageResp = await SendConfirmationMessage(whatsappAccountID, contact.Phone, sessionID.Value, autoReply.ReplyID);
                                    }
                                    else
                                    {
                                        //Add Enquiry directly-- If Submit button not in Enquiry
                                        addEnquiry = true;
                                    }
                                }
                                else //Confirmed
                                {
                                    addEnquiry = true;
                                }

                                if (addEnquiry)
                                {
                                    message += autoReply.Detailed ?? "";
                                    messageResp = await SendTextMediaReply(whatsappAccountID, contact.Phone, autoReply.ReplyID, autoReply.Detailed);


                                    firstOption = await _dbContext.GetByQueryAsync<ChatbotReply>($@"Select Top(1) * 
                                        from ChatbotReply 
                                        where ParentReplyID=@ReplyID and IsDeleted=0
                                        Order By Code", new { ReplyID = replyId });

                                    if (firstOption != null)
                                    {
                                        isFinished = false;
                                    }
                                    else
                                    {
                                        var parentReply = await _dbContext.GetAsync<ChatbotReply>(autoReply.ParentReplyID.Value);
                                        if (parentReply.ReplyTypeID == (int)ChatbotReplyTypes.Enquiry)
                                        {
                                            firstOption = await _dbContext.GetByQueryAsync<ChatbotReply>($@"Select Top(1) * 
                                        from ChatbotReply 
                                        where ParentReplyID={autoReply.ParentReplyID} and IsDeleted=0 and Code>(Select Code From ChatbotReply Where ReplyID = {autoReply.ReplyID})
                                        Order By Code", null);

                                            if (firstOption != null)
                                            {
                                                isFinished = false;
                                            }
                                        }
                                    }
                                }
                            }
                            break;

                        case ChatbotReplyTypes.Address:
                            message = autoReply.Detailed ?? "";
                            messageResp = await SendGetAddress(whatsappAccountID, contact.Phone, message);
                            break;

                        case ChatbotReplyTypes.Purchase:
                            var purchaseBotReply = await _dbContext.GetAsync<ChatbotPurchaseReply>($"ReplyID={autoReply.ReplyID}", null);
                            messageResp = await SendTextMediaReply(whatsappAccountID, contact.Phone, autoReply.ReplyID, purchaseBotReply.QuantityLabel);
                            break;

                        default:
                            message += autoReply.Detailed ?? "";
                            messageResp = await SendAutoReplyList(whatsappAccountID, contact.Phone, replyId, autoReply.ParentReplyID, message, buttonText: autoReply.ListButtonText, documentName: autoReply.Short, header: autoReply.Header, footer: autoReply.Footer);
                            break;
                    }

                    if (autoReply.NotifyUser)
                    {
                        var entitIds = await GetClientEntityIds(whatsappAccountID);
                        foreach (var entityId in entitIds)
                        {
                            await _notification.SendNotification(entityId, $"{autoReply.Short} from {contact.Name}");
                        }
                    }
                }
            }
            else
            {
                messageResp = await SendText(whatsappAccountID, contact.Phone, message);
            }

            if (autoReply.ReplyTypeID != (int)ChatbotReplyTypes.ListOption)
            {
                WhatsappChat chat = new()
                {
                    ContactID = contact.ContactID,
                    AddedOn = DateTime.UtcNow,
                    Message = messageResp != null ? messageResp.Message : message,
                    MessageTypeID = messageResp != null ? messageResp.MediaTypeID : (int)MessageTypes.Text,
                    WhatsappAccountID = whatsappAccountID,
                    ReplyID = replyId,
                    SessionID = sessionID,
                };
                if (messageResp != null)
                {
                    chat.MessageID = messageResp.MessageID;
                    chat.AttachedMediaID = messageResp.MediaID;
                    if (messageResp.IsFailed)
                    {
                        chat.MessageStatus = (int)MessageStatus.Failed;
                        chat.FailedReason = messageResp.ErrorMessage;
                        chat.FailedOn = DateTime.UtcNow;
                    }
                }
                chat.ChatID = await _dbContext.SaveAsync(chat, needLog: false);

                if (autoReply != null)
                    await SendMultiReply(whatsappAccountID, contact.Phone, autoReply.ReplyID, contact.ContactID, sessionID);
            }

            if (firstOption != null) // If first option is not null, then send the header first, then send first option in second
            {
                await Task.Delay(2000);
                await SendAutoReplyMessage(whatsappAccountID, contact, "", firstOption.ReplyID, sessionID, clientId, 0, lastChatMessage, firstOption.ReplyID);
            }

            #region Enquiry
            var enquiryId = await _dbContext.GetByQueryAsync<int>($@"Select ISNULL(EnquiryID,0) From WhatsappChatSession Where SessionID={sessionID}", null);
            if (addEnquiry)
            {
                #region Add Enquiry

                if (enquiryId == 0)
                {
                    int branchID = await _dbContext.GetByQueryAsync<int>($"Select BranchID from Branch Where ClientID={clientId} and IsDeleted=0", null);
                    var enquiry = new Enquiry()
                    {
                        EnquiryID = enquiryId,
                        ClientID = clientId,
                        BranchID = branchID,
                        CustomerEntityID = await GetContactEntityID(contact.ContactID, clientId),
                        Date = DateTime.UtcNow,
                        LeadThroughID = (int)CRM.Model.Enum.LeadThroughs.Whatsapp,
                        Description = await GetEnquiryDescription(sessionID),
                        EnquiryNo = await _dbContext.GetByQueryAsync<int>($@"Select ISNULL(MAX(EnquiryNo),0)+1 from Enquiry Where BranchID={branchID}", null),
                        FirstFollowUpDate = System.DateTime.UtcNow.Date
                    };
                    enquiryId = enquiry.EnquiryID = await _dbContext.SaveAsync(enquiry);

                    #region Assignee

                    var assignees = await _dbContext.GetListByQueryAsync<int>($@"Select EntityID
                    From ChatbotSubmitAssignee
                    Where IsDeleted=0 and ReplyID={autoReply.ReplyID}", null);
                    if (assignees != null)
                    {
                        var enqAssignees = new List<EnquiryAssignee>();
                        foreach (var assignee in assignees)
                        {
                            enqAssignees.Add(new() { EntityID = assignee });
                        }
                        await _dbContext.SaveSubItemListAsync(enqAssignees, "EnquiryID", enquiry.EnquiryID);
                    }

                    #endregion

                    #region Enquiry Item

                    if (autoReply.ItemVariantID.HasValue)
                    {
                        EnquiryItem item = new EnquiryItem()
                        {
                            ItemVariantID = autoReply.ItemVariantID.Value,
                            Description = "",
                            EnquiryID = enquiry.EnquiryID,
                            Quantity = 1
                        };
                        await _dbContext.SaveAsync(item);
                    }

                    #endregion

                }

                await _dbContext.ExecuteAsync($"Update WhatsappChatSession Set EnquiryID={enquiryId},IsFinished=@IsFinished Where SessionID={sessionID}", new { IsFinished = isFinished });
                await _dbContext.ExecuteAsync($@"Update Enquiry Set Description=@Description Where EnquiryID=@EnquiryID", new { EnquiryID = enquiryId, Description = await GetEnquiryDescription(sessionID) });

                var entitIds = await GetClientEntityIds(whatsappAccountID);
                foreach (var entityId in entitIds)
                {
                    await _notification.SendNotification(entityId, $"New Enquiry Received from {contact.Name}");
                }

                #endregion
            }
            else if (enquiryId != 0)
            {
                //Update enquiry description after the enquiry submit if there is any question after the submit
                await _dbContext.ExecuteAsync($@"Update Enquiry Set Description=@Description Where EnquiryID=@EnquiryID", new { EnquiryID = enquiryId, Description = await GetEnquiryDescription(sessionID) });
            }
            #endregion
        }

        public async Task<string> GetEnquiryDescription(int? sessionID)
        {
            return await _dbContext.GetByQueryAsync<string>($@"Select String_Agg(Case When R.ReplyTYpeID in (1,2) then R.Short +'->' when R.ReplyTYpeID in(3) then  R.Short when R.ReplyTYpeID=8 then '' when R.ReplyTYpeID=7 then CHAR(13)+CHAR(10)+ PR.Short+':'+Message else CHAR(13)+CHAR(10)+ R.Short+':'+Message end,' ') Short
                from WhatsappChatSession S
                JOIN WhatsappChat C on S.SessionID=C.SessionID	
                JOIN ChatbotReply R on R.ReplyID=C.ReplyID
                LEFT JOIN ChatbotReply PR on PR.ReplyID=R.ParentReplyID and R.ReplyTypeID=7
                Where IsIncoming=1 and S.SessionID={sessionID} and C.IsDeleted=0", null);
        }

        public async Task<int?> GetContactEntityID(int contactId, int? clientID)
        {
            var contact = await _dbContext.GetAsync<WhatsappContact>($"ContactID=@ContactID", new { ContactID = contactId });
            if (contact.EntityID == null)
            {
                Entity entity = new()
                {
                    ClientID = clientID,
                    Phone = contact.Phone,
                    EntityTypeID = (int)EntityType.Customer
                };
                entity.EntityID = await _dbContext.SaveAsync(entity);

                contact.EntityID = entity.EntityID;
                await _dbContext.SaveAsync(contact);

                EntityPersonalInfo entityPersonalInfo = new()
                {
                    EntityID = entity.EntityID,
                    FirstName = contact.Name,
                };
                await _dbContext.SaveAsync(entityPersonalInfo);

                Customer customer = new()
                {
                    ClientID = clientID,
                    EntityID = entity.EntityID,
                    Type = (int)CustomerTypes.Individual,
                    Status = (int)CustomerStatus.Active
                };
                await _dbContext.SaveAsync(customer);

                return entity.EntityID;
            }
            else
            {
                return contact.EntityID;
            }
        }

        private async Task<ChatbotMultiReplyListModel> GetFirstMedia(int replyID)
        {
            return await _dbContext.GetByQueryAsync<ChatbotMultiReplyListModel>($@"Select Top(1) FileName,Caption,MediaTypeID,WMediaID,R.MediaID 
                    from ChatbotMultiReply R
                    LEFT JOIN Media M on M.MediaID=R.MediaID and M.IsDeleted=0
                    Where R.IsDeleted=0 and R.ReplyID={replyID} and (FileName is not null or WMediaID is not null) and ISNULL(IsFirst,0)=1 
                    Order by SlNo", null);
        }

        public async Task<SendMessageReposnseModel> SendTextMediaReply(int whatsappAccountID, string toNumber, int replyId, string? textMessage)
        {
            SendMessageReposnseModel res = null;
            var media = await GetFirstMedia(replyId);
            if (media != null)
            {
                //if (media.WMediaID != null)
                //{
                //    switch ((MessageTypes)media.MediaTypeID)
                //    {
                //        case MessageTypes.Image:
                //            res = await SendImageById(whatsappAccountID, toNumber, media.WMediaID, textMessage);
                //            break;
                //        case MessageTypes.Document:
                //            res = await SendDocumentByUrl(whatsappAccountID, toNumber, textMessage, _config["ServerURL"] + media.FileName, media.Caption);
                //            break;
                //        case MessageTypes.Audio:
                //            res = await SendAudioByUrl(whatsappAccountID, toNumber, _config["ServerURL"] + media.FileName, textMessage);
                //            break;
                //        case MessageTypes.Video:
                //            res = await SendVideoByUrl(whatsappAccountID, toNumber, _config["ServerURL"] + media.FileName, textMessage);
                //            break;
                //        case MessageTypes.Location:
                //            var locationDetails = await _dbContext.GetAsync<ChatbotMultiReplyLocation>($"MultiReplyID={media.MultiReplyID}");
                //            res = await SendLocation(whatsappAccountID, toNumber, locationDetails.Latitude, locationDetails.Longitude, locationDetails.Name, locationDetails.Address);
                //            break;
                //    }
                //}
                //else
                //{
                switch ((MessageTypes)media.MediaTypeID)
                {
                    case MessageTypes.Text:
                        res = await SendText(whatsappAccountID, toNumber, textMessage, true);
                        break;
                    case MessageTypes.Image:
                        res = await SendImageByUrl(whatsappAccountID, toNumber, _config["ServerURL"] + media.FileName, textMessage);
                        break;
                    case MessageTypes.Document:
                        res = await SendDocumentByUrl(whatsappAccountID, toNumber, textMessage, _config["ServerURL"] + media.FileName, media.Caption);
                        break;
                    case MessageTypes.Audio:
                        res = await SendAudioByUrl(whatsappAccountID, toNumber, _config["ServerURL"] + media.FileName, textMessage);
                        break;
                    case MessageTypes.Video:
                        res = await SendVideoByUrl(whatsappAccountID, toNumber, _config["ServerURL"] + media.FileName, textMessage);
                        break;
                    case MessageTypes.Location:
                        var locationDetails = await _dbContext.GetAsync<ChatbotMultiReplyLocation>($"MultiReplyID={media.MultiReplyID}", null);
                        res = await SendLocation(whatsappAccountID, toNumber, locationDetails.Latitude, locationDetails.Longitude, locationDetails.Name, locationDetails.Address);
                        break;
                }
                //}
                res.Message = textMessage;
                res.MediaID = media.MediaID;
                res.MediaTypeID = media.MediaTypeID;
            }
            else
            {
                res = await SendText(whatsappAccountID, toNumber, textMessage);
                res.Message = textMessage;
                res.MediaTypeID = (int)MessageTypes.Text;
            }
            return res;
        }

        private async Task SendMultiReply(int whatsappAccountID, string toNumber, int replyId, int? contactID, int? sessionID)
        {
            WhatsappChatLocation loc = null;
            SendMessageReposnseModel messageResp = null;
            var multiReply = await _dbContext.GetListByQueryAsync<ChatbotMultiReplyListModel>($@"Select MultiReplyID,FileName,Caption,SlNo,MediaTypeID,WMediaID,DocumentName,R.MediaID
                    from ChatbotMultiReply R
                    LEFT JOIN Media M on M.MediaID=R.MediaID and M.IsDeleted=0
                    Where R.IsDeleted=0 and R.ReplyID={replyId} and ISNULL(IsFirst,0)=0
                    Order by SlNo", null);

            if (multiReply != null)
            {
                foreach (var reply in multiReply)
                {
                    if (reply.WMediaID != null)
                    {
                        switch ((MessageTypes)reply.MediaTypeID)
                        {
                            case MessageTypes.Image:
                                messageResp = await SendImageById(whatsappAccountID, toNumber, reply.WMediaID, reply.Caption);
                                break;
                            case MessageTypes.Document:
                                messageResp = await SendDocumentById(whatsappAccountID, toNumber, reply.Caption, reply.WMediaID, reply.DocumentName);
                                break;
                            case MessageTypes.Audio:
                                messageResp = await SendAudioById(whatsappAccountID, toNumber, reply.WMediaID);
                                break;
                            case MessageTypes.Video:
                                messageResp = await SendVideoById(whatsappAccountID, toNumber, reply.WMediaID, reply.Caption);
                                break;
                        }
                    }
                    else
                    {
                        switch ((MessageTypes)reply.MediaTypeID)
                        {
                            case MessageTypes.Text:
                                messageResp = await SendText(whatsappAccountID, toNumber, reply.Caption, true);
                                break;
                            case MessageTypes.Image:
                                messageResp = await SendImageByUrl(whatsappAccountID, toNumber, _config["ServerURL"] + reply.FileName, reply.Caption);
                                break;
                            case MessageTypes.Document:
                                messageResp = await SendDocumentByUrl(whatsappAccountID, toNumber, reply.Caption, _config["ServerURL"] + reply.FileName, reply.Caption);
                                break;
                            case MessageTypes.Audio:
                                messageResp = await SendAudioByUrl(whatsappAccountID, toNumber, _config["ServerURL"] + reply.FileName, reply.Caption);
                                break;
                            case MessageTypes.Video:
                                messageResp = await SendVideoByUrl(whatsappAccountID, toNumber, _config["ServerURL"] + reply.FileName, reply.Caption);
                                break;
                            case MessageTypes.Location:
                                var locationDetails = await _dbContext.GetAsync<ChatbotMultiReplyLocation>($"MultiReplyID={reply.MultiReplyID}", null);
                                messageResp = await SendLocation(whatsappAccountID, toNumber, locationDetails.Latitude, locationDetails.Longitude, locationDetails.Name, locationDetails.Address);
                                loc = new()
                                {
                                    Latitude = locationDetails.Latitude,
                                    Longitude = locationDetails.Longitude,
                                    Name = locationDetails.Name,
                                    Address = locationDetails.Address
                                };
                                break;
                        }
                    }

                    WhatsappChat chat = new()
                    {
                        ContactID = contactID,
                        AddedOn = DateTime.UtcNow,
                        Message = reply.Caption,
                        MessageTypeID = reply.MediaTypeID,
                        WhatsappAccountID = whatsappAccountID,
                        ReplyID = replyId,
                        SessionID = sessionID,
                    };
                    if (messageResp != null)
                    {
                        chat.MessageID = messageResp.MessageID;
                        if (messageResp.IsFailed)
                        {
                            chat.MessageStatus = (int)MessageStatus.Failed;
                            chat.FailedReason = messageResp.ErrorMessage;
                            chat.FailedOn = DateTime.UtcNow;
                        }
                    }
                    chat.ChatID = await _dbContext.SaveAsync(chat, needLog: false);

                    if (reply.MediaID != null || reply.WMediaID != null)
                    {
                        await _dbContext.SaveAsync(new WhatsappChatMedia()
                        {
                            ChatID = chat.ChatID,
                            MediaID = reply.MediaID,
                            WMediaID = reply.WMediaID,
                        }, needLog: false);
                    }

                    if (loc != null)
                    {
                        loc.ChatID = chat.ChatID;
                        await _dbContext.SaveAsync(loc, needLog: false);
                    }

                }
            }
        }

        private async Task<SendMessageReposnseModel> SendAutoReplyList(int whatsappAccountID, string toNumber, int? parentReplyID, int? grandParentId, string body = "", string footer = "", string buttonText = "View", string documentName = "", string header = "")
        {
            string headerMessage = "";
            SendMessageReposnseModel res = null;
            var options = await _dbContext.GetListByQueryAsync<ChatbotReplyOptionModel>($@"Select ReplyID,Code,Short,Detailed,SmallDescription
                            from ChatbotReply where ParentReplyID=@ReplyID and IsDeleted=0
                            Order by Code", new { ReplyID = parentReplyID });

            var backButtonLabel = await _dbContext.GetFieldsAsync<WhatsappAccount, string>("BackButtonLabel", $"WhatsappAccountID={whatsappAccountID}", null);

            var bigCaptionsCount = 0;
            if (options != null)
                bigCaptionsCount = options.Where(s => s.Short.Length > 24).Count();

            if (grandParentId != null)
            {
                if (options.Count > 9 || bigCaptionsCount > 0)
                {
                    options.Add(new()
                    {
                        Code = "0",
                        Short = backButtonLabel ?? "Go Back",
                    });
                }
                else
                {
                    options.Add(new()
                    {
                        Code = "b" + grandParentId,
                        Short = backButtonLabel ?? "Go Back",
                    });
                }
            }

            if (options != null)
            {
                ////Each row must have a title (Maximum length: 24 characters) and an ID (Maximum length: 200 characters). 
                ///You can add a description (Maximum length: 72 characters), but it is optional. 
                ///Required if the message has more than one section.

                if (options.Count > 10 || bigCaptionsCount > 0)
                {
                    if (!string.IsNullOrEmpty(header))
                    {
                        body = header + "\n" + body;
                    }
                    foreach (var option in options)
                    {
                        body += "\n" + option.Code + "." + option.Short;
                    }
                    return await SendTextMediaReply(whatsappAccountID, toNumber, parentReplyID.Value, body);
                }
                else if (options.Count <= 3)
                {
                    InteractiveButtonMessageModel msg = new()
                    {
                        type = "button"
                    };

                    var media = await GetFirstMedia(parentReplyID.Value);
                    if (media != null)
                    {
                        string link = "";
                        if (media.WMediaID == null)
                            link = _config["ServerURL"] + media.FileName;

                        switch ((MessageTypes)media.MediaTypeID)
                        {
                            case MessageTypes.Image:
                                msg.header = new()
                                {
                                    type = "image",
                                    image = new()
                                    {
                                        id = media.WMediaID,
                                        link = link
                                    }
                                };

                                break;
                            case MessageTypes.Video:
                                msg.header = new()
                                {
                                    type = "video",
                                    video = new()
                                    {
                                        id = media.WMediaID,
                                        link = link
                                    }
                                };
                                break;
                            case MessageTypes.Document:
                                msg.header = new()
                                {
                                    type = "document",
                                    document = new()
                                    {
                                        id = media.WMediaID,
                                        link = link,
                                        filename = media.Caption ?? documentName
                                    }
                                };
                                break;
                        }
                    }
                    else if (!string.IsNullOrEmpty(header))
                    {
                        msg.header = new InteractiveHeaderModel()
                        {
                            type = "text",
                            text = header
                        };
                        headerMessage = header;
                    }

                    if (!string.IsNullOrEmpty(body))
                    {
                        msg.body = new InteractiveBodyModel()
                        {
                            text = body
                        };
                    }
                    if (!string.IsNullOrEmpty(footer))
                    {
                        msg.footer = new InteractiveBodyModel()
                        {
                            text = footer
                        };
                    }
                    foreach (var option in options)
                    {
                        if (option.ReplyID == 0)
                            msg.action.buttons.Add(new InteractiveButtonModel()
                            {
                                reply = new InteractiveButtonReplyModel()
                                {
                                    id = option.Code,
                                    title = option.Short,
                                }
                            });
                        else
                            msg.action.buttons.Add(new InteractiveButtonModel()
                            {
                                reply = new InteractiveButtonReplyModel()
                                {
                                    id = "f" + option.ReplyID.ToString(),
                                    title = option.Short,
                                }
                            });
                    }
                    res = await SendInteractiveButtonMessage(whatsappAccountID, toNumber, msg);
                    if (media != null)
                    {
                        res.MediaID = media.MediaID;
                        res.MediaTypeID = media.MediaTypeID;
                    }
                }
                else if (options.Count <= 10)
                {
                    InteractiveListMessageModel msg = new()
                    {
                        type = "list"
                    };
                    var media = await GetFirstMedia(parentReplyID.Value);
                    if (media != null)
                    {
                        string link = "";
                        if (media.WMediaID == null)
                            link = _config["ServerURL"] + media.FileName;

                        switch ((MessageTypes)media.MediaTypeID)
                        {
                            case MessageTypes.Image:
                                msg.header = new()
                                {
                                    type = "image",
                                    image = new()
                                    {
                                        id = media.WMediaID,
                                        link = link
                                    }
                                };
                                break;
                            case MessageTypes.Video:
                                msg.header = new()
                                {
                                    type = "video",
                                    video = new()
                                    {
                                        id = media.WMediaID,
                                        link = link
                                    }
                                };
                                break;
                            case MessageTypes.Document:
                                msg.header = new()
                                {
                                    type = "document",
                                    document = new()
                                    {
                                        id = media.WMediaID,
                                        link = link
                                    }
                                };
                                break;
                        }
                    }
                    else if (!string.IsNullOrEmpty(header))
                    {
                        msg.header = new InteractiveHeaderModel()
                        {
                            type = "text",
                            text = header
                        };
                        headerMessage = header;
                    }


                    if (!string.IsNullOrEmpty(body))
                    {
                        msg.body = new InteractiveBodyModel()
                        {
                            text = body
                        };
                    }
                    if (!string.IsNullOrEmpty(footer))
                    {
                        msg.footer = new InteractiveBodyModel()
                        {
                            text = footer
                        };
                    }

                    msg.action.button = string.IsNullOrEmpty(buttonText) == true ? "View" : buttonText;

                    msg.action.sections.Add(new()
                    {
                        title = "Choose from the list",

                    });

                    foreach (var option in options)
                    {
                        if (option.ReplyID == 0)
                            msg.action.sections[0].rows.Add(new()
                            {
                                id = option.Code,
                                title = option.Short,
                            });
                        else
                            msg.action.sections[0].rows.Add(new()
                            {
                                id = "f" + option.ReplyID.ToString(),
                                title = option.Short,
                                description = option.SmallDescription
                            });
                    }

                    res = await SendInteractiveListMessage(whatsappAccountID, toNumber, msg);
                    if (media != null)
                    {
                        res.MediaID = media.MediaID;
                        res.MediaTypeID = media.MediaTypeID;
                    }
                }

                if (!string.IsNullOrEmpty(headerMessage))
                {
                    res.Message = headerMessage + "\n";
                }

                res.Message += body;

                if (!string.IsNullOrEmpty(footer))
                {
                    res.Message += "\n" + footer;
                }

                foreach (var option in options)
                {
                    res.Message += "\n" + option.Code + "." + option.Short;
                }
            }
            return res;
        }

        private async Task<SendMessageReposnseModel> SendEnquiryMessage(int whatsappAccountID, string toNumber, string message, int parentId, int replyId, string? header, string? footer, string? buttonCaption)
        {
            string headerMessage = "";
            InteractiveButtonMessageModel msg = new()
            {
                type = "button"
            };

            var media = await GetFirstMedia(replyId);
            if (media != null)
            {
                string link = "";
                if (media.WMediaID == null)
                    link = _config["ServerURL"] + media.FileName;

                switch ((MessageTypes)media.MediaTypeID)
                {
                    case MessageTypes.Image:
                        msg.header = new()
                        {
                            type = "image",
                            image = new()
                            {
                                link = link,
                                id = media.WMediaID
                            }
                        };
                        break;
                    case MessageTypes.Video:
                        msg.header = new()
                        {
                            type = "video",
                            video = new()
                            {
                                link = link,
                                id = media.WMediaID
                            }
                        };
                        break;
                    case MessageTypes.Document:
                        msg.header = new()
                        {
                            type = "document",
                            document = new()
                            {
                                link = link,
                                id = media.WMediaID
                            }
                        };
                        break;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(header))
                {
                    msg.header = new()
                    {
                        type = "text",
                        text = header
                    };
                    headerMessage = header;
                }
            }

            msg.body = new InteractiveBodyModel()
            {
                text = message
            };
            msg.footer = new InteractiveBodyModel()
            {
                text = footer ?? "You can go back any time by pressing Go Back button"
            };

            msg.action.buttons.Add(new InteractiveButtonModel()
            {
                reply = new InteractiveButtonReplyModel()
                {
                    id = "b" + parentId.ToString(),
                    title = buttonCaption ?? "Go Back",
                }
            });

            var res = await SendInteractiveButtonMessage(whatsappAccountID, toNumber, msg);
            if (!string.IsNullOrEmpty(headerMessage))
            {
                res.Message = headerMessage + "\n";
            }
            res.Message += message + "\n";

            res.Message += footer ?? "You can go back any time by pressing Go Back button";

            res.MediaID = media != null ? media.MediaID : null;

            return res;
        }

        private async Task<SendMessageReposnseModel> SendConfirmationMessage(int whatsappAccountID, string toNumber, int sessionId, int replyId)
        {
            var whastappAccount = await _dbContext.GetAsync<WhatsappAccount>(whatsappAccountID);

            InteractiveButtonMessageModel msg = new()
            {
                type = "button"
            };

            string message = await _dbContext.GetByQueryAsync<string>($@"Select String_Agg(Case when R.ReplyTYpeID=7 then CHAR(13)+CHAR(10)+ PR.Short+':'+Message when R.ReplyTYpeID in(4,6) then CHAR(13)+CHAR(10)+ R.Short+':'+Message end,' ') Short
                from WhatsappChatSession S
                JOIN WhatsappChat C on S.SessionID=C.SessionID	
                JOIN ChatbotReply R on R.ReplyID=C.ReplyID
                LEFT JOIN ChatbotReply PR on PR.ReplyID=R.ParentReplyID and R.ReplyTypeID=7
                Where IsIncoming=1 and S.SessionID={sessionId} and C.IsDeleted=0", null);

            msg.body = new InteractiveBodyModel()
            {
                text = message ?? "Are you sure?"
            };

            msg.action.buttons.Add(new InteractiveButtonModel()
            {
                reply = new InteractiveButtonReplyModel()
                {
                    id = "enq_c" + replyId.ToString(),
                    title = whastappAccount.ConfirmButtonLabel ?? "Confirm",
                }
            });

            msg.action.buttons.Add(new InteractiveButtonModel()
            {
                reply = new InteractiveButtonReplyModel()
                {
                    id = "enq_e" + replyId.ToString(),
                    title = whastappAccount.EditButtonLabel ?? "Edit",
                }
            });

            var res = await SendInteractiveButtonMessage(whatsappAccountID, toNumber, msg);
            res.Message = (message ?? "Are you sure?") + "\n" + (whastappAccount.ConfirmButtonLabel ?? "Confirm") + " | " + (whastappAccount.EditButtonLabel ?? "Edit");
            return res;
        }



        #endregion

        #region Sent Message

        public async Task MarkMessageasRead(int whatsappAccountID, string incomingMessageId)
        {
            var content = new MarkasReadMessageMode()
            {
                message_id = incomingMessageId
            };
            await SendMessage(whatsappAccountID, content);

        }

        public async Task<SessionResponse> UploadMediaToFb(int whatsappAccountId, IFormFile file)
        {
            var whatsappAccount = await _dbContext.GetAsync<WhatsappAccount>(whatsappAccountId);

            HttpClient client = new HttpClient();
            var url = $"https://graph.facebook.com/{whatsappAccount.Version}/{whatsappAccount.PhoneNoID}/media";
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", whatsappAccount.Token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            MultipartFormDataContent form = new();

            string boundary = $"----------{Guid.NewGuid():N}";
            var content = new MultipartFormDataContent(boundary);
            MemoryStream ms = new MemoryStream();
            file.CopyTo(ms);

            ByteArrayContent mediaFileContent = new ByteArrayContent(ms.ToArray());
            mediaFileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "file",
                FileName = file.FileName,
            };
            mediaFileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(file.ContentType);

            var fileData = new
            {
                messaging_product = "whatsapp"
            };

            content.Add(mediaFileContent);
            content.Add(new StringContent(fileData.messaging_product), "messaging_product");

            var response = await client.PostAsync(url, content).ConfigureAwait(false);

            //HttpResponseMessage response = await client.PostAsync(url, form);


            //response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();

            client.Dispose();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var r = JsonConvert.DeserializeObject<SessionResponse>(responseContent);
                if (r != null && r.id != null)
                {
                    return r;
                }
                return null;
            }
            else
            {
                throw new PBException(response.Headers.WwwAuthenticate.FirstOrDefault().ToString());
            }
        }

        private async Task<SendMessageReposnseModel> SendGetAddress(int whatsappAccountID, string to, string message)
        {
            //https://developers.facebook.com/docs/whatsapp/cloud-api/guides/send-messages/#address-messages
            if (!string.IsNullOrEmpty(message))
            {
                var content = new SendGetAddressModel()
                {
                    to = to,
                    type = "interactive",
                    interactive = new()
                    {
                        body = new()
                        {
                            text = message
                        },
                        action = new()
                        {
                            parameters = new()
                            {
                                country = "IN",
                                values = new()
                                {
                                    phone_number = to,
                                    name = ""
                                }
                            }
                        }
                    },
                };
                return await SendMessage(whatsappAccountID, content);
            }
            return null;
        }

        private async Task<SendMessageReposnseModel> SendGetLocation(int whatsappAccountID, string to, string message)
        {
            //https://developers.facebook.com/docs/whatsapp/guides/interactive-messages/
            if (!string.IsNullOrEmpty(message))
            {
                var content = new SendGetLocationModel()
                {
                    to = to,
                    type = "interactive",
                    interactive = new()
                    {
                        type = "location_request_message",
                        body = new()
                        {
                            text = message,
                            type = "text"
                        },
                        action = new()
                        {
                            name = "send_location"
                        }
                    },
                };
                var res = await SendMessage(whatsappAccountID, content);
                res.Message = message;
            }
            return null;
        }
        private async Task<SendMessageReposnseModel> SendImageByUrl(int whatsappAccountID, string to, string link, string caption = "")
        {
            var content = new SendMediaModel()
            {
                to = to,
                type = "image",
                image = new()
                {
                    link = link,
                    caption = caption
                }
            };
            return await SendMessage(whatsappAccountID, content);
        }
        private async Task<SendMessageReposnseModel> SendImageById(int whatsappAccountID, string to, string id, string caption = "")
        {
            var content = new SendMediaModel()
            {
                to = to,
                type = "image",
                image = new()
                {
                    id = id,
                    caption = caption
                }
            };
            return await SendMessage(whatsappAccountID, content);
        }
        private async Task<SendMessageReposnseModel> SendAudioByUrl(int whatsappAccountID, string to, string link, string caption = null)
        {
            var content = new SendMediaModel()
            {
                to = to,
                type = "audio",
                audio = new()
                {
                    link = link,
                }
            };
            return await SendMessage(whatsappAccountID, content);
        }
        private async Task<SendMessageReposnseModel> SendAudioById(int whatsappAccountID, string to, string id)
        {
            var content = new SendMediaModel()
            {
                to = to,
                type = "audio",
                audio = new()
                {
                    id = id
                }
            };
            return await SendMessage(whatsappAccountID, content);
        }
        private async Task<SendMessageReposnseModel> SendDocumentByUrl(int whatsappAccountID, string to, string caption, string link, string fileName = "document")
        {
            var content = new SendMediaModel()
            {
                to = to,
                type = "document",
                document = new()
                {
                    caption = caption,
                    link = link,
                    filename = fileName
                }
            };
            return await SendMessage(whatsappAccountID, content);
        }
        private async Task<SendMessageReposnseModel> SendDocumentById(int whatsappAccountID, string to, string caption, string id, string filename = "document")
        {
            if (filename == "")
                filename = "document";
            var content = new SendMediaModel()
            {
                to = to,
                type = "document",
                document = new()
                {
                    filename = filename,
                    id = id,
                    caption = caption
                }
            };
            return await SendMessage(whatsappAccountID, content);
        }
        private async Task<SendMessageReposnseModel> SendStickerByUrl(int whatsappAccountID, string to, string link)
        {
            var content = new SendMediaModel()
            {
                to = to,
                type = "sticker",
                sticker = new()
                {
                    link = link
                }
            };
            return await SendMessage(whatsappAccountID, content);
        }
        private async Task<SendMessageReposnseModel> SendStickerById(int whatsappAccountID, string to, string id)
        {
            var content = new SendMediaModel()
            {
                to = to,
                type = "sticker",
                sticker = new()
                {
                    id = id
                }
            };
            return await SendMessage(whatsappAccountID, content);
        }
        private async Task<SendMessageReposnseModel> SendVideoByUrl(int whatsappAccountID, string to, string link, string caption)
        {
            var content = new SendMediaModel()
            {
                to = to,
                type = "video",
                video = new()
                {
                    link = link,
                    caption = caption
                }
            };
            return await SendMessage(whatsappAccountID, content);
        }
        private async Task<SendMessageReposnseModel> SendVideoById(int whatsappAccountID, string to, string id, string caption)
        {
            var content = new SendMediaModel()
            {
                to = to,
                type = "video",
                video = new()
                {
                    id = id,
                    caption = caption
                }
            };
            return await SendMessage(whatsappAccountID, content);
        }
        private async Task<SendMessageReposnseModel> SendContacts(int whatsappAccountID, string to, string formattedName, string contactname, string phone_no)
        {
            var content = new SendMediaModel()
            {
                to = to,
                type = "contacts",
                contacts = new()
                {
                   new SendMediaContactsModel(){ name = new()
                   {
                        formatted_name = formattedName,
                        first_name = contactname
                   },
                   phones = new()
                   {
                        new SendContactsPhonesModel(){ phone = phone_no }

                   }
                }
            }
            };
            return await SendMessage(whatsappAccountID, content);
        }
        private async Task<SendMessageReposnseModel> SendLocation(int whatsappAccountID, string to, double? latitude, double? longitude, string name, string address)
        {
            var content = new SendMediaModel()
            {
                to = to,
                type = "location",
                location = new()
                {
                    latitude = latitude,
                    longitude = longitude,
                    name = name,
                    address = address
                }
            };
            return await SendMessage(whatsappAccountID, content);
        }
        private async Task<SendMessageReposnseModel> SendText(int whatsappAccountID, string to, string message, bool previewUrl = true)
        {
            if (!string.IsNullOrEmpty(message))
            {
                var content = new SendTextMessageModel()
                {
                    to = to,
                    type = "text",
                    text = new()
                    {
                        preview_url = previewUrl,
                        body = message
                    }
                };
                var res = await SendMessage(whatsappAccountID, content);
                res.Message = message;
                return res;
            }
            return null;
        }
        private async Task<SendMessageReposnseModel> SendTemplate(int whatsappAccountID, string to, string templateName, string lang, object templateParameters = null)
        {
            var content = new SendTemplateModel()
            {
                to = to,
                type = "template",
                template = new TemplateModel()
                {
                    name = templateName,
                    language = new()
                    {
                        code = lang
                    },
                    components = templateParameters
                },
            };
            return await SendMessage(whatsappAccountID, content);

        }
        private async Task<SendMessageReposnseModel> SendInteractiveListMessage(int whatsappAccountID, string to, InteractiveListMessageModel data)
        {
            var content = new SendInteractiveListMessageModel()
            {
                to = to,
                type = "interactive",
                interactive = data
            };
            return await SendMessage(whatsappAccountID, content);
        }
        private async Task<SendMessageReposnseModel> SendInteractiveButtonMessage(int whatsappAccountID, string to, InteractiveButtonMessageModel data)
        {
            var content = new SendInteractiveButtonMessageModel()
            {
                to = to,
                type = "interactive",
                interactive = data
            };
            return await SendMessage(whatsappAccountID, content);
        }

        #endregion

        #region Private Functions

        private async Task<WhatsappAccount> GetWhatsappAccount(string phoneNumberId)
        {
            return await _dbContext.GetAsync<WhatsappAccount>($"PhoneNoID='{phoneNumberId}'", null);
        }

        public async Task<string> GetContactNumber(int contactId)
        {
            return (await _dbContext.GetFieldsAsync<WhatsappContact, string>("Phone", $"ContactID={contactId}", null));
        }

        public async Task<WhatsappChatSession> GetSession(int whatsappAccountID, int contactID)
        {
            return await _dbContext.GetByQueryAsync<WhatsappChatSession>(@$"Select *
                    From WhatsappChatSession
                    Where ContactID={contactID} and WhatsappAccountID={whatsappAccountID} and (LastMessageOn>(DateADD(MINUTE,-30,GETUTCDATE())) or HasSupportChat=1) and ISNULL(IsFinished,0)=0", null);

        }

        public async Task<WhatsappChatSession> GetLastSession(int whatsappAccountID, int contactID)
        {
            return await _dbContext.GetByQueryAsync<WhatsappChatSession>(@$"Select  Top 1 *
                    From WhatsappChatSession
                    Where ContactID={contactID} and WhatsappAccountID={whatsappAccountID}
                    order by 1 desc", null);

        }

        private int GetMessageTypeID(string messageType)
        {
            switch (messageType)
            {
                case "text":
                    return (int)MessageTypes.Text;
                case "template":
                    return (int)MessageTypes.Template;
                case "image":
                    return (int)MessageTypes.Image;
                case "audio":
                    return (int)MessageTypes.Audio;
                case "video":
                    return (int)MessageTypes.Video;
                case "document":
                    return (int)MessageTypes.Document;
                case "contacts":
                    return (int)MessageTypes.Contacts;
                case "location":
                    return (int)MessageTypes.Location;
                case "voice":
                    return (int)MessageTypes.Voice;
                case "sticker":
                    return (int)MessageTypes.Sticker;
                case "interactive":
                    return (int)MessageTypes.Interactive;
                case "address":
                    return (int)MessageTypes.Address;
                default:
                    return (int)MessageTypes.None;
            }
        }

        private int GetMessageStatusID(string messageStatus)
        {
            switch (messageStatus)
            {
                case "sent":
                    return (int)MessageStatus.Sent;
                case "delivered":
                    return (int)MessageStatus.Delivered;
                case "read":
                    return (int)MessageStatus.Read;
                case "deleted":
                    return (int)MessageStatus.Deleted;
                default:
                    return (int)MessageStatus.Failed;
            }
        }

        private async Task<List<int>> GetClientEntityIds(int whatsappAccountId)
        {
            return await _dbContext.GetListByQueryAsync<int>($@"Select EntityID from Users Where ClientID=(Select ClientID from WhatsappAccount Where WhatsappAccountID={whatsappAccountId})", null);
        }

        public async Task<WhatsappContact> GetContact(string phone, int clientId, string? name = "", int importBatchID = 0)
        {
            var contact = await _dbContext.GetAsync<WhatsappContact>($"Phone=@Phone and ClientID={clientId}", new { Phone  = phone });
            if (contact == null)
            {
                contact = new WhatsappContact()
                {
                    Name = name,
                    Phone = phone,
                    ClientID = clientId
                };
                contact.ContactID = await _dbContext.SaveAsync(contact);
            }
            else if (string.IsNullOrEmpty(contact.Name) && !string.IsNullOrEmpty(name))
            {
                contact.Name = name;
                await _dbContext.SaveAsync(contact);
            }
            return contact;
        }

        static bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }
            return true;
        }

        private async Task<string?> GetToken(string phoneNumberId)
        {
            var whatsapp = await _dbContext.GetAsync<WhatsappAccount>($"PhoneNoID=@PhoneNoID", new { PhoneNoID = phoneNumberId });
            if (whatsapp != null)
                return whatsapp.Token;
            else
                return null;
        }

        public async Task<WhatsappChatHistoryItemModel> GetLastChatHistoryItem(int chatId)
        {
            return await _dbContext.GetByQueryAsync<WhatsappChatHistoryItemModel>($@"Select C.ChatID,C.ContactId,Case When R.ReplyID is not null then CB.Detailed else Message end as Message,
                    MessageTypeID,IsIncoming,Convert(varchar,DateAdd(MINUTE,330,AddedOn),0) as MessageOn,
                    MessageStatus StatusID,FailedReason,Latitude,Longitude,WMediaID,Case When FileName is not null then @ServerURL+FileName else '' end as FileName,CB.Caption as FileCaption,
                    FormattedName,P.WAID as Phone,M.DocumentName
                    From WhatsappChat C
					LEFT JOIN ChatbotReply R on R.ReplyID=C.ReplyID and IsIncoming=0
					LEFT JOIN viChatbotReply CB on CB.ReplyID=R.ReplyID
					LEFT JOIN WhatsappChatLocation L on L.ChatID=C.ChatID
                    LEFT JOIN WhatsappChatMedia M on M.ChatID=C.ChatID
                    LEFT JOIN Media MM on MM.MediaID= Case When R.ReplyID is not null then CB.MediaID else  M.MediaID end
                    LEFT JOIN WhatsappChatContact CC on CC.ChatID=C.ChatID
                    LEFT JOIN WhatsappChatContactPhone P on P.WhatsappChatContactID=CC.WhatsappChatContactID
                    Where C.ChatID={chatId}", new { ServerURL = _config["ServerURL"] });
        }

        private async Task NotifyUser(int whatsappAccountID, string message, string messageFrom, int chatId, int? contactId)
        {
            var chat = await GetLastChatHistoryItem(chatId);
            string extra = contactId.ToString();
            var entitIds = await GetClientEntityIds(whatsappAccountID);
            foreach (var entityId in entitIds)
            {
                await _notification.SendSignalRPush(entityId, chat, null, "chat");
                await _notification.SendPush(entityId, message, $"Message from {messageFrom}", "Chat", 1, extra);
            }
        }

        private async Task<SendMessageReposnseModel> SendMessage(int whatsappId, object content)
        {
            SendMessageReposnseModel res = new();
            var whatsappAccount = await _dbContext.GetAsync<WhatsappAccount>(whatsappId);

            //HttpResponseMessage? responseMessage = null;
            HttpClient client = new HttpClient();
            var url = $"https://graph.facebook.com/{whatsappAccount.Version}/{whatsappAccount.PhoneNoID}/messages";
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", whatsappAccount.Token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var itemjson = JsonConvert.SerializeObject(content);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new StringContent(itemjson, Encoding.UTF8, "application/json");//CONTENT-TYPE header

            var response = await client.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var r = JsonConvert.DeserializeObject<MessageResponseModel>(responseContent);
                if (r != null && r.messages != null)
                {
                    res.MessageID = r.messages[0].id;
                }
            }
            else
            {
                res.IsFailed = true;
                res.ErrorMessage = response.Headers.WwwAuthenticate.FirstOrDefault().ToString();
            }
            return res;
        }

        private async Task<T> GetTempURL<T>(int whatsappId, string url)
        {
            HttpClient client = new HttpClient();
            var whatsappAccount = await _dbContext.GetAsync<WhatsappAccount>(whatsappId);
            url = $"https://graph.facebook.com/{whatsappAccount.Version}/{url}";
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", whatsappAccount.Token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);

            var response = await client.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var result = System.Text.Json.JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result;
            }
            else
                return default(T);
        }

        private async Task<byte[]> GetMediaFileFromFB(int whatsappId, string url, string mimeType)
        {
            HttpClient client = new HttpClient();
            var whatsappAccount = await _dbContext.GetAsync<WhatsappAccount>(whatsappId);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", whatsappAccount.Token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("User-Agent", "C# App");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);

            var response = await client.SendAsync(request);

            Console.WriteLine(response.Content);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Stream imageStream = await response.Content.ReadAsStreamAsync();
                byte[] bytes = new byte[imageStream.Length];
                imageStream.Read(bytes, 0, (int)imageStream.Length);
                return bytes;
            }
            return null;
        }


        private async Task<int> SaveChat(int whatsappAccountID, int contactId, string message, SendMessageReposnseModel messageRes, string messageType, bool isSupportChatStarted, int? recipientId = null, int? mediaID = null)
        {
            WhatsappChat chat = new()
            {
                ContactID = contactId,
                AddedOn = DateTime.UtcNow,
                Message = message,
                MessageTypeID = GetMessageTypeID(messageType),
                WhatsappAccountID = whatsappAccountID,
                IsIncoming = false,
                MessageID = messageRes.MessageID,
                ReceipientID = recipientId
            };
            if (messageRes.IsFailed)
            {
                chat.MessageStatus = (int)MessageStatus.Failed;
                chat.FailedReason = messageRes.ErrorMessage;
            }
            else
            {
                chat.MessageStatus = (int)MessageStatus.Sent;
            }
            chat.ChatID = await _dbContext.SaveAsync(chat, needLog: false);

            if (mediaID.HasValue)
            {
                WhatsappChatMedia media = new()
                {
                    ChatID = chat.ChatID,
                    MediaID = mediaID.Value,
                };
                await _dbContext.SaveAsync(media, needLog: false);
            }

            if (isSupportChatStarted)
            {
                var session = await GetLastSession(whatsappAccountID, contactId);
                if (session != null)
                {
                    await _dbContext.ExecuteAsync("Update WhatsappChatSession Set HasSupportChat=1,IsFinished=0,ReminderSent=0 Where SessionID=@SessionID", session);
                }
            }
            return chat.ChatID;
        }
        #endregion

        public async Task<string> UploadSessionMedia(int whatsappId, IFormFile file)
        {
            var sessionId = "";

            var whatsappAccount = await _dbContext.GetAsync<WhatsappAccount>(whatsappId);
            HttpClient client = new HttpClient();
            var url = $"https://graph.facebook.com/{whatsappAccount.Version}/{whatsappAccount.AppID}/uploads";
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", whatsappAccount.Token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var itemjson = JsonConvert.SerializeObject(new
            {
                file_length = file.Length,
                file_type = file.ContentType,
                file_name = file.FileName
            });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new StringContent(itemjson, Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var uploadSessionResponse = JsonConvert.DeserializeObject<SessionResponse>(responseContent);
                if (uploadSessionResponse != null && uploadSessionResponse.id != null)
                {
                    sessionId = uploadSessionResponse.id;
                }

                var dataBinary = new byte[50000000];
                await file.OpenReadStream().ReadAsync(dataBinary);
                using (var httpClient = new HttpClient())
                {
                    // Set the timeout to 5 minutes.
                    httpClient.Timeout = new TimeSpan(0, 5, 0);

                    using (var request1 = new HttpRequestMessage(new HttpMethod("POST"), $"https://graph.facebook.com/{whatsappAccount.Version}/{sessionId}"))
                    {
                        request1.Headers.TryAddWithoutValidation("Authorization", "OAuth " + whatsappAccount.Token);
                        request1.Headers.TryAddWithoutValidation("file_offset", "0");

                        request1.Content = new ByteArrayContent(dataBinary);
                        request1.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

                        var response1 = await httpClient.SendAsync(request1);
                        if (!response1.IsSuccessStatusCode)
                        {
                            return null;
                        }

                        var responseContent1 = response1.Content.ReadAsStringAsync().Result;
                        var result = System.Text.Json.JsonSerializer.Deserialize<MediaUploadSessionResponse>(responseContent1);
                        return result.h;
                    }
                }
            }
            else
            {
                throw new PBException(response.Headers.WwwAuthenticate.FirstOrDefault().ToString());
            }
        }

        #region Template

        public async Task<int> SaveWhatsappTemplate(WhatsappTemplatePageModel templateModel, IDbTransaction? tran = null)
        {
            WhatsappTemplate template = new()
            {
                TemplateID = templateModel.TemplateID,
                TemplateName = templateModel.TemplateName,
                ClientID = templateModel.ClientID,
                WhatsappAccountID = templateModel.WhatsappAccountID,
                CategoryID = templateModel.CategoryID,
                LanguageID = templateModel.LanguageID,
                HeaderTypeID = templateModel.HeaderTypeID,
                HeaderText = templateModel.HeaderText,
                HeaderMediaTypeID = templateModel.HeaderMediaTypeID,
                HeaderMediaID = templateModel.HeaderMediaID,
                HeaderMediaHandle = templateModel.HeaderMediaHandle,
                Body = templateModel.Body,
                Footer = templateModel.Footer,
                WhatsappTemplateID = templateModel.WhatsappTemplateID,
                StatusReason = templateModel.StatusReason,
                StatusID = templateModel.StatusID
            };
            template.TemplateID = await _dbContext.SaveAsync(template, tran);

            //Variables
            if (templateModel.Variables is not null && templateModel.Variables.Count > 0)
            {
                List<WhatsappTemplateVariable> variableList = new();
                foreach (var variable in templateModel.Variables)
                {
                    WhatsappTemplateVariable variables = new()
                    {
                        VariableID = variable.VariableID,
                        VariableName = variable.VariableName,
                        TemplateID = variable.TemplateID,
                        Section = variable.Section,
                        DataType = variable.DataType,
                        SampleValue = variable.SampleValue,
                        OrderNo = variable.OrderNo,
                    };
                    variableList.Add(variables);
                }
                await _dbContext.SaveSubItemListAsync(variableList, "TemplateID", template.TemplateID, tran);
            }

            //Butons
            if (templateModel.Buttons is not null && templateModel.Buttons.Count > 0)
            {
                List<WhatsappTemplateButton> ButtonList = new();
                foreach (var templateButton in templateModel.Buttons)
                {
                    WhatsappTemplateButton Button = new()
                    {
                        ButtonID = templateButton.ButtonID,
                        Text = templateButton.Text,
                        TemplateID = templateButton.TemplateID,
                        Type = templateButton.Type,
                        Url = templateButton.Url,
                        UrlType = templateButton.UrlType,
                        Phone = templateButton.Phone,
                        CountryID = templateButton.CountryID,
                        VariableID = templateButton.VariableID
                    };

                    if (templateButton.ButtonVariable is not null)
                    {
                        //Saving url button variable
                        WhatsappTemplateVariable urlButtonVariable = new()
                        {
                            VariableID = templateButton.ButtonVariable.VariableID,
                            VariableName = templateButton.ButtonVariable.VariableName,
                            TemplateID = template.TemplateID,
                            Section = templateButton.ButtonVariable.Section,
                            DataType = templateButton.ButtonVariable.DataType,
                            SampleValue = templateButton.ButtonVariable.SampleValue,
                            OrderNo = templateButton.ButtonVariable.OrderNo,
                        };
                        Button.VariableID = await _dbContext.SaveAsync(urlButtonVariable, tran);
                    }

                    ButtonList.Add(Button);
                }
                await _dbContext.SaveSubItemListAsync(ButtonList, "TemplateID", template.TemplateID, tran);
            }

            return template.TemplateID;

        }

        public async Task<WhatsappTemplateTableModel> GetTemplate(int templateID, IDbTransaction? tran = null)
        {
            var result = await _dbContext.GetByQueryAsync<WhatsappTemplateTableModel>($@"Select T.*,A.Name As AccountName,L.LanguageName,M.FileName
                                                                                    From WhatsappTemplate T 
                                                                                    Left Join WhatsappAccount A ON A.WhatsappAccountID=T.WhatsappAccountID And A.IsDeleted=0
                                                                                    Left Join Language L ON L.LanguageID=T.LanguageID And L.IsDeleted=0
                                                                                    Left Join Media M ON M.MediaID=T.HeaderMediaID AND M.IsDeleted=0
                                                                                    Where TemplateID={templateID}", null, tran);

            result.FileName = _config["ServerURL"] + result.FileName;
            result.Variables = await GetTemplateVariables(templateID, tran);
            result.Buttons = await GetTemplateButtons(templateID, tran);
            return result;
        }

        public async Task<bool> DeleteTemplate(int templateID, IDbTransaction? tran = null)
        {
            if (await DeleteTemplateWACloudRequest(templateID, tran))
            {
                await _dbContext.DeleteAsync<WhatsappTemplate>(templateID, tran);
                await _dbContext.ExecuteAsync($"Update WhatsappTemplateVariable Set IsDeleted=1 Where TemplateID={templateID}",null, tran);
                await _dbContext.ExecuteAsync($"Update WhatsappTemplateButton Set IsDeleted=1 Where TemplateID={templateID}", null, tran);
                return true;
            }
            else
            {
                throw new Exception("SomethingWentWrong");
            }
        }

        public async Task<TemplateDetailsModel> GetTemplateDetailsForBroadcast(int templateID)
        {
            TemplateDetailsModel res = await _dbContext.GetByQueryAsync<TemplateDetailsModel>($"Select HeaderTypeID,HeaderMediaID,HeaderMediaTypeID From WhatsappTemplate Where TemplateID={templateID}",null);
            res.Variables = await _dbContext.GetListByQueryAsync<IdnValuePair>($@"Select VariableID as ID,VariableName as Value
                    from WhatsappTemplateVariable
                    Where IsDeleted=0 and TemplateID={templateID}
                    Order by Section,VariableID", null);
            return res;
        }

        public async Task<TemplateVariableListModel?> GetTemplateVariableList(int templateID)
        {
            TemplateVariableListModel res = await _dbContext.GetByQueryAsync<TemplateVariableListModel>($"Select HeaderTypeID,HeaderMediaID,HeaderMediaTypeID From WhatsappTemplate Where TemplateID={templateID}", null);

            res.Variables = await _dbContext.GetListByQueryAsync<IdnValuePair>($@"Select VariableID as ID,VariableName as Value
                    from WhatsappTemplateVariable
                    Where IsDeleted=0 and TemplateID={templateID}
                    Order by Section,VariableID", null);
            return res;
        }

        public async Task<byte[]?> GetTemplateVariableExcelData(int TempalteID, IDbTransaction? tran = null)
        {

            TemplateVariableListModel res = await _dbContext.GetByQueryAsync<TemplateVariableListModel>($"Select HeaderTypeID,HeaderMediaID,HeaderMediaTypeID From WhatsappTemplate Where TemplateID={TempalteID}", null);
            if (res != null)
            {
                res.Variables = await _dbContext.GetListByQueryAsync<IdnValuePair>($@"Select VariableID as ID,VariableName as Value
                    from WhatsappTemplateVariable
                    Where IsDeleted=0 and TemplateID={TempalteID}
                    Order by Section,VariableID", null);
            }

            string? DomainUrl = _config.GetValue<string>("ServerURL");
            IWorkbook workbook;
            workbook = new XSSFWorkbook();
            ISheet excelSheet = workbook.CreateSheet("AISC Membership List");
            excelSheet.DefaultColumnWidth = 20;
            // Create cell style for header row
            ICellStyle headerStyle = workbook.CreateCellStyle();
            headerStyle.FillForegroundColor = IndexedColors.BlueGrey.Index;
            headerStyle.FillPattern = FillPattern.SolidForeground;
            headerStyle.BorderBottom = BorderStyle.Thin;
            headerStyle.BottomBorderColor = IndexedColors.White.Index;

            // Create font for header row
            IFont headerFont = workbook.CreateFont();
            headerFont.IsBold = true;
            headerFont.Color = IndexedColors.White.Index;
            headerFont.FontHeightInPoints = 12;
            headerStyle.SetFont(headerFont);

            #region SetColumnWidth

            excelSheet.SetColumnWidth(0, 10 * 256);
            excelSheet.SetColumnWidth(1, 15 * 256);
            excelSheet.SetColumnWidth(8, 10 * 256);
            excelSheet.SetColumnWidth(9, 10 * 256);
            excelSheet.SetColumnWidth(10, 23 * 256);

            #endregion

            IRow row = excelSheet.CreateRow(0);
            excelSheet.DefaultRowHeightInPoints = 20;

            row.CreateCell(0).SetCellValue("Phone");

            int Count = res.Variables.Count;
            if (Count > 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    row.CreateCell(i + 1).SetCellValue(res.Variables[i].Value);
                }

            }

            byte[]? fileContents = null;
            using (var memoryStream = new MemoryStream())
            {
                workbook.Write(memoryStream);
                fileContents = memoryStream.ToArray();
            }

            return fileContents;
        }

        #region Template Variable

        public async Task<int> SaveTemplateVariable(TemplateVariablePageModel variableModel, IDbTransaction? tran = null)
        {
            WhatsappTemplateVariable templateVariable = new()
            {
                VariableID = variableModel.VariableID,
                VariableName = variableModel.VariableName,
                TemplateID = variableModel.TemplateID,
                Section = variableModel.Section,
                DataType = variableModel.DataType,
                SampleValue = variableModel.SampleValue,
                OrderNo = variableModel.OrderNo,
            };
            return (await _dbContext.SaveAsync(templateVariable, tran));
        }

        public async Task<List<WhatsappTemplateVariableModel>?> GetTemplateVariables(int templateID, IDbTransaction? tran = null)
        {
            return await _dbContext.GetListByQueryAsync<WhatsappTemplateVariableModel>($"Select * From WhatsappTemplateVariable V Where V.IsDeleted=0 And V.TemplateID={templateID} And V.Section<>{(int)WhatsappTemplateVariableSection.BUTTON}", null, tran);
        }

        #endregion

        #region Template Button

        public async Task<int> SaveTemplateButton(TemplateButtonPageModel buttonModel, IDbTransaction? tran = null)
        {
            WhatsappTemplateButton Button = new()
            {
                ButtonID = buttonModel.ButtonID,
                Text = buttonModel.Text,
                TemplateID = buttonModel.TemplateID,
                Type = buttonModel.Type,
                Url = buttonModel.Url,
                UrlType = buttonModel.UrlType,
                Phone = buttonModel.Phone,
                CountryID = buttonModel.CountryID,
                VariableID = buttonModel.VariableID
            };

            if (buttonModel.ButtonVariable is not null)
            {
                WhatsappTemplateVariable buttonVariable = new()
                {
                    VariableID = buttonModel.ButtonVariable.VariableID,
                    VariableName = buttonModel.ButtonVariable.VariableName,
                    TemplateID = buttonModel.ButtonVariable.TemplateID,
                    Section = buttonModel.ButtonVariable.Section,
                    DataType = buttonModel.ButtonVariable.DataType,
                    SampleValue = buttonModel.ButtonVariable.SampleValue,
                    OrderNo = buttonModel.ButtonVariable.OrderNo,
                };
                buttonVariable.VariableID = await _dbContext.SaveAsync(buttonVariable, tran);
            }

            Button.ButtonID = await _dbContext.SaveAsync(Button, tran);
            return (Button.ButtonID);
        }
        public async Task<List<WhatsappTemplateButtonModel>?> GetTemplateButtons(int templateID, IDbTransaction? tran = null)
        {
            var buttons = await _dbContext.GetListByQueryAsync<WhatsappTemplateButtonModel>($@"Select B.*,C.CountryName
                                                                                            From WhatsappTemplateButton B 
                                                                                            Left Join Country C ON C.CountryID=B.CountryID And C.IsDeleted=0
                                                                                            Where B.IsDeleted=0 And B.TemplateID={templateID}", null, tran);

            foreach (var button in buttons)
            {
                if (button.VariableID is not null)
                {
                    button.ButtonVariable = _mapper.Map<WhatsappTemplateVariableModel>(await _dbContext.GetAsync<WhatsappTemplateVariable>(button.VariableID.Value, tran));
                }
            }
            return buttons;
        }

        #endregion

        #region Cloud Api Template 

        public async Task<CreateTemplateResultModel> GenerateCloudAPICreateTemplatePostRequest(WhatsappTemplatePageModel templateModel, IDbTransaction? tran = null)
        {
            object templateCloudApiObject = await GetCreateTemplateJsonModel(templateModel, tran);
            var result = await CreateTemplateWACloudPostRequest(templateModel.WhatsappAccountID.Value, templateCloudApiObject, tran);
            return result;
        }

        private async Task<WANewTemplatePostModel> GetCreateTemplateJsonModel(WhatsappTemplatePageModel templateModel, IDbTransaction? tran = null)
        {
            var language = await _dbContext.GetAsync<Language>(Convert.ToInt32(templateModel.LanguageID), tran);

            // Create a new CloudAPITemplateModel object and populate its properties from whatsappTemplateModel
            WANewTemplatePostModel cloudAPITemplate = new()
            {
                name = templateModel.TemplateName,
                category = ((WhatsappTemplateCategory)Convert.ToInt32(templateModel.CategoryID)).ToString(),
                language = language.LanguageCode,
                components = new()
            };

            // Populate the components list

            #region HEADER

            //HEADER TEXT
            if (templateModel.HeaderTypeID == (int)WhatsappTemplateHeaderType.TEXT)
            {
                List<string>? header_text = null;
                if (templateModel.Variables is not null)
                {
                    var headerVariable = templateModel.Variables.Where(variable => variable.Section == (int)WhatsappTemplateVariableSection.HEADER).FirstOrDefault();
                    if (headerVariable is not null && headerVariable.SampleValue is not null)
                    {
                        header_text = new();
                        header_text.Add(headerVariable.SampleValue);
                    }
                }

                cloudAPITemplate.components.Add(new()
                {
                    type = "HEADER",
                    format = "TEXT",
                    text = templateModel.HeaderText,
                    example = header_text is not null ? new() { header_text = header_text } : null,
                });
            }

            //HEADER MEDIA
            if (templateModel.HeaderTypeID == (int)WhatsappTemplateHeaderType.MEDIA && templateModel.HeaderMediaID.HasValue)
            {
                //var media = await _dbContext.GetAsync<Media>(templateModel.HeaderMediaID.Value,tran);
                //string mediaFileURL = _config["ServerURL"] + media.FileName;

                switch (templateModel.HeaderMediaTypeID)
                {
                    case (int)WhatsappTemplateHeaderMediaType.IMAGE:
                        cloudAPITemplate.components.Add(new()
                        {
                            type = "HEADER",
                            format = "IMAGE",
                            example = new()
                            {
                                header_handle = templateModel.HeaderMediaHandle
                            }
                        });
                        break;

                    case (int)WhatsappTemplateHeaderMediaType.DOCUMENT:
                        cloudAPITemplate.components.Add(new()
                        {
                            type = "HEADER",
                            format = "DOCUMENT",
                            example = new()
                            {
                                header_handle = templateModel.HeaderMediaHandle
                            }
                        });
                        break;

                    case (int)WhatsappTemplateHeaderMediaType.VIDEO:
                        cloudAPITemplate.components.Add(new()
                        {
                            type = "HEADER",
                            format = "VIDEO",
                            example = new()
                            {
                                header_handle = templateModel.HeaderMediaHandle
                            }
                        });
                        break;
                }
            }

            #endregion

            #region BODY

            List<string?>? body_text = null;
            if (templateModel.Variables is not null)
            {
                var bodyVariable = templateModel.Variables.Where(variable => variable.Section == (int)WhatsappTemplateVariableSection.BODY).ToList();
                if (bodyVariable is not null && bodyVariable.Count > 0)
                {
                    body_text = new();
                    for (int i = 0; i < bodyVariable.Count; i++)
                    {
                        body_text.Add(bodyVariable[i].SampleValue);
                    }
                }
            }

            cloudAPITemplate.components.Add(new()
            {
                type = "BODY",
                text = templateModel.Body,
                example = body_text is not null ? new() { body_text = body_text } : null,
            });

            #endregion

            #region FOOTER

            if (!string.IsNullOrEmpty(templateModel.Footer))
            {
                cloudAPITemplate.components.Add(new()
                {
                    type = "FOOTER",
                    text = templateModel.Footer,
                });
            }

            #endregion

            #region BUTTONS

            if (templateModel.Buttons is not null && templateModel.Buttons.Count > 0)
            {
                List<PB.Shared.Models.WhatsAppModels.Button> buttons = new();
                foreach (var button in templateModel.Buttons)
                {
                    PB.Shared.Models.WhatsAppModels.Button btn = new()
                    {
                        type = ((WhatsappTemplateButtonType)button.Type).ToString(),
                        text = button.Text,
                    };

                    switch (button.Type)
                    {
                        case (int)WhatsappTemplateButtonType.URL:
                            btn.url = button.Url;
                            if (button.UrlType == (int)WhatsappTemplateButtonUrlType.DYNAMIC && button.VariableID.HasValue)
                            {
                                string? sampleValue = button.ButtonVariable is not null ? button.ButtonVariable.SampleValue : null;
                                btn.example = new List<string?> { sampleValue };
                            }
                            break;

                        case (int)WhatsappTemplateButtonType.PHONE_NUMBER:
                            if (button.CountryID.HasValue)
                            {
                                string? isdCode = await _dbContext.GetFieldsAsync<Country, string?>("ISDCode", $"CountryID={button.CountryID.Value}", null, tran);
                                btn.phone_number = isdCode + button.Phone;
                            }
                            else
                            {
                                btn.phone_number = button.Phone;
                            }
                            break;
                    }

                    buttons.Add(btn);
                }
                cloudAPITemplate.components.Add(new()
                {
                    type = "BUTTONS",
                    buttons = buttons,
                });
            }

            #endregion

            return cloudAPITemplate;

        }

        public async Task<EditTemplateResponseModel> GenerateCloudAPIEditTemplatePostRequest(WhatsappTemplatePageModel templateModel, IDbTransaction? tran = null)
        {
            object templateCloudApiObject = await GetEditTemplateJsonModel(templateModel, tran);
            var result = await EditTemplateWACloudPostRequest(templateModel.WhatsappAccountID.Value, templateModel.WhatsappTemplateID, templateCloudApiObject, tran);
            return result;
        }

        private async Task<WATemplateEditPostModel> GetEditTemplateJsonModel(WhatsappTemplatePageModel templateModel, IDbTransaction? tran = null)
        {
            WATemplateEditPostModel editTemplateModel = new();
            editTemplateModel.components = new();
            var template = await _dbContext.GetAsync<WhatsappTemplate>(templateModel.TemplateID, tran);
            if (templateModel.CategoryID is not null && template.CategoryID != templateModel.CategoryID)
            {
                editTemplateModel.category = (((WhatsappTemplateCategory)templateModel.CategoryID).ToString());
            }

            #region HEADER

            //HEADER TEXT
            if (templateModel.HeaderTypeID == (int)WhatsappTemplateHeaderType.TEXT)
            {
                List<string?>? header_text = new();
                if (templateModel.Variables is not null)
                {
                    var headerVariable = templateModel.Variables.Where(variable => variable.Section == (int)WhatsappTemplateVariableSection.HEADER).FirstOrDefault();
                    if (headerVariable is not null && headerVariable.SampleValue is not null)
                    {
                        header_text.Add(headerVariable.SampleValue);
                    }
                }

                editTemplateModel.components.Add(new()
                {
                    type = "HEADER",
                    format = "TEXT",
                    text = templateModel.HeaderText,
                    example = header_text is not null ? new() { header_text = header_text } : null,
                });
            }

            //HEADER MEDIA
            if (templateModel.HeaderTypeID == (int)WhatsappTemplateHeaderType.MEDIA && templateModel.HeaderMediaID.HasValue)
            {
                //var media = await _dbContext.GetAsync<Media>(templateModel.HeaderMediaID.Value,tran);
                //string mediaFileURL = _config["ServerURL"] + media.FileName;

                switch (templateModel.HeaderMediaTypeID)
                {
                    case (int)WhatsappTemplateHeaderMediaType.IMAGE:
                        editTemplateModel.components.Add(new()
                        {
                            type = "HEADER",
                            format = "IMAGE",
                            example = new()
                            {
                                header_handle = templateModel.HeaderMediaHandle
                            }
                        });
                        break;

                    case (int)WhatsappTemplateHeaderMediaType.DOCUMENT:
                        editTemplateModel.components.Add(new()
                        {
                            type = "HEADER",
                            format = "DOCUMENT",
                            example = new()
                            {
                                header_handle = templateModel.HeaderMediaHandle
                            }
                        });
                        break;

                    case (int)WhatsappTemplateHeaderMediaType.VIDEO:
                        editTemplateModel.components.Add(new()
                        {
                            type = "HEADER",
                            format = "VIDEO",
                            example = new()
                            {
                                header_handle = templateModel.HeaderMediaHandle
                            }
                        });
                        break;
                }
            }

            #endregion

            #region BODY

            List<string?>? body_text = null;
            if (templateModel.Variables is not null)
            {
                var bodyVariable = templateModel.Variables.Where(variable => variable.Section == (int)WhatsappTemplateVariableSection.BODY).ToList();
                if (bodyVariable is not null && bodyVariable.Count > 0)
                {
                    body_text = new();
                    for (int i = 0; i < bodyVariable.Count; i++)
                    {
                        body_text.Add(bodyVariable[i].SampleValue);
                    }
                }
            }

            editTemplateModel.components.Add(new()
            {
                type = "BODY",
                text = templateModel.Body,
                example = body_text is not null ? new() { body_text = body_text } : null,
            });

            #endregion

            #region FOOTER

            if (!string.IsNullOrEmpty(templateModel.Footer))
            {
                editTemplateModel.components.Add(new()
                {
                    type = "FOOTER",
                    text = templateModel.Footer,
                });
            }

            #endregion

            #region BUTTONS

            if (templateModel.Buttons is not null && templateModel.Buttons.Count > 0)
            {
                List<PB.Shared.Models.WhatsAppModels.Button> buttons = new();
                foreach (var button in templateModel.Buttons)
                {
                    PB.Shared.Models.WhatsAppModels.Button btn = new()
                    {
                        type = ((WhatsappTemplateButtonType)button.Type).ToString(),
                        text = button.Text,
                    };

                    switch (button.Type)
                    {
                        case (int)WhatsappTemplateButtonType.URL:
                            btn.url = button.Url;
                            if (button.UrlType == (int)WhatsappTemplateButtonUrlType.DYNAMIC && button.VariableID.HasValue)
                            {
                                string? sampleValue = button.ButtonVariable is not null ? button.ButtonVariable.SampleValue : null;
                                btn.example = new List<string?> { sampleValue };
                            }
                            break;

                        case (int)WhatsappTemplateButtonType.PHONE_NUMBER:
                            if (button.CountryID.HasValue)
                            {
                                string? isdCode = await _dbContext.GetFieldsAsync<Country, string?>("ISDCode", $"CountryID={button.CountryID.Value}", null, tran);
                                btn.phone_number = isdCode + button.Phone;
                            }
                            else
                            {
                                btn.phone_number = button.Phone;
                            }
                            break;
                    }

                    buttons.Add(btn);
                }
                editTemplateModel.components.Add(new()
                {
                    type = "BUTTONS",
                    buttons = buttons,
                });
            }

            #endregion

            return editTemplateModel;

        }

        public async Task<CreateTemplateResultModel?> CreateTemplateWACloudPostRequest(int whatsappId, object content, IDbTransaction? tran = null)
        {
            var whatsappAccount = await _dbContext.GetAsync<WhatsappAccount>(whatsappId, tran);

            HttpClient client = new HttpClient();
            var url = $"https://graph.facebook.com/{whatsappAccount.Version}/{whatsappAccount.BusinessAccountID}/message_templates";
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", whatsappAccount.Token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var itemjson = JsonConvert.SerializeObject(content);

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(itemjson, Encoding.UTF8, "application/json")//CONTENT-TYPE header
            };

            var response = await client.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            var res = JsonConvert.DeserializeObject<CreateTemplateResultModel>(responseContent);
            return res;
        }

        public async Task<EditTemplateResponseModel> EditTemplateWACloudPostRequest(int whatsappAccountID, string whatsappTemplateID, object content, IDbTransaction? tran = null)
        {
            var whatsappAccount = await _dbContext.GetAsync<WhatsappAccount>(whatsappAccountID, tran);

            HttpResponseMessage? responseMessage = null;
            HttpClient client = new HttpClient();

            var url = $"https://graph.facebook.com/{whatsappAccount.Version}/{whatsappTemplateID}";
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", whatsappAccount.Token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var itemjson = JsonConvert.SerializeObject(content);

            HttpRequestMessage request = new(HttpMethod.Post, url)
            {
                Content = new StringContent(itemjson, Encoding.UTF8, "application/json")//CONTENT-TYPE header
            };

            responseMessage = await client.SendAsync(request);
            var responseContent = await responseMessage.Content.ReadAsStringAsync();

            var res = JsonConvert.DeserializeObject<EditTemplateResponseModel>(responseContent);
            if (res is not null)
            {

                if (res.error is not null)
                {
                    throw new Exception(res.error.error_user_msg);

                }
                else
                {
                    return res;
                }

            }
            return null;
        }

        public async Task<bool> DeleteTemplateWACloudRequest(int templateID, IDbTransaction? tran = null)
        {
            var data = await _dbContext.GetByQueryAsync<TemplateDeleteDetaisModel>($@"Select T.TemplateName,T.WhatsappTemplateID,A.*
                                                                                        From WhatsappTemplate T
                                                                                        Left Join WhatsappAccount A ON A.WhatsappAccountID=T.WhatsappAccountID And A.IsDeleted=0
                                                                                        Where T.TemplateID={templateID}", null, tran);

            HttpClient client = new HttpClient();
            var url = $"https://graph.facebook.com/{data.Version}/{data.BusinessAccountID}/message_templates?hsm_id={data.WhatsappTemplateID}&name={data.TemplateName}";
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", data.Token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, url);

            var response = await client.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }
            else
            {
                throw new PBException(response.Headers.WwwAuthenticate.FirstOrDefault().ToString());
            }
        }

        public async Task SyncTemplatesWithFB(int clientID)
        {
            var whatsappAccountModel = await _dbContext.GetByQueryAsync<WhatsappAccountDetailsModel>(@$"Select W.*,C.EntityID
                                                                                                            From WhatsappAccount W
                                                                                                            Left Join Client C ON C.ClientID=W.ClientID And C.IsDeleted=0
                                                                                                            Where W.IsDeleted=0 And W.ClientID={clientID}", null);
            if (whatsappAccountModel is not null)
            {
                WhatsappTemplatePagedListModel? fetchedData = null;
                string? url = $"https://graph.facebook.com/{whatsappAccountModel.Version}/{whatsappAccountModel.BusinessAccountID}/message_templates";

                List<Template> templateList = new();

                do
                {
                    if (fetchedData is not null && fetchedData.paging is not null && fetchedData.paging.next is not null)
                        url = fetchedData.paging.next;

                    fetchedData = await FetchFacebookTemplatesPagedList(url, whatsappAccountModel.WhatsappAccountID);

                    if (fetchedData is not null && fetchedData.data is not null)
                        templateList.AddRange(fetchedData.data);
                }
                while (fetchedData is not null && fetchedData.paging is not null && fetchedData.paging.next is not null);

                if (templateList.Count > 0)
                {
                    await VerifyFbTemplates(templateList, whatsappAccountModel.WhatsappAccountID, whatsappAccountModel.ClientID.Value);
                    await _notification.SendSignalRPush(whatsappAccountModel.EntityID.Value, "Fb template syncing is completed, please referesh the templates list page");
                }
            }
        }

        private async Task<WhatsappTemplatePagedListModel?> FetchFacebookTemplatesPagedList(string url, int whatsappAccountID)
        {
                WhatsappTemplatePagedListModel? fetchedData = null;
                string token = await _dbContext.GetFieldsAsync<WhatsappAccount, string>("Token", $"WhatsappAccountID={whatsappAccountID}", null);

                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);

                var response = await client.SendAsync(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    fetchedData = JsonConvert.DeserializeObject<WhatsappTemplatePagedListModel>(responseContent);
                }

                return fetchedData;
            
        }

        private async Task VerifyFbTemplates(List<Template> templates, int whatsappAccountID, int clientID)
        {
            for (int i = templates.Count - 1; i >= 0; i--)
            {
                string? whatsappTemplateID = templates[i].id;
                int templateID = await _dbContext.GetByQueryAsync<int>($"Select TemplateID From WhatsappTemplate Where WhatsappAccountID=@WhatsappAccountID AND WhatsappTemplateID=@WhatsappTemplateID AND IsDeleted=0", new { WhatsappAccountID = whatsappAccountID, WhatsappTemplateID = whatsappTemplateID });
                if (templateID == 0)
                    await SaveFacebookTemplateToDB(templates[i], templateID, whatsappAccountID, clientID);
            }
        }

        private async Task SaveFacebookTemplateToDB(Template fbTemplate, int templateID, int whatsappAcountID, int clientID)
        {
            //if templateID > 0 then should update the table details correspond to the fbTemplate, do later
            WhatsappTemplatePageModel template = new()
            {
                TemplateID = templateID,
                TemplateName = fbTemplate.name,
                WhatsappTemplateID = fbTemplate.id,
                WhatsappAccountID = whatsappAcountID,
                ClientID = clientID,
                LanguageID = await _dbContext.GetByQueryAsync<int>("Select TOP 1 LanguageID From Language Where LanguageCode=@LanguageCode AND IsDeleted=0", new { LanguageCode = fbTemplate.language }),
                StatusID = fbTemplate.status switch
                {
                    "PENDING" => (int)WhatsappTemplateStatus.PENDING,
                    "APPROVED" => (int)WhatsappTemplateStatus.APPROVED,
                    "REJECTED" => (int)WhatsappTemplateStatus.REJECTED,
                    "PAUSED" => (int)WhatsappTemplateStatus.PAUSED,
                    "PENDING_DELETION" => (int)WhatsappTemplateStatus.PENDING_DELETION,
                    _ => null
                },
            };

            List<TemplateVariablePageModel> templateVariables = new();
            List<TemplateButtonPageModel> templateButtons = new();

            //Template Category
            template.CategoryID = fbTemplate.category switch
            {
                "MARKETING" => (int)WhatsappTemplateCategory.MARKETING,
                "UTILITY" => (int)WhatsappTemplateCategory.UTILITY,
                "AUTHENTICATION" => (int)WhatsappTemplateCategory.AUTHENTICATION,
                _ => null
            };

            //Template Status
            template.StatusID = fbTemplate.status switch
            {
                "PENDING" => (int)WhatsappTemplateStatus.PENDING,
                "APPROVED" => (int)WhatsappTemplateStatus.APPROVED,
                "REJECTED" => (int)WhatsappTemplateStatus.REJECTED,
                "PAUSED" => (int)WhatsappTemplateStatus.PAUSED,
                "PENDING_DELETION" => (int)WhatsappTemplateStatus.PENDING_DELETION,
                _ => null
            };

            #region Header

            //Template component header type

            template.HeaderTypeID = null;
            template.HeaderMediaTypeID = null;
            template.HeaderMediaHandle = null;
            var header = fbTemplate.components.Where(component => component.type == "HEADER").FirstOrDefault();
            if (header is not null)
            {
                //Header Type
                if (header.format == "TEXT")
                {
                    //TEXT
                    template.HeaderTypeID = (int)WhatsappTemplateHeaderType.TEXT;
                    template.HeaderText = header.text;
                    if (header.example is not null && header.example.header_text is not null)
                    {
                        TemplateVariablePageModel variable = new()
                        {
                            OrderNo = 1,
                            VariableName = "HeaderVariable",
                            Section = (int)WhatsappTemplateVariableSection.HEADER,
                            DataType = (int)WhatsappTemplateVariableDataType.TEXT,
                            SampleValue = header.example.header_text[0] is not null ? header.example.header_text[0] : null,
                        };
                        templateVariables.Add(variable);
                    }
                }
                else
                {
                    //MEDIA
                    template.HeaderTypeID = (int)WhatsappTemplateHeaderType.MEDIA;
                    //Header Media Type
                    template.HeaderMediaTypeID = header.format switch
                    {
                        "IMAGE" => (int)WhatsappTemplateHeaderMediaType.IMAGE,
                        "VIDEO" => (int)WhatsappTemplateHeaderMediaType.VIDEO,
                        "DOCUMENT" => (int)WhatsappTemplateHeaderMediaType.DOCUMENT,
                        "LOCATION" => (int)WhatsappTemplateHeaderMediaType.LOCATION,
                        _ => null,
                    };

                    if (header.example is not null && header.example.header_handle is not null)
                    {
                        template.HeaderMediaHandle = header.example.header_handle[0];
                        var fileUploadResult = await DownloadFileAsync(header.example.header_handle[0]);
                        template.HeaderMediaID = fileUploadResult is not null ? fileUploadResult.MediaID : null;
                    }
                }
            }

            template.HeaderTypeID = template.HeaderTypeID is null ? (int)WhatsappTemplateHeaderType.NONE : template.HeaderTypeID;

            #endregion

            #region Body

            //Template component body type
            var body = fbTemplate.components.Where(component => component.type == "BODY").FirstOrDefault();
            if (body is not null)
            {
                template.Body = body.text;
                if (body.example is not null && body.example.body_text is not null)
                {
                    for (int i = 0; i < body.example.body_text.Count; i++)
                    {
                        // Check if the inner list exists before accessing its elements
                        if (body.example.body_text[i] != null)
                        {
                            for (int j = 0; j < body.example.body_text[i].Count; j++)
                            {
                                TemplateVariablePageModel variable = new()
                                {
                                    OrderNo = (j + 1),
                                    VariableName = "BodyVariable-" + (j + 1),
                                    Section = (int)WhatsappTemplateVariableSection.BODY,
                                    DataType = (int)WhatsappTemplateVariableDataType.TEXT,
                                    SampleValue = body.example.body_text[i][j],
                                };
                                templateVariables.Add(variable);
                            }
                        }
                    }
                }
            }

            #endregion

            #region Footer

            //Template component footer type
            var footer = fbTemplate.components.Where(component => component.type == "FOOTER").FirstOrDefault();
            if (footer is not null)
            {
                template.Footer = footer.text;
            }

            #endregion

            #region Buttons

            //Filtering buttons
            var buttons = fbTemplate.components.Where(component => component.type == "BUTTONS").FirstOrDefault();
            if (buttons is not null && buttons.buttons is not null)
            {
                foreach (var fbTemplateButton in buttons.buttons)
                {
                    TemplateButtonPageModel btn = new();
                    switch (fbTemplateButton.type)
                    {
                        case "URL":

                            MatchCollection? matches = null;
                            string pattern = @"\{\{.*?\}\}";
                            matches = Regex.Matches(fbTemplateButton.url, pattern);
                            int count = matches.Count;

                            btn.Url = fbTemplateButton.url;
                            btn.Text = fbTemplateButton.text;
                            btn.Type = (int)WhatsappTemplateButtonType.URL;

                            if (count > 0 && fbTemplateButton.example is not null)
                            {
                                btn.UrlType = (int)WhatsappTemplateButtonUrlType.DYNAMIC;

                                bool isAlreadyAdded = templateVariables.Any(url => url.Section == (int)WhatsappTemplateVariableSection.BUTTON);
                                TemplateVariablePageModel buttonVariable = new()
                                {
                                    OrderNo = 1,
                                    VariableName = !isAlreadyAdded ? "UrlVariable-1" : "UrlVariable-2",
                                    Section = (int)WhatsappTemplateVariableSection.BUTTON,
                                    DataType = (int)WhatsappTemplateVariableDataType.TEXT,
                                    SampleValue = fbTemplateButton.example[0],
                                };
                                btn.ButtonVariable = buttonVariable;
                            }
                            else
                            {

                                btn.UrlType = (int)WhatsappTemplateButtonUrlType.STATIC;
                            }
                            templateButtons.Add(btn);

                            break;

                        case "QUICK_REPLY":

                            btn.Text = fbTemplateButton.text;
                            btn.Type = (int)WhatsappTemplateButtonType.QUICK_REPLY;
                            templateButtons.Add(btn);

                            break;

                        case "PHONE_NUMBER":

                            btn.Text = fbTemplateButton.text;
                            btn.Type = (int)WhatsappTemplateButtonType.PHONE_NUMBER;
                            btn.Phone = fbTemplateButton.phone_number;
                            templateButtons.Add(btn);

                            break;
                    }
                }
            }

            #endregion

            template.Variables = templateVariables;
            template.Buttons = templateButtons;
            template.TemplateID = await SaveWhatsappTemplate(template);
        }

        private async Task<FileUploadResultModel?> DownloadFileAsync(string remoteFileUrl)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                HttpResponseMessage response = await httpClient.GetAsync(remoteFileUrl);

                if (response.IsSuccessStatusCode)
                {

                    // Find the start and end positions to extract the filename
                    int startIndex = remoteFileUrl.LastIndexOf('/') + 1;
                    int endIndex = remoteFileUrl.IndexOf('?', startIndex);

                    if (endIndex == -1)
                    {
                        endIndex = remoteFileUrl.Length;
                    }

                    // Extract the filename
                    string filename = remoteFileUrl.Substring(startIndex, endIndex - startIndex);
                    string contentType = response.Content.Headers.ContentType.MediaType;
                    long fileLength = response.Content.Headers.ContentLength ?? 0;
                    string fileExtension = Path.GetExtension(filename).TrimStart('.');

                    var res = await _media.UploadFile(0, filename, "TemplateImage", filename, contentType, fileLength, fileExtension);

                    if (res != null)
                    {
                        using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                        {
                            using (FileStream fileStream = File.Create(res.FilePath))
                            {
                                await contentStream.CopyToAsync(fileStream);
                            }
                        }
                        return res;
                    }
                }
                return null;
            }
        }

        #endregion

        #endregion

        #region Tag

        public async Task<int> GetTagID(string? tag, int clientID)
        {
            var tagMaster = await _dbContext.GetByQueryAsync<TagMaster>("Select * from TagMaster Where Tag=@Tag and ClientID=@ClientID", new { Tag = tag, ClientID = clientID });
            if (tagMaster == null)
            {
                tagMaster = new()
                {
                    Tag = tag
                };
                tagMaster.TagID = await _dbContext.SaveAsync(tagMaster);
            }
            else if (tagMaster.IsDeleted.Value)
            {
                await _dbContext.ExecuteAsync($"Update TagMaster Set IsDeleted=0 Where TagID={tagMaster.TagID}");
            }
            return tagMaster.TagID;
        }

        #endregion

        #region Broadcast

        public async Task<WhatsappBroadcastSaveResultModel> SaveBroadcastMessage(CreateBroadcastModel createBroadcastModel, IDbTransaction? tran = null)
        {
            List<WhatsappBroadcastRecipientSaveModel> recipients = new();

            int variableCount = 0;
            TemplateVariableListModel? variables = await GetTemplateVariableList(createBroadcastModel.TemplateID);

            if (variables != null && variables.Variables.Count > 0)
                variableCount = variables.Variables.Count;

            string folderName = "gallery/import/";
            if (!Directory.Exists(Path.Combine("wwwroot", folderName)))
            {
                Directory.CreateDirectory(Path.Combine("wwwroot", folderName));
            }

            string filePath = Path.Combine(_env.ContentRootPath, "wwwroot", folderName);
            filePath = filePath + "/" + "import_broadcast_" + createBroadcastModel.Title + "." + createBroadcastModel.Extension;

            //Saving excel file
            string sFileExtension = Path.GetExtension(filePath).ToLower();
            ISheet sheet;
            var stream = System.IO.File.Create(filePath);
            stream.Write(createBroadcastModel.Content, 0, createBroadcastModel.Content.Length);

            //Reading excel data and 
            using (stream)
            {
                stream.Position = 0;
                if (sFileExtension == ".xls")
                {
                    HSSFWorkbook hssfwb = new HSSFWorkbook(stream); //This will read the Excel 97-2000 formats  
                    sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook  
                }
                else
                {
                    XSSFWorkbook hssfwb = new XSSFWorkbook(stream); //This will read 2007 Excel format  
                    sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook   
                }

                IRow headerRow = sheet.GetRow(0); //Get Header Row
                int cellCount = headerRow.LastCellNum;

                //checkig submitted invalid excel file
                if (cellCount != 1 + variableCount && variables != null && variables.Variables.Count > 0)
                {
                    var columnName = "";
                    foreach (var variable in variables.Variables)
                    {
                        columnName += ", " + variable.Value;
                    }
                    throw new Exception("Data should contain Phone Number" + columnName);
                }

                for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++) //Read Excel File
                {
                    IRow row = sheet.GetRow(i);
                    if (row == null) continue;
                    if (row.Cells.All(d => d.CellType == CellType.Blank)) continue;

                    string phone = row.GetCell(0).ToString();

                    //if (string.IsNullOrEmpty(phone))
                    //{
                    //    return BadRequest(new Error() { ResponseMessage = $"Phone does not extist at {i} th row" });
                    //}

                    //adding excel sheet value to the recipient list
                    if (!string.IsNullOrEmpty(phone) && recipients.Where(s => s.Phone == phone).Count() == 0 && variables != null && variableCount > 0)
                    {
                        var recipient = new WhatsappBroadcastRecipientSaveModel()
                        {
                            Phone = phone,
                        };

                        for (int k = 0; k < variableCount; k++)
                        {
                            recipient.Parameters.Add(new()
                            {
                                VariableID = variables.Variables[k].ID,
                                Value = row.GetCell(k + 1).ToString()
                            });
                        }
                        recipients.Add(recipient);
                    }
                }
            }
            stream.Close();

            //Saving broadcast
            WhatsappBroadcast message = new()
            {
                BroadcastID = createBroadcastModel.BroadcastID,
                BroadcastName = createBroadcastModel.Title,
                TemplateID = createBroadcastModel.TemplateID,
                ScheduleTime = createBroadcastModel.ScheduleTime,
                PeriodicDay = createBroadcastModel.PeriodicDay,
                AddedOn = DateTime.UtcNow,
                HeaderMediaID = createBroadcastModel.MediaID
            };
            message.BroadcastID = await _dbContext.SaveAsync(message);

            //Saving broadcast recipients
            foreach (var recipient in recipients)
            {
                recipient.ReceipientID = await _dbContext.SaveAsync(new WhatsappBroadcastReceipient()
                {
                    BroadcastID = message.BroadcastID,
                    Phone = recipient.Phone,
                });

                if (variableCount > 0)
                {
                    List<WhatsappBroadcastParameter> paras = new();
                    foreach (var parameter in recipient.Parameters)
                    {
                        paras.Add(new WhatsappBroadcastParameter()
                        {
                            VariableID = parameter.VariableID,
                            Value = parameter.Value,
                        });
                    }
                    await _dbContext.SaveSubItemListAsync(paras, "ReceipientID", recipient.ReceipientID);
                }
            }

            return new WhatsappBroadcastSaveResultModel() { BroadcastID = message.BroadcastID, Receipients = recipients };
        }

        public async Task<WhatsappBroadcastSaveResultModel?> RecreateBroadcastMessage(RecreatBroadcastModel recreateBroadcastModel, IDbTransaction? tran = null)
        {
            //Reading recipients by applied filter
            string whereCondition = $"Where IsDeleted=0 AND BroadCastID={recreateBroadcastModel.BroadcastID}";
            var filterModel = _mapper.Map<PagedListPostModelWithFilter>(recreateBroadcastModel);
            whereCondition += _common.GeneratePagedListFilterWhereCondition(filterModel);

            var recipients = await _dbContext.GetListByQueryAsync<WhatsappBroadcastRecipientSaveModel>($@"
                                    Select ReceipientID,Phone 
                                    From WhatsappBroadcastReceipient
                                    {whereCondition}
            ", null, tran);

            //Creating new broad cast on the basis of post model
            if (recipients is not null && recipients.Count > 0)
            {
                //collecting the saved parameter for each recipient
                foreach (var recipient in recipients)
                {
                    recipient.Parameters = await _dbContext.GetListByQueryAsync<WhatsappBroadcastRecipientVariableModel>($"Select VariableID,Value From WhatsappBroadcastParameter Where ReceipientID={recipient.ReceipientID} AND IsDeleted=0", null, tran);
                }

                int variableCount = await _dbContext.GetByQueryAsync<int>($"Select Count(*) From WhatsappTemplateVariable Where TemplateID={recreateBroadcastModel.BroadcastModel.TemplateID}", null, tran);

                //Saving broadcast
                WhatsappBroadcast message = new()
                {
                    BroadcastName = recreateBroadcastModel.BroadcastModel.Title,
                    TemplateID = recreateBroadcastModel.BroadcastModel.TemplateID,
                    ScheduleTime = recreateBroadcastModel.BroadcastModel.ScheduleTime,
                    PeriodicDay = recreateBroadcastModel.BroadcastModel.PeriodicDay,
                    AddedOn = DateTime.UtcNow,
                    HeaderMediaID = recreateBroadcastModel.BroadcastModel.MediaID
                };
                message.BroadcastID = await _dbContext.SaveAsync(message, tran);

                //Saving broadcast recipients
                foreach (var recipient in recipients)
                {
                    recipient.ReceipientID = await _dbContext.SaveAsync(new WhatsappBroadcastReceipient()
                    {
                        BroadcastID = message.BroadcastID,
                        Phone = recipient.Phone,
                    }, tran);

                    if (variableCount > 0)
                    {
                        List<WhatsappBroadcastParameter> broadcastParameters = new();
                        foreach (var parameter in recipient.Parameters)
                        {
                            broadcastParameters.Add(new WhatsappBroadcastParameter()
                            {
                                VariableID = parameter.VariableID,
                                Value = parameter.Value,
                            });
                        }
                        await _dbContext.SaveSubItemListAsync(broadcastParameters, "ReceipientID", recipient.ReceipientID, tran);
                    }
                }

                return new WhatsappBroadcastSaveResultModel() { BroadcastID = message.BroadcastID, Receipients = recipients };
            }
            return null;
        }

        public async Task<string?> GetBroadcastReceipientExcelData(MessageReceipientSearchModel model)
        {
            string? condition = null;

            var filterModel = _mapper.Map<PagedListPostModelWithFilter>(model);
            condition = _common.GeneratePagedListFilterWhereCondition(filterModel, "");
            var result = await _dbContext.GetListByQueryAsync<BroadcastReceipientHistoryExcelModel>($@"Select C.Phone, C.Name,MessageStatus,Convert(varchar,W.AddedOn,20) as SentOn
            ,Convert(varchar,ISNULL(W.DeliveredOn,W.SeenOn),20) as DeliveredOn,FailedReason
            ,Convert(varchar,W.SeenOn,20) as SeenOn,Message
            from WhatsappChat W
            JOIN WhatsappBroadcastReceipient R on R.ReceipientID=W.ReceipientID and R.IsDeleted=0
            JOIN WhatsappBroadcast B on B.BroadcastID=R.BroadcastID and B.IsDeleted=0
            JOIN WhatsappContact C on C.ContactID=W.ContactID and C.IsDeleted=0
             Where B.BroadcastID={model.BroadcastID} and W.IsDeleted=0 {condition}
            ", null);

            string? BroadcastName = await _dbContext.GetByQueryAsync<string?>($@"Select BroadcastName 
                                                                                From WhatsappBroadcast
                                                                                Where BroadcastID={model.BroadcastID} and IsDeleted=0", null);


            string? DomainUrl = _config.GetValue<string>("ServerURL");
            IWorkbook workbook;
            workbook = new XSSFWorkbook();
            ISheet excelSheet = workbook.CreateSheet("AISC Membership List");
            excelSheet.DefaultColumnWidth = 20;
            // Create cell style for header row
            ICellStyle headerStyle = workbook.CreateCellStyle();
            headerStyle.FillForegroundColor = IndexedColors.BlueGrey.Index;
            headerStyle.FillPattern = FillPattern.SolidForeground;
            headerStyle.BorderBottom = BorderStyle.Thin;
            headerStyle.BottomBorderColor = IndexedColors.White.Index;

            // Create font for header row
            IFont headerFont = workbook.CreateFont();
            headerFont.IsBold = true;
            headerFont.Color = IndexedColors.White.Index;
            headerFont.FontHeightInPoints = 12;
            headerStyle.SetFont(headerFont);

            #region SetColumnWidth

            excelSheet.SetColumnWidth(0, 10 * 256);
            excelSheet.SetColumnWidth(1, 15 * 256);
            excelSheet.SetColumnWidth(8, 10 * 256);
            excelSheet.SetColumnWidth(9, 10 * 256);
            excelSheet.SetColumnWidth(10, 23 * 256);

            #endregion

            IRow row = excelSheet.CreateRow(0);

            excelSheet.DefaultRowHeightInPoints = 20;

            row.CreateCell(0).SetCellValue("Name");
            row.CreateCell(1).SetCellValue("Phone");
            row.CreateCell(2).SetCellValue("Message");
            row.CreateCell(3).SetCellValue("Status");
            row.CreateCell(4).SetCellValue("Sent On");
            row.CreateCell(5).SetCellValue("Delivered On");
            row.CreateCell(6).SetCellValue("Seen On");
            row.CreateCell(7).SetCellValue("Failed Reason");

            int rowindex = 0;
            for (int i = 0; i < row.LastCellNum; i++)
            {
                row.GetCell(i).CellStyle = headerStyle;
            }
            byte[]? fileContents = null;
            string? filePath = null;
            string? newpath = null;
            string? pathValue = null;

            try
            {
                foreach (var item in result)
                {
                    row = excelSheet.CreateRow(++rowindex);

                    row.CreateCell(0).SetCellValue(item.Name);
                    row.CreateCell(1).SetCellValue(item.Phone);
                    row.CreateCell(2).SetCellValue(item.Message);
                    row.CreateCell(3).SetCellValue(((MessageStatus)item.MessageStatus).ToString());
                    row.CreateCell(4).SetCellValue(item.SentOn);
                    row.CreateCell(5).SetCellValue(item.DeliveredOn);
                    row.CreateCell(6).SetCellValue(item.SeenOn);
                    row.CreateCell(7).SetCellValue(item.FailedReason);
                }

                using (var memoryStream = new MemoryStream())
                {
                    workbook.Write(memoryStream);
                    fileContents = memoryStream.ToArray();
                }

                string value = BroadcastName + "_logs" + ".xlsx";
                string folderName = "gallery/Broadcast_logs";
                string wwwrootPath = Path.Combine(_env.ContentRootPath, "wwwroot");
                filePath = Path.Combine(wwwrootPath, folderName, value);
                newpath = filePath.Replace("\\", "/");

                try
                {
                    if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    }

                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                    System.IO.File.WriteAllBytes(filePath, fileContents);
                    pathValue = folderName + "/" + value;
                }
                catch (Exception ex)
                {
                    // Log or handle the exception appropriately
                    pathValue = "Error: " + ex.Message;
                }

            }
            catch (Exception ex)
            {

            }

            return pathValue;
        }

        #endregion








    }
}
