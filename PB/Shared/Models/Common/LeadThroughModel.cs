using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Model.Models
{
	public class LeadThroughModel
	{
		public int LeadThroughID { get; set; }
		[Required(ErrorMessage = "Please provide a lead through name")]
		public string? Name { get; set; }
		public int? ClientID { get; set; }
	}
}
