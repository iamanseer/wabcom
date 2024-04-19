using PB.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Tables.Whatsapp
{
    public class WhatsappTemplateVariable : Table
    {
        [PrimaryKey]
        public int VariableID { get; set; }
        public string? VariableName { get; set; }
        public int? TemplateID { get; set; }
        public int Section { get; set; }//1 for Header,2 for Body and 3 for Button
        public int DataType { get; set; } //WhatsappTemplateVariableDataType
        public string? SampleValue { get; set; }
        public int OrderNo { get; set; }
    }
}
