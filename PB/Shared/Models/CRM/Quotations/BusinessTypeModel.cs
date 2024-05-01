using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.CRM.Quotations
{
    public class BusinessTypeModel
    {
        public int BusinessTypeID { get; set; }
        [Required (ErrorMessage ="Please add business type name")]
        public string? BusinessTypeName { get; set; }
        public int? ClientID { get; set; }
    }
}
