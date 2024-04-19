using PB.Shared.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.SuperAdmin.Custom
{
    public class CustomInvoiceGenerationModel
    {
        public int ClientID { get; set; }
        public int CurrentUserID { get; set; } 
        public int PackageID { get; set; }
        public int PaymentStatus { get; set; } = (int)PB.Shared.Enum.PaymentStatus.Pending;
        public DateTime? StartDate { get; set; }
    }
}
