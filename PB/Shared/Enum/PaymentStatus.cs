using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Enum
{
    public enum PaymentStatus
    {
        //Pending=1,
        //Verifying,
        //Completed,
        //CheckoutNotComplete,

        Pending=0,
        Paid,
        Verified,
        Rejected,
        CheckoutNotComplete,
    }
}
