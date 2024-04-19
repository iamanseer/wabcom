using PB.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables
{
    public class CourtCounterCode:Table
    {
        [PrimaryKey]
        public int CounterCodeID { get; set; }
        [Required(ErrorMessage ="Please enter counter code")]
        public string? CounterCode { get; set; }
    }
}
