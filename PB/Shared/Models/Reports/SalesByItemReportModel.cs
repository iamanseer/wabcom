using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Reports
{
    public class SalesByItemReportModel
    {
        public string? ClientName { get; set; }
        public List<SalesByItemReportItemModel> Items { get; set; } = new();
    }
    public class SalesByItemReportItemModel
    {
        public int ID { get; set; }
        public string? ItemName { get; set; }
        public decimal QuantitySold { get; set; }
        public decimal Amount { get; set; }
        public decimal AverageAmount { get; set; }
    }
}
