using System.ComponentModel.DataAnnotations;

namespace BiblioMit.Blazor
{
    public enum Variable
    {
        [Display(Name = "Temperature")]
        t = 0,
        [Display(Name = "Ph")]
        ph = 1,
        [Display(Name = "Oxigen")]
        o2 = 2,
        [Display(Name = "Salinity")]
        sal = 3,
        [Display(Name = "Total Phytoplankton")]
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
