using PB.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Common
{
    public class PdfGeneratedResponseModel
    {
        public MailDetailsModel MailDetails { get; set; } = new();
        public int MediaID { get; set; }
    }
}
