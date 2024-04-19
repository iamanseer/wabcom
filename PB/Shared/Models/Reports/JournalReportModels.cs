using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Reports
{
    public class JournalReportDataModel 
    {
        public int JournalMasterID { get; set; }
        public DateTime Date { get; set; }
        public string? JournalNoPrefix { get; set; }
        public int JournalNo { get; set; } 
        public string? Particular { get; set; }
        public string? LedgerName { get; set; } 
        public decimal Debit { get; set; }
		public decimal Credit { get; set; }
	}

    public class JournalReportModel
    {
        public string? ClientName { get; set; }
        public List<JournalReportGroupModel> Entries { get; set; } = new();
    }
    public class JournalReportGroupModel
    {
        public int JournalMasterID { get; set; }
		public DateTime Date { get; set; }
        public string? Particular { get; set; } 
        public string? JournalNoWithPrefix { get; set; }
        public List<JournalReportGroupItemModel> Items { get; set; } = new(); 
    }

    public class JournalReportGroupItemModel
	{
		public string? Account { get; set; }
		public decimal Debit { get; set; }
		public decimal Credit { get; set; }
	}


}
