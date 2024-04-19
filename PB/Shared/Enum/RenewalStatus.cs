﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Enum
{
    public enum RenewalStatus
    {
        Paid=0,
        Disconnected,
        Due,
        Generated,
        NotCompleted,
        Blocked,
    }
}
