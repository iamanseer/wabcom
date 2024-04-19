using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Accounts.AccountGroups
{
    public class AccAccountGroupType : Table
    {
        [PrimaryKey]
        public int GroupTypeID { get; set; }
        public string? GroupTypeName { get; set; }
        public int? Nature { get; set; }  
    }
}
