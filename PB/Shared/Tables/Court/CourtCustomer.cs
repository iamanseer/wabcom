using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.CourtClient
{
    public class CourtCustomer:Table
    {
        [PrimaryKey] 
        public int CustomerID { get; set; }
        public int EntityID { get; set; }
        public int ClientID { get; set; }
    }
}
