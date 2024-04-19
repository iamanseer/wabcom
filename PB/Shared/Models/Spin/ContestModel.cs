using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Spin
{
    public class ContestModel
    {
        public int ContestID { get; set; }
        [Required(ErrorMessage = "Please Enter a GiftName")]
        public string? ContestName { get; set; }
        [Required(ErrorMessage = "Please Enter StartDate")]
        public DateTime? StartDate { get; set; }
        [Required(ErrorMessage = "Please Enter EndDate")]
        public DateTime? EndDate { get; set; }
        [Required(ErrorMessage = "Please Enter ExpectedParticipants")]
        public int ExpectedParticipants { get; set; }
        public List<ContestGiftModel> Gifts { get; set; } = new();
        public List<WinnersListModel> Winners { get; set; } = new();
    }

    public class ContestGiftModel
    {
        public int RowNumber { get; set; }
        public int GiftID { get; set; }
        [Required(ErrorMessage = "Please Enter a GiftName")]
        public string? GiftName { get; set; }
        public int? MediaID { get; set; }
        [Required(ErrorMessage = "Please Enter a Count Of Gift")]
        public int NumberOfPiece { get; set; }
        [Required(ErrorMessage = "Please Choose a PrizeType")]
        public int PrizeType { get; set; }
        public int? ContestID { get; set; }
        public int? ContactID { get; set; }
        public int PrizeID { get; set; }

    }
}
