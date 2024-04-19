using PB.Model;
using PB.Shared.Tables;
using PB.Shared.Tables.Accounts.AccountGroups;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Accounts.AccountGroups
{
    public class AccountGroupModel
    {
        public int AccountGroupID { get; set; }
        [Required(ErrorMessage = "Please proide account group name")]
        public string? AccountGroupName { get; set; }
        [Required(ErrorMessage = "Please provide account group code")]
        public string? AccountGroupCode { get; set; }
        [RequiredIf(nameof(IsSuperParent), false, ErrorMessage = "Please choose an account group parent")]
        public int? ParentID { get; set; }
        [RequiredIf(nameof(IsSuperParent), true, ErrorMessage = "Please choose an account group type")]
        public int? GroupTypeID { get; set; }
        public string? ParentGroupName { get; set; }
        public bool IsSuperParent { get; set; } = false;
        public int? BranchID { get; set; } 
        public int? ClientID { get; set; }
    }
}
