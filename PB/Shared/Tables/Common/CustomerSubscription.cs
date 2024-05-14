using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Common
{
    public class CustomerSubscription:Table
    {
        [PrimaryKey]
        public int SubscriptionID { get; set; }
        public string? SubscriptionName { get; set; }
        public int? ClientID { get; set; }
    }
}
