using PB.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Accounts.Ledgers
{
    public class AccLedger : Table
    {
        [PrimaryKey]
        public int LedgerID { get; set; }

        [Required(ErrorMessage = "Please select an account group")]
        public int? AccountGroupID { get; set; }

        [Required(ErrorMessage = "Please provide an account code")]
        [RegularExpression("^[A-Z0-9-]+$", ErrorMessage = "The ledger code must consist of uppercase letters, digits, and hyphens only.")]
        public string? LedgerCode { get; set; }

        [Required(ErrorMessage = "Please provide an account name")]
        public string? LedgerName { get; set; }
        public string? LedgerName2 { get; set; }
        public string? Alias { get; set; }
        public bool IsBillToBill { get; set; }
        public int? EntityID { get; set; }
        public int? ClientID { get; set; }
        public int? BranchID { get; set; }
    }
}
