using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Reports
{
    public class SalesByStaffReportModel
    {
        public string? ClientName { get; set; }
        public List<SalesByStaffReportItemModel> Items { get; set; } = new(); 
    } 

    public class SalesByStaffReportItemModel
    {
        public int UserEntityID { get; set; } 
        public string? StaffName { get; set; }
        public int InvoiceCount { get; set; }
        public decimal SalesAmount { get; set; }
        public decimal SalesWithTaxAmount { get; set; } 

    }
}
