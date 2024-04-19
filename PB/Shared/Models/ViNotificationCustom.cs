using PB.Model;
using PB.Shared.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models
{
    public class ViNotificationCustom: ViNotification
    {
        public string IconBackground
        {
            get
            {
                switch ((NotificationTypes)NotificationTypeID)
                {
                    case NotificationTypes.Enquiry:
                        return "bg-danger";
                    default:
                        return "bg-success";
                }
            }
        }

        public string Title
        {
            get
            {
                switch ((NotificationTypes)NotificationTypeID)
                {
                    case NotificationTypes.Enquiry:
                        return "New inquiry received";
                    default:
                        return "New inquiry received";
                }
            }
        }

        public string Title2
        {
            get
            {
                switch ((NotificationTypes)NotificationTypeID)
                {
                    case NotificationTypes.Enquiry:
                        return "New inquiry received 2";
                    default:
                        return "New inquiry received 2";
                }
            }
        }

        public string Icon
        {
            get { 
                switch((NotificationTypes)NotificationTypeID)
                {
                    case NotificationTypes.Enquiry:
                        return "mdi-message-text-outline";
                    default:
                        return "mdi-message-text-outline";
                }
            }
        }

        public string Active
        {
            get
            {
                if (ReadOn == null)
                    return "active";

                return "";
            }
        }
    }
}
