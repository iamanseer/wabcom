using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.eCommerce.SEO
{
    public class BlogModel
    {
        public int BlogID { get; set; }
        [Required]
        public string? Name { get; set; }
        public DateTime? Date { get; set; }
        public int? MediaID { get; set; }
        public string? BodyContent { get; set; }
        public string? AuthorName { get; set; }
        public string? AuthorDescription { get; set; }
        public int? CategoryID { get; set; }
        public string? ShortDescription { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? SlugURL { get; set; }
        public string? MetaTittle { get; set; }
        public string? MetaDescription { get; set; }
        public string? FacebookLink { get; set; }
        public string? InstaLink { get; set; }
        public string? LinkedInLink { get; set; }
        public int? BlogTagID { get; set; }
        public String? FileName { get; set; }
        public int? TagID { get; set; }
        public List<BlogTgModel> BlogTags { get; set; } = new();
    }
}
