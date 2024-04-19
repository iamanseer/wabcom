using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.eCommerce.SEO
{
    public class EC_NewsLetter:Table
    {
        [PrimaryKey]
        public int ID { get; set; } 
        public string Subject { get; set; }
        public string BodyContent { get; set; }


    }
}
