using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.eCommerce.SEO
{
    public class EC_NewsLetterSubscription:Table
    {
        [PrimaryKey]
        public int ID { get; set; }
        public string? Email { get; set; }
        public DateTime? SubscribedON { get; set; }
    }
}
