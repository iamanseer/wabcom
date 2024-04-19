using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Spin
{
    public class CountModel
    {
        public int ExpectedParticipant { get; set; }
        public int TotalNoOfGifts { get; set; }
        public int CountOfParticipants { get; set; }
        public int PositionOfParticipant { get; set; }
        public string? GiftName { get; set; }
        public int? GiftID { get; set; }
    }
}
