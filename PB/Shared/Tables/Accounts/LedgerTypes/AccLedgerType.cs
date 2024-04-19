using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Accounts.LedgerTypes
{
    public class AccLedgerType : Table
    {
        [PrimaryKey]
        public int LedgerTypeID { get; set; }
        public string? LedgerTypeName { get; set; }
    }
}
