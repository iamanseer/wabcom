using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.CourtClient
{
    public class HallSection:Table
    {
        [PrimaryKey]
        public int SectionID { get; set; }
        public string? SectionName  { get; set; }
        public int HallID { get; set; }
    }
}
