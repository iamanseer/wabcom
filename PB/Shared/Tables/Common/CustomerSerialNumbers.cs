using PB.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Common
{
    public class CustomerSerialNumbers:Table
    {
        [PrimaryKey]
        public int ID { get; set; }
        [Required(ErrorMessage = "Please add tally serial no")]
        [StringLength(maximumLength: 9, ErrorMessage = "Length must be between 0 and 9 characters")]
        [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "tally serial no field should only contain white spaces,small letters,numbers and hyphens")]
        public string? TallySerialNo { get; set; }
        [StringLength(maximumLength: 9, ErrorMessage = "Length must be between 0 and 9 characters")]
        [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "tally serial no2 field should only contain white spaces,small letters,numbers and hyphens")]
        public string? TallySerialNo2 { get; set; }
        [StringLength(maximumLength: 9, ErrorMessage = "Length must be between 0 and 9 characters")]
        [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "tally serial no3 field should only contain white spaces,small letters,numbers and hyphens")]
        public string? TallySerialNo3 { get; set; }
        public int? CustomerID { get; set; }
    }
}
