using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models
{
    public class AppLogoutModel
    {
        [Required]
        public string MachineID { get; set; }
    }
}
