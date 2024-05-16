using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Common
{
    public class CustomerServices:Table
    {
        [PrimaryKey]
        public int ServiceID { get; set; }
        public int? ItemID { get; set; }
        public DateTime? Date { get; set; }
        public decimal? Amount {  get; set; }
        public int? Quantity { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public int? CustomerEntityID { get; set; }
    }
}
