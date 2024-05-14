using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Common
{
    public class CustomerCategory:Table
    {
        [PrimaryKey]
        public int CategoryID { get; set; }
        public string? CategoryName { get; set; }
        public int? ClientID { get; set; }

    }
}
