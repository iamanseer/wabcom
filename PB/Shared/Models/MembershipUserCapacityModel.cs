﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models
{
    public class MembershipUserCapacityModel
    {
        public int CapacityID { get; set; }
        [Required]
        public string? Capacity { get; set; }
    }
}
