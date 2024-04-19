using PB.Model;
using PB.Shared.Enum.Accounts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Accounts.JournalMaster
{
    public class AccJournalMaster : Table
    {
        [PrimaryKey]
        public int JournalMasterID { get; set; }
        [Required(ErrorMessage = "Please choose a date for the voucher entry")]
        public DateTime? Date { get; set; } = DateTime.UtcNow;
        //[RequiredIf("IsAdmin", true, ErrorMessage = "Please choose a branch for the voucher entry")]
        public int? BranchID { get; set; }
        [Required(ErrorMessage = "Please choose a voucher type for the voucher entry")]
        public int? VoucherTypeID { get; set; }
        public string? JournalNoPrefix { get; set; }
        public int JournalNo { get; set; }
        public int? EntityID { get; set; }
        public string? Name { get; set; }
        public int NarrationType { get; set; } = (int)VoucherNarrationType.Single;
        public string? Particular { get; set; }
        public bool IsSuccess { get; set; }
        public bool IsVerified { get; set; }
        public int? VerifiedBy { get; set; }
        public DateTime? VerifiedOn { get; set; }
        public bool IsOnlinePayment { get; set; }
        public string? PaymentMode { get; set; }
        public string? Remarks { get; set; }
        public string? PaymentGatewayID { get; set; }
        public string? ReferenceNo { get; set; }
    }
}
