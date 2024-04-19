using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Enum
{
    public enum LogType
    {
        SendWhatsapp = 1,
        WhatsappAccounts,
        WhatsappTemplate,
        SendEmail,
        EmailAccounts,
        EmailTemplate,
        WhatsappChat,
        WhatsappChatbot,
        Gallery,

    }
    public enum LogMode
    {
        SendMessage = 1,
        Viewed,
        Added,
        Edited,
        Deleted,


    }
}
