using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Reports
{
    public class SalesByCustomerReportModel
    {
        public string? ClientName { get; set; }
        public List<SalesByCustomerItemModel> Items { get; set; } = new();
    }

    public class SalesByCustomerItemModel
    {
        public int CustomerEntityID { get; set; }
        public string? CustomerName { get; set; }
        public int InvoiceCount { get; set; } 
        public decimal SalesAmount { get; set; }
        public decimal SalesWithTaxAmount { get; set; } 
    }
}
