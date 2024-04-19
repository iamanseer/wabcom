using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Court
{
    public class CourtSalesJournalMasterBasicDataModel
    {
        public int BranchID { get; set; }
        public int ReceiptLedgerID { get; set; }
        public int SalesLedgerID { get; set; }
        public int DiscountLedgerID { get; set; }
        public int SalesVoucherTypeID { get; set; }
        public VoucherNumberModel VoucherNumber { get; set; } = new();
    }
}
