using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.CRM.Customer
{
    public class CustomerServiceModel
    {
        public int ServiceID { get; set; }
        public int? ItemID { get; set; }
        public DateTime? Date { get; set; }
        public decimal? Amount { get; set; }
        public int? Quantity { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public int? CustomerEntityID { get; set; }
        public string? ServiceName { get; set; }
        public bool IsSubscription { get; set; }
        public int RowIndex { get; set; }
        public bool IsEditMode { get; set; } = false; 
    }
}
