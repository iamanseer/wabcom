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
        public string Link
        {
            get
            {
                switch ((NotificationTypes)NotificationTypeID)
                {
                    case NotificationTypes.Enquiry:
                        return $"enquiry-view";
                    case NotificationTypes.Quotation:
                        return $"quotation-view";
                    case NotificationTypes.Followup:
                        return $"follow-ups";
                    default:
                        return "";
                }
            }
        }
        public string IconBackground
        {
            get
            {
                switch ((NotificationTypes)NotificationTypeID)
                {
                    case NotificationTypes.Enquiry:
                        return "bg-danger";
					case NotificationTypes.Quotation:
						return "bg-danger";
					case NotificationTypes.Followup:
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
                        return "New enquiry received";
                    case NotificationTypes.Quotation:
                        return "New quotation received";
                    case NotificationTypes.Followup:
                        return "New followup received";
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
                        return "New enquiry received 2";
                    case NotificationTypes.Quotation:
                        return "New quotation received 2";
                    case NotificationTypes.Followup:
                        return "New followup received 2";
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
					case NotificationTypes.Quotation:
						return "mdi-message-text-outline";
					case NotificationTypes.Followup:
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
