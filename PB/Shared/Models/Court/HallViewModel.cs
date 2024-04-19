using PB.Shared.Tables.CourtClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Court
{
    public class HallViewModel:HallModel
    {
        public List<HallSectionModel> Sections { get; set; } = new();
        public int AtomCourtCount { get; set; }
        public List<HallTimingModel> HallTimings { get; set; } = new();
    }

    public class HallSectionModel: HallSection
    {
        public int? RowIndex { get; set; }
    }
}
