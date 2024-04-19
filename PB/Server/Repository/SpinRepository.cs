using PB.DatabaseFramework;
using PB.Shared.Helpers;
using System.Data;
using PB.Shared.Tables;
using PB.Shared.Enum;
using PB.Shared.Models.SuperAdmin.Client;
using PB.Model;
using Org.BouncyCastle.Asn1.Cmp;
using PB.Shared.Models.SuperAdmin.Custom;
using PB.Client.Pages.SuperAdmin;
using PB.Shared.Tables.Accounts.JournalMaster;
using PB.Shared.Tables.Accounts.Ledgers;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using PB.Shared.Models.Spin;
using System.Net;
using PB.Model.Models;
using Microsoft.AspNetCore.Mvc;
using PB.Shared.Tables.CRM;
using PB.Client.Pages.CRM;
using System.Text;
using static System.Net.WebRequestMethods;
using System.IO;
using PB.EntityFramework;

namespace PB.Server.Repository
{
    public interface ISpinRepository
    {
        Task<string?> GetASICMembershipExcelData();
        Task<string> GetMembershipcardHtmlContent(int MembershipID, IDbTransaction? tran = null);
    }
    public class SpinRepository : ISpinRepository
    {
        private readonly IDbContext _dbContext;
        private readonly IEmailSender _email;
        private readonly INotificationRepository _notification;
        private readonly IHostEnvironment _env;
        private readonly IPDFRepository _newPDf;
        private readonly ICommonRepository _newCommon;
        private readonly IConfiguration _config;
        public SpinRepository(IDbContext dbCotext, IEmailSender emailSender, INotificationRepository notification, IHostEnvironment env, IPDFRepository newPDF, ICommonRepository newCommon, IConfiguration config)
        {
            this._dbContext = dbCotext;
            this._email = emailSender;
            this._notification = notification;
            this._env = env;
            this._newPDf = newPDF;
            this._newCommon = newCommon;
            this._config = config;
        }


