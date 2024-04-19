using PB.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Accounts.AccountGroups 
{
    public class AccAccountGroup : Table
    {
        [PrimaryKey]
        public int AccountGroupID { get; set; }
        [Required(ErrorMessage = "Please proide account group name")]
        public string? AccountGroupName { get; set; }
        [Required(ErrorMessage = "Please provide account group code")]
        public string? AccountGroupCode { get; set; }
        public int? ParentID { get; set; }
        public int? GroupTypeID { get; set; }
        public int? ClientID { get; set; }
        public int? BranchID { get; set; }
        public bool IsSuperParent { get; set; }
    }
}
