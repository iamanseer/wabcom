using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Reports
{
    public class ProfitAndLossReportModel
    {
        public string? ClientName { get; set; }
        public string? CurrencySymbol { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal GrossLoss { get; set; }
        public decimal OperatingProfit { get; set; }
        public decimal OperatingLoss { get; set; }
        public decimal NetProfit { get; set; }
        public decimal NetLoss { get; set; }
        public ProfitAndLossReportCategory DirectIncome { get; set; } = new();
        public ProfitAndLossReportCategory DirectExpense { get; set; } = new(); 
        public ProfitAndLossReportCategory IndirectIncome { get; set; } = new();
        public ProfitAndLossReportCategory IndirectExpense { get; set; } = new();
        public ProfitAndLossReportCategory Asset { get; set; } = new();
        public ProfitAndLossReportCategory Liability { get; set; } = new(); 
    }

    public class ProfitAndLossReportCategory
    {
        public int Nature { get; set; } 
        public decimal Amount { get; set; }
    }

    public class ProfitAndLossReportDataModel
    {
        public int Nature { get; set; }
        public decimal Amount { get; set; } 
    }
}
