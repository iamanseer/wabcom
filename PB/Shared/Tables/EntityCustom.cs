using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables
{
    [TableName("Entity")]
    public class EntityCustom:Entity
    {
        public int? CountryID { get; set; }
    }
}
