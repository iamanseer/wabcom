using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models
{
    public class SearchingPostModel : PagedListPostModel
    {
        public DateTime? Date { get; set; }
        public int MailSettingsID { get; set; }
        public int TemplateID { get; set; }
        public int MessageSendTypeId { get; set; }
    }
}
