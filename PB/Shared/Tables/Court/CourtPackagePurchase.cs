using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.CourtClient
{
    public class CourtPackagePurchase:Table
    {
        [PrimaryKey]
        public int PurchaseID { get; set; }
        public string? PurchaseCode { get; set; }
        public int? CourtPackageID { get; set; }
        public int? EntityID { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set;}
        public int? JournalMasterID { get; set; }
    }
}
