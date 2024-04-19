using PB.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables
{
    public class AccBillToBill : Table
    {
        [PrimaryKey]
        public int BillID { get; set; }
        public DateTime? Date { get; set; } = DateTime.Now;
        public int? ReferenceTypeID { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter Reference number")]
        public string? ReferenceNo { get; set; }
        public int? JournalEntryID { get; set; }
        public int Days { get; set; }
        public int? LedgerID { get; set; }
        public decimal? Debit { get; set; }
        public decimal? Credit { get; set; }
        public int? ParentBillID { get; set; }
    }
}
