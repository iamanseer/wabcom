using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.eCommerce.SEO
{
    public class EC_Tag:Table
    {
        [PrimaryKey]
        public int TagID { get; set; }
        public string? TagName { get; set; }
    }
}
