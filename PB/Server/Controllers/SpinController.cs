using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PB.DatabaseFramework;
using PB.Model;
using PB.Server.Repository;
using PB.Shared;
using PB.Shared.Enum;
using PB.Shared.Models;
using PB.Shared.Tables;
using System.Data;
using AutoMapper;
using PB.Shared.Models.Spin;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using PB.Shared.Tables.Spin;
using PB.Model.Models;
using NPOI.HSSF.UserModel;
using PB.Shared.Tables.Whatsapp;
using PB.EntityFramework;
using static System.Net.WebRequestMethods;
using System.Net.Mail;
using PB.Shared.Enum.Court;

namespace PB.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpinController : ControllerBase
    {
        private readonly IDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IHostEnvironment _env;
        private readonly IMediaRepository _media;
        private readonly ISpinRepository _spin;
        public SpinController(IDbContext dbContext, IMapper mapper, IHostEnvironment env, IMediaRepository media, ISpinRepository spin)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _env = env;
            _media = media;
            _spin = spin;
        }

        #region Registration


        [HttpPost("generate-otp")]
        public async Task<IActionResult> Signup(WhatsappContactModel model)
        {
            //try
            //{
            var exist = await _dbContext.GetAsync<WhatsappContact>($@"Phone=@Phone", new { Phone = model.Phone });
            int Contestid = await _dbContext.GetByQueryAsync<int>("select * from Contest Where IsDeleted=0",null);
            if (exist != null && Contestid == model.ContestId)
            {
                model.ContactID = exist.ContactID;
                var check = await _dbContext.GetAsync<ContestParticipant>($"ContactID={exist.ContactID}", null);
                if (check != null && check.ContactID != 0 && check.ContestID == Contestid)
                {
                    return BadRequest(new Error("You have already participated in this contest !...."));
                }
            }
            else
            {
                var res = _mapper.Map<WhatsappContact>(model);
                model.ContactID = await _dbContext.SaveAsync(res);
            }

            Random rnd = new();
            string Otp = (rnd.Next(1000, 9999)).ToString();
            await _dbContext.ExecuteAsync($"Update WhatsappContact Set OTP='{Otp}' Where ContactID={model.ContactID}");

            //await SendMesage(model.Phone, Otp);
            ContestParticipant participant = new()
            {
                ContactID = model.ContactID,
                ContestID = model.ContestId
            };
            await _dbContext.SaveAsync(participant);
            IdnValuePair value = new()
            {
                ID = model.ContactID,
                Value = model.Name
            };
            return Ok(value);
            //}
            //catch (Exception ex)
            //{
            //    return BadRequest(new Error(ex.Message));
            //}
        }






        #region confirm and resend OTp

        [HttpPost("confirm-otp")]
        public async Task<IActionResult> ConfirmOTP(WhatsappContact model)
        {
            var user = await _dbContext.GetAsync<WhatsappContact>(@$"Phone=@Phone and OTP=@OTP", new { Phone = model.Phone , OTP = model.OTP });

            if (user.OTP != model.OTP)
            {
                return BadRequest(new Error("InvalidOTP"));
            }
            else
            {
                await _dbContext.ExecuteAsync($"UPDATE WhatsappContact SET IsVerified=1, OTP=NULL WHERE ContactID={user.ContactID}");
            }
            return Ok(new Success());
        }




        [HttpGet("resend-otp/{phone}")]
        public async Task<IActionResult> ResendOTP(string phone)
        {
            var user = await _dbContext.GetByQueryAsync<WhatsappContact>($@"Select * From  WhatsappContact                    
                                                                        where Phone=@Phone  and IsDeleted=0", new { Phone  = phone });

            Random rnd = new();
            user.OTP = (rnd.Next(1000, 9999)).ToString();
            WhatsappContact contact = new()
            {
                ContactID = user.ContactID,
                OTP = user.OTP,
                //OTPSentOn = user.OTPSentOn
            };
            await _dbContext.ExecuteAsync($"Update WhatsappContact Set OTP=@OTP Where ContactID={user.ContactID}", new { OTP = user.OTP });
            if (user.Phone == phone)
                await SendMesage(user.Phone, user.OTP);

            return Ok(new Success());

        }


        #endregion  

        async Task SendMesage(string Phone, string Otp)
        {

            string url = "https://message.progbiz.io/api/Message/send-otp/3/" + Phone + "/" + Otp + "?lang=en";
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

            using (var httpClient = new HttpClient(clientHandler))
            {
                using (var response = await httpClient.GetAsync(url))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                }
            }
        }

        #endregion

        #region Contest

        [HttpPost("save-contest")]
        public async Task<IActionResult> Save(ContestModel model)
        {
            var res = _mapper.Map<Contest>(model);
            model.ContestID = await _dbContext.SaveAsync(res);

            foreach (var giftItem in model.Gifts.Where(s => s.GiftID == 0))
            {
                var gift = _mapper.Map<ContestGift>(giftItem);
                gift.ContestID = model.ContestID;
                gift.GiftID = await _dbContext.SaveAsync(gift);

                var member = new List<ContestPrize>();
                for (int i = 1; i <= gift.NumberOfPiece; i++)
                {
                    member.Add(new ContestPrize()
                    {
                        GiftID = gift.GiftID,
                    });
                    await _dbContext.SaveListAsync<ContestPrize>(member, $"GiftID={gift.GiftID}", null, false);
                }
            }
            return Ok(new Success());
        }


        [HttpGet("delete-gift")]
        public async Task<IActionResult> DeleteGift(int Id)
        {
            var prizeCount = await _dbContext.GetByQueryAsync<int>($@"Select Count(*) from ContestPrize Where GiftID={Id} and IsDeleted=0 and ContactID is not null", null);
            if (prizeCount == 0)
            {
                var gift = await _dbContext.GetAsync<ContestGift>(Id);
                var logId = await _dbContext.InsertDeleteLogSummary(Id, gift.GiftName);
                await _dbContext.DeleteAsync<ContestGift>(Id);
                await _dbContext.DeleteSubItemsAsync<ContestPrize>("GiftID", Id, logSummaryID: logId);
                return Ok(new Success());
            }
            else
            {
                return BadRequest(new Error("Gift already distributed"));
            }

        }

        [HttpGet("get-contest-by/{contestId}")]
        public async Task<IActionResult> GetContestSingle(int ContestId)
        {
            var res = await _dbContext.GetByQueryAsync<ContestModel>($"SELECT * From Contest Where ContestID={ContestId} and IsDeleted=0", null);
            res.Winners = await _dbContext.GetListByQueryAsync<WinnersListModel>($@"Select GiftName,Name,ProfileName
                                                                                    From ContestPrize P 
                                                                                    Join ContestGift g On g.GiftID=P.GiftID
                                                                                    join WhatsappContact W On W.ContactID=P.ContactID
                                                                                    Where G.ContestID={ContestId} and P.IsDeleted=0 and G.PrizeType={(int)PrizeTypes.BumberPrize}", null);

            return Ok(res);
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("get-all-contest")]
        public async Task<IActionResult> GetAllContests(PagedListPostModel model)
        {
            PagedListQueryModel query = model;
            query.Select = $@"SELECT * FROM Contest";
            query.WhereCondition = $"IsDeleted=0";
            query.SearchLikeColumnNames = new() { "ContestName" };
            var res = await _dbContext.GetPagedList<Contest>(query, null);
            return Ok(res);
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("delete-contest")]
        public async Task<IActionResult> DeleteContest(int id)
        {
            await _dbContext.DeleteAsync<Contest>(id);
            await _dbContext.DeleteSubItemsAsync<ContestGift>("ContestID", id);
            await _dbContext.ExecuteAsync($"Update ContestPrize Set IsDeleted=1 Where GiftID in(Select GiftID From ContestGift Where ContestID={id})");
            return Ok(new Success());
        }
        [HttpGet("get-gift-by/{contestid}")]
        public async Task<IActionResult> GetGift(int contestId)
        {
            var res = await _dbContext.GetListByQueryAsync<ContestGiftModel>($"Select Row_Number() Over(Order By GiftID) As RowNumber,* from ContestGift Where ContestID={contestId} and IsDeleted=0 ", null);
            return Ok(res);
        }
        #endregion

        #region Winners
        [HttpGet("get-all-winners-by/{contestid}")]
        public async Task<IActionResult> GetAllWinnersByContestId(int contestid)
        {
            var winners = await _dbContext.GetListByQueryAsync<WinnersListModel>($@"Select  P.*,Name,GiftName,ProfileName
                                                                                From ContestPrize P
                                                                                Join WhatsappContact C On C.ContactID=P.ContactID and C.IsDeleted=0
                                                                                JOin ContestGift  G On G.GiftID=P.GiftID and G.IsDeleted=0
                                                                                Where P.IsDeleted=0 and G.ContestID={contestid} and PrizeType={(int)PrizeTypes.NormalPrize}", null);
            return Ok(winners);
        }
        #endregion

        #region Assigining Prize
        [HttpGet("get-count/{contestid}/{contactid}")]
        public async Task<IActionResult> GetCount(int contestid, int contactid)
        {
            var res = new CountModel();
            res.ExpectedParticipant = await _dbContext.GetByQueryAsync<int>(@$"select ExpectedParticipants From Contest Where COntestID={contestid} and IsDeleted=0", null);
            res.TotalNoOfGifts = await _dbContext.GetByQueryAsync<int>($@"SELECT SUM(NumberOfPiece) AS TotalNoOfGifts
                                                                                    FROM ContestGift G
                                                                                    WHERE ContestID = {contestid} and PrizeType={(int)PrizeTypes.NormalPrize}
                                                                                    Group By ContestID", null);
            res.CountOfParticipants = await _dbContext.GetByQueryAsync<int>(@$"SELECT Count(ParticipantID) AS CountOfParticipants
                                                                                FROM ContestParticipant
                                                                                WHERE ContestID = {contestid}
                                                                                Group By ContestID", null);
            res.PositionOfParticipant = await _dbContext.GetByQueryAsync<int>($@"SELECT  PositionOfParticipant,  CountOfParticipants,  ContestID,   ContactID
                                                                        FROM (
                                                                            SELECT
                                                                                ROW_NUMBER() OVER (ORDER BY CountOfParticipants DESC) AS PositionOfParticipant,
                                                                                CountOfParticipants,
                                                                                ContestID,
                                                                                ContactID
                                                                            FROM (
                                                                                SELECT
                                                                                    COUNT(ContactID) AS CountOfParticipants,
                                                                                    ContestID,
                                                                                    ContactID
                                                                                FROM
                                                                                    ContestParticipant
                                                                                WHERE
                                                                                    ContestID = {contestid}
                                                                                GROUP BY
                                                                                    ContestID,
                                                                                    ContactID
                                                                            ) AS Subquery
                                                                        ) AS FinalResult
                                                                        WHERE
                                                                            ContactID ={contactid} ", null);
            List<ContestGift> availableGifts = await _dbContext.GetListByQueryAsync<ContestGift>($@"SELECT G.*
                                                                                    FROM ContestGift G
                                                                                    JOIN ContestPrize P ON P.GiftID = G.GiftID
                                                                                    WHERE G.ContestID = {contestid} and PrizeType={(int)PrizeTypes.NormalPrize} AND P.ContactID IS NULL", null);

            if (availableGifts.Count > 0)
            {
                Random random = new Random();
                int selectedIndex = random.Next(0, availableGifts.Count);

                ContestGift selectedGift = availableGifts[selectedIndex];
                res.GiftName = selectedGift.GiftName;
                res.GiftID = selectedGift.GiftID;
            }

            return Ok(res);


        }

        [HttpGet("get-contest-giftname/{contestid}")]
        public async Task<IActionResult> getGiftsForWheel(int contestid)
        {

            var res = await _dbContext.GetListByQueryAsync<SpinGiftModel>(@$"SELECT * From ContestGift Where ContestID={contestid}
                                                           and IsDeleted=0 and PrizeType={(int)PrizeTypes.NormalPrize}", null);

            //if (res != null)
            //{
            //    Random r = new Random();
            //    foreach (var item in res)
            //    {
            //        item.Color = String.Format("#{0:X6}", r.Next(0x1000000));
            //    }
            //}
            return Ok(res);
        }

        [HttpPost("update-winner")]
        public async Task<IActionResult> UpdateWinner(ContactIDModel model)
        {
            await _dbContext.ExecuteAsync(@$"UPDATE TOP (1) ContestPrize SET  ContactID = {model.ContactID}
                                            WHERE GiftID = {model.GiftID} and ContactId Is NULL");
            return Ok(new Success());
        }
        #endregion

        #region Select BumberPrize Winners
        [HttpGet("get-bumber-prize-winners/{contestid}")]
        public async Task<IActionResult> GetWinners(int contestId)
        {
            int result = await _dbContext.GetByQueryAsync<int>(@$" SELECT COUNT(*) 
                                                FROM ContestPrize P
                                                JOIN ContestGift G On G.GiftID=P.GiftID
                                                WHERE ContestID = {contestId} AND ContactID =Null and 
                                                PrizeType = {(int)PrizeTypes.BumberPrize}", null);
            if (result != 0)
            {
                return BadRequest(new Error("Sorry!... Already Bumber Prize Selected"));
            }
            else
            {


                List<int> participants = await _dbContext.GetListByQueryAsync<int>($@"Select CP.ContactID
                                                                                        From Contest C
                                                                                        Left Join ContestParticipant CP on CP.ContestID=C.ContestID and CP.IsDeleted=0
                                                                                        Left Join ContestPrize CZ on CZ.ContactID=CP.ContactID and CZ.IsDeleted=0
                                                                                        WHERE  C.ContestID ={contestId} and CZ.ContactID IS NULL and C.IsDeleted=0", null);

                List<ContestGift> availableGifts = await _dbContext.GetListByQueryAsync<ContestGift>($@" SELECT  G.*
                                                                                        FROM ContestGift G
                                                                                        JOIN ContestPrize P ON P.GiftID = G.GiftID and P.IsDeleted=0
                                                                                        WHERE G.ContestID = {contestId} AND P.ContactID IS NULL AND PrizeType={(int)PrizeTypes.BumberPrize} and G.IsDeleted=0", null);

                List<int> WinnersList = new();
                if (availableGifts.Count > 0)
                {
                    Random random = new Random();

                    foreach (var gift in availableGifts)
                    {
                        for (int j = 0; j < gift.NumberOfPiece; j++)
                        {
                            int selectedIndex = random.Next(0, participants.Count);
                            int WinnerID = participants[selectedIndex];
                            await _dbContext.ExecuteAsync(@$"UPDATE  ContestPrize SET  ContactID = {WinnerID}
                                            WHERE giftId = {gift.GiftID} and ContactId Is NULL");
                            WinnersList.Add(WinnerID);
                            participants.RemoveAt(selectedIndex);
                        }
                    }
                }
                List<WinnersListModel> WinnersData = new();
                if (WinnersList.Count > 0)
                {
                    string winnersIDs = string.Join(',', WinnersList);
                    WinnersData = await _dbContext.GetListByQueryAsync<WinnersListModel>($@"SELECT  C.*, W.Name,GiftName, G.GiftID
                                                                                                                FROM Contestparticipant C
                                                                                                                JOIN WhatsappContact W ON W.ContactID = C.ContactID and W.IsDeleted=0
                                                                                                                Left JOIN ContestPrize P ON P.ContactID = C.ContactID and P.IsDeleted=0
                                                                                                                Left JOIN ContestGift G ON G.GiftID = P.GiftID and G.IsDeleted=0
                                                                                                                WHERE C.ContactID in ({winnersIDs}) and C.IsDeleted=0

                    ", null);
                }

                return Ok(WinnersData);
            }


        }
        #endregion

        #region Contest Report
        [HttpGet("get-contest-list")]
        public async Task<IActionResult> GetContest()
        {
            var res = await _dbContext.GetListByQueryAsync<IdnValuePair>(@$"Select ContestID as ID,ContestName as  Value
                                                                            From Contest 
                                                                            Where IsDeleted=0", null);

            return Ok(res ?? new());
        }

        [HttpPost("get-contest-report-list")]
        public async Task<IActionResult> GetReportListStatus(ReportIDSentModel model)
        {
            string condition = "";
            if(model.StatusID!=0)
            {
                switch(model.StatusID)
                {
                    case (int)ContestParticipantStatus.GiftSend:
                        condition = $" and CZ.GiftID is not null and IsSent=1";
                        break;
                    case (int)ContestParticipantStatus.GiftNotSend:
                        condition = $" and CZ.GiftID is not null and IsSent=0";
                        break;
                    case (int)ContestParticipantStatus.Winner:
                        condition = $" and CZ.GiftID is not null";
                        break;
                    case (int)ContestParticipantStatus.Looser:
                        condition = $" and CZ.GiftID is null";
                        break;
                }
            }
            string query= @$"Select Name,Phone,ContestName,GiftName,C.ContestID,CZ.ContactID,IsFinished,IsSent,SentOn,CZ.GiftID
                                                                                    From Contest C
                                                                                    Left Join ContestParticipant CP on CP.ContestID=C.ContestID and CP.IsDeleted=0
                                                                                    Left Join WhatsappContact WC on WC.ContactID=CP.ContactID and WC.IsDeleted=0
                                                                                    Left Join (Select P.*,GiftName,ContestID
			                                                                                        From ContestPrize P
			                                                                                        Join ContestGift G on G.GiftID=P.GiftID and G.IsDeleted=0
			                                                                                        Where P.IsDeleted=0) CZ on CZ.ContestID=C.ContestID and CP.ContactID=CZ.ContactID
                                                                                    Where IsFinished=1 and C.IsDeleted=0 and C.ContestID=@ContestID"+condition;
            var res = await _dbContext.GetListByQueryAsync<ContestReportViewModel>(query,model);

            return Ok(res ?? new());
        }

        [HttpPost("update-gift-send")]
        public async Task<IActionResult> SaveIsFinished(ContestGiftSendModel model)
        {
            string? Condition = "";
            if (model.IsSent == true)
            {
                Condition = $"SentOn='{DateTime.UtcNow}'";
            }
            else
            {
                Condition = "SentOn=NULL";
            }

            await _dbContext.ExecuteAsync($@"UPDATE ContestPrize
                                            SET IsSent='{model.IsSent}',{Condition}
                                            Where ContactID={model.ContactID}");

            int contestID = await _dbContext.GetByQueryAsync<int>($@"Select ContestID From ContestParticipant Where ContactID={model.ContactID} and IsDeleted=0",null);

            return Ok(contestID);
        }


        #endregion

        #region AISC

        #region AISC Membership

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("get-membership-application-list")]
        public async Task<IActionResult> GetMembership(PagedListPostModelWithFilter model)
        {
            PagedListQueryModel query = model;
            query.Select = $@"Select AM.* ,M.FileName
                                From AISCMembership AM
                                Left Join Media M on M.MediaID=AM.MediaID and M.IsDeleted=0";
            query.WhereCondition = $"AM.IsDeleted=0";
            query.SearchLikeColumnNames = new() { "FirstName"};
            var res = await _dbContext.GetPagedList<AISCMembershipViewModel>(query, null);
            return Ok(res ?? new());
        }

        [HttpPost("save-aisc-membership")]
        public async Task<IActionResult> SaveMembership(AISCMembershipModel model)
        {
            var number = await _dbContext.GetByQueryAsync<int>($@"
                                        Select Number 
                                        From AISCMembership
                                        Where ID<>@ID and Number=@Number AND IsDeleted=0
                ",model);

            if (number!=0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ResponseTitle = "InvalidSubmission",
                    ResponseMessage = "NumberExist"
                });
            }
            AISCMembership Membership = new()
            {
                ID = model.ID,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Gender = model.Gender,
                EmailAddress = model.EmailAddress,
                PhoneNo = model.PhoneNo,
                WhatsappNo = model.WhatsappNo,
                Nationality = model.Nationality,
                Region = model.Region,
                VisaStatus = model.VisaStatus,
                Age = model.Age,
                LevelOfGame = model.LevelOfGame,
                TeamID = model.TeamID,
                Prefix = model.Prefix,
                Number = model.Number.Value,
                MediaID = model.MediaID,
                PhoneISDCode = model.PhoneISDCode,
                WhatsappISDCode = model.WhatsappISDCode,
                Bloodgroup = model.BloodGroup,
                Message = model.Message,
            };
            Membership.ID = await _dbContext.SaveAsync(Membership);

            return Ok(new Success());
        }


        [HttpGet("get-aisc-membership/{ID}")]
        public async Task<IActionResult> GetPackage(int ID)
        {
            var res = await _dbContext.GetByQueryAsync<AISCMembershipModel>($@"Select AM.*,TeamName,FileName 
                                                                                From AISCMembership AM
                                                                                Left Join AISCTeam AT on AT.TeamID=AM.TeamID and AT.IsDeleted=0
                                                                                Left Join Media M on M.MediaID=AM.MediaID and M.IsDeleted=0
                                                                                Where AM.ID={ID} and AM.IsDeleted=0", null);

            return Ok(res ?? new());
        }


        [HttpPost("get-membership-number")]
        public async Task<IActionResult> GetEnquiryNumber(MembershipNumberPostModel model)
        {
            var res = await _dbContext.GetByQueryAsync<IntModel>($@"Select ID as Id From AISCMembership
                                                                            Where Number=@Number and ID<>@ID and IsDeleted=0", model);
            return Ok(res ?? new());
        }

        [HttpGet("get-aisc-membership-view/{ID}")]
        public async Task<IActionResult> GetCourtPackageView(int ID)
        {
            var res = await _dbContext.GetByQueryAsync<AISCMembershipViewModel>($@"Select AM.*,TeamName
                                                                                    From AISCMembership AM
                                                                                    Left Join AISCTeam AT on AT.TeamID=AM.TeamID and AT.IsDeleted=0
                                                                                    Where AM.ID={ID} and AM.IsDeleted=0", null);
            return Ok(res);
        }

        [HttpGet("get-aisc-membership-menu-list")]
        public async Task<IActionResult> GetMenuList()
        {
            var result = await _dbContext.GetListByQueryAsync<ViewPageMenuModel>($@"Select ID as ID,FirstName as MenuName
                                                                                    From AISCMembership
                                                                                    Where IsDeleted=0", null);
            return Ok(result);
        }

        [HttpGet("delete-aisc-membership")]
        public async Task<IActionResult> DeletePackage(int Id)
        {
            await _dbContext.DeleteAsync<AISCMembership>(Id);
            return Ok(true);
        }


        [HttpGet("get-membership-numbers")]
        public async Task<IActionResult> GetMembershipNumber()
        {
            var result = await _dbContext.GetByQueryAsync<int>($@"Select ISNULL(MAX(Number)+ 1,1) as Number
                                                                    From AISCMembership
                                                                    Where IsDeleted=0", null);
            return Ok(result);
        }


        [HttpGet("get-membership-card-pdf/{ID}")]
        public async Task<IActionResult> GetQuotationPDFDetails(int ID)
        {
            StringModel htmlContent = new();
            htmlContent.Value = await _spin.GetMembershipcardHtmlContent(ID, null);
            return Ok(htmlContent);
        }

        #endregion

        #region AISC Membership Html

        [HttpPost("save-html-aisc-membership")]
        public async Task<IActionResult> SaveTeam()
        {
            int? MediaID = null;
            int Number = await _dbContext.GetByQueryAsync<int>($@"Select ISNULL(MAX(Number)+ 1,1) as Number
                                                                    From AISCMembership
                                                                    Where IsDeleted=0", null);
            var form = HttpContext.Request.Form;
            if (HttpContext.Request.Form.Files.Any())
            {
                var file = HttpContext.Request.Form.Files[0];
                if (file.Length > 0)
                {
                    var res = await _media.UploadFile(Convert.ToInt32(form["MediaID"]), file.FileName, form["FolderName"].ToString(), form["FileName"].ToString(), file.ContentType, file.Length, System.IO.Path.GetExtension(file.FileName));
                    using (var stream = System.IO.File.Create(res.FilePath))
                    {
                        await file.CopyToAsync(stream);
                    }
                    MediaID = res.MediaID;
                    // res.FilePath = await _dbContext.GetFieldsAsync<Media, string>("FileName", $"MediaID={res.MediaID}");
                }
                else
                {
                    throw new PBException("FileNotFound");
                }
            }

            AISCMembership membership = new()
            {
                FirstName = form["fname"].ToString(),
                LastName = form["lname"].ToString(),
                Gender = form["gender"].ToString(),
                EmailAddress = form["email"].ToString(),
                PhoneISDCode = form["PhoneISDCode"].ToString(),
                PhoneNo = form["phone"].ToString(),
                WhatsappISDCode = form["WhatsappISDCode"].ToString(),
                WhatsappNo = form["whatsapp"].ToString(),
                Nationality = form["nationality"].ToString(),
                Region = form["region"].ToString(),
                VisaStatus = form["visa"].ToString(),
                Age = form["age"].ToString(),
                LevelOfGame = form["level"].ToString(),
                Message = form["message"].ToString(),
                Prefix = "AISC",
                Number = Number,
                MediaID = MediaID,
                Bloodgroup = form["blood"].ToString(),

            };
            await _dbContext.SaveAsync(membership);


            return Ok(new Success());
        }



        [HttpGet("get-all-aisc-members-list")]
        public async Task<IActionResult> GetAllMembersList()
        {

            var result = await _dbContext.GetListByQueryAsync<AISCMembersListViewModel>($@"Select FirstName +' '+LastName as Name,(Prefix+CAST(Number AS CHAR)) as MembershipNo,FileName
                                                                    From AISCMembership AM 
                                                                    Left Join Media M on M.MediaID=AM.MediaID and M.IsDeleted=0
                                                                    Where  AM.IsDeleted=0", null);
            return Ok(result);
        }


        [HttpGet("get-aisc-members-list")]
        public async Task<IActionResult> GetAllMembersListByTeam()
        {
            List<AISCTeamViewListModel> TeamList = new();

            TeamList = await _dbContext.GetListByQueryAsync<AISCTeamViewListModel>($@"Select AT.*,MM.FileName as LogoName From AISCTeam AT
                                                                                        Left Join Media MM on MM.MediaID=AT.MediaID and MM.IsDeleted=0
                                                                                        Where AT.IsDeleted=0", null);

            foreach (var team in TeamList)
            {
                team.Members = await _dbContext.GetListByQueryAsync<AISCMembersListViewModel>($@"Select CASE WHEN AM.ID=AT.ManagerMembershipID THEN 'Team Lead' ELSE ' ' END as Teamleader,
                                                                                            FirstName +' '+LastName as Name,Prefix+CAST(Number AS CHAR) as MembershipNo,FileName
                                                                                            From AISCMembership AM
                                                                                            Left Join AISCTeam AT on AT.TeamID=AM.TeamID and AT.IsDeleted=0
                                                                                            Left Join Media M on M.MediaID=AM.MediaID and M.IsDeleted=0
                                                                                            Where AM.TeamID={team.TeamID} and AM.IsDeleted=0
                                                                                            Order By Teamleader desc ", null);
            }
            return Ok(TeamList ?? new());
        }

        #endregion

        #region AISC Team


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("get-aisc-teams")]
        public async Task<IActionResult> GetTeams(PagedListPostModel model)
        {
            PagedListQueryModel query = model;
            query.Select = $@"Select AT.TeamID,TeamName,FirstName 
                                From AISCTeam AT
                                Join AISCMembership AM on AM.ID=AT.ManagerMembershipID and AM.IsDeleted=0";
            query.WhereCondition = $"AT.IsDeleted=0";
            query.SearchLikeColumnNames = new() { "FirstName", "TeamName" };
            var res = await _dbContext.GetPagedList<AISCTeamViewModel>(query, null);
            return Ok(res ?? new());
        }

        [HttpPost("save-aisc-team")]
        public async Task<IActionResult> SaveTeam(AISCTeamModel model)
        {
            AISCTeam Team = new()
            {
                TeamID = model.TeamID,
                TeamName = model.TeamName,
                ManagerMembershipID = model.ManagerMembershipID,
                MediaID = model.MediaID,
                SponsoredBy = model.SponsoredBy
            };
            Team.TeamID = await _dbContext.SaveAsync(Team);

            await _dbContext.ExecuteAsync($"UPDATE AISCMembership SET TeamID={Team.TeamID} Where ID={model.ManagerMembershipID}");
            return Ok(new Success());
        }

        [HttpGet("get-aisc-team/{TeamID}")]
        public async Task<IActionResult> GetTeams(int TeamID)
        {
            var res = await _dbContext.GetByQueryAsync<AISCTeamModel>($@"Select AT.*,FirstName 
                                                                            From AISCTeam AT
                                                                            Join AISCMembership AM on AM.ID=AT.ManagerMembershipID and AM.IsDeleted=0
                                                                            Where AT.TeamID={TeamID} and AT.IsDeleted=0", null);

            return Ok(res ?? new());
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("delete-aisc-team")]
        public async Task<IActionResult> DeleteTeam(int TeamID)
        {
            await _dbContext.DeleteAsync<AISCTeam>(TeamID);
            return Ok(new Success());
        }

        #endregion

        #region Application Excell download

        [HttpGet("get-membership-application-excel-file")]
        public async Task<IActionResult> GetCustomerExcelFile()
        {
            StringModel stringModel = new();
            stringModel.Value = await _spin.GetASICMembershipExcelData();
            return Ok(stringModel);
        }


        #endregion

        #endregion

        #region Excel Import

        [HttpPost("upload-members")]
        public async Task<IActionResult> ImportExcelFile(FileUploadModel model)
        {
            List<AISCMembershipModel> members = new();
            string folderName = "gallery/import/";
            if (!Directory.Exists(Path.Combine("wwwroot", folderName)))
            {
                Directory.CreateDirectory(Path.Combine("wwwroot", folderName));
            }
            string filePath = Path.Combine(_env.ContentRootPath, "wwwroot", folderName);
            filePath = filePath + "/" + "import" + "." + model.Extension;
            string sFileExtension = Path.GetExtension(filePath).ToLower();
            ISheet sheet;
            var stream = System.IO.File.Create(filePath);
            stream.Write(model.Content, 0, model.Content.Length);

            try
            {
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
                    var Number = await _dbContext.GetByQueryAsync<int>($@"Select ISNULL(MAX(Number),1) as Number
                                                                    From AISCMembership
                                                                    Where IsDeleted=0", null);
                    for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++) //Read Excel File
                    {
                        IRow row = sheet.GetRow(i);
                        if (row == null) continue;
                        if (row.Cells.All(d => d.CellType == CellType.Blank)) continue;
                        string? filename = row.GetCell(0) != null ? row.GetCell(0).ToString() : "";
                        string? firstname = row.GetCell(1) != null ? row.GetCell(1).ToString() : "";
                        string? lastname = row.GetCell(2) != null ? row.GetCell(2).ToString() : "";
                        string? gender = row.GetCell(3) != null ? row.GetCell(3).ToString() : "";
                        string? phoneisdcode = row.GetCell(4) != null ? row.GetCell(4).ToString() : "";
                        string? phoneno = row.GetCell(5) != null ? row.GetCell(5).ToString() : "";
                        string? whatsappisdcode = row.GetCell(6) != null ? row.GetCell(6).ToString() : "";
                        string? whatsappno = row.GetCell(7) != null ? row.GetCell(7).ToString() : "";
                        string? email = row.GetCell(8) != null ? row.GetCell(8).ToString() : "";
                        string? nationality = row.GetCell(9) != null ? row.GetCell(9).ToString() : "";
                        string? region = row.GetCell(10) != null ? row.GetCell(10).ToString() : "";
                        string? visastatus = row.GetCell(11) != null ? row.GetCell(11).ToString() : "";
                        string? age = row.GetCell(12) != null ? row.GetCell(12).ToString() : "";
                        string? levelofname = row.GetCell(13) != null ? row.GetCell(13).ToString() : "";
                        string? message = row.GetCell(14) != null ? row.GetCell(14).ToString() : "";
                        string? bloodgroup = row.GetCell(15) != null ? row.GetCell(15).ToString() : "";

                        if (phoneno != null || email != null)
                        {
                            var existingphoneoremail = await _dbContext.GetAsync<AISCMembership>($"(PhoneNo=@PhoneNo OR EmailAddress=@EmailAddress OR WhatsappNo=@WhatsappNo)", new { PhoneNo = phoneno , EmailAddress = email , WhatsappNo = whatsappno });
                            if (existingphoneoremail != null)
                            {
                                if (existingphoneoremail.PhoneNo != null)
                                {
                                    return BadRequest(new Error() { ResponseMessage = $"Member with same Phone number already exist {i} th row.<br> Please check and upload again" });
                                }
                                if (existingphoneoremail.EmailAddress != null)
                                {
                                    return BadRequest(new Error() { ResponseMessage = $"Member with same Email address already exist {i} th row.<br> Please check and upload again" });
                                }
                                if (existingphoneoremail.WhatsappNo != null)
                                {
                                    return BadRequest(new Error() { ResponseMessage = $"Member with same Email address already exist {i} th row.<br> Please check and upload again" });
                                }
                            }
                        }

                        int? MediaID = null;
                        if (filename != "")
                        {
                            string folderNames = "gallery/Profile/";
                            //if (!Directory.Exists(Path.Combine("wwwroot", folderNames)))
                            //{
                            //    Directory.CreateDirectory(Path.Combine("wwwroot", folderNames));
                            //}
                            //string filePaths = Path.Combine(_env.ContentRootPath, "wwwroot", folderNames);
                            //filePaths = filePaths + "/" + filename;
                            //string FileExtension = Path.GetExtension(filePaths).ToLower();
                            //var streams = System.IO.File.Create(filePath);

                            string FileExtension = Path.GetExtension(filename).ToLower();
                            var media = new Media
                            {
                                FileName = folderNames + filename,
                                Extension = FileExtension,
                                ContentType = "image",
                            };
                            MediaID = media.MediaID = await _dbContext.SaveAsync(media);
                        }


                        var memberships = new AISCMembershipModel()
                        {
                            FileName=filename,
                            FirstName = firstname,
                            LastName = lastname,
                            Gender = gender,
                            PhoneNo = phoneno,
                            PhoneISDCode = phoneisdcode,
                            WhatsappISDCode = whatsappisdcode,
                            WhatsappNo = whatsappno,
                            EmailAddress = email,
                            Nationality = nationality,
                            Region = region,
                            VisaStatus = visastatus,
                            Age = age,
                            LevelOfGame = levelofname,
                            Message = message,
                            BloodGroup = bloodgroup,
                            Prefix="AISC",
                            Number= ++Number,
                            MediaID= MediaID,
                        };
                        members.Add(memberships);
                    }
                }
                foreach (var item in members)
                {
                    await SaveMembership(item);
                }
                stream.Close();
            }
            catch (Exception ex)
            {

            }


            return Ok(new Success());
        }



        #endregion

    }
}
