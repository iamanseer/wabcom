using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.eCommerce.SEO
{
    public class TestimonialModel
    {
        public int ID { get; set; }
        public int? MediaID { get; set; }
        public string? Name { get; set; }
        public string? Designation { get; set; }
        public string? Comment { get; set; }
        public string? FileName { get; set; }
        public string? URL { get; set; }
    }
}
