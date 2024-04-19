using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables
{
    public  class MembershipUserCapacity:Table
    {
        [PrimaryKey]
        public int CapacityID { get; set; }
        public int Capacity { get; set; }    

    }
}
