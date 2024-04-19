using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.CRM.Quotation
{
    public class UpdateItemVariantsPostRequestModel
    {
        public int PlaceOfSupplyID { get; set; }
        public int SourceOfSupplyID { get; set; } 
        public List<int> ItemVariantIDs { get; set; } = new();
    }
}
