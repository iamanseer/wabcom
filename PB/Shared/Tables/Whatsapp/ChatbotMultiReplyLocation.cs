﻿using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Whatsapp
{
    public class ChatbotMultiReplyLocation : Table
    {
        [PrimaryKey]
        public int LocationID { get; set; }
        public int? MultiReplyID { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
