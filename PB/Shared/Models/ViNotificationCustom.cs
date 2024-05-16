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
                    case NotificationTypes.EnquiryFollowup:
                        return $"enquiry-view";
					case NotificationTypes.QuotationFollowup:
						return $"quotation-view";
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
					case NotificationTypes.EnquiryFollowup:
						return "bg-danger";
					case NotificationTypes.QuotationFollowup:
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
                    case NotificationTypes.EnquiryFollowup:
                        return "New Enquiry Follow-up Received.";
					case NotificationTypes.QuotationFollowup:
						return "New Quotation Follow-up Received.";
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
                    case NotificationTypes.EnquiryFollowup:
                        return "New Enquiry Follow-up Received 2";
					case NotificationTypes.QuotationFollowup:
						return "New Quotation Follow-up Received 2";
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
					case NotificationTypes.EnquiryFollowup:
						return "mdi-message-text-outline";
					case NotificationTypes.QuotationFollowup:
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
