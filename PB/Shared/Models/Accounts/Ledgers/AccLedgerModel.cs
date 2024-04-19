using PB.Model;
using PB.Shared.Enum;
using PB.Shared.Enum.Accounts;
using PB.Shared.Helpers;
using PB.Shared.Tables.Accounts.Ledgers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Accounts.Ledgers
{
    public class AccLedgerModel
    {
        public int LedgerID { get; set; }

        [Required(ErrorMessage = "Please select an account group")]
        public int? AccountGroupID { get; set; }
        public int GroupTypeID { get; set; }

        [Required(ErrorMessage = "Please provide an account code")]
        [RegularExpression("^[A-Z0-9-]+$", ErrorMessage = "The ledger code must consist of uppercase letters, digits, and hyphens only.")]
        public string? LedgerCode { get; set; }

        [Required(ErrorMessage = "Please provide an account name")]
        public string? LedgerName { get; set; }
        public string? LedgerName2 { get; set; }
        public string? Alias { get; set; }
        public bool IsBillToBill { get; set; }
        public int? EntityID { get; set; }
        public string? AccountGroupName { get; set; }
        public string? AgentLedgerName { get; set; }
        public string? AccountTypeName { get; set; }
        public int EntityPersonalInfoID { get; set; }
        [Helpers.RequiredIfMultiple(nameof(GroupTypeID), new int[] { (int)AccountGroupTypes.SundryCreditors, (int)AccountGroupTypes.SundryCreditors }, ErrorMessage = "Please provide mobile number for the account")]
        [RegularExpression("^[0-9]+$", ErrorMessage = "Only numbers are allowed for mobile number")]
        public string? Phone { get; set; }
        [RegularExpression("^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$", ErrorMessage = "Please provide a valid Email like example@test.com")]
        public string? EmailAddress { get; set; }
        public int AddressID { get; set; }
        [Helpers.RequiredIfMultiple(nameof(GroupTypeID), new int[] { (int)AccountGroupTypes.SundryCreditors, (int)AccountGroupTypes.SundryCreditors }, ErrorMessage = "Please provide address line 1 for the account")]
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? AddressLine3 { get; set; }
        public int? CityID { get; set; }
        public string? CityName { get; set; }

        [Helpers.RequiredIfMultiple(nameof(GroupTypeID), new int[] { (int)AccountGroupTypes.SundryCreditors, (int)AccountGroupTypes.SundryCreditors }, ErrorMessage = "Please choose a state for the account")]
        public int? StateID { get; set; }
        public string? StateName { get; set; }

        [Helpers.RequiredIfMultiple(nameof(GroupTypeID), new int[] { (int)AccountGroupTypes.SundryCreditors, (int)AccountGroupTypes.SundryCreditors }, ErrorMessage = "Please choose a country for the account")]
        public int? CountryID { get; set; }
        public string? CountryName { get; set; }
        public List<BillToBillModel> BillToBillItems { get; set; } = new();
        public decimal OpeningBalance { get; set; } = 0;
        public int JournalEntryID { get; set; } 
        public int? DrorCr { get; set; }
        public int? ClientID { get; set; }
        public int? BranchID { get; set; } 
    }
}
