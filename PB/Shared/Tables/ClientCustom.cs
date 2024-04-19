using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 
namespace PB.Shared.Tables
{
    [TableName("Client")]
    public class ClientCustom : PB.Model.Client 
    {
        public string? GSTNo { get; set; }
        public int? PackageID { get; set; }
        //public int? PaymentStatus { get; set; }
        //public string? PaymentRefNo { get; set; }
        //public int? MediaID { get; set; }
        public bool IsPaid { get; set; }
        public bool IsBlock { get; set; }
        public string? BlockReason { get; set; }
    }
}
