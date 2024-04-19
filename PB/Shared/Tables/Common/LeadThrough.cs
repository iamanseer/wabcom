using PB.Model;
namespace PB.Shared.Tables
{
    public class LeadThrough : Table
    {
        [PrimaryKey]
        public int LeadThroughID { get; set; }
        public string? Name { get; set; }
        public int? ClientID { get; set; }

    }
}
