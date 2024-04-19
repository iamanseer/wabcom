using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.eCommerce.Users
{
    public class EC_Users:Table
    {
        [PrimaryKey]
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string? PasswordHash { get; set; }
        public int? EntityID { get; set; }
        public int? UserTypeID { get; set; }
        public int? BranchID { get; set; }
        public bool LoginStatus { get; set; }
        public string? OTP { get; set; }
        public DateTime? OTPGeneratedAt { get; set; }
        public bool IsVerified { get; set; }

    }
}
