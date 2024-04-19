using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.CourtClient
{
    public class BookingSection:Table
    {
        [PrimaryKey] 
        public int BookingSectionID { get; set; }
        public int BookingCourtID { get; set; } 
        public int SectionID { get; set; }
        public int HourID { get; set; }
    }
}
