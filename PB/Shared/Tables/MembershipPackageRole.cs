using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables
{
    public class MembershipPackageRole:Table
    {
        [PrimaryKey]
        public int PackageRoleID { get; set; }
        public int? PackageID { get; set; }
        public int? RoleID { get; set; }
    }
}
