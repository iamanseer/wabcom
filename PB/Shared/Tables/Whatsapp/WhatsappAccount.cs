using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Whatsapp
{
    public class WhatsappAccount : Table
    {
        [PrimaryKey]
        public int WhatsappAccountID { get; set; }
        public int? ClientID { get; set; }
        public string? Name { get; set; }
        public string? PhoneNo { get; set; }
        public string? PhoneNoID { get; set; }
        public string? BusinessAccountID { get; set; }
        public string? Token { get; set; }
        public string? WelcomeMessage { get; set; } = "Hello @Name,\nHow may I assist you today?";
        public string? Version { get; set; }
        public string? AppID { get; set; }
        public bool NeedReminder { get; set; }
        public int ReinitiationHour { get; set; } = 2;
        public int ReminderHour { get; set; }
        public string? ConfirmButtonLabel { get; set; } = "Confirm";
        public string? EditButtonLabel { get; set; } = "Edit";
        public string? BackButtonLabel { get; set; } = "Go Back";
        public string? ChooseValidOptionLabel { get; set; } = "Sorry.Please choose a valid option from the list";
        public bool? IsChatbotEnabled { get; set; }
    }
}
