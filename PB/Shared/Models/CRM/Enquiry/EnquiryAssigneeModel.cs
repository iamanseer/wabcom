

using PB.Shared.Tables.CRM;

namespace PB.Shared.Models.CRM
{
    public class EnquiryAssigneeModel : EnquiryAssignee
    {
        public bool IsAssigned { get; set; } = false;
        public string? Name { get; set; }
    }
}
