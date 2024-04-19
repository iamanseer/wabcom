using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.CourtClient
{
    public class CourtSectionMap:Table
    {
        [PrimaryKey] 
        public int MapID { get; set; }
        public int SectionID { get; set; }
        public int CourtID { get; set; }
    }
}
