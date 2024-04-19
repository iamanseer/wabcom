using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PB.DatabaseFramework;
using PB.Model;
using PB.Server.Repository;
using PB.Shared.Enum;
using PB.Shared;
using PB.Shared.Models;
using PB.Shared.Tables;
using System.ComponentModel;
using Hangfire;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using PB.Client.Pages;
using Org.BouncyCastle.Cms;
using PB.Client.Pages.Whatsapp;
using NPOI.SS.Formula.Functions;
using Microsoft.Extensions.Options;
using System;
using NAudio.Utils;
using NPOI.OpenXmlFormats.Wordprocessing;
using PB.Client.Pages.SuperAdmin;
using NPOI.POIFS.Crypt.Dsig;
using System.Data;
using AutoMapper;
using PB.Shared.Tables.Whatsapp;
using Microsoft.Web.Http;
using PB.Shared.Models.WhatsaApp;
using PB.Shared.Models.WhatsAppModels;
using PB.Shared.Enum.WhatsApp;
using PB.EntityFramework;

namespace PB.Server.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class WhatsappController : BaseController
    {
        private readonly IDbContext _dbContext;
        private readonly IWhatsappRepository _whatsapp;
        private readonly IBackgroundJobClient _job;
        private readonly IHostEnvironment _env;
        private readonly IConfiguration _config;
        private readonly IMediaRepository _media;
        private readonly IDbConnection _cn;
        private readonly ICommonRepository _common;
        private readonly IMapper _map;
        private readonly IInventoryRepository _inventory;

        public WhatsappController(IDbContext dbContext, IWhatsappRepository whatsapp, IBackgroundJobClient job, IHostEnvironment env, IConfiguration config, IMediaRepository media, IDbConnection cn, ICommonRepository common, IMapper map, IInventoryRepository inventory)
        {
            _dbContext = dbContext;
            _whatsapp = whatsapp;
            _job = job;
            _env = env;
            _config = config;
            _media = media;
            _cn = cn;
            _common = common;
            _map = map;
            _inventory = inventory;
        }



        #region Account

        [HttpGet("get-accounts")]
        public async Task<IActionResult> GetWhatsappAccounts()
        {
            var result = await _dbContext.GetListOfIdValuePairAsync<WhatsappAccount>("Name", $"IsDeleted=0 and ClientID={CurrentClientID}", null);
            return Ok(result);
        }

        [HttpPost("get-all-whatsapp-accounts")]
        public async Task<IActionResult> GetAllWhatsappAccounts(PagedListPostModel model)
        {
            PagedListQueryModel SearchData = model;
            SearchData.Select = "Select WhatsappAccountID,Name,PhoneNo,PhoneNoID,BusinessAccountID,Token from WhatsappAccount";
            SearchData.WhereCondition = $"IsDeleted=0 and ClientID={CurrentClientID}";
            var result = await _dbContext.GetPagedList<WhatsappAccount>(SearchData, null);
            return Ok(result);
        }

        [HttpPost("save-whatsapp-account")]
        public async Task<IActionResult> SaveWhatsAppAccount(WhatsappAccount model)
        {

            var logSummaryID = await _dbContext.InsertAddEditLogSummary(model.WhatsappAccountID, "Whatsapp Account " + model.PhoneNo);

            model.ClientID = CurrentClientID;
            model.WhatsappAccountID = await _dbContext.SaveAsync(model);

            var chatReply = await _dbContext.GetAsync<ChatbotReply>($"WhatsappAccountID={model.WhatsappAccountID} and ParentReplyID is null", null);
            if (chatReply == null)
            {
                chatReply = new()
                {
                    WhatsappAccountID = model.WhatsappAccountID,
                    Detailed = "Hi, Thank you for contacting us. Please choose the desired service from the list",
                    ReplyTypeID = (int)ChatbotReplyTypes.Forward
                };
                await _dbContext.SaveAsync(chatReply, logSummaryID: logSummaryID);
            }
            return Ok(new Success());
        }

        [HttpGet("get-whatsapp-account/{Id}")]
        public async Task<IActionResult> GetWhatsappAccount(int Id)
        {
            var result = await _dbContext.GetAsync<WhatsappAccount>(Id);
            return Ok(result);
        }

        [HttpGet("delete-whatsapp-account")]
        public async Task<IActionResult> DeleteWhatsappAccount(int Id)
        {
            var result = await _dbContext.GetAsync<WhatsappAccount>(Id);
            await _dbContext.DeleteAsync<WhatsappAccount>(Id);
            await _dbContext.InsertDeleteLogSummary(Id, result.Name);
            return Ok(result);
        }


        [HttpGet("get-languages")]
        public async Task<IActionResult> GetLanguages()
        {
            var result = await _dbContext.GetListOfIdValuePairAsync<Language>("LanguageName", "", null);
            result = result.OrderBy(s => s.Value).ToList();
            return Ok(result);
        }

        #endregion

        #region Chat Bot

        [HttpGet("get-chatbot-replies/{replyID}")]
        public async Task<IActionResult> GetChatbotReplies(int replyID)
        {
            if (replyID == 0)
            {
                ChatbotReplyListModel res = new()
                {
                    ReplyList = await _dbContext.GetListByQueryAsync<ChatbotReplyListItemModel>($@"Select CR.ReplyID, CR.ParentReplyID,CR.ReplyTypeID, CR.Code, W.Name Description,ListButtonText
                    From ChatbotReply CR
					JOIN WhatsappAccount W on W.WhatsappAccountID=CR.WhatsappAccountID and W.IsDeleted=0
                    Where ParentReplyID is null and CR.IsDeleted=0 and W.ClientID={CurrentClientID}", null)
                };

                return Ok(res);
            }

            ChatbotReply reply = await _dbContext.GetAsync<ChatbotReply>(replyID);

            ChatbotReplyListModel result = new()
            {
                WhatsappAccountID = reply.WhatsappAccountID,
                ReplyID = reply.ReplyID,
                ReplyBody = reply.Detailed,
                ReplyFooter = reply.Footer,
                ReplyHeader = reply.Header,
                ReplyTypeID = reply.ReplyTypeID,
                ParentReplyID = Convert.ToInt32(reply.ParentReplyID),
                ListButtonText = reply.ListButtonText,
                ReplyList = await _dbContext.GetListByQueryAsync<ChatbotReplyListItemModel>($@"Select PR.WhatsappAccountID, CR.ReplyID, CR.ParentReplyID,CR.ReplyTypeID, CR.Code, CR.Short Description
                    From ChatbotReply PR
                    JOIN ChatbotReply CR on CR.ParentReplyID=PR.ReplyID and CR.IsDeleted=0
                    Where PR.ReplyID={reply.ReplyID} and PR.IsDeleted=0
                    Order by CR.Code", null),
                MultiReplyList = await _dbContext.GetListByQueryAsync<ChatbotMultiReplyListModel>($@"Select R.MultiReplyID,Coalesce(Address,DocumentName, FileName) as FileName,Case When L.MultiReplyID is null then Caption else L.Name end as Caption,SlNo,MediaTypeID
                    from ChatbotMultiReply R
                    LEFT JOIN Media M on M.MediaID=R.MediaID and M.IsDeleted=0
                    LEFT JOIN ChatbotMultiReplyLocation L on L.MultiReplyID=R.MultiReplyID
                    Where R.IsDeleted=0 and R.ReplyID={reply.ReplyID}
                    Order by SlNo", null)
            };

            return Ok(result);
        }

        [HttpGet("get-bot-reply/{replyID}")]
        public async Task<IActionResult> GetChatbotReply(int replyID)
        {
            ChatbotReplyModel res = await _dbContext.GetByQueryAsync<ChatbotReplyModel>($@"Select R.*,I.ItemName,PurchaseReplyID,QuantityLabel,ValidationFailedMessage,PaymentSuccessMessage,PaymentFailedMessage,PaymentLabel
                                                                                                from ChatbotReply R
                                                                                                Left Join viItem I ON I.ItemVariantID=R.ItemVariantID
                                                                                                LEFT JOIN ChatbotPurchaseReply P on P.ReplyID=R.ReplyID
                                                                                                Where R.ReplyID={replyID}", null);

            if (res != null)
            {
                if (res.ReplyTypeID == (int)ChatbotReplyTypes.List)
                {
                    res.Options = await _dbContext.GetListByQueryAsync<ChatbotReplyOptionModel>($@"Select * from ChatbotReply Where ParentReplyID={res.ReplyID} and IsDeleted=0", null);
                }
                else if (res.ReplyTypeID == (int)ChatbotReplyTypes.Submit)
                {
                    res.Assignees = await _whatsapp.GetChatbotSumbitAssignees(replyID, CurrentClientID);
                }
            }
            return Ok(res);
        }

        [HttpGet("get-bot-submit-assignees/{replyID}")]
        public async Task<IActionResult> GetChatbotSumbitAssignees(int replyID)
        {
            var res = await _whatsapp.GetChatbotSumbitAssignees(replyID, CurrentClientID);
            return Ok(res);
        }

        [HttpPost("save-bot-reply")]
        public async Task<IActionResult> Post(ChatbotReplyModel reply)
        {
            var ex = await _dbContext.GetAsync<ChatbotReply>("Code=@Code and ReplyID<>@ReplyID and ParentReplyID=@ParentReplyID", reply);
            if (ex != null)
                return BadRequest(new Error("Reply code already exist"));

            ChatbotReply r = reply;
            reply.ReplyID = await _dbContext.SaveAsync(r);

            if (reply.Options != null && reply.ReplyTypeID == (int)ChatbotReplyTypes.List)
            {
                List<ChatbotReply> options = new();
                foreach (var option in reply.Options)
                {
                    options.Add(new ChatbotReply()
                    {
                        ReplyID = option.ReplyID,
                        Short = option.Short,
                        Code = Convert.ToInt16(option.Code),
                        ReplyTypeID = (int)ChatbotReplyTypes.ListOption,
                        WhatsappAccountID = reply.WhatsappAccountID,
                    });
                }
                await _dbContext.SaveSubItemListAsync(options, "ParentReplyID", reply.ReplyID);
            }
            else if (reply.Assignees != null && reply.ReplyTypeID == (int)ChatbotReplyTypes.Submit)
            {
                List<ChatbotSubmitAssignee> assignees = new();
                foreach (var assignee in reply.Assignees.Where(s => s.IsAssigned))
                {
                    assignees.Add(new ChatbotSubmitAssignee()
                    {
                        AssigneeID = assignee.AssigneeID,
                        EntityID = assignee.EntityID,
                    });
                }
                await _dbContext.SaveSubItemListAsync(assignees, "ReplyID", reply.ReplyID);
            }

            if (reply.ReplyTypeID == (int)ChatbotReplyTypes.Purchase)
            {
                ChatbotPurchaseReply purchaseReply = new()
                {
                    PurchaseReplyID = reply.PurchaseReplyID,
                    PaymentFailedMessage = reply.PaymentFailedMessage,
                    PaymentSuccessMessage = reply.PaymentSuccessMessage,
                    QuantityLabel = reply.QuantityLabel,
                    ReplyID = reply.ReplyID,
                    ValidationFailedMessage = reply.ValidationFailedMessage,
                    PaymentLabel = reply.PaymentLabel
                };
                await _dbContext.SaveAsync(purchaseReply);

                //await _inventory.InsertClientDefaultInvoiceTypeEntries(CurrentClientID);
            }

            return Ok(new Success());
        }

        [HttpPost("update-bot-reply-header")]
        public async Task<IActionResult> UpdateBotReplyHeader(ChatbotReplyHeaderUpdateModel model)
        {
            var reply = await _dbContext.GetAsync<ChatbotReply>(model.ReplyID);
            reply.Header = model.ReplyHeader;
            reply.Detailed = model.ReplyBody;
            reply.Footer = model.ReplyFooter;
            reply.ListButtonText = model.ListButtonText;
            await _dbContext.SaveAsync(reply);
            return Ok(new Success());
        }

        [HttpGet("delete-chatbot-reply")]
        public async Task<IActionResult> DeleteChatBotReply(int id)
        {
            await _dbContext.DeleteAsync<ChatbotReply>(id);
            return Ok(new Success());
        }

        [HttpPost("save-bot-multi-reply")]
        public async Task<IActionResult> SaveBotMultiReply(ChatbotMultiReplyModel reply)
        {
            if (reply.Reply.MediaTypeID == (int)MessageTypes.Location || reply.Reply.MediaTypeID == (int)MessageTypes.Text)
            {
                reply.Reply.IsFirst = false;
            }

            if (reply.Reply.IsFirst)
            {
                await _dbContext.ExecuteAsync("Update ChatbotMultiReply Set IsFirst=0 Where ReplyID=@ReplyID and IsDeleted=0 and IsFirst=1", reply.Reply);
            }

            reply.Reply.MultiReplyID = await _dbContext.SaveAsync(reply.Reply);
            if (reply.Reply.MediaTypeID == (int)MessageTypes.Location)
            {
                reply.Location.MultiReplyID = reply.Reply.MultiReplyID;
                await _dbContext.SaveAsync(reply.Location);
            }
            return Ok(new Success());
        }

        [HttpGet("get-bot-multi-reply/{multiReplyID}")]
        public async Task<IActionResult> GetChatbotMultiReply(int multiReplyID)
        {
            ChatbotMultiReplyModel res = new()
            {
                Reply = await _dbContext.GetAsync<ChatbotMultiReply>(multiReplyID),
            };
            if (res.Reply != null)
            {
                if (res.Reply.MediaTypeID == (int)MessageTypes.Location)
                {
                    res.Location = await _dbContext.GetAsync<ChatbotMultiReplyLocation>($"MultiReplyID={multiReplyID}", null);
                }
            }

            return Ok(res);
        }

        [HttpGet("delete-chatbot-multi-reply")]
        public async Task<IActionResult> DeleteChatBotMultiReply(int id)
        {
            await _dbContext.DeleteAsync<ChatbotMultiReply>(id);
            return Ok(new Success());
        }

        #endregion

        #region Chat

        [HttpPost("save-contacts")]
        public async Task<IActionResult> SaveContacts(WhatsappContact newcontact)
        {
            var contact = await _dbContext.GetAsync<WhatsappContact>($"Phone=@Phone and ClientID={CurrentClientID}", newcontact);
            if (contact == null)
            {
                contact = new WhatsappContact()
                {
                    Name = newcontact.Name,
                    Phone = newcontact.Phone,
                    ClientID = CurrentClientID
                };
                contact.ContactID = await _dbContext.SaveAsync(contact);
            }
            return Ok(contact);
        }

        [HttpPost("get-chat-list")]
        public async Task<IActionResult> GetWhatsappChatList(ChatListPostModel Model)
        {
            var result = await _dbContext.GetListByQueryAsync<WhatsappChatListModel>($@"Select C.ContactID,Case When ISNULL(Name,'')='' then Phone else Name end as Name, Message,UnreadMessages,Phone
                from 
                (Select ContactID,Max(ChatID) as ChatID
                From WhatsappChat
                Where WhatsappAccountID={Model.WhatsappAccountID}
                Group by ContactID)as C
                JOIN WhatsappChat W on W.ChatID=C.ChatID
                JOIN WhatsappContact WC on WC.ContactID=C.ContactID
                LEFT JOIN(Select ContactID,Count(ChatID) as UnreadMessages 
                From WhatsappChat
                Where WhatsappAccountID={Model.WhatsappAccountID} and SeenOn is null  and IsIncoming=1 
                Group by ContactID) U on U.ContactID=C.ContactID
                 Where Name LIKE @SearchString or Message LIKE @SearchString or Phone LIKE @SearchString
                Order by C.ChatID desc", new { SearchString = '%' + Model.SearchString + '%' });
            return Ok(result);
        }

        [HttpPost("get-all-contacts")]
        public async Task<IActionResult> GetAllWhatsappContacts(ChatListPostModel Model)
        {
            var result = await _dbContext.GetListByQueryAsync<WhatsappChatListModel>($@"Select C.ContactID,Case When ISNULL(Name,'')='' then Phone else Name end as Name,Phone
                from 
                (Select ContactID,Max(ChatID) as ChatID
                From WhatsappChat
                Where WhatsappAccountID={Model.WhatsappAccountID}
                Group by ContactID)as C
                JOIN WhatsappChat W on W.ChatID=C.ChatID
                JOIN WhatsappContact WC on WC.ContactID=C.ContactID  
                 Where Name LIKE @SearchString or Phone LIKE @SearchString
                Order by Name asc", new { SearchString = '%' + Model.SearchString + '%' });
            return Ok(result);
        }

        [HttpGet("get-chat-header/{contactId}")]
        public async Task<IActionResult> GetWhatsappChatHeader(int contactId)
        {
            var res = await _dbContext.GetByQueryAsync<WhatsappChatHeaderModel>($@"Select Case When ISNULL(C.Name,'')='' then C.Phone else C.Name end as Name,C.Phone,EmailAddress,
                Case When S.SessionID is null then 0 else 1 end SupportChatStarted
                from WhatsappContact C
                LEFT JOIN Entity E on E.EntityID=C.EntityID
                LEFT JOIN WhatsappChatSession S on S.ContactID=C.ContactID and HasSupportChat=1 and IsFinished=0
                Where C.ContactId={contactId}", null);

            if (res != null)
            {
                res.Notes = await _dbContext.GetListByQueryAsync<NoteListModel>($@"Select NoteID,Note,ContactID,U.Name as AddedBy,DATEADD(MINUTE,10,AddedOn) AddedOn,ProfileImage
                    from WhatsappContactNote N
                    LEFT JOIN viEntity U on U.EntityID=N.AddedBy
                    Where ContactID={contactId} and N.IsDeleted=0", null);

                res.Tags = await _dbContext.GetListByQueryAsync<ContactTagModel>($@"Select ContactTagID,Tag
                    from WhatsappContactTag T
                    LEFT JOIN TagMaster M on M.TagID=T.TagID
                    Where ContactID={contactId} and T.IsDeleted=0", null);
            }
            return Ok(res);
        }

        [HttpGet("get-chat-header/{name}/{phone}")]
        public async Task<IActionResult> GetChatHeader(string name, string phone)
        {
            var res = await _dbContext.GetByQueryAsync<WhatsappContact>($"Select Case When ISNULL(Name,'')='' then Phone else Name end as Name,Phone,ContactID from WhatsappContact Where Name=@Name and Phone=@Phone", new { Name = name, Phone= phone });
            return Ok(res);
        }

        [HttpGet("get-full-address/{addressId}")]
        public async Task<IActionResult> GetFullAddress(int addressId)
        {
            var res = await _dbContext.GetByQueryAsync<StringModel>($@"SELECT 
                                                                       CONCAT_WS(', ',
		                                                                    CASE WHEN EA.AddressLine1 IS NOT NULL AND EA.AddressLine1 != '' THEN EA.AddressLine1 ELSE NULL END,
                                                                            CASE WHEN EA.AddressLine2 IS NOT NULL AND EA.AddressLine2 != '' THEN EA.AddressLine2 ELSE NULL END,
                                                                            CASE WHEN EA.AddressLine2 IS NOT NULL AND EA.AddressLine3 != '' THEN EA.AddressLine3 ELSE NULL END,
		                                                                    CASE WHEN EA.State IS NOT NULL AND EA.State!='' THEN EA.State ELSE NULL END,
		                                                                    CASE WHEN EA.City IS NOT NULL AND EA.City!='' THEN EA.City ELSE NULL END,
                                                                            CASE WHEN EA.PinCode IS NOT NULL AND EA.PinCode!='' THEN EA.PinCode ELSE NULL END,
                                                                            CASE WHEN C.CountryName IS NOT NULL AND C.CountryName!='' THEN C.CountryName ELSE NULL END
                                                                        ) AS Value
                                                                    FROM EntityAddress EA
                                                                    LEFT JOIN Country C ON C.CountryID = EA.CountryID 
                                                                    WHERE EA.IsDeleted = 0 and C.IsDeleted=0 and EA.AddressID = {addressId}", null);
            return Ok(res);
        }

        [HttpPost("get-chat-history")]
        public async Task<IActionResult> GetWhatsappChatList(ChatHistoryPostModel model)
        {
            PagedListQueryModel searchModel = new()
            {
                Select = $@"Select C.ChatID,ContactId, Message,
                    MessageTypeID,IsIncoming,Convert(varchar,DateAdd(MINUTE,330,AddedOn),0) as MessageOn,
                    MessageStatus StatusID,FailedReason,Latitude,Longitude,WMediaID,Case When FileName is not null then '{_config["ServerURL"]}'+FileName else '' end as FileName,--CB.Caption as FileCaption,
                    FormattedName,P.WAID as Phone,M.DocumentName                   
                    From WhatsappChat C
					LEFT JOIN ChatbotReply R on R.ReplyID=C.ReplyID and IsIncoming=0
					--LEFT JOIN viChatbotReply CB on CB.ReplyID=R.ReplyID
					LEFT JOIN WhatsappChatLocation L on L.ChatID=C.ChatID
                    LEFT JOIN WhatsappChatMedia M on M.ChatID=C.ChatID
                    LEFT JOIN Media MM on MM.MediaID=Coalesce(C.AttachedMediaID,M.MediaID)
                    LEFT JOIN WhatsappChatContact CC on CC.ChatID=C.ChatID
                    LEFT JOIN WhatsappChatContactPhone P on P.WhatsappChatContactID=CC.WhatsappChatContactID",
                WhereCondition = $"ContactID={model.ContactId}",
                PageIndex = model.PageIndex,
                PageSize = model.PageSize,
                OrderByFieldName = model.OrderByFieldName
            };
            var result = await _dbContext.GetPagedList<WhatsappChatHistoryItemModel>(searchModel, null);


            #region Set as read

            var unreadMessageIds = await _dbContext.GetListByQueryAsync<string>($@"Select MessageID
                From WhatsappChat
                Where MessageID is not null and SeenOn is null and ContactID={model.ContactId} and WhatsappAccountID={model.WhatsappAccountID} and IsIncoming=1 and ReplyID is null", null);
            foreach (var msgId in unreadMessageIds)
            {
                await _whatsapp.MarkMessageasRead(model.WhatsappAccountID, msgId);
            }

            await _dbContext.ExecuteAsync($"Update WhatsappChat Set SeenOn=GETUTCDATE() Where SeenOn is null and ContactID={model.ContactId} and WhatsappAccountID={model.WhatsappAccountID} and IsIncoming=1");

            #endregion

            return Ok(result);
        }

        [HttpGet("stop-support-chat/{contactId}")]
        public async Task<IActionResult> StopSupportChat(int contactId)
        {
            await _whatsapp.StopSupportChat(contactId);
            return Ok(new Success());
        }

        [HttpGet("get-media-details/{whatsappAccountID}/{WMediaID}")]
        public async Task<IActionResult> GetMediaDetail(int whatsappAccountID, string WMediaID)
        {
            var res = await _whatsapp.GetMediaURL(whatsappAccountID, WMediaID);
            return Ok(res);
        }

        [HttpPost("send-text-message")]
        public async Task<IActionResult> SendTextMessage(SendWhatsappChatModel model)
        {
            var chatId = await _whatsapp.SendTextMessage(model.WhatsappAccountID, model.ContactID, model.Message);
            var chat = await _whatsapp.GetLastChatHistoryItem(chatId);
            chat.AppChatID = model.AppChatID;
            return Ok(chat);
        }

        [HttpPost("send-image")]
        public async Task<IActionResult> SendImage(SendWhatsappChatModel model)
        {
            if (model.MediaID.HasValue)
            {
                model.Link = _config["ServerURL"] + await _dbContext.GetByQueryAsync<string>($"Select FileName From Media WHere MediaID={model.MediaID}", null);
            }

            var chatId = await _whatsapp.SendImageMessage(model.WhatsappAccountID, model.ContactID, model.Message, model.Link, model.MediaID);
            var chat = await _whatsapp.GetLastChatHistoryItem(chatId);
            return Ok(chat);
        }

        [HttpPost("send-image-by-id")]
        public async Task<IActionResult> SendImageById(SendWhatsappChatModel model)
        {
            var chatId = await _whatsapp.SendImageMessageByMediaID(model.WhatsappAccountID, model.ContactID, model.Message, model.WMediaID);
            var chat = await _whatsapp.GetLastChatHistoryItem(chatId);
            chat.AppChatID = model.AppChatID;
            return Ok(chat);
        }



        [HttpPost("send-audio")]
        public async Task<IActionResult> SendAudio(SendWhatsappChatModel model)
        {
            if (model.MediaID.HasValue)
            {
                model.Link = _config["ServerURL"] + await _dbContext.GetByQueryAsync<string>($"Select FileName From Media WHere MediaID={model.MediaID}", null);
            }
            var chatId = await _whatsapp.SendAudioMessage(model.WhatsappAccountID, model.ContactID, model.Message, model.Link, model.MediaID);
            var chat = await _whatsapp.GetLastChatHistoryItem(chatId);
            return Ok(chat);
        }

        [HttpPost("send-audio-by-id")]
        public async Task<IActionResult> SendAudioById(SendWhatsappChatModel model)
        {
            var chatId = await _whatsapp.SendAudioMessageByMediaID(model.WhatsappAccountID, model.ContactID, model.WMediaID);
            var chat = await _whatsapp.GetLastChatHistoryItem(chatId);
            chat.AppChatID = model.AppChatID;
            return Ok(chat);
        }

        [HttpPost("send-document")]
        public async Task<IActionResult> SendDocument(SendWhatsappChatModel model)
        {
            if (model.MediaID.HasValue)
            {
                model.Link = _config["ServerURL"] + await _dbContext.GetByQueryAsync<string>($"Select FileName From Media WHere MediaID={model.MediaID}", null);
            }
            var chatId = await _whatsapp.SendDocumentMessage(model.WhatsappAccountID, model.ContactID, model.Message, model.Link, model.MediaID);
            var chat = await _whatsapp.GetLastChatHistoryItem(chatId);
            return Ok(chat);
        }

        [HttpPost("send-document-by-id")]
        public async Task<IActionResult> SendDocumentById(SendWhatsappChatModel model)
        {
            var chatId = await _whatsapp.SendDocumentMessageByMediaID(model.WhatsappAccountID, model.ContactID, model.Message, model.WMediaID, model.DocumentName);
            var chat = await _whatsapp.GetLastChatHistoryItem(chatId);
            chat.AppChatID = model.AppChatID;
            return Ok(chat);
        }

        [HttpPost("send-sticker-by-id")]
        public async Task<IActionResult> SendStickerById(SendWhatsappChatModel model)
        {
            var chatId = await _whatsapp.SendStickerMessageByMediaID(model.WhatsappAccountID, model.ContactID, model.WMediaID);
            var chat = await _whatsapp.GetLastChatHistoryItem(chatId);
            chat.AppChatID = model.AppChatID;
            return Ok(chat);
        }

        [HttpPost("save-note")]
        public async Task<IActionResult> SaveNote(NoteListModel model)
        {
            WhatsappContactNote note = new()
            {
                NoteID = model.NoteID,
                AddedBy = CurrentEntityID,
                AddedOn = System.DateTime.UtcNow,
                ContactID = model.ContactID,
                Note = model.Note
            };
            model.NoteID = await _dbContext.SaveAsync(note);
            model.AddedOn = System.DateTime.UtcNow.AddMinutes(TimeOffset);
            model.AddedBy = CurrentUserName;
            return Ok(model);
        }

        [HttpGet("delete-note")]
        public async Task<IActionResult> DeleteNote(int Id)
        {
            var result = await _dbContext.GetAsync<WhatsappContactNote>(Id);
            await _dbContext.DeleteAsync<WhatsappContactNote>(Id);
            return Ok(new Success());
        }

        [HttpPost("save-contact-tag")]
        public async Task<IActionResult> SaveContactTag(ContactTagModel model)
        {
            var tagId = await _whatsapp.GetTagID(model.Tag, CurrentClientID);
            WhatsappContactTag tag = await _dbContext.GetAsync<WhatsappContactTag>("ContactID=@ContactID and TagID=@TagID", new { model.ContactID, TagID = tagId });
            if (tag == null)
            {
                tag = new()
                {
                    ContactID = model.ContactID,
                    TagID = tagId,
                };
                model.ContactTagID = await _dbContext.SaveAsync(tag);
            }
            return Ok(model);
        }

        [HttpGet("delete-contact-tag")]
        public async Task<IActionResult> DeleteContactTag(int Id)
        {
            await _dbContext.DeleteAsync<WhatsappContactTag>(Id);
            return Ok(new Success());
        }

        [HttpPost("update-email")]
        public async Task<IActionResult> UpdateEmail(ContactEmailEditModal model)
        {
            var userCount = await _dbContext.GetByQueryAsync<int>($@"SELECT Count(*) FROM Entity
                                                                WHERE ClientID={CurrentClientID} and EmailAddress=@EmailAddress and IsDeleted =0
                                                                GROUP BY ClientID", model);
            if (userCount > 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "Something went wrong",
                    ResponseMessage = "Email Address already exist, try different one"
                });
            }
            var entityId = await _whatsapp.GetContactEntityID(model.ContactID, CurrentClientID);
            var entity = await _dbContext.GetAsync<Entity>(entityId.Value);
            entity.EmailAddress = model.EmailAddress;
            await _dbContext.SaveAsync(entity);
            return Ok(new Success());
        }

        [HttpGet("get-entityId/{contactId}")]
        public async Task<IActionResult> GetEntityId(int contactId)
        {
            var entityId = await _whatsapp.GetContactEntityID(contactId, CurrentClientID);
            return Ok(entityId);
        }

        #endregion

        #region Template

        [HttpPost("get-whatsapp-template-paged-list")]
        public async Task<IActionResult> GetWhatsAppTemplates(PagedListPostModelWithFilter model)
        {
            PagedListQueryModel searchData = model;
            searchData.Select = @$"Select  W.TemplateID,TemplateName,LanguageName,Name,StatusID,StatusReason,CategoryID
                                from WhatsappTemplate W
								LEFT JOIN WhatsappAccount A on A.WhatsappAccountID=W.WhatsappAccountID
								LEFT JOIN Language L on L.LanguageID=W.LanguageID";
            searchData.WhereCondition = $"W.IsDeleted=0 and W.ClientID={CurrentClientID}";
            searchData.SearchLikeColumnNames = new() { "TemplateName" };
            searchData.WhereCondition += _common.GeneratePagedListFilterWhereCondition(model, "TemplateName");
            var result = await _dbContext.GetPagedList<WhatsappTemplateListModel>(searchData, null);
            return Ok(result);
        }


        [HttpGet("get-template/{Id}")]
        public async Task<IActionResult> GetV1Template(int Id)
        {
            var result = await _dbContext.GetByQueryAsync<WhatsappTemplateModel>($@"Select WT.*,CountryName as CallCountryName,LanguageName,Name 
                                                                                    from WhatsappTemplate WT
                                                                                    Left Join WhatsappAccount WA on WA.WhatsappAccountID=WT.WhatsappAccountID and WA.IsDeleted=0
                                                                                    Left Join Language L on L.LanguageID=WT.LanguageID and L.IsDeleted=0
                                                                                    Left Join Country C on C.CountryID=WT.CallCountryID and C.IsDeleted=0
                                                                                    where TemplateID={Id}", null);
            if (result != null)
            {
                result.HeaderVariables = await _dbContext.GetListAsync<WhatsappTemplateVariable>(@$"WhatsappTemplateID ={Id} and Section={(int)WhatsappTemplateVariableSection.HEADER}", null);
                result.BodyVariables = await _dbContext.GetListAsync<WhatsappTemplateVariable>(@$"WhatsappTemplateID ={Id} and Section={(int)WhatsappTemplateVariableSection.BODY}", null);
            }
            return Ok(result);
        }

        [HttpGet("view-header-variable/{Id}")]
        public async Task<IActionResult> ViewHeaderVariable(int Id)
        {
            var result = await _dbContext.GetByQueryAsync<WhatsappTemplateVariable>($@"Select * from WhatsappTemplateVariable
                                                                                        Where VariableID={Id} and IsDeleted=0", null);
            return Ok(result);
        }

        [DisableRequestSizeLimit]
        [AllowAnonymous]
        [HttpPost("add-template-header-media")]
        public async Task<IActionResult> AddTemplateMedia()
        {
            var form = HttpContext.Request.Form;
            var fileContent = form["file"];
            if (HttpContext.Request.Form.Files.Any())
            {
                var file = HttpContext.Request.Form.Files[0];
                if (file.Length > 0)
                {
                    var extension = file.FileName.Substring(file.FileName.LastIndexOf('.') + 1);

                    var handle = await _whatsapp.UploadSessionMedia(Convert.ToInt32(form["WhatsappAccountID"]), file);

                    string folderName = form["FolderName"].ToString();
                    string contentType = form["ContentType"].ToString();
                    int mediaID = Convert.ToInt32(form["MediaID"]);
                    var res = await _media.UploadFile(mediaID, file.FileName, folderName, file.FileName, contentType, file.Length, extension);
                    using (var stream = System.IO.File.Create(res.FilePath))
                    {
                        await file.CopyToAsync(stream);
                    }
                    res.FilePath = handle;


                    return Ok(res);
                }
                else
                {
                    throw new PBException("FileNotFound");
                }
            }
            else
            {
                throw new PBException("FileNotFound");
            }
        }

        [HttpPost("save-template")]
        public async Task<IActionResult> SaveV1Template(WhatsappTemplateModel model)
        {

            _cn.Open();
            using (var tran = _cn.BeginTransaction())
            {
                try
                {
                    if (model.WhatsappTemplateID == null)
                    {
                        WhatsappTemplate whatsapptemplate = new()
                        {
                            ClientID = CurrentClientID,
                            TemplateID = model.TemplateID,
                            TemplateName = model.TemplateName.ToLower(),
                            WhatsappAccountID = model.WhatsappAccountID,
                            LanguageID = model.LanguageID,
                            CategoryID = model.CategoryID,
                            HeaderTypeID = model.HeaderTypeID,
                            HeaderText = model.HeaderText,
                            HeaderMediaTypeID = model.HeaderMediaTypeID,
                            HeaderMediaID = model.HeaderMediaID,
                            HeaderMediaHandle = model.HeaderMediaHandle,
                            Body = model.Body,
                            Footer = model.Footer,

                            WhatsappTemplateID = model.WhatsappTemplateID,
                        };

                        whatsapptemplate.TemplateID = await _dbContext.SaveAsync(whatsapptemplate, tran);

                        var lstVariables = new List<WhatsappTemplateVariable>();
                        if (model.HeaderVariables != null && model.HeaderVariables.Count > 0)
                            lstVariables.AddRange(model.HeaderVariables);
                        if (model.BodyVariables != null && model.BodyVariables.Count > 0)
                            lstVariables.AddRange(model.BodyVariables);
                        await _dbContext.SaveSubItemListAsync(lstVariables, "TemplateID", whatsapptemplate.TemplateID, tran);


                        #region Cloud Template API

                        var language = await _dbContext.GetAsync<Language>(model.LanguageID.Value, tran);
                        CloudAPITemplateModel cloudModel = new()
                        {
                            name = model.TemplateName.ToLower(),
                            category = ((WhatsappTemplateCategory)model.CategoryID).ToString(),
                            language = language.LanguageCode,
                            components = new()
                        };

                        if (model.HeaderTypeID == (int)WhatsappTemplateHeaderType.TEXT)
                        {
                            string headertext = model.HeaderText;
                            List<string> headerSampleItems = new();
                            var headerVariableCnt = model.HeaderVariables.Count;
                            if (headerVariableCnt > 0)
                            {
                                for (int i = 0; i < headerVariableCnt; i++)
                                {
                                    headertext = headertext.Replace("{" + model.HeaderVariables[i].VariableName + "}", "{{" + (i + 1) + "}}");
                                    headerSampleItems.Add(model.HeaderVariables[i].SampleValue);
                                }
                            }
                            else
                            {
                                headerSampleItems.Add(model.HeaderText);
                            }
                            cloudModel.components.Add(new()
                            {
                                type = "HEADER",
                                format = "TEXT",
                                text = headertext,
                                example = new()
                                {
                                    header_text = headerSampleItems
                                }
                            });
                        }
                        else if (model.HeaderTypeID == (int)WhatsappTemplateHeaderType.MEDIA && model.HeaderMediaID.HasValue)
                        {
                            var media = await _dbContext.GetAsync<Media>(model.HeaderMediaID.Value);
                            string mediaFileURL = _config["ServerURL"] + media.FileName;
                            switch ((WhatsappTemplateHeaderMediaType)model.HeaderMediaTypeID)
                            {
                                case WhatsappTemplateHeaderMediaType.IMAGE:
                                    cloudModel.components.Add(new()
                                    {
                                        type = "HEADER",
                                        format = "IMAGE",
                                        example = new()
                                        {
                                            header_handle = new() { model.HeaderMediaHandle }
                                        }
                                    });
                                    break;
                                case WhatsappTemplateHeaderMediaType.DOCUMENT:
                                    cloudModel.components.Add(new()
                                    {
                                        type = "HEADER",
                                        format = "Document",
                                        example = new()
                                        {
                                            header_handle = new() { model.HeaderMediaHandle }
                                        }
                                    });
                                    break;
                                case WhatsappTemplateHeaderMediaType.VIDEO:
                                    cloudModel.components.Add(new()
                                    {
                                        type = "HEADER",
                                        format = "Video",
                                        example = new()
                                        {
                                            header_handle = new() { model.HeaderMediaHandle }
                                        }
                                    });
                                    break;
                            }
                        }


                        if (!string.IsNullOrEmpty(model.Body))
                        {
                            string bodytext = model.Body;

                            List<string> bodySampleItems = new();
                            var bodyVariableCnt = model.BodyVariables.Count;
                            if (bodyVariableCnt > 0)
                            {
                                for (int i = 0; i < bodyVariableCnt; i++)
                                {
                                    bodytext = bodytext.Replace("{" + model.BodyVariables[i].VariableName + "}", "{{" + (i + 1) + "}}");
                                    bodySampleItems.Add(model.BodyVariables[i].SampleValue);
                                }
                            }

                            var bodyComponent = new TemplateComponentModel()
                            {
                                type = "BODY",
                                text = bodytext,
                            };

                            if (bodyVariableCnt > 0)
                            {
                                bodyComponent.example = new()
                                {
                                    body_text = new() { bodySampleItems }
                                };
                            }

                            cloudModel.components.Add(bodyComponent);
                        }

                        if (!string.IsNullOrEmpty(model.Footer))
                        {
                            cloudModel.components.Add(new()
                            {
                                type = "FOOTER",
                                text = model.Footer,
                            });
                        }

                        if (model.FooterButtonTypeID == (int)WhatsappFooterButtonType.CallToAction)
                        {
                            List<TemplateButtonModel> buttons = new();
                            if (model.HasCallButton)
                            {
                                var cntry = await _dbContext.GetAsync<Country>(model.CallCountryID.Value);
                                buttons.Add(new()
                                {
                                    type = "PHONE_NUMBER",
                                    text = model.CallButtonText,
                                    phone_number = cntry.ISDCode + model.CallPhoneNo
                                });
                            }
                            if (model.HasVisitButton)
                            {
                                if (model.VisitButton1TypeID == (int)WhatsappTemplateButtonUrlType.STATIC)
                                {
                                    buttons.Add(new()
                                    {
                                        type = "URL",
                                        text = model.VisitButton1Text,
                                        url = model.VisitButton1URL
                                    });
                                }
                                else
                                {
                                    buttons.Add(new()
                                    {
                                        type = "URL",
                                        text = model.VisitButton1Text,
                                        url = model.VisitButton1URL + "{{1}}"
                                    });
                                }
                            }

                            cloudModel.components.Add(new TemplateComponentModel()
                            {
                                type = "BUTTONS",
                                buttons = buttons,
                            });
                        }
                        else if (model.FooterButtonTypeID == (int)WhatsappFooterButtonType.QuickReply)
                        {
                            List<TemplateButtonModel> buttons = new();
                            if (!string.IsNullOrEmpty(model.QuickButton1))
                            {
                                buttons.Add(new()
                                {
                                    type = "QUICK_REPLY",
                                    text = model.QuickButton1,
                                });
                            }
                            if (!string.IsNullOrEmpty(model.QuickButton2))
                            {
                                buttons.Add(new()
                                {
                                    type = "QUICK_REPLY",
                                    text = model.QuickButton2,
                                });
                            }
                            if (!string.IsNullOrEmpty(model.QuickButton3))
                            {
                                buttons.Add(new()
                                {
                                    type = "QUICK_REPLY",
                                    text = model.QuickButton3,
                                });
                            }

                            cloudModel.components.Add(new TemplateComponentModel()
                            {
                                type = "BUTTONS",
                                buttons = buttons,
                            });
                        }

                        var result = await _whatsapp.CreateTemplateWACloudPostRequest(model.WhatsappAccountID.Value, cloudModel, tran);

                        if (result.id != null)
                        {
                            whatsapptemplate.WhatsappTemplateID = result.id;
                            await _dbContext.SaveAsync(whatsapptemplate, tran);
                        }
                        else
                        {
                            return BadRequest(new Error(result.error.error_user_msg));
                        }
                        #endregion

                        tran.Commit();
                        return Ok(new Success());
                    }
                    else
                    {
                        return BadRequest(new Error("You cannot edit template"));
                    }

                }
                catch (Exception err)
                {
                    tran.Rollback();
                    return BadRequest(new DbError(err.Message));
                }
            }

        }

        [HttpGet("delete-template/{id}")]
        public async Task<IActionResult> DeleteTemplate(int id)
        {
            var model = await _dbContext.GetAsync<WhatsappTemplate>(id);

            if (model.WhatsappTemplateID != null)
            {
                if (await _whatsapp.DeleteTemplateWACloudRequest(id))
                {
                    var logSummaryId = await _dbContext.InsertDeleteLogSummary(model.TemplateID, "Delete template " + model.TemplateName);
                    await _dbContext.DeleteAsync<WhatsappTemplate>(id, logSummaryID: logSummaryId);
                    return Ok(true);
                }
            }
            else
            {
                var logSummaryId = await _dbContext.InsertDeleteLogSummary(model.TemplateID, "Delete template " + model.TemplateName);
                await _dbContext.DeleteAsync<WhatsappTemplate>(id, logSummaryID: logSummaryId);
                return Ok(true);
            }
            return null;
        }

        [HttpGet("delete-whatsapp-template")]
        public async Task<IActionResult> DeleteWhatsAppTemplate(int Id)
        {
            var result = await _dbContext.GetAsync<WhatsappTemplate>(Id);
            await _dbContext.DeleteAsync<WhatsappTemplate>(Id);
            await _dbContext.InsertDeleteLogSummary(Id, result.TemplateName);
            return Ok(new Success());
        }

        #endregion

        #region Message Broadcast

        [HttpGet("get-templates")]
        public async Task<IActionResult> GetAllTemplates()
        {
            var result = await _dbContext.GetListOfIdValuePairAsync<WhatsappTemplate>("TemplateName", $"ClientID={CurrentClientID} and StatusID={(int)WhatsappTemplateStatus.APPROVED}", null);
            return Ok(result);
        }

        [HttpGet("get-template-variable-list/{templateId}")]
        public async Task<IActionResult> GetTemplateVariableList(int templateId)
        {
            var result = await _whatsapp.GetTemplateVariableList(templateId);
            return Ok(result);
        }

        [HttpPost("get-broadcast-list")]
        public async Task<IActionResult> GetBroadcastList(BroadcastSearchModel model)
        {
            PagedListQueryModel searchData = model;
            searchData.Select = $@"Select T.BroadcastID,T.BroadcastName,C.TemplateName,TotalRecipients,T.AddedOn as Date,ISNULL(Sent,0) as Sent,ISNULL(Delivered,0) as Delivered,
                ISNULL([Read],0) as [Read],ISNULL(Failed,0) as Failed,IsSent
                from WhatsappBroadcast T
                JOIN WhatsappTemplate C on C.TemplateID=T.TemplateID
                LEFT JOIN viBroadcast B on B.BroadcastID=T.BroadcastID
                LEFT JOIN (Select BroadcastID,Count(BroadcastID) TotalRecipients From WhatsappBroadcastReceipient Where IsDeleted=0 Group By BroadcastID) as R on R.BroadcastID=T.BroadcastID";

            searchData.WhereCondition = $"T.IsDeleted=0 and C.ClientID={CurrentClientID}";

            switch (model.MessageSendTypeId)
            {
                case (int)MessageSendingTypes.SendImmediately:
                    searchData.WhereCondition += $" and T.ScheduleTime is null and (T.PeriodicDay is null or T.PeriodicDay = 0)";
                    break;
                case (int)MessageSendingTypes.Scheduled:
                    searchData.WhereCondition += $" and T.ScheduleTime is not null and (T.PeriodicDay is null or T.PeriodicDay = 0)";
                    break;
                case (int)MessageSendingTypes.Periodic:
                    searchData.WhereCondition += $" and T.PeriodicDay is not null and  T.PeriodicDay != 0";
                    break;
                default:
                    searchData.WhereCondition += "";
                    break;
            }

            var searchModel = _map.Map<PagedListPostModelWithFilter>(model);
            searchData.WhereCondition += _common.GeneratePagedListFilterWhereCondition(searchModel, "BroadcastName");
            //searchData.SearchLikeColumnNames = new List<string>() { "TemplateName", "Name" };

            var result = await _dbContext.GetPagedList<MessageListModel>(searchData, model);
            return Ok(result);
        }

        [DisableRequestSizeLimit]
        [HttpPost("create-broadcast")]
        public async Task<IActionResult> CreateBroadcast(CreateBroadcastModel model)
        {
            int variableCount = 0;
            var variables = await _whatsapp.GetTemplateVariableList(model.TemplateID);

            if (variables != null)
                variableCount = variables.Variables.Count;

            #region Excel

            List<WhatsappBroadcastRecipientSaveModel> recipients = new();

            string folderName = "gallery/import/";
            if (!Directory.Exists(Path.Combine("wwwroot", folderName)))
            {
                Directory.CreateDirectory(Path.Combine("wwwroot", folderName));
            }

            string filePath = Path.Combine(_env.ContentRootPath, "wwwroot", folderName);
            filePath = filePath + "/" + "import_" + CurrentUserID.ToString() + "." + model.Extension;

            string sFileExtension = Path.GetExtension(filePath).ToLower();
            ISheet sheet;
            var stream = System.IO.File.Create(filePath);
            stream.Write(model.Content, 0, model.Content.Length);
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
                if (cellCount != 1 + variableCount)
                {
                    var columnName = "";
                    foreach (var variable in variables.Variables)
                    {
                        columnName += ", " + variable.Value;
                    }
                    return BadRequest(new Error() { ResponseMessage = "Data should contain Phone Number" + columnName });
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
                    if (!string.IsNullOrEmpty(phone) && recipients.Where(s => s.Phone == phone).Count() == 0)
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

            #endregion

            WhatsappBroadcast message = new()
            {
                BroadcastID = model.BroadcastID,
                BroadcastName = model.Title,
                TemplateID = model.TemplateID,
                ScheduleTime = model.ScheduleTime,
                PeriodicDay = model.PeriodicDay,
                AddedOn = DateTime.UtcNow,
                HeaderMediaID = model.MediaID
            };
            message.BroadcastID = await _dbContext.SaveAsync(message);

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

            return Ok(new WhatsappBroadcastSaveResultModel() { BroadcastID = message.BroadcastID, Receipients = recipients });
        }

        [HttpGet("delete-receipient/{receipientId}")]
        public async Task<IActionResult> DeleteReceipient(int receipientId)
        {
            var result = await _dbContext.DeleteAsync<WhatsappBroadcastReceipient>(receipientId);
            return Ok(new Success());
        }

        [HttpGet("delete-broadcast")]
        public async Task<IActionResult> DeleteBroadcast(int id)
        {
            await _dbContext.DeleteAsync<WhatsappBroadcast>(id);
            return Ok(new Success());
        }

        [HttpPost("get-recipients")]
        public async Task<IActionResult> GetAll(MessageReceipientSearchModel model)
        {
            PagedListQueryModel searchData = model;
            searchData.Select = $@"Select C.Phone, C.Name,MessageStatus AS SentStatus,Convert(varchar,W.AddedOn,20) as SentOn
            ,Convert(varchar,ISNULL(W.DeliveredOn,W.SeenOn),20) as DeliveredOn,FailedReason
            ,Convert(varchar,W.SeenOn,20) as SeenOn
            from WhatsappChat W
            LEFT JOIN WhatsappBroadcastReceipient R on R.ReceipientID=W.ReceipientID
            LEFT JOIN WhatsappBroadcast B on B.BroadcastID=R.BroadcastID
            LEFT JOIN WhatsappContact C on C.ContactID=W.ContactID";
            searchData.WhereCondition = $"B.BroadcastID={model.BroadcastID}";
            if (model.Phone != null)
            {
                searchData.WhereCondition += $"and C.Phone='{model.Phone}'";
            }
            searchData.OrderByFieldName = model.OrderByFieldName;
            //searchData.SearchLikeColumnNames = new List<string>() { "C.Name", "C.Phone" };

            var filterModel = _map.Map<PagedListPostModelWithFilter>(model);
            searchData.WhereCondition += _common.GeneratePagedListFilterWhereCondition(filterModel, "C.Name");
            var result = await _dbContext.GetPagedList<BroadcastReceipientHistory>(searchData, null);
            return Ok(result);
        }


        [AllowAnonymous]
        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOTP(SendOTPModel model)
        {
            var contact = await _whatsapp.GetContact(model.To, model.ClientID, model.ContactName);
            Random rnd = new();
            contact.OTP = (rnd.Next(1000, 9999)).ToString();
            await _dbContext.SaveAsync(contact);

            List<TemplateComponent> data = new()
            {
                new TemplateComponent()
                {
                    type = "body",
                    parameters = new List<TemplateParameter>()
                    {
                        new TemplateParameter()
                        {
                            type="text",
                            text=contact.OTP
                        }
                    }
                },
                new TemplateComponent()
                {
                    type="button",
                    sub_type="url",
                    index=0,
                    parameters = new List<TemplateParameter>()
                    {
                        new TemplateParameter()
                        {
                            type="text",
                            text=contact.OTP
                        }
                    }
                }
            };

            await _whatsapp.SendTemplateMessage(model.WhatsappAccountID, contact.ContactID, model.TemplateName, model.Lang, data, contact.OTP, null);

            return Ok(new Success());
        }

        #endregion

        #region Chat Reinitiation

        [AllowAnonymous]
        [HttpGet("start-reinitiation")]
        public async Task StartReInitiate()
        {
            try
            {
                RecurringJob.AddOrUpdate(() => ChatReInitiate(), "*/30 * * * *", TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
            }
            catch (PBException ex)
            {
            }
        }

        [AllowAnonymous]
        [HttpGet("chat-reinitiate")]
        public async Task ChatReInitiate()
        {
            var pending = await _dbContext.GetListByQueryAsync<ChatReinitiateListModel>($@"Select SessionID,S.WhatsappAccountID,Phone
                from WhatsappChatSession S
                JOIN WhatsappContact C on S.ContactID=C.ContactID
                JOIN WhatsappAccount A on A.WhatsappAccountID=S.WhatsappAccountID
                Where S.HasSupportChat=1 and IsFinished=0 and NeedReminder=1 and DATEADD(HOUR,A.ReminderHour,LastMessageOn)<=GETUTCDATE() 
                and ISNULL(ReminderSent,0)=0", null);

            await _dbContext.ExecuteAsync($@"Update S
                Set S.ReminderSent=1
                from WhatsappChatSession S
                JOIN WhatsappContact C on S.ContactID=C.ContactID
                JOIN WhatsappAccount A on A.WhatsappAccountID=S.WhatsappAccountID
                Where S.HasSupportChat=1 and IsFinished=0 and NeedReminder=1 and DATEADD(HOUR,A.ReminderHour,LastMessageOn)<=GETUTCDATE() and
                ISNULL(ReminderSent,0)=0");

            await _dbContext.ExecuteAsync($@"Update S
                Set S.IsFinished=1
                from WhatsappChatSession S
                JOIN WhatsappContact C on S.ContactID=C.ContactID
				JOIN WhatsappAccount A on A.WhatsappAccountID=S.WhatsappAccountID
                Where S.HasSupportChat=1 and IsFinished=0 and DATEADD(HOUR,ISNULL(A.ReinitiationHour,2),LastMessageOn)<=GETUTCDATE()");

            foreach (var p in pending)
            {
                await _whatsapp.SendChatReInitialisationMessage(p.WhatsappAccountID, p.Phone, p.SessionID);
            }
        }

        #endregion

        #region Contact

        [DisableRequestSizeLimit]
        [HttpPost("import-contact")]
        public async Task<IActionResult> ImportContact()
        {
            List<ImportContactModel> contacts = new();
            var form = HttpContext.Request.Form;
            if (HttpContext.Request.Form.Files.Any())
            {
                var file = HttpContext.Request.Form.Files[0];
                if (file.Length > 0)
                {
                    var stream = new FileStream(Path.GetTempFileName(), FileMode.Create);
                    await file.CopyToAsync(stream);

                    ISheet sheet;
                    using (stream)
                    {
                        stream.Position = 0;
                        if (Path.GetExtension(file.FileName) == ".xls")
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
                        if (cellCount != 3)
                        {
                            return BadRequest(new Error() { ResponseMessage = "Data should contain Phone Number,Name,Category" });
                        }
                        for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++) //Read Excel File
                        {
                            IRow row = sheet.GetRow(i);
                            if (row == null) continue;
                            if (row.Cells.All(d => d.CellType == CellType.Blank)) continue;

                            string phone = row.GetCell(0).ToString();

                            if (!string.IsNullOrEmpty(phone) && contacts.Where(s => s.Phone == phone).Count() == 0)
                            {
                                var recipient = new ImportContactModel()
                                {
                                    Phone = phone,
                                    Name = row.GetCell(1).ToString(),
                                    Category = row.GetCell(2).ToString()
                                };

                                contacts.Add(recipient);
                            }
                        }
                    }
                    stream.Close();
                }

                var categories = contacts.GroupBy(r => r.Category)
                            .Select(g => new { CategoryName = g.Key, PropertyValues = g });
                foreach (var category in categories)
                {

                }
            }

            return Ok(new Success());
        }

        #endregion

        #region Media Upload to fb server

        [DisableRequestSizeLimit]
        [AllowAnonymous]
        [HttpPost("upload-media-to-fb")]
        public async Task<IActionResult> UploadMediaToFb()
        {
            var form = HttpContext.Request.Form;
            if (HttpContext.Request.Form.Files.Any())
            {
                var file = HttpContext.Request.Form.Files[0];
                if (file.Length > 0)
                {
                    var handle = await _whatsapp.UploadMediaToFb(Convert.ToInt32(form["WhatsappAccountID"]), file);

                    var needToSaveFile = Convert.ToInt32(form["NeedToSaveFile"]);
                    if (needToSaveFile == 1)
                    {
                        var res = await _media.UploadFile(Convert.ToInt32(form["MediaID"]), file.FileName, form["FolderName"].ToString(), form["FileName"].ToString(), file.ContentType, file.Length, System.IO.Path.GetExtension(file.FileName));
                        using (var stream = System.IO.File.Create(res.FilePath))
                        {
                            await file.CopyToAsync(stream);
                        }
                        return Ok(new WhatsappMediaUploadResponseModel() { MediaID = res.MediaID, WMediaID = handle.id });
                    }
                    return Ok(new WhatsappMediaUploadResponseModel() { WMediaID = handle.id });
                }
                else
                {
                    throw new PBException("FileNotFound");
                }
            }
            else
            {
                throw new PBException("FileNotFound");
            }
        }

        #endregion

        #region ChatTemplate

        [HttpGet("get-template-count")]
        public async Task<IActionResult> GetTemplateCount()
        {
            var result = await _dbContext.GetByQueryAsync<int>($@"Select Count(*) TemplateCount From WhatsappTemplate
                                                            Where ClientID={CurrentClientID} and IsDeleted=0 and StatusID={(int)WhatsappTemplateStatus.APPROVED}", null);

            return Ok(result);
        }

        [HttpGet("get-chat-template")]
        public async Task<IActionResult> GetChatTemplate()
        {
            List<ChatTemplateListModel> result = new();
            result = await _dbContext.GetListByQueryAsync<ChatTemplateListModel>($@"Select WT.*,WV.VariableCount 
                                                                                        From WhatsappTemplate WT
                                                                                        Left Join (Select Count(*)VariableCount,WhatsappTemplateID From WhatsappTemplateVariable Where IsDeleted=0
                                                                                        Group By WhatsappTemplateID)WV on WV.WhatsappTemplateID=WT.TemplateID
                                                                                        Where ClientID={CurrentClientID} and WT.IsDeleted=0 and WhatsappTemplateStatus='APPROVED'", null);

            return Ok(result);
        }


        [HttpGet("get-chat-template-variable/{id}")]
        public async Task<IActionResult> GetChatTemplate(int id)
        {
            ChatTemplateSentModel result = new();

            result.template = await _dbContext.GetListByQueryAsync<WhatsappVariableValueModel>($@"Select * From WhatsappTemplateVariable 
                                                                                            Where  WhatsappTemplateID={id} and IsDeleted=0", null);

            result.TemplateID = id;

            return Ok(result);
        }


        [HttpGet("get-whatsapp-template-details/{id}")]
        public async Task<IActionResult> GetWhatsappTemplate(int id)
        {

            var result = await _dbContext.GetByQueryAsync<WhatsappTemplateMediaSentModel>($@"Select WT.*,FileName From WhatsappTemplate WT
                                                                                                 Left Join Media M on M.MediaID=WT.HeaderMediaID and M.IsDeleted=0
                                                                                                 Where  TemplateID={id} and WT.IsDeleted=0",null);



            return Ok(result ?? new());
        }
        #endregion


        [HttpPost("send-chat-template")]
        public async Task<IActionResult> SendChatTemplate(ChatTemplateSentModel model)
        {
            var template = await _dbContext.GetAsync<WhatsappTemplate>(model.TemplateID);
            string? lang = (await _dbContext.GetAsync<Language>(template.LanguageID.Value)).LanguageCode;
            //var messageRecipients = await _dbContext.GetListAsync<WhatsappBroadcastReceipient>($"BroadcastID={broadcastID}");

            WhatsappVariableValueModel headeImage = null;
            if (template.HeaderTypeID == (int)WhatsappTemplateHeaderType.MEDIA)
            {
                Media? media = null;
                if (template.HeaderMediaID == null)
                {
                    media = await _dbContext.GetAsync<Media>(model.HeaderMediaID.Value);
                }
                else
                {
                    media = await _dbContext.GetAsync<Media>(template.HeaderMediaID.Value);
                }
                if (media != null)
                {
                    headeImage = new()
                    {
                        Section = (int)WhatsappTemplateVariableSection.HEADER,
                        Value = _config["ServerURL"] + media.FileName,
                        VariableName = "Header Image"
                    };

                }
                switch ((WhatsappTemplateHeaderMediaType)template.HeaderMediaTypeID.Value)
                {
                    case WhatsappTemplateHeaderMediaType.DOCUMENT:
                        headeImage.DataType = (int)WhatsappTemplateVariableDataType.DOCUMENT;
                        break;
                    case WhatsappTemplateHeaderMediaType.IMAGE:
                        headeImage.DataType = (int)WhatsappTemplateVariableDataType.IMAGE;
                        break;
                    case WhatsappTemplateHeaderMediaType.VIDEO:
                        headeImage.DataType = (int)WhatsappTemplateVariableDataType.VIDEO;
                        break;
                    case WhatsappTemplateHeaderMediaType.LOCATION:
                        headeImage.DataType = (int)WhatsappTemplateVariableDataType.LOCATION;
                        break;
                }
            }


            var contact = await _dbContext.GetAsync<WhatsappContact>($"ContactID={model.ContactID}", null);

            //var variables = await _dbContext.GetListByQueryAsync<WhatsappVariableValueModel>($@"Select VariableName,Value,Section,DataType 
            //        from WhatsappBroadcastParameter P
            //        JOIN WhatsappTemplateVariable V on P.VariableID=V.VariableID
            //        Where P.IsDeleted=0 and V.IsDeleted=0 and ReceipientID={recipient.ReceipientID}");

            if (headeImage != null)
                model.template.Add(headeImage);

            List<TemplateComponent> data = new();
            List<TemplateParameter> header = new();
            List<TemplateParameter> body = new();
            List<TemplateParameter> buttons = new();
            string parameter = "";
            foreach (var variable in model.template)
            {
                parameter += variable.VariableName + ":" + variable.Value + "\n";

                TemplateParameter c = new();
                string type = "";
                switch ((WhatsappTemplateVariableDataType)variable.DataType)
                {
                    case WhatsappTemplateVariableDataType.TEXT:
                        c.type = "text";
                        c.text = variable.Value;
                        break;
                    case WhatsappTemplateVariableDataType.DATETIME:
                        c.type = "date_time";
                        c.date_time = new TemplateDateTime()
                        {
                            fallback_value = variable.Value
                        };
                        break;
                    case WhatsappTemplateVariableDataType.CURRENCY:
                        c.type = "currency";
                        c.currency = new()
                        {
                            fallback_value = variable.Value,
                            code = "INR",
                            amount_1000 = 2
                        };
                        break;
                    case WhatsappTemplateVariableDataType.IMAGE:
                        c.type = "image";
                        c.image = new TemplateImage()
                        {
                            link = variable.Value
                        };
                        break;
                    case WhatsappTemplateVariableDataType.DOCUMENT:
                        c.type = "document";
                        c.document = new()
                        {
                            link = variable.Value
                        };
                        break;
                    case WhatsappTemplateVariableDataType.VIDEO:
                        c.type = "video";
                        c.video = new()
                        {
                            link = variable.Value
                        };
                        break;
                }
                switch ((WhatsappTemplateVariableSection)variable.Section)
                {
                    case WhatsappTemplateVariableSection.HEADER: header.Add(c); break;
                    case WhatsappTemplateVariableSection.BODY: body.Add(c); break;
                }

            }
            if (header.Count > 0)
            {
                data.Add(new()
                {
                    type = "header",
                    parameters = header
                });
            }
            if (body.Count > 0)
            {
                data.Add(new()
                {
                    type = "body",
                    parameters = body
                });
            }
            if (buttons.Count > 0)
            {
                data.Add(new()
                {
                    type = "buttons",
                    parameters = buttons
                });
            }

            await _whatsapp.SendTemplateMessage(template.WhatsappAccountID.Value, contact.ContactID, template.TemplateName, lang, data, parameter, null);
            return Ok(new Success());

        }

        [HttpPost("get-broadcast-recipient-details")]
        public async Task<IActionResult> GetBroadcastExcelFile(MessageReceipientSearchModel model)
        {
            StringModel stringModel = new();
            stringModel.Value = await _whatsapp.GetBroadcastReceipientExcelData(model);
            return Ok(stringModel);
        }




        [HttpGet("get-template-variable-sample-excel-file/{templateID}")]
        public async Task<IActionResult> GetCustomerExcelFile(int templateID)
        {
            return Ok(await _whatsapp.GetTemplateVariableExcelData(templateID));
        }












    }

    [ApiVersion("2.0")]
    [Route("api/whatsapp")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class WhatsappV2Controller : BaseController
    {
        private readonly IDbContext _dbContext;
        private readonly IWhatsappRepository _whatsapp;
        private readonly IBackgroundJobClient _job;
        private readonly IHostEnvironment _env;
        private readonly IConfiguration _config;
        private readonly IMediaRepository _media;
        private readonly IDbConnection _cn;
        private readonly ICommonRepository _common;
        private readonly IMapper _map;


        public WhatsappV2Controller(IDbContext dbContext, IWhatsappRepository whatsapp, IBackgroundJobClient job, IHostEnvironment env, IConfiguration config, IMediaRepository media, IDbConnection cn, ICommonRepository common, IMapper map)
        {
            _dbContext = dbContext;
            _whatsapp = whatsapp;
            _job = job;
            _env = env;
            _config = config;
            _media = media;
            _cn = cn;
            _common = common;
            _map = map;
        }


        [HttpGet("v2/get-template/{Id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetV2Template(int Id)
        {
            return Ok(await _whatsapp.GetTemplate(Id));
        }

        [HttpPost("v2/save-template")]
        public async Task<IActionResult> SaveV2Template(WhatsappTemplatePageModel templateModel)
        {
            _cn.Open();
            using (var tran = _cn.BeginTransaction())
            {
                try
                {
                    templateModel.ClientID = CurrentClientID;
                    if (templateModel.TemplateID == 0)
                    {
                        var result = await _whatsapp.GenerateCloudAPICreateTemplatePostRequest(templateModel, tran);
                        if (result.error is null)
                        {
                            //Saving template to table
                            templateModel.WhatsappTemplateID = result.id;
                            templateModel.StatusID = result.status switch
                            {
                                "PENDING" => (int)WhatsappTemplateStatus.PENDING,
                                "APPROVED" => (int)WhatsappTemplateStatus.APPROVED,
                                "REJECTED" => (int)WhatsappTemplateStatus.REJECTED,
                                "PAUSED" => (int)WhatsappTemplateStatus.PAUSED,
                                "PENDING_DELETION" => (int)WhatsappTemplateStatus.PENDING_DELETION,
                                _ => null
                            };
                            templateModel.TemplateID = await _whatsapp.SaveWhatsappTemplate(templateModel, tran);
                            tran.Commit();
                            return Ok(new BaseSuccessResponse()
                            {
                                ResponseMessage = "TemplateSavedSuccessfully"
                            });
                        }
                        else
                        {
                            tran.Rollback();
                            return BadRequest(new BaseErrorResponse()
                            {
                                ResponseTitle = result.error.error_user_title,
                                ResponseMessage = result.error.error_user_msg
                            });
                        }
                    }
                    else
                    {
                        var result = await _whatsapp.GenerateCloudAPIEditTemplatePostRequest(templateModel, tran);
                        if (result.error is null)
                        {
                            //Saving template to table
                            templateModel.StatusID = (int)WhatsappTemplateStatus.PENDING;
                            templateModel.TemplateID = await _whatsapp.SaveWhatsappTemplate(templateModel, tran);
                            tran.Commit();
                            return Ok(new BaseSuccessResponse()
                            {
                                ResponseMessage = "TemplateEditedSuccessfully"
                            });
                        }
                        else
                        {
                            tran.Rollback();
                            return BadRequest(new BaseErrorResponse()
                            {
                                ResponseTitle = result.error.error_user_title,
                                ResponseMessage = result.error.error_user_msg
                            });
                        }
                    }



                }
                catch (Exception e)
                {
                    tran.Rollback();
                    return BadRequest(new BaseErrorResponse()
                    {
                        ResponseMessage = e.Message
                    });
                }
            }
        }

        [HttpGet("v2/import-facebook-templates")]
        public async Task<IActionResult> ImportFbTemplates()
        {
            try
            {
                await _whatsapp.SyncTemplatesWithFB(CurrentClientID);
                //_job.Enqueue(() => _whatsapp.SyncTemplatesWithFB(CurrentClientID));

                return Ok(new BaseSuccessResponse()
                {
                    ResponseMessage = "SyncBackgroundProcessStarted"
                });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("v2/get-template-details/{templateID}")]
        public async Task<IActionResult> GetTemplateDetails(int templateID)
        {
            return Ok(await _whatsapp.GetTemplateDetailsForBroadcast(templateID));
        }

        [HttpPost("v2/create-broadcast")]
        public async Task<IActionResult> ResendBroadcastMessage(CreateBroadcastModel BroadcastModel)
        {
            _cn.Open();
            using (var tran = _cn.BeginTransaction())
            {
                try
                {
                    var result = await _whatsapp.SaveBroadcastMessage(BroadcastModel, tran);
                    tran.Commit();
                    return Ok(result);
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    return BadRequest(new BaseErrorResponse()
                    {
                        ResponseMessage = ex.Message
                    });
                }
            }
        }

        [HttpPost("v2/recreate-broadcast")]
        public async Task<IActionResult> RecreateBroadcast(RecreatBroadcastModel resendBroadcastModel)
        {
            _cn.Open();
            using (var tran = _cn.BeginTransaction())
            {
                try
                {
                    var result = await _whatsapp.RecreateBroadcastMessage(resendBroadcastModel, tran);
                    tran.Commit();


                    var broadcast = await _dbContext.GetAsync<WhatsappBroadcast>(result.BroadcastID);
                    var template = await _dbContext.GetAsync<WhatsappTemplate>(resendBroadcastModel.BroadcastModel.TemplateID);
                    if (template.ClientID != CurrentClientID)
                    {
                        return BadRequest(new Error("Invalid Broadcast"));
                    }
                    if (broadcast.ScheduleTime.HasValue)
                    {
                        _job.Schedule(() => SendBroadcast(broadcast.BroadcastID), broadcast.ScheduleTime.Value.AddMinutes(TimeOffset));
                    }
                    else
                    {
                        _job.Enqueue(() => SendBroadcast(broadcast.BroadcastID));
                    }
                    broadcast.IsSent = true;
                    await _dbContext.SaveAsync(broadcast);

                    return Ok(result);
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    return BadRequest(new BaseErrorResponse()
                    {
                        ResponseMessage = ex.Message
                    });
                }
            }
        }

        [HttpPost("v2/save-template-variable")]
        public async Task<IActionResult> SaveTemplateVariable(TemplateVariablePageModel variableModel)
        {
            try
            {
                int variableID = await _whatsapp.SaveTemplateVariable(variableModel);
                if (variableID > 0)
                {
                    return Ok(new BaseSuccessResponse()
                    {
                        ResponseMessage = "TemplateVariableSaved"
                    });
                }
                else
                {
                    return BadRequest(new BaseErrorResponse()
                    {
                        ResponseMessage = "SomethingWentWrong"
                    });
                }
            }
            catch (Exception e)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ResponseMessage = e.Message
                });
            }

        }

        [HttpPost("v2/save-template-button")]
        public async Task<IActionResult> SaveTemplateButton(TemplateButtonPageModel buttonModel)
        {
            try
            {
                int buttonID = await _whatsapp.SaveTemplateButton(buttonModel);
                if (buttonID > 0)
                {
                    return Ok(new BaseSuccessResponse()
                    {
                        ResponseMessage = "TemplateButtonSaved"
                    });
                }
                else
                {
                    return BadRequest(new BaseErrorResponse()
                    {
                        ResponseMessage = "SomethingWentWrong"
                    });
                }
            }
            catch (Exception e)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ResponseMessage = e.Message
                });
            }

        }


        [HttpPost("v2/send-otp-template")]
        [AllowAnonymous]
        public async Task<IActionResult> SendOtpTemplate(WhatsappTemplateOtpPostModel model)
        {
            if ("12321" != model.Password)
            {
                return BadRequest(new Error("InvalidPassword"));
            }

            var template = await _dbContext.GetByQueryAsync<WhatsappTemplate>($"Select * From WhatsappTemplate Where TemplateID={model.TemplateID} And IsDeleted=0", null);
            string? langCode = (await _dbContext.GetAsync<Language>(template.LanguageID.Value)).LanguageCode;
            var contact = await _whatsapp.GetContact(model.ToNumber, template.ClientID.Value);
            string parameter = model.OTP;
            if (template is not null && langCode is not null && contact is not null)
            {
                List<TemplateComponent> data = new()
                {
                    new TemplateComponent()
                    {
                        type = "body",
                        parameters = new List<TemplateParameter>()
                        {
                            new TemplateParameter()
                            {
                                type="text",
                                text=model.OTP
                            }
                        }
                    },
                    new TemplateComponent()
                    {
                        type="button",
                        sub_type="url",
                        index=0,
                        parameters = new List<TemplateParameter>()
                        {
                            new TemplateParameter()
                            {
                                type="text",
                                text=model.OTP
                            }
                        }
                    }
                };

                await _whatsapp.SendTemplateMessage(template.WhatsappAccountID.Value, contact.ContactID, template.TemplateName, langCode, data, parameter, null);
                return Ok(new Success());
            }
            else
            {
                return BadRequest(new Error("Please provide valid TemplateID,TemplateName and WhatsappAccountID for sending the otp"));
            }
        }

        [HttpGet("v2/schedule-broadcast/{broadcastID}")]
        public async Task<IActionResult> ScheduleBroadcast(int broadcastID)
        {
            var broadcast = await _dbContext.GetAsync<WhatsappBroadcast>(broadcastID);
            var template = await _dbContext.GetAsync<WhatsappTemplate>(broadcast.TemplateID.Value);
            if (template.ClientID != CurrentClientID)
            {
                return BadRequest(new Error("Invalid Broadcast"));
            }
            if (broadcast.ScheduleTime.HasValue)
            {
                _job.Schedule(() => SendBroadcast(broadcast.BroadcastID), broadcast.ScheduleTime.Value.AddMinutes(TimeOffset));
            }
            else
            {
                _job.Enqueue(() => SendBroadcast(broadcast.BroadcastID));
            }
            broadcast.IsSent = true;
            await _dbContext.SaveAsync(broadcast);
            return Ok(new Success("MessageProcessBackground"));
        }

        [HttpGet("v2/send-broadcast/{broadcastID}")]
        public async Task SendBroadcast(int broadcastID)
        {
            var broadcast = await _dbContext.GetAsync<WhatsappBroadcast>(broadcastID);
            if (broadcast != null)
            {
                string? documentFileName = "";
                var template = await _dbContext.GetAsync<WhatsappTemplate>(broadcast.TemplateID.Value);
                string? lang = (await _dbContext.GetAsync<Language>(template.LanguageID.Value)).LanguageCode;
                var messageRecipients = await _dbContext.GetListAsync<WhatsappBroadcastReceipient>($"BroadcastID={broadcastID}", null);

                WhatsappVariableValueModel headeImage = null;
                if (template.HeaderTypeID == (int)WhatsappTemplateHeaderType.MEDIA)
                {
                    int? mediaID = broadcast.HeaderMediaID ?? template.HeaderMediaID;
                    if (mediaID != null)
                    {
                        var media = await _dbContext.GetAsync<Media>(mediaID.Value);
                        documentFileName = media.FileName;
                        headeImage = new()
                        {
                            Section = (int)WhatsappTemplateVariableSection.HEADER,
                            Value = _config["ServerURL"] + media.FileName,
                            VariableName = "Header Image"
                        };
                        switch ((WhatsappTemplateHeaderMediaType)template.HeaderMediaTypeID.Value)
                        {
                            case WhatsappTemplateHeaderMediaType.DOCUMENT:
                                headeImage.DataType = (int)WhatsappTemplateVariableDataType.DOCUMENT;
                                break;
                            case WhatsappTemplateHeaderMediaType.IMAGE:
                                headeImage.DataType = (int)WhatsappTemplateVariableDataType.IMAGE;
                                break;
                            case WhatsappTemplateHeaderMediaType.VIDEO:
                                headeImage.DataType = (int)WhatsappTemplateVariableDataType.VIDEO;
                                break;
                            case WhatsappTemplateHeaderMediaType.LOCATION:
                                headeImage.DataType = (int)WhatsappTemplateVariableDataType.LOCATION;
                                break;
                        }
                    }
                }

                foreach (var recipient in messageRecipients)
                {
                    var contact = await _whatsapp.GetContact(recipient.Phone, template.ClientID.Value, "", broadcast.BroadcastID);

                    var variables = await _dbContext.GetListByQueryAsync<WhatsappVariableValueModel>($@"Select VariableName,Value,Section,DataType 
                        from WhatsappBroadcastParameter P
                        JOIN WhatsappTemplateVariable V on P.VariableID=V.VariableID
                        Where P.IsDeleted=0 and V.IsDeleted=0 and ReceipientID={recipient.ReceipientID}", null);

                    if (headeImage != null)
                        variables.Add(headeImage);

                    TemplateSendParameters data = new();
                    List<TemplateParameter> header = new();
                    List<TemplateParameter> body = new();
                    List<TemplateButton> buttons = new();
                    string parameter = "";
                    foreach (var variable in variables)
                    {
                        parameter += variable.VariableName + ":" + variable.Value + "\n";

                        TemplateParameter c = new();
                        string type = "";
                        switch ((WhatsappTemplateVariableDataType)variable.DataType)
                        {
                            case WhatsappTemplateVariableDataType.TEXT:
                                c.type = "text";
                                c.text = variable.Value;
                                break;
                            case WhatsappTemplateVariableDataType.DATETIME:
                                c.type = "date_time";
                                c.date_time = new TemplateDateTime()
                                {
                                    fallback_value = variable.Value
                                };
                                break;
                            case WhatsappTemplateVariableDataType.CURRENCY:
                                c.type = "currency";
                                c.currency = new()
                                {
                                    fallback_value = variable.Value,
                                    code = "INR",
                                    amount_1000 = 2
                                };
                                break;
                            case WhatsappTemplateVariableDataType.IMAGE:
                                c.type = "image";
                                c.image = new TemplateImage()
                                {
                                    link = variable.Value
                                };
                                break;
                            case WhatsappTemplateVariableDataType.DOCUMENT:
                                c.type = "document";
                                c.document = new()
                                {
                                    link = variable.Value,
                                    filename = documentFileName.Substring(documentFileName.LastIndexOf('/')+1)
                                };
                                break;
                            case WhatsappTemplateVariableDataType.VIDEO:
                                c.type = "video";
                                c.video = new()
                                {
                                    link = variable.Value
                                };
                                break;
                        }
                        switch ((WhatsappTemplateVariableSection)variable.Section)
                        {
                            case WhatsappTemplateVariableSection.HEADER: header.Add(c); break;
                            case WhatsappTemplateVariableSection.BODY: body.Add(c); break;
                            case WhatsappTemplateVariableSection.BUTTON:

                                buttons.Add(new TemplateButton()
                                {
                                    type = "url",
                                    url = variable.Value
                                });

                                break;
                        }

                    }

                    //Header
                    if (header.Count > 0)
                    {
                        data.Components ??= new List<TemplateComponent>();
                        data.Components.Add(new TemplateComponent()
                        {
                            type = "header",
                            parameters = header
                        });
                    }

                    //Body
                    if (body.Count > 0)
                    {
                        data.Components ??= new List<TemplateComponent>();
                        data.Components.Add(new TemplateComponent()
                        {
                            type = "body",
                            parameters = body
                        });
                    }

                    //Button
                    if (buttons.Count > 0)
                    {
                        data.Buttons = new();
                        data.Buttons = buttons;
                    }

                    await _whatsapp.SendTemplateMessage(template.WhatsappAccountID.Value, contact.ContactID, template.TemplateName, lang, data, parameter, recipient.ReceipientID);
                }
            }
        }

        [HttpGet("v2/activate-chatbot/{whatsappAccountID}")]
        public async Task<IActionResult> ActivateChatbot(int whatsappAccountID)
        {
            await _dbContext.ExecuteAsync($"Update WhatsappAccount Set IsChatbotEnabled=1 Where WhatsappAccountID={whatsappAccountID}", null);
            return Ok(true);
        }

        [HttpGet("v2/deactivate-chatbot/{whatsappAccountID}")]
        public async Task<IActionResult> DeactivateChatbot(int whatsappAccountID)
        {
            await _dbContext.ExecuteAsync($"Update WhatsappAccount Set IsChatbotEnabled=0 Where WhatsappAccountID={whatsappAccountID}", null);
            return Ok(true);
        }
    }
}
