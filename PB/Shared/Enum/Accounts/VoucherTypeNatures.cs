using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Enum
{
    public enum VoucherTypeNatures
    {
        Receipt = 1,
        Payment,
        Contra,
        Journal,
        Sales,
        SalesReturn,
        Purchase,
        PurchaseReturn
    }
}
