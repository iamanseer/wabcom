using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Model.Models
{
    public class MailDetailsModel
    {
        public string? Subject { get; set; }
        public string? Message { get; set; }
        public string? MailRecipients { get; set; }
        public string? FileName { get; set; }
        public int ID { get; set; }
        public string? CompanyName { get; set; } 
        public int ClientID { get; set; }
        public int BranchID { get; set; }
    }


    public class CustomerMailUpdateModel
    {
        public int? CustomerEntityID { get; set; }
        public string? EmailAddress { get; set; }
    }
}
