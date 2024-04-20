using PB.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.CRM
{
    public class FollowupStatus : Table
    {
        [PrimaryKey]
        public int FollowUpStatusID { get; set; }
        [Required(ErrorMessage = "Please provide a followup status")]
        public string? StatusName { get; set; }
        [Required(ErrorMessage = "Please choose a nature for the followup")]
        [Range(1, 10, ErrorMessage = "Please choose a valid option")]
        public int Nature { get; set; }
        //[Required(ErrorMessage = "Please choose a type of the followup")]
        //[Range(1, 10, ErrorMessage = "Please choose a valid option")]
        public int Type { get; set; }
        public int? ClientID { get; set; }
    }
}
