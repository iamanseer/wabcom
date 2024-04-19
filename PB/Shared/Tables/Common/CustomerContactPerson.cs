using PB.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables
{
    public class CustomerContactPerson : Table
    {
        [PrimaryKey]
        public int ContactPersonID { get; set; }
        public int? EntityID { get; set; }
        public string? Designation { get; set; }
        public string? Department { get; set; }
        public int? CustomerEntityID { get; set; }
    }
}
