using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Common
{
    public class PLaceOfSupplyChangedPostModel
    {
        public int? PlaceOfSupplyID { get; set; }
        public List<int> ItemVariantIDList { get; set; } = new();
    }
}
