using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables
{
    public class AccSubType : Table
    {
        [PrimaryKey] public int? SubTypeID { get; set; }
        public string? SubTypeName { get; set; }
    }
}
