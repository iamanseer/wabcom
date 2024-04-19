using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.SuperAdmin.Client
{
    public class PaymentVerificationPostModel
    {
        [Required(ErrorMessage = "Pease provide an Invoice ID")]
        public int InvoiceID { get; set; }
        [Required(ErrorMessage = "Pease choose reciept voucher from the list")]
        public int RecieptVoucherTypeID { get; set; } 
    }
    public class PaymentRecieptJournalMasterBasicDataModel
    {
        public int ProgbizBranchID { get; set; }
        public int ClientLedgerID { get; set; }
        public int BankLedgerID { get; set; }
        public VoucherNumberModel Vouchernumber { get; set; } = new();
    }
}
