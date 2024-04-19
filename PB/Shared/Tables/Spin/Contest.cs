using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables
{
    public class Contest:Table
    {
        [PrimaryKey]
        public int ContestID { get; set; }
        public string? ContestName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int ExpectedParticipants { get; set; } 
    }
}
