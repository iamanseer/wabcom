using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models
{
    public class PackageRoleListModel
    {
        public List<RoleGroupModel> RoleGroups { get; set; } = new();
        public List<RoleModel> Roles { get; set; } = new();
    }


    public class RoleGroupsModel
    {
        public int? RoleGroupID { get; set; }
        public string? RoleGroupName { get; set; }
        public bool IsSelected { get; set; } = false;
        public List<RoleModel> Roles { get; set; } = new();
    }

    public class PackageRoleModel
    {
        public int PackageRoleID { get; set; }
        public int? RoleID { get; set; }
        public int? RoleGroupID { get; set; }
        public int? PackageID { get; set; }
        
    }

}
