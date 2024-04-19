using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables
{
    public class ClientInvoice:Table
    {
        [PrimaryKey] 
        public int InvoiceID  { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public int? ClientID { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? DisconnectionDate { get; set; }
        public int? InvoiceJournalMasterID { get; set; }
        public int? ReceiptJournalMasterID { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? PackageID { get; set; }
        public decimal? Fee { get; set; }
        public decimal? Discount { get; set; }
        public decimal? NetFee { get; set; }
        public decimal? TaxPercentage { get; set; }
        public int? TaxCategoryID { get; set; }
        public decimal? Tax { get;set; }
        public decimal? GrossFee { get; set; }
        public int? PaidStatus { get; set; }
        public string? PaymentRefNo { get; set; }
        public int? MediaID { get; set; }
        public int? InvoiceMediaID { get; set; }
        public int? InvoiceNo { get; set; }



    }
}
