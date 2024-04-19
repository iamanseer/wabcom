using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PB.Model;
using System.ComponentModel.DataAnnotations;

namespace PB.Shared.Tables.Tax
{
    public class TaxCategory : Table
    {
        [PrimaryKey]
        public int TaxCategoryID { get; set; }
        [Required(ErrorMessage = "Please provide Tax category Name")]
        public string? TaxCategoryName { get; set; }
        public int? ClientID { get; set; }
    }
}
