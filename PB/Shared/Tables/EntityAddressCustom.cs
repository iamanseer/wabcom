using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Model.Models
{
    [TableName("EntityAddress")]
    public class EntityAddressCustom : EntityAddress
    {
        public int? StateID { get; set; } 
        public int? CityID { get; set; }
    }
}
