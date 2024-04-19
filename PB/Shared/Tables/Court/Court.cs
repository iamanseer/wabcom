using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.CourtClient
{
    public class Court:Table
    {
        [PrimaryKey] 
        public int CourtID { get; set; }
        public string? CourtName { get; set; }
        public int? GameID { get; set; }
        public int? PriceGroupID { get; set; }
        public int? HallID { get; set; }
        public bool IsActive { get; set; }

    }
}
