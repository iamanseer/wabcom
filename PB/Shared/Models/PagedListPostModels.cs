using PB.Shared.Models;
using PB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models
{
    public class PagedListPostModelWithFilter : PagedListPostModel
    {
        public int CurrentFollowupNature { get; set; } = -1;
        public List<IdnValueFilterModel> FilterByIdOptions { get; set; } = new();
        public List<DateFilterModel> FilterByDateOptions { get; set; } = new();
        public List<BooleanFilterModel> FilterByBooleanOptions { get; set; } = new();
        public List<FieldFiltermodel> FilterByFieldOptions { get; set; } = new();
    }

    public class BroadcastSearchModel : PagedListPostModelWithFilter
    {
        public int MessageSendTypeId { get; set; } 
    }

    public class PagedListPostModelWithoutFilter : PagedListPostModel
    {
        public int StatusTypeID { get; set; } = 0;
    }

    public class FollowupStatusPostModel : PagedListPostModel
    {
        public int StatusTypeID { get; set; } = 0;
    }

    public class IdnValueFilterModel
    {
        public string? FieldName { get; set; }
        public List<int>? SelectedEnumValues { get; set; } = null;
    }

    public class DateFilterModel
    {
        public string? FieldName { get; set; }
        public DateTime? StartDate { get; set; } = null;
        public DateTime? EndDate { get; set; } = null;
    }

    public class BooleanFilterModel
    {
        public string? FieldName { get; set; }
        public int? Value { get; set; }
    }

    public class FieldFiltermodel
    {
        public string? FieldName { get; set; }
        public bool Value { get; set; }
    }


    //Filter summary model
    public class FilterSummaryModel
    {
        public string? FilterSummaryHeading { get; set; }
        public List<FilterSummaryItem> SummaryIems { get; set; } = new();
    }

    public class FilterSummaryItem
    {
        public string? FieldName { get; set; }
        public string? FilterValues { get; set; } 
    }
}
