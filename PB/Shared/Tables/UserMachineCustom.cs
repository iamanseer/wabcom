using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables
{
    [TableName("UserMachine")]
    public class UserMachineCustom:UserMachine
    {
        public bool IsApp { get; set; }
        public string? IPAddress { get; set; }
        public string? DeviceID { get; set; }
    }
}
