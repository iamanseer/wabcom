using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models
{
    public class RoleModel
    {
        public int RoleID { get; set; }

        public string? RoleName { get; set; }

        public string? DisplayName { get; set; }

        public int RoleGroupID { get; set; }

        public bool IsForSupport { get; set; }
        public bool HasAccess { get; set; }
    }

    public class RoleGroupModel
    {
        public int RoleGroupID { get; set; }

        public string? RoleGroupName { get; set; }

        public string? DisplayName { get; set; }
        public bool SelectAll { get; set; }
    }
}
