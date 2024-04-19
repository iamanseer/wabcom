using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models
{
    public class MailSettingsPostModel
    {
        public int MailSettingsID { get; set; }
        [Required]
        public string? SMTPHost { get; set; }
        [Required]
        public string? Port { get; set; }
        [Required]
        public string? Name { get; set; }
        [Required]
        public string? EmailAddress { get; set; }
        [Required]
        public string? Password { get; set; }
        public bool EnableSSL { get; set; }
        public string? UserName { get; set; }
        public bool ChangeMail { get; set; } = false;
    }
    public class MailSettingsID
    {
        public int Id { get; set; }
    }
}
