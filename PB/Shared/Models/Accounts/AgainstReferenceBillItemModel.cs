using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Accounts
{
    public class AgainstReferenceBillItemModel
    {
        public int? BillID { get; set; }
        public string? ReferenceNo { get; set; }
        public DateTime? Date { get; set; } 
        public int Days { get; set; }
        public string? Remarks { get; set; }
        public decimal? Amount { get; set; }
        public bool IsDebit { get; set; }
    }
}
