using PB.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables
{
    [TableName("Media")]
    public class MediaCustom : Media
    {
        [Required]
        public string? Title { get; set; }

    }
}
