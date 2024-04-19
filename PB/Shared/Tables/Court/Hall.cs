using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.CourtClient
{
    public class Hall:Table
    {
        [PrimaryKey] 
        public int HallID { get; set; }
        public string? HallName { get; set; }
        public int? BranchID { get; set; }
        public bool IsActive { get; set; }

    }
}
