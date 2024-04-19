using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.eCommerce.SEO
{
    public class EC_BlogTag : Table
    {
        [PrimaryKey]
        public int BlogTagID { get; set; }
        public int? BlogID { get; set; }
        public int? TagID { get; set; }
   
    }

}