        public async Task<string?> GetASICMembershipExcelData()
        {
            
            var result = await _dbContext.GetListByQueryAsync<AISCMembershipExcelDataModel>($@"
                                        Select AM.* ,M.FileName
                                        From AISCMembership AM
                                        Left Join Media M on M.MediaID=AM.MediaID and M.IsDeleted=0
                                        Where AM.IsDeleted=0
            ", null);
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

            row.CreateCell(0).SetCellValue("Image");
            row.CreateCell(1).SetCellValue("First Name");
            row.CreateCell(2).SetCellValue("Last Name");
            row.CreateCell(3).SetCellValue("Gender");
            row.CreateCell(4).SetCellValue("Phone Number");
            row.CreateCell(5).SetCellValue("Whatsapp Number");
            row.CreateCell(6).SetCellValue("EmailAddress");
            row.CreateCell(7).SetCellValue("Nationality");
            row.CreateCell(8).SetCellValue("Region");
            row.CreateCell(9).SetCellValue("Visa Status");
            row.CreateCell(10).SetCellValue("Age");
            row.CreateCell(11).SetCellValue("Level of Name");
            row.CreateCell(12).SetCellValue("Category");
            row.CreateCell(13).SetCellValue("Message");
            row.CreateCell(14).SetCellValue("Blood Group");

            int rowindex = 0;
            for (int i = 0; i < row.LastCellNum; i++)
            {
                row.GetCell(i).CellStyle = headerStyle;
            }
            byte[]? fileContents = null;
            string filePath = null;
            string newpath = null;
            string pathValue = null;
            try
            {
                foreach (var item in result)
                {
                    row = excelSheet.CreateRow(++rowindex);

                    byte[] imageBytes;
                    using (var webClient = new WebClient())
                    {
                        string imageUrl = DomainUrl + "/" + item.FileName; // Replace with the correct URL
                        imageBytes = webClient.DownloadData(imageUrl);
                    }

                    IClientAnchor anchor = excelSheet.CreateDrawingPatriarch().CreateAnchor(0, 0, 0, 0, 0, rowindex, 1, rowindex + 1);

                    // Add the image to the workbook
                    int pictureIndex = workbook.AddPicture(imageBytes, PictureType.PNG);

                    // Create a picture and set its size
                    IPicture picture = excelSheet.CreateDrawingPatriarch().CreatePicture(anchor, pictureIndex);
                    // Scale the picture (adjust the scale values as needed)
                    double scaleX = 1; // 50% width
                    double scaleY = 1; // 50% height
                    row.CreateCell(0).SetCellValue("");
                    row.CreateCell(1).SetCellValue(item.FirstName);
                    row.CreateCell(2).SetCellValue(item.LastName);
                    row.CreateCell(3).SetCellValue(item.Gender);
                    row.CreateCell(4).SetCellValue(item.PhoneNo);
                    row.CreateCell(5).SetCellValue(item.WhatsappNo);
                    row.CreateCell(6).SetCellValue(item.EmailAddress);
                    row.CreateCell(7).SetCellValue(item.Nationality);
                    row.CreateCell(8).SetCellValue(item.Region);
                    row.CreateCell(9).SetCellValue(item.VisaStatus);
                    row.CreateCell(10).SetCellValue(item.Age);
                    row.CreateCell(11).SetCellValue(item.LevelOfGame);
                    row.CreateCell(12).SetCellValue(item.Category);
                    row.CreateCell(13).SetCellValue(item.Message);
                    row.CreateCell(14).SetCellValue(item.BloodGroup);
                }

                using (var memoryStream = new MemoryStream())
                {
                    workbook.Write(memoryStream);
                    fileContents = memoryStream.ToArray();
                }


                //string folderName = "gallery/aisc_excel";
                //if (!Directory.Exists(Path.Combine("wwwroot", folderName)))
                //{
                //    Directory.CreateDirectory(Path.Combine("wwwroot", folderName));
                //}
                //filePath = Path.Combine(_env.ContentRootPath, "wwwroot", folderName);
                //filePath = filePath + "/" + "AISC_Membership_List.xlsx";
                //newpath = filePath.Replace("\\", "/");
                //if (System.IO.File.Exists(newpath))
                //{
                //    System.IO.File.Delete(newpath);

                //}

                //System.IO.File.WriteAllBytes(newpath, fileContents);
                //pathValue = folderName + "/" + "AISC_Membership_List.xlsx";


                string folderName = "gallery/aisc_excel";
                string wwwrootPath = Path.Combine(_env.ContentRootPath, "wwwroot");
                filePath = Path.Combine(wwwrootPath, folderName, "AISC_Membership_List.xlsx");
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
                    pathValue = folderName + "/" + "AISC_Membership_List.xlsx";
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


        #region AISC Card

        public async Task<string> GetMembershipcardHtmlContent(int MembershipID, IDbTransaction? tran = null)
        {
            try
            {
                var MembershipCard = await GetMembershipCardPdfDetailsModel(MembershipID, tran);
                string? DomainUrl = _config.GetValue<string>("ServerURL");
                string? ProfileImage = "";
                if(MembershipCard.FileName!=null)
                {
                    ProfileImage = DomainUrl+"/"+MembershipCard.FileName;
                }
                else
                {
                    ProfileImage = DomainUrl+"/assets/profile.png";
                }

                var mc = new StringBuilder();
                mc.Append($@"<!DOCTYPE html>
                                <html lang=""en"">
                                <head>
                                <meta charset=""UTF-8"">
                                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                                <title>Document</title>
                 ");
                mc.Append($@"    <style>
                                *,
                                *:before,
                                *:after {{box - sizing: border-box;
                                }}
                                html{{overflow: hidden;
                                }}
                                body {{width: 100vw;
                                    height: 100vh;
                                    font-family:   'Poppins', sans-serif !important;
                                    font-size: 14px;
                                    line-height: 1.3;
                                    background-color: #fff;
                                }}
                                .inspiration {{position: fixed;
                                    bottom: 0;
                                    right: 0;
                                    padding: 10px;
                                    text-align: center;
                                    text-decoration: none;
                                    font-family: 'Gill Sans', sans-serif;
                                    font-size: 12px;
                                    color: #fff;
                                }}
                                .left {{position: absolute;
                                    left: 0;
                                    width: 60vw;
                                    height: 100vh;
                                    background-image: linear-gradient(to right, #202020, #808080);
                                }}
                                .right {{position: absolute;
                                    right: 0;
                                    width: 60vw;
                                    height: 100vh;
                                    overflow: hidden;
                                }}
                                .right .strip-top {{width: calc(50vw + 90px);
                                    transform: skewX(20deg) translateX(160px);
                                }}
                                .right .strip-bottom {{width: calc(50vw + 40px);
                                    transform: skewX(-15deg) translateX(90px);
                                }}
                                .center {{position: absolute;
                                    top: 50%;
                                    left: 50%;
                                    transform: translate(-50%, -50%);
                                }}
                                .card {{width: 400px;
                                    height: 250px;
                                }}

                                .front,
                                .back {{position: absolute;
                                    width: inherit;
                                    height: inherit;
                                    border-radius: 15px;
                                    color: #fff;
                                    text-shadow: 0 1px 1px rgba(0,0,0,0.3);
                                    box-shadow: 0 1px 10px 1px rgba(0,0,0,0.3);
                                    -webkit-backface-visibility: hidden;
                                            backface-visibility: hidden;
                                    background-image: linear-gradient(to right, #111, #555);
                                    overflow: hidden;
                                }}
                                .front {{transform: translateZ(0);
                                }}
                                .strip-bottom,
                                .strip-top {{position: absolute;
                                    right: 0;
                                    height: inherit;
                                    background-image: linear-gradient(to bottom, #8BC34A, #617f35);
                                    box-shadow: 0 0 10px 0px rgba(0,0,0,0.5);
                                }}
                                .strip-bottom {{width: 200px;
                                    transform: skewX(-15deg) translateX(50px);
                                }}
                                .strip-top {{width: 180px;
                                    transform: skewX(20deg) translateX(50px);
                                }}
                                .logo {{position: absolute;
                                    top: 30px;
                                    right: 25px;
                                }}

                                .card-name {{position: relative;
                                    display: flex;
                                    justify-content: space-between;
                                    align-items: center;
                                    margin: 9px 25px 10px;
                                    font-size: 23px;
                                    font-family:   'Poppins', sans-serif;;
                                }}
                                .blood {{margin-left: 25px;
 
                                    font-family:   'Poppins', sans-serif;;
                                }}
                                .blood .blood-group {{font - size: 12px;
                                    line-height: 24px;

                                }}
                                .ID {{margin-left: 25px;
 
                                    font-family:   'Poppins', sans-serif;;
                                }}
                                .ID .ID-group {{font - size: 12px;
                                }}
                                .card-holder {{margin: 10px 25px;
 
                                    font-family:'Poppins', sans-serif;
                                }}
                                .card-download{{
                                    font-size: 18px;
                                    color: #fff;
                                    font-weight: 700;
                                    margin-top: 25px;
                                    text-align: center;
                                }}

                                </style>
                                </head>
                ");

                mc.Append($@"<body style='background-image: linear-gradient(to right, #111, #555);'>
                              <div class='center'>
                                <div class='card' id='divToCapture'>
                                    <div class='front'>
                                      <div class='strip-bottom'></div>
                                      <div class='strip-top'></div>
              
                                        <img class='logo' src='{DomainUrl}/assets/images/aisc-card/logo-w.png' style='width:16%'>
          
                                      <img src='{ProfileImage}' style='width:100px; height:100px; object-fit:contain; margin:30px 0 0 26px; border-radius:5px'>
                                      <div class='card-name'> 
                                        <div>{MembershipCard.Name}</div>
                                      </div>
                                      <div class='ID'>
                                    <span class='ID-group'>ID<strong> :</strong></span>
                                    <span class='ID-group' style='letter-spacing: 2px; margin-left:5px'>{MembershipCard.MembershipNo}</span>
                                    </div>
                                      <div class='blood'>
                                        <span class='blood-group'>BLOOD GROUP <strong>:</strong></span>
                                        <span class='blood-group'> {MembershipCard.BloodGroup}</span>
                                        </div>
             
                                      <img src='{DomainUrl}/assets/images/aisc-card/qr.png' style='width: 16%;position: absolute;right: 20px;bottom: 25px;'>
                                    </div>
                                  </div>
                                <div class='card-download' id=""captureButton""><button onclick=""captureAndDownload()"">Download Now</div>
                                <script src='https://html2canvas.hertzen.com/dist/html2canvas.min.js'></script>
                                <script src='_content/Pb.ComponentHelper/progbiz.js'></script>
                        </body>
                        </html>");
                return mc.ToString();
            }
            catch (Exception ex)
            {
                return "error";
            }
        }

        public async Task<AISCCardViewModel?> GetMembershipCardPdfDetailsModel(int memberId, IDbTransaction? tran = null)
        {

            try
            {
                var membershipDetails = await _dbContext.GetByQueryAsync<AISCCardViewModel>($@"Select FirstName +' '+LastName as Name,(Prefix+CAST(Number AS CHAR)) as MembershipNo,FileName,BloodGroup
                                                                                                From AISCMembership AM 
                                                                                                Left Join Media M on M.MediaID=AM.MediaID and M.IsDeleted=0
                                                                                                Where  AM.IsDeleted=0 and ID={memberId}", null, tran);
                return membershipDetails ?? new();
            }
            catch (Exception ex)
            {
                return null;
            }
        }



        #endregion
    }
}
