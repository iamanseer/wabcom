using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Common
{
    public class PaymentTermSlab:Table
    {
        [PrimaryKey]
        public int SlabID { get; set; }
        public string? SlabName { get; set; }
        public int? PaymentTermID { get; set; }
        public int? Day { get; set; }
        public decimal? Percentage { get; set; }
    }
}
