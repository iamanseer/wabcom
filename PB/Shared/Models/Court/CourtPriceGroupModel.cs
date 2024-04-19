using PB.Shared.Tables.Court;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Court
{
    public class CourtPriceGroupModel:CourtPriceGroup
    {
        public string? CurrencyName { get; set; }
        public List<CourtPriceGroupItemModel> Items { get; set; } = new();
    }
}
