using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables
{
    [TableName("UserTypeRole")]
    public class UserTypeRoleCustom : UserTypeRole
    {
        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanMail { get; set; }
        public bool CanWhatsapp { get; set; }
    }
}
