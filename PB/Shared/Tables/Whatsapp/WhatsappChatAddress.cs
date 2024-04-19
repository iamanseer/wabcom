using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Whatsapp
{
    public class WhatsappChatAddress : Table
    {
        [PrimaryKey]
        public int ChatAdressID { get; set; }
        public int? ChatID { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PinCode { get; set; }
        public string? HouseNo { get; set; }
        public string? FloorNo { get; set; }
        public string? TowerNo { get; set; }
        public string? BuildingName { get; set; }
        public string? LandmarkArea { get; set; }
    }
}
