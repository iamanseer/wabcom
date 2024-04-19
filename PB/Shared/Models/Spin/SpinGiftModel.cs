using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Spin
{
    public class SpinGiftModel
    {
        public int GiftID { get; set; }
        public string? GiftName { get; set; }
        public int? MediaID { get; set; }
        public int NumberOfPiece { get; set; }
        public int PrizeType { get; set; }
        public int? ContestID { get; set; }
        public string? Color { get; set; }
        public string? PrizeWon { get; set; }
    }
}
