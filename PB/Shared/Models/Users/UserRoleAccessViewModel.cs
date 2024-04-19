using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Users
{
    public class UserRoleAccessViewModel
    {
        public List<RoleGroupViewModel> RoleGroups { get; set; } = new();
    }

    public class RoleGroupViewModel : RoleGroupModel
    {
        public List<RoleViewModel> Roles { get; set; } = new();
    }

    public class RoleViewModel
    {
        public int? ID { get; set; }
        public int? RoleID { get; set; }
        public string? RoleName { get; set; }
        public bool HasAccess { get; set; }
        public bool IsRowSelected { get; set; }
        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanMail { get; set; }
        public bool CanWhatsapp { get; set; }
    }
}