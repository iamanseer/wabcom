using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Court
{
    public class CourtListModel
    {
        public int? CourtID { get; set; }
        public string? CourtName { get; set; }
        public bool? IsChecked { get; set; } = false;
    }
}
