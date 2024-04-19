using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables
{
    [TableName("Branch")]
    public class BranchCustom:Branch
    {
        public int? ZoneID { get; set; }
    }
}
