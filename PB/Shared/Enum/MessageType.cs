using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Enum
{
    public enum MessageTypes
    {
        None=0,
        Text=1,
        Template,
        Image,
        Audio,
        Video,
        Document,
        Contacts,
        Location,
        Sticker,
        Voice,
        Interactive,
        Address,
    }

    public enum MessageStatus
    {
        Sent=1,
        Delivered,
        Read,
        Deleted,
        Failed
    }
}
