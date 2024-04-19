
using PB.Shared.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Accounts.AccountGroups
{
    public class AccountGroupListModel
    {
        public int AccountGroupID { get; set; }
        public string? AccountGroupName { get; set; }
        public string? AccountGroupCode { get; set; }
        public int Nature { get; set; }
        public string? NatureName
        {
            get
            {
                switch (Nature)
                {
                    case (int)AccountGroupNatures.Asset:
                        return "Asset";
                    case (int)AccountGroupNatures.Liability:
                        return "Liability";
                    case (int)AccountGroupNatures.DirectIncome:
                        return "Direct Income";
                    case (int)AccountGroupNatures.DirectExpense:
                        return "Direct Expense";
                    case (int)AccountGroupNatures.IndirectIncome:
                        return "Indirect Income";
                    case (int)AccountGroupNatures.IndirectExpense:
                        return "Indirect Expense";
                    default:
                        return null;
                }
            }
        }
        public string? ParentOrGroupTypeName { get; set; } 
    }
}
