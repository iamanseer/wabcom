using PB.Shared.Tables.Court;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Court
{
    public class CourtPriceGroupItemModel:CourtPriceGroupItem
    {
        public bool IsEditMode { get; set; }
        public string? StartHourName { get; set; }
        public string? EndHourName { get; set; }
        public int RowIndex { get; set; }
    }
}
