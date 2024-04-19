using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Accounts.VoucherEntry
{
    public class VoucheVerificationResultModel
    {
        public int VerifiedBy { get; set; } 
        public DateTime? VerifiedOn { get; set; } = DateTime.UtcNow;
        public string? VerifiedByName { get; set; } 
    }
}
