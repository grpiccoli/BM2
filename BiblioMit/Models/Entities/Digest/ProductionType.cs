using System.ComponentModel.DataAnnotations;

namespace BiblioMit.Models.Entities.Digest
{
    public enum ProductionType
    {
        [Display(Name = "Not informed", GroupName = "Plant Reports", Description = "")]
        Unknown = 0,
        [Display(Name = "Frozen Food", GroupName = "Plant Reports", Description = "")]
        Frozen = 1,
        [Display(Name = "Preserved Food", GroupName = "Plant Reports", Description = "")]
        Preserved = 2,
        [Display(Name = "Refrigerated", GroupName = "Plant Reports", Description = "")]
        Refrigerated = 3
    }
}
