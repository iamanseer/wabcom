using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Inventory.Items
{
    public class RemoveFromCartModel
    {
        public int EnquiryID { get; set; }
        public int EnquiryItemID { get; set; }  
    }
}
