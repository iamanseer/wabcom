using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.CourtClient
{
    public class HourMaster:Table
    {
        [PrimaryKey]
        public int HourID { get; set; }
        public string? HourName{ get; set; }
        public int HourInMinute { get; set; }
    }
}
