using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables
{
    [TableName("Users")]
    public class UserCustom:User
    {
        public DateTime? OTPGeneratedAt { get; set; }
    }
}
