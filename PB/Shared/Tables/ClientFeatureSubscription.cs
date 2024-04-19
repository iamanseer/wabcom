using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables
{
    public class ClientFeatureSubscription:Table
    {
        [PrimaryKey] 
        public int SubscriptionID { get; set; }
        public int ClientID { get; set; }
        public int FeatureID { get; set; }


    }
}
