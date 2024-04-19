using PB.Shared.Tables;
    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.SuperAdmin.Client
{
    public class SalesJournalMasterBasicDataModel 
    {
        public int ProgbizBranchID { get; set; }
        public int ClientLedgerID { get; set; }
        public int SalesLedgerID { get; set; }
        public int DiscountLedgerID { get; set; } 
        public int SalesVoucherTypeID { get; set; } 
        public VoucherNumberModel VoucherNumber { get; set; } = new();

    }
}
