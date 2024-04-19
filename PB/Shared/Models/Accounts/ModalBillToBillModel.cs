using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Accounts
{
    public class ModalBillToBillModel
    {
        public decimal TotalAmount { get; set; }
        public decimal RemainingAmount { get; set; } 
        public int LedgerID { get; set; }
        public int VoucherTypeID { get; set; }
        public int VoucherTypeNatureID { get; set; }
        public string? VoucherNumer { get; set; }
        public bool IsDebit { get; set; } 
        public string? DrOrCr { get; set; } 
        public List<BillToBillModel> BillToBillItems { get; set; } = new(); 
    }
}
