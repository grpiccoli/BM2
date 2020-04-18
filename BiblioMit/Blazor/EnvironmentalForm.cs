using System;
using System.Collections.Generic;
using System.Linq;
using BiblioMit.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BiblioMit.Blazor
{
    public class EnvironmentalForm
    {
        public DateTimeOffset? Start { get; set; }
        public DateTimeOffset? End { get; set; }
        public List<int> Areas { get; } = new List<int>();
        public IReadOnlyCollection<Variable> Variables { get; } = Variable.t.Enum2List().ToList();
        public IReadOnlyCollection<SelectListItem> Orders { get; set; }
        public IReadOnlyCollection<SelectListItem> Genus { get; set; }
        public IReadOnlyCollection<SelectListItem> Sp { get; set; }
        public IReadOnlyCollection<SelectListItem> Psmbs { get; set; }
        public IReadOnlyCollection<SelectListItem> Communes { get; set; }
    }
}
