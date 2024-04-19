using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.eCommerce.SEO
{
    public class EC_Testimonial:Table
    {
        [PrimaryKey]
        public int ID { get; set; }
        public int? MediaID { get; set; }
        public string? Name { get; set; }
        public string? Designation { get; set; }
        public string? Comment { get; set; }
        public int? ItemID { get; set; }
        public int? ItemVariantID { get; set; }
    }
}
