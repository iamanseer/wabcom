using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables
{
    public class ContestParticipant:Table
    {
        [PrimaryKey]
        public int ParticipantID { get; set; }
        public int? ContestID { get; set; }
        public int? ContactID { get; set; }
    }
}
