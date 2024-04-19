using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Enum
{
    public enum EntityType
    {
        User = 1,
        Client,
        Branch,
        Staff,
        Customer,
        ContactPerson,
        CourtCustomer,

        //Newly added
        Agent,
        Supplier 
    }
}
