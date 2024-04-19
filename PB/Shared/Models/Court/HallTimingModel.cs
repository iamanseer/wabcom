using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Court
{
    public class HallTimingModel
    {
        public int HallTimingID { get; set; }
        public int? HallID { get; set; }
        public int? DayID { get; set; }
        public int? StartHourID { get; set; }
        public int? EndHourID { get; set; }
        public int? RowIndex { get; set; }
        public string? Timing { get; set; }
        public bool IsEditMode { get; set; } = false;
        public string? StartHourName { get; set; }
        public string? EndHourName { get; set; }
    }

}
