using PB.Model;
using PB.Shared.Enum.Court;
using PB.Shared.Tables.CourtClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Court
{
    public class CourtModel
    {
        public int CourtID { get; set; }
        [Required(ErrorMessage = "Enter court name")]
        public string? CourtName { get; set; }
        [Required(ErrorMessage = "Choose a game")]
        public int? GameID { get; set; }
        public string? GameName { get; set; }
        [Required(ErrorMessage = "Choose a hall")]
        public int HallID { get; set; }
        public string? HallName { get; set; }
        public List<CourtSectionMapModel> CourtSections { get; set; } = new();
        public string? Sections { get; set; }
        public int BranchID { get; set; }
        [Required(ErrorMessage ="Please choose price group")]
        [Range(1,int.MaxValue,ErrorMessage = "Please choose valid price group")]
        public int? PriceGroupID { get; set; }
        public string? PriceGroupName { get; set; }
        public bool IsActive { get; set; }
    }

    public class CourtSectionMapModel
    {
        public int MapID { get; set; }
        public int SectionID { get; set; }
        public string? SectionName { get; set; }
    }


    public class ApplyDiscountPostModel
    {
        public int BookingID { get; set; }
        public CourtDiscountType DiscountType { get; set; } = CourtDiscountType.Amount;
        [Required(ErrorMessage ="Please enter value")]
        public decimal Value { get; set; }
    }
}
