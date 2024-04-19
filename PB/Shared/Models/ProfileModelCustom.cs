using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models
{
    public class ProfileModelCustom : ProfileModel
    {
        public string? MediaURL { get; set; }
        public string? UserName { get; set; } 
    }
}
