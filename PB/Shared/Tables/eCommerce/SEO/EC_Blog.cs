using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.eCommerce.SEO
{
    public class EC_Blog:Table
    {
        [PrimaryKey]
        public int BlogID { get; set; }
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
    }
}
