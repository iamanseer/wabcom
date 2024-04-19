using PB.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Inventory.InvoiceType
{
    public class InvoiceTypeCharge : Table
    {
        [PrimaryKey]
        public int InvoiceTypeChargeID { get; set; }
        [Required(ErrorMessage = "Please provide the name of the charge")]
        public string? ChargeName { get; set; }
        [Required(ErrorMessage = "Enter the order for the charge")]
        [Range(1, int.MaxValue, ErrorMessage = "Order number should be equal or above one")]
        public int OrderNumber { get; set; }
        public int ClientID { get; set; }
        public int InvoiceTypeID { get; set; }
        [Range(1, 10, ErrorMessage = "Please choose nature for the charge")]
        public int ChargeNature { get; set; }

        [Range(1, 2, ErrorMessage = "Please choose operation for the charge")]
        public int ChargeOperation { get; set; }

        [Range(1, 10, ErrorMessage = "Please choose calculation type of the charge")]
        public int ChargeCalculation { get; set; }

        [Range(1, 10, ErrorMessage = "Please choose the effect type (Invoice/Invoice item)")]
        public int ChargeEffect { get; set; }
        public int? LedgerID { get; set; }
    }
}
