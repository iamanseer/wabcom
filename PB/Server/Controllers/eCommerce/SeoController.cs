using Microsoft.AspNetCore.Mvc;
using PB.EntityFramework;
using PB.Model;
using PB.Shared;
using PB.Shared.Models.eCommerce.Customers;
using PB.Shared.Models.eCommerce.SEO;
using PB.Shared.Tables.eCommerce.SEO;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using PB.Shared.Models;
using PB.Server.Repository;
using PB.Shared.Tables.Inventory.Invoices;
using AutoMapper;
using Hangfire.Common;
using Hangfire;

namespace PB.Server.Controllers.eCommerce
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class SeoController : Controller
    {
        private readonly IDbContext _dbContext;
        private readonly IConfiguration _config;
        private readonly ICommonRepository _common;
        private readonly IMapper _mapper;
        private readonly IBackgroundJobClient _job;
       

        public SeoController(IDbContext dbContext, IConfiguration config, ICommonRepository common, IMapper mapper, IBackgroundJobClient job)
        {
            _dbContext = dbContext;
            _config = config;
            _common = common;
            _mapper = mapper;
            _job = job;
        }

        #region testiomial
        [HttpPost("save-testimonial")]
        public async Task<IActionResult> SaveTestinomial(TestimonialModel model)
        {
            EC_Testimonial testinomial = new()
            {
                ID = model.ID,
                MediaID = model.MediaID,
                Name = model.Name,
                Designation = model.Designation,
                Comment = model.Comment
            };
            model.ID = await _dbContext.SaveAsync(testinomial);
            return Ok(new Success());
        }
        [HttpGet("get-testimonial/{testimonialID}")]
        public async Task<IActionResult> GetTesimonial(int testimonialID)
        {
            string imagePath = _config["ServerURL"];
            var testimonial = await _dbContext.GetByQueryAsync<TestimonialModel>($@"Select ID,concat('{imagePath}',FileName)FileName,T.MediaID,Name,Designation,Comment
                                                                                    fROM EC_Testimonial T
                                                                                    LEFT JOIN Media M ON M.MediaID=T.MediaID 
                                                                                    Where M.IsDeleted=0 and ID={testimonialID}", null);
            return Ok(testimonial);
        }
        [HttpPost("get-all-testimonials")]
        public async Task<IActionResult> GetAllTestimonials(PagedListPostModelWithFilter model)
        {
            PagedListQueryModel query = model;
            query.Select = $@"Select ID,FileName,Name,Designation,Comment
                                fROM EC_Testimonial T
                                LEFT JOIN Media M ON M.MediaID=T.MediaID 
                                ";
            query.WhereCondition = "T.IsDeleted=0";
            query.WhereCondition += _common.GeneratePagedListFilterWhereCondition(model, "Name");
            var res = await _dbContext.GetPagedList<TestimonialModel>(query, null);
            return Ok(res ?? new());
        }
        [HttpGet("delete-testimonial")]
        public async Task<IActionResult> DeleteTestimonial(int id)
        {
            await _dbContext.ExecuteAsync($"Update EC_Testimonial set IsDeleted=1 where ID={id}");
            return Ok(new Success());
        }
        #endregion
        #region Blog
        [HttpPost("save-Blog")]
        public async Task<IActionResult> SaveBlog(BlogModel model)
        {
            EC_Blog blog = new()
            {
                BlogID = model.BlogID,
                Name = model.Name,
                Date = model.Date,
                MediaID = model.MediaID,
                BodyContent = model.BodyContent,
                AuthorName = model.AuthorName,
                AuthorDescription = model.AuthorDescription,
                CategoryID = model.CategoryID,
                ShortDescription = model.ShortDescription,
                ModifiedDate = model.ModifiedDate,
                SlugURL = model.SlugURL,
                MetaTittle = model.MetaTittle,
                MetaDescription = model.MetaDescription,
                FacebookLink = model.FacebookLink,
                InstaLink = model.InstaLink,
                LinkedInLink = model.LinkedInLink

            };
            model.BlogID = await _dbContext.SaveAsync(blog);

            if (model.BlogTags.Count > 0)
            {
                var BlogTag = _mapper.Map<List<EC_BlogTag>>(model.BlogTags);
                await _dbContext.SaveSubItemListAsync(BlogTag, "BlogID", model.BlogID);
            }
            return Ok(model);

        }
        [HttpGet("get-blog/{blogID}")]
        public async Task<IActionResult> GetBlog(int blogID)
        {
            string imagePath = _config["ServerURL"];
            var BlogDetails = await _dbContext.GetByQueryAsync<BlogModel>($@"Select BlogID,concat('{imagePath}',FileName)FileName,B.MediaID,Name,Date,BodyContent,
                                                                                AuthorName,AuthorDescription,CategoryID,ShortDescription,ModifiedDate,SlugUrl,Metatittle,
																				MetaDescription,FaceBookLink,InstaLink,LinkedInLink
																				  fROM EC_Blog B
                                                                                    LEFT JOIN Media M ON M.MediaID=B.MediaID 
                                                                                    Where M.IsDeleted=0 and BlogID={blogID}", null);
            BlogDetails.BlogTags = await _dbContext.GetListByQueryAsync<BlogTgModel>($@"select BlogTagID,BlogID,T.TagID 
								                                                from EC_BlogTag B
								                                                Left Join EC_Tag T ON T.TagID=B.TagID and T.IsDeleted=0
								                                                where B.IsDeleted=0 and BlogID={blogID} ", null);
            return Ok(BlogDetails);
        }
        [HttpPost("get-all-blogs")]
        public async Task<IActionResult> GetAllBlogs(PagedListPostModelWithFilter model)
        {
            string imagePath = _config["ServerURL"];
            PagedListQueryModel query = model;
            query.Select = $@"Select BlogID,B.MediaID,Name,
                                    AuthorName,concat('{imagePath}',FileName)FileName
                                    fROM EC_Blog B
                                    LEFT JOIN Media M ON M.MediaID=B.MediaID ";
            query.WhereCondition = " B.IsDeleted=0 ";
            query.WhereCondition += _common.GeneratePagedListFilterWhereCondition(model, "Name");
            var res = await _dbContext.GetPagedList<BlogModel>(query, null);
            return Ok(res ?? new());
        }
        [HttpGet("delete-blog")]
        public async Task<IActionResult> DeleteBlog(int id)
        {
            await _dbContext.ExecuteAsync($"Update EC_Blog set IsDeleted=1 where BlogID={id}");
            return Ok(new Success());
        }
        #endregion
        #region Tag 
        [HttpPost("save-Tag")]
        public async Task<IActionResult> SaveTag(TagModel model)
        {
            EC_Tag tag = new()
            {
                TagID = model.TagID,
                TagName = model.TagName
            };
            model.TagID = await _dbContext.SaveAsync(tag);
            return Ok(new Success());
        }
        [HttpPost("get-all-tags")]
        public async Task<IActionResult> GetAllTag(PagedListPostModelWithFilter model)
        {
            PagedListQueryModel query = model;
            query.Select = $@"Select TagID,TagName
                                fROM EC_Tag ";
            query.WhereCondition = "IsDeleted=0";
            var res = await _dbContext.GetPagedList<TagModel>(query, null);
            return Ok(res ?? new());
        }
        [HttpGet("delete-Tag")]
        public async Task<IActionResult> DeleteTag(int id)
        {
            await _dbContext.ExecuteAsync($"Update EC_Tag set IsDeleted=1 where TagID={id}");
            return Ok(new Success());
        }
        [HttpGet("get-tags")]
        public async Task<IActionResult> GetCountTag()
        {
            var Tags = await _dbContext.GetListByQueryAsync<IdnValuePair>($@"Select TagID as ID,TagName as Value
                                                                    from EC_Tag
								                                    where IsDeleted=0", null);
            return Ok(Tags);
        }


        #endregion
        #region News Letter Subscription
        [AllowAnonymous]
        [HttpPost("save-subscription-news-letter")]
        public async Task<IActionResult> SaveNewsLetterSubscription(NewsLetterSubscriptionModel model)
        {
            var res = await _dbContext.GetByQueryAsync<int?>(@"SELECT ID FROM EC_NewsLetterSubscription 
                            WHERE Email = @Email", new { Email = model.Email });
            if (res!= null)
            {
                throw new PBException("EmailExist");
            }
           
                EC_NewsLetterSubscription subscription = new()
                {
                    Email = model.Email,
                    SubscribedON =DateTime.UtcNow
                };
                subscription.ID = await _dbContext.SaveAsync(subscription);
                return Ok(new Success());
          
            
        }
        [HttpPost("get-all-subscription-news-letter")]
        public async Task<IActionResult> GetAllSubscriptionNewsLetter(PagedListPostModelWithFilter model)
        {
            PagedListQueryModel query = model;
            query.Select = $@"Select ID,Email,SubscribedON
                                fROM EC_NewsLetterSubscription ";
            query.WhereCondition = "IsDeleted=0";
            var res = await _dbContext.GetPagedList<NewsLetterSubscriptionPagedListModel>(query, null);
            return Ok(res ?? new());
        }
       
        [HttpPost("save-news-letter")]
        public async Task<IActionResult> SaveNewsLetter(NewLetterModel model)
        {
         EC_NewsLetter newsletter = new()
            {
                Subject =model.Subject,
                BodyContent =model.BodeyContent
            };
            newsletter.ID= await _dbContext.SaveAsync(newsletter);
            _job.Enqueue(() => _common.SendNewsLetter(model, newsletter.ID));
            return Ok(new Success());
        }
        [HttpPost("get-all-news-letter")]
        public async Task<IActionResult> GetAllNewsLetter(PagedListPostModelWithFilter model)
        {
            PagedListQueryModel query = model;
            query.Select = $@"Select ID,Subject
                                fROM EC_NewsLetter ";
            query.WhereCondition = "IsDeleted=0";
            var res = await _dbContext.GetPagedList<NewLetterModel>(query, null);
            return Ok(res ?? new());
        }
        [HttpGet("delete-newsletter")]
        public async Task<IActionResult> DeleteNewsLeter(int id)
        {
            await _dbContext.ExecuteAsync($"Update EC_NewsLetter set IsDeleted=1 where ID={id}");
            return Ok(new Success());
        }
        #endregion


    }
}
