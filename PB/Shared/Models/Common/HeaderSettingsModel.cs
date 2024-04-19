using PB.Model.Models;
using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models.Common
{
    public class HeaderSettingsModel
    {
        public string? Heading { get; set; }
        public List<HeaderNotifyCountModel>? Notifications { get; set; } = null;
        public bool NeedAddButton { get; set; } = true;
        public string? AddButtonText { get; set; }
        public string? SinglePageURL { get; set; }

        public bool NeedCustomButton { get; set; } = false;
        public string? CustomButtonIconClass { get; set; }
        public string? CustomButtonApiPath { get; set; }

        public bool NeedSearchOption { get; set; } = true;
        public string? SearchPlaceHolder { get; set; } = "Search..";

        public string? OrderByFieldName { get; set; }

        public bool HasDefaultSettings { get; set; } = false;

        public bool NeedSortOption { get; set; } = false;
        public List<FilterSortInputModel>? SortMenuItems { get; set; } = null;

        public bool NeedFilterOption { get; set; } = false;
        public List<FilterDateInputModel>? DateFilters { get; set; } = null;
        public List<FilterIdnValueInputModel>? IdnValueFilters { get; set; } = null;
        public List<FilterBooleanInputModel>? BooleanFilters { get; set; } = null;
        public FilterFieldInputModel? FieldFilters { get; set; } = null;

    }

    public class FilterSortInputModel
    {
        public string? DisplayName { get; set; }
        public string? FieldName { get; set; }
    }

    public class FilterDateInputModel
    {
        public string? FieldName { get; set; }
        public string? DisplayName { get; set; }
        public bool SetTodayDate { get; set; } = false;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class FilterIdnValueInputModel
    {
        public string? FiledName { get; set; }
        public string? DisplayName { get; set; }
        public object EnumObject { get; set; } = new();
        public List<int> SelectedEnumValues { get; set; } = new();
        public List<int> AvoidEnumValues { get; set; } = new();
        public string? CustomOptionApiPath { get; set; }
        public bool IsGet { get; set; } = false;
        public List<IdnValuePair> CustomOptions { get; set; } = new();
    }

    public class FilterBooleanInputModel
    {
        public string? FiledName { get; set; }
        public string? DisplayName { get; set; }
        public bool? Value { get; set; } = null;
        public List<BooleanFilterDiplayItemValues> DisplayItems = new();
    }

    public class BooleanFilterDiplayItemValues
    {
        public string? ItemDisplayName { get; set; }
        public bool ItemValue { get; set; }

    }
    public class FilterFieldInputModel
    {
        public string? DisplayName { get; set; }
        public List<FieldInputItemModel> Fields { get; set; } = new();
    }

    public class FieldInputItemModel
    {
        public string? FiledName { get; set; }
        public string? DisplayName { get; set; }
        public bool Value { get; set; }

    }

    public class HeaderNotifyCountModel
    {
        public string? NotifyValue { get; set; }
        public int NotifyCount { get; set; }
        public int Nature { get; set; }
        public string? BadgeClass
        {
            get
            {
                switch (Nature)
                {
                    case -1:
                        return "badge bg-danger-transparent br-5 text-danger pb-1 me-3";
                    case 0:
                        return "badge bg-success-transparent br-5 text-success pb-1 me-3";
                    case 1:
                        return "badge bg-info-transparent br-5 text-info pb-1 me-3";
                }
                return "";
            }
        }
    }
}
