using PB.Shared.Tables.Accounts.JournalMaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.SuperAdmin.Client
{
    public class ClientPackageAccountsModel
    {
        public AccJournalMaster JournalMaster { get; set; } = new();
        public List<AccJournalEntry> JournalEntries { get; set; } = new();

    }
}
