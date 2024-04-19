using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.CourtClient
{
    public class HallTiming:Table
    {
        [PrimaryKey]
        public int HallTimingID { get; set; }
        public int? HallID { get; set; }
        public int? DayID { get; set; }
        public int? StartHourID { get; set; }
        public int? EndHourID { get; set; }
    }
}
