using PB.CRM.Model.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Accounts.VoucherEntry
{
    public class VoucherListModel
    {
        public int JournalMasterID { get; set; }
        public DateTime? Date { get; set; }
        public string? PrefixNJournalNumber { get; set; }
        public string? EntityName { get; set; }
        public bool IsSuccess { get; set; }  
        public bool IsVerified { get; set; }
        public DateTime? VerifiedOn { get; set; }
        public string? VerifiedByName { get; set; }
        public string? SuccessStatusBadgeClass
        {
            get
            {
                if(IsSuccess)
                {
                    return "badge bg-success";
                }
                else
                {
                    return "badge bg-danger";
                }
            }
        }
        public string? VerficationStatusBadgeClass
        {
            get
            {
                if (IsVerified)
                {
                    return "badge bg-success";
                }
                else
                {
                    return "badge bg-danger";
                }
            }
        }
    }
}
