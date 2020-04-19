using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BiblioMit.Blazor
{
    public interface IEnvironmental
    {
        DateTime GetMinDate();
        DateTime GetMaxDate();
        Task<IReadOnlyCollection<SelectListItem>> GetPhytoplanktonOrders();
        Task<IReadOnlyCollection<SelectListItem>> GetPhytoplanktonGenus();
        Task<IReadOnlyCollection<SelectListItem>> GetPhytoplanktonSp();
        Task<IReadOnlyCollection<SelectListItem>> GetPsmbs();
        Task<IReadOnlyCollection<SelectListItem>> GetCommunes();
        Task<IReadOnlyCollection<SelectListItem>> GetCatchments();
        Task<ICollection<ChartData>> GetDatas(
            DateTimeOffset? start,
            DateTimeOffset? endDt,
            IList<Variable> variables,
            IList<SelectListItem> orders, 
            IList<SelectListItem> genus,
            IList<SelectListItem> species,
            IList<SelectListItem> catchments,
            IList<SelectListItem> communes,
            IList<SelectListItem> psmbs);
        Task<ChartData> GetData(DateTimeOffset start, DateTimeOffset endDt, LocationType type, int locationId, string locationName, Variable variable);
    }
}
