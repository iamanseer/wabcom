using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Common
{
    public class PaymentTermModel
    {
        public int PaymentTermID { get; set; }
        [Required (ErrorMessage ="Please add payment term name")]
        public string? PaymentTermName { get; set; }
        public string? Description { get; set; }
        public int? ClientID { get; set; }
        public List<PaymentTermSlabModel> PaymentTermSlabs { get; set; } = new();
    }

    public class PaymentTermSlabModel
    {
        public int SlabID { get; set; }
        [Required(ErrorMessage = "Please add slab name")]
        public string? SlabName { get; set; }
        public int? PaymentTermID { get; set; }
        [Required(ErrorMessage = "Please add day")]
        public int? Day { get; set; }
        [Required(ErrorMessage ="Please add percentage")]
        public decimal? Percentage { get; set; }
    }

    public class PaymentTermListModel
    {
        public int PaymentTermID { get; set; }
        public string? PaymentTermName { get; set; }
        public string? Description { get; set; }
    }
}
