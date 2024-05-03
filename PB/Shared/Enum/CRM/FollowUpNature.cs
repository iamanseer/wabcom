using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Enum.CRM
{
    public enum FollowUpNatures
    {
        New=0,
        Followup,
        Dropped,
        ClosedWon,
        Interested


        //New = 0,
        //Followup,
        //Cancelled,-->Droped
        //Converted-->ClosedWon
    }
}
