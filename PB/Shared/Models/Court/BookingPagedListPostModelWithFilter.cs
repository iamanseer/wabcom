using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Court
{
    public class BookingPagedListPostModelWithFilter:PagedListPostModelWithFilter
    {
        public string? CustomerPhone { get; set; }
    }
}
