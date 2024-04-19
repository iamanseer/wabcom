using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables
{
    public   class ClientInvoiceItem:Table
    {
        [PrimaryKey] 
        public int InvoiceItemID { get; set; } 
        public int? InvoiceID { get; set; }
        public int? PackageID { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal Fee { get; set; }
        public int Discount { get; set; }
        public int NetFee { get; set; }
        public int? TaxCategoryID { get; set; }
        public int TaxPercentage { get; set; }
        public int Tax { get; set; }
        public int GrossFee { get; set; }

    }
}
