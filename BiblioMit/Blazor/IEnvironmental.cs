using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace BiblioMit.Blazor
{
    public interface IEnvironmental
    {
        DateTime GetMinDate();
        DateTime GetMaxDate();
        IEnumerable<SelectListItem> GetPhytoplanktonOrders();
        IEnumerable<SelectListItem> GetPhytoplanktonGenus();
        IEnumerable<SelectListItem> GetPhytoplanktonSp();
        IEnumerable<SelectListItem> GetPsmbs();
        IEnumerable<SelectListItem> GetCommunes();
    }
}
