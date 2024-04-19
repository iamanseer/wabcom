using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PB.Model;

namespace PB.Shared.Tables
{
    public class AccReferenceType:Table
    {
        [PrimaryKey]
        public int ReferenceTypeID { get; set; }
        public string? ReferenceTypeName { get; set; }
}
}
