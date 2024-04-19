using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using PB.DatabaseFramework;
using PB.Model;
using PB.Shared.Enum;
using PB.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using PB.Server.Repository;
using PB.Shared.Tables;
using PB.Shared.Models;
using NPOI.SS.Formula.Functions;
using Microsoft.IdentityModel.Tokens;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using PB.EntityFramework;

namespace PB.Server.Controllers
{

    [Route("api/media")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class MediaController : BaseController
    {
        private readonly IDbContext _dbContext;
        private readonly IMediaRepository _media;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public MediaController(IDbContext dbContext, IMediaRepository media, IConfiguration config, IWebHostEnvironment hostingEnvironment)
        {
            _dbContext = dbContext;
            _media = media;
            _config = config;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet("get")]
        public async Task<IActionResult> Get(int id)
        {
            return Ok(await _dbContext.GetByQueryAsync<FileDetailsModel>($@"SELECT FileName,Extension
                FROM  Media
                Where MediaID={id}", null));
        }

        [DisableRequestSizeLimit]
        [AllowAnonymous]
        [HttpPost("form-file-upload")]
        public async Task<IActionResult> FileUpload()
        {
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
                    res.FilePath = await _dbContext.GetFieldsAsync<Media, string>("FileName", $"MediaID={res.MediaID}", null);
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

        [DisableRequestSizeLimit]
        [HttpPost("upload-file")]
        public async Task<IActionResult> UploadFile(FileUploadModel model)
        {
            return Ok(await _media.SaveMedia(model));
        }

        [HttpPost("save")]
        public async Task<IActionResult> Save(MediaCustom model)
        {
            var media = await _dbContext.GetAsync<MediaCustom>(model.MediaID.Value);
            if (media == null)
                media = new();
            media.Title = model.Title;
            await _dbContext.SaveAsync(media);
            await _dbContext.InsertAddEditLogSummary(Convert.ToInt32(model.MediaID), media.Title);
            return Ok(new Success());

        }

        [HttpGet("get-gallery-image/{Id}")]
        public async Task<IActionResult> GetGalleryImage(int Id)
        {
            var result = await _dbContext.GetAsync<MediaCustom>(Id);
            return Ok(result);
        }

        [HttpPost("get-all-images")]
        public async Task<IActionResult> GetAllWhatsAppAccounts(PagedListPostModel model)
        {
            string imagePath = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            PagedListQueryModel searchdata = model;
            searchdata.Select = $"select MediaID,concat('{imagePath}',FileName)URL,Title,FileName from Media";
            searchdata.WhereCondition = "Title is NOT NULL and IsDeleted=0";
            var result = await _dbContext.GetPagedList<GalleryModel>(searchdata, null);
            return Ok(result);
        }

        [HttpGet("delete-image")]
        public async Task<IActionResult> DeleteImage(int Id)
        {
            var result = await _dbContext.GetAsync<Media>(Id);
            await _dbContext.DeleteAsync<Media>(Id);
            await _dbContext.InsertDeleteLogSummary(Id, result.FileName);
            return Ok(new Success());
        }

        [HttpGet("get-file/{MediaID}")]
        public async Task<IActionResult> GetFile(int? MediaID)
        {
            string imagePath = _config["ServerURL"];
            var res = await _dbContext.GetByQueryAsync<GalleryModel>(@$"select MediaID,concat('{imagePath}',FileName)URL,FileName,Title,ContentLength from Media 
                            where MediaID={MediaID} and IsDeleted = 0", null);
            return Ok(res);

        }

        [DisableRequestSizeLimit]
        [AllowAnonymous]
        [HttpPost("single-file-upload")]
        public async Task<IActionResult> SingleFileUpload(FileUploadModel model)
        {
            return Ok(await _media.SaveMedia(model));
        }

        [DisableRequestSizeLimit]
        [AllowAnonymous]
        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage(FileUploadModel model)
        {
            return Ok(await _media.SaveMedia(model));
        }

        [DisableRequestSizeLimit]
        [AllowAnonymous]
        [HttpPost("new-upload-file")]
        public async Task<IActionResult> SaveItemModelImage()
        {
            if (HttpContext.Request.Form.Files.Any())
            {
                var formFile = HttpContext.Request.Form.Files[0];
                string folderName = HttpContext.Request.Form["FolderName"].ToString();
                string contentType = HttpContext.Request.Form["ContentType"].ToString();
                int mediaID = Convert.ToInt32(HttpContext.Request.Form["MediaID"]);
                string extension = formFile.FileName.Substring(formFile.FileName.LastIndexOf('.') + 1);
                // Combine the web root path and the target folder
                string targetFolder = Path.Combine(_hostingEnvironment.WebRootPath, "gallery", folderName);
                // Ensure the target folder exists, or create it if it doesn't
                if (!Directory.Exists(targetFolder))
                {
                    Directory.CreateDirectory(targetFolder);
                }

                //string uniqueFileName = $"{Guid.NewGuid()}_{formFile.FileName}";
                // Combine the target folder and the unique file name
                string fileName = formFile.FileName;
                string filePath = Path.Combine(targetFolder, fileName);
                if (System.IO.File.Exists(filePath))
                {
                    fileName = Guid.NewGuid().ToString() + '.' + extension;
                    filePath = Path.Combine(targetFolder, fileName);
                }
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    // Copy the uploaded file to the target folder
                    formFile.CopyTo(stream);
                }

                Media media = new Media()
                {
                    MediaID = mediaID,
                    IsURL = false,
                    FileName = $"/gallery/{folderName}/" + fileName,
                    ContentType = contentType,
                    ContentLength = formFile.Length,
                    Extension = extension
                };
                media.MediaID = await _dbContext.SaveAsync(media);
                return Ok(new FileUploadResultModel() { MediaID = media.MediaID.Value, FilePath = _config["ServerURL"] + media.FileName });
            }
            else
            {
                return BadRequest(new BaseErrorResponse() { ResponseMessage = "FileNotFound" });
            }
        }
    }
}
