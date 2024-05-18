using PB.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Common
{
    public class CustomerSerialNumbers:Table
    {
        [PrimaryKey]
        public int ID { get; set; }
       public string? TallySerialNo { get; set; }
        public string? TallySerialNo2 { get; set; }
       public string? TallySerialNo3 { get; set; }
        public int? CustomerID { get; set; }
    }
}
