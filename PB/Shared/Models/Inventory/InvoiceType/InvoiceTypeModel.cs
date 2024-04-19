using PB.Shared.Models.Accounts.VoucherTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Inventory.Invoice
{
    public class InvoiceTypeModel : VoucherTypeModel
    {
        public int InvoiceTypeID { get; set; }
        [Required(ErrorMessage = "Please provide Invoice Type Name")] 
        public string? InvoiceTypeName { get; set; }
        [Required(ErrorMessage ="Please choose a nature from the list")]
        public int InvoiceTypeNatureID { get; set; }
        [Required(ErrorMessage = "Please choose default posting ledger")]
        public int? LedgerID { get; set; }
        public bool IsAdmin { get; set; }
        public string? BranchName { get; set; }
        public string? LedgerName { get; set; }
        public List<InvoiceTypeChargeModel> InvoiceCharge { get; set; } = new();

    }
}
