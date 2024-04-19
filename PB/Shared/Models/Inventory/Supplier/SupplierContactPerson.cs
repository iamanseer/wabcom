using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Inventory.Supplier
{
    public class SupplierContactPersonModel
    {
        [Required(ErrorMessage = "Please enter name of the person")]
        public string? Name { get; set; }
        [RegularExpression("^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$", ErrorMessage = "Please provide a valid Email like example@test.com")]
        public string? Email { get; set; }
        [Required(ErrorMessage = "Please provide a valid Phone Number")]
        [RegularExpression("^[0-9]+$", ErrorMessage = "Only numbers are allowed")]
        public string? Phone { get; set; }
        public int ContactPersonID { get; set; }
        public int? EntityID { get; set; }
        public string? Department { get; set; }
        public string? Designation { get; set; }
        public int? SupplierEntityID { get; set; }
        public int RowIndex { get; set; }
        public bool IsRowEditMode { get; set; } = false;
        public int EntityPersonalInfoID { get; set; }
    }
}
