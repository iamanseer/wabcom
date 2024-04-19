using PB.Shared.Tables;
using System.ComponentModel.DataAnnotations;

namespace PB.Shared.Models.CRM.Customer
{
    public class CustomerContactPersonModel : CustomerContactPerson
    {
        [Required(ErrorMessage = "Please enter name of the person")]
        public string? Name { get; set; }
        [RegularExpression("^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$", ErrorMessage = "Please provide a valid Email like example@test.com")]
        public string? Email { get; set; }
        [Required(ErrorMessage = "Please provide a valid Phone Number")]
        [RegularExpression("^[0-9]+$", ErrorMessage = "Only numbers are allowed")]
        public string? Phone { get; set; }
        public int RowIndex { get; set; }
        public string? ISDCode { get; set; } 
        public bool IsRowEditMode { get; set; } = false;
        public int EntityPersonalInfoID { get; set; }

    }
}
