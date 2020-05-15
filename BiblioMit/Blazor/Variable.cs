using BiblioMit.Resources;
using System.ComponentModel.DataAnnotations;

namespace BiblioMit.Blazor
{
    public enum Variable
    {
        [Display(ResourceType = typeof(Blazor_Variable), Name = "Surface_Temperature", GroupName = "Oceanographic_Variables", Prompt = "°C")]
        t = 0,
        [Display(ResourceType = typeof(Blazor_Variable), Name = "Acidification", GroupName = "Oceanographic_Variables", Prompt = "pH")]
        ph = 1,
        [Display(ResourceType = typeof(Blazor_Variable), Name = "Oxigen", GroupName = "Oceanographic_Variables", Prompt = "ppm")]
        o2 = 2,
        [Display(ResourceType = typeof(Blazor_Variable), Name = "Salinity", GroupName = "Oceanographic_Variables", Prompt = "ppm")]
        sal = 3,
        [Display(ResourceType = typeof(Blazor_Variable), Name = "Total_Phytoplankton", GroupName = "Oceanographic_Variables", Prompt = "Cel/mL")]
        phy = 4
    }
    public enum LocationType
    {
        [Display(Name = "Cuenca")]
        Cuenca = 0,
        [Display(Name = "Comuna")]
        Commune = 1,
        [Display(Name = "Psmb")]
        Psmb = 2
    }
    public enum SelectType
    {
        Variables,
        Orders,
        Genus,
        Species,
        Communes,
        Psmbs
    }
}
