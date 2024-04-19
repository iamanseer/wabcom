using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Enum
{
    public enum ChatbotReplyTypes
    {
        Forward = 1,
        //Template=2,
        Enquiry=3,
        Text=4,
        Location=5,
        List=6,
        ListOption=7,
        Submit=8,
        ChatWithAgent=9,
        GoToMain=10,
        Address=11,
        Purchase=12
    }
}