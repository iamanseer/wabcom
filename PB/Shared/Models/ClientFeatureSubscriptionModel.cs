using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models
{
    public class ClientFeatureSubscriptionModel
    {
        public int SubscriptionID { get; set; }
        public int ClientID { get; set; }
        public int FeatureID { get; set; }

    }
}
