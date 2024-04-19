using PB.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Tax
{
    public class TaxPreference : Table
    {
        [Required][PrimaryKey] public int? TaxPreferenceTypeID { get; set; }
        [Required] public string? TaxPreferenceName { get; set; }
    }
}
