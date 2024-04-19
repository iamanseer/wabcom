using PB.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables
{
    public class ClientSetting : Table
    {
        [PrimaryKey]
        public int SettingID { get; set; }
        public string? QuotationSubject { get; set; }
        public string? QuotationCustomerNote { get; set; } 
        public string? QuotationTermsAndCondition { get; set; } 
        public bool QuotationNeedShippingAddress { get; set; }
		public string? InvoiceSubject { get; set; }
		public string? InvoiceCustomerNote { get; set; }
		public string? InvoiceTermsAndCondition { get; set; }
		public bool InvoiceNeedShippingAddress { get; set; } 
		public bool ItemQrCodeState { get; set; } = false;
		public string? TaxCategoryName { get; set; }
		[Range(1,int.MaxValue,ErrorMessage = "Please provide valid tax category item count")]
		public int? TaxCategoryItemCount { get; set; }
		public string? TaxCategoryItemNames { get; set; }
		public int? ItemQrPdfMediaID { get; set; }
        public int? ItemGroupQrPdfMediaID { get; set; }  
        public int ClientID { get; set; }
	}
}
