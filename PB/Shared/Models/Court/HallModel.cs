using PB.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Court
{
    public class HallModel
    {
        public int HallID { get; set; }
        [Required(ErrorMessage = "Provide a Hall Name")]
        public string? HallName { get; set; }
        public bool IsActive { get; set; }
    }

    public class NewHallModel
    {
        [Required(ErrorMessage = "Provide a Hall Name")]
        public string? HallName { get; set; }
        [Required(ErrorMessage = "Provide No of courts")]
        public int? AtomCourtCount { get; set; }

        [Required(ErrorMessage = "Choose a game")]
        public int? GameID { get; set; }
        public string? GameName { get; set; }

        [Required(ErrorMessage = "Choose Price Group")]
        public int? PriceGroupID { get; set; }
        public string? PriceGroupName { get; set; }
    }
}
