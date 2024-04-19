using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables
{
    [TableName("MailSettings")]
    public class MailSettingsCustom : MailSettings
    {
        public int? ClientID { get; set; }
    }
}
