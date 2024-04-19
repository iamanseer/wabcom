using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables
{
    public class Language : Table
    {
        [PrimaryKey]
        public int LanguageID { get; set; }
        public string? LanguageName { get; set; }
        public string? LanguageCode { get; set; }
    }
}
