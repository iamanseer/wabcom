using PB.Shared.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.SuperAdmin.Client
{
    public class ClientSettingModel : ClientSetting
    {
        public List<ClientSettingTaxCategoryItemModel> TaxCategoyItems { get; set; } = new();  
    }

    public class ClientSettingTaxCategoryItemModel
    {
        public bool IsRowEditMode { get; set; } = false;
        public string? TaxCategoyItemName { get; set; } 
    }
}
