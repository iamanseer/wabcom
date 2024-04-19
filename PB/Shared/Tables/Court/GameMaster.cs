using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.CourtClient
{
    public class GameMaster:Table
    {
        [PrimaryKey]
        public int GameID { get; set; }
        public string? GameName { get; set; }
    }
}
