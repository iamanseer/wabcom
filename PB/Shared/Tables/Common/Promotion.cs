using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Common
{
    public class Promotion:Table
    {
        [PrimaryKey]
        public int PromotionID { get; set; }
        public string? PromotionName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? ClientID { get; set; }
    }
}
