using BiblioMit.Blazor;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BiblioMit.Pages
{
    public class BiblioMitComponent : ComponentBase
    {
        [Inject]
        protected IEnvironmental EnvironmentalService { get; set; }
    }
}
