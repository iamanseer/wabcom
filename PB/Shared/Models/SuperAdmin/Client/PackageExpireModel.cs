using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.SuperAdmin.Client
{
    public class PackageExpireModel
    {
        public int Days { get; set; }
        public string? ClientIDs { get; set; }
    }
}
