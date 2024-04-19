using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables
{
    public class TagMaster:Table
    {
        [PrimaryKey]
        public int TagID { get; set; }
        public string? Tag { get; set; }
        public int? ClientID { get; set; }
    }
}
