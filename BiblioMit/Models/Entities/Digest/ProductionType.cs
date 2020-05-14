using System.ComponentModel.DataAnnotations;

namespace BiblioMit.Models.Entities.Digest
{
    public enum ProductionType
    {
        [Display(Name = "", GroupName = "Plant Reports", Description = "Not informed")]
        Unknown = 0,
        [Display(Name = "CONGELADO", GroupName = "Plant Reports", Description = "Frozen Food")]
        Frozen = 1,
        [Display(Name = "CONSERVAS", GroupName = "Plant Reports", Description = "Preserved Food")]
        Preserved = 2,
        [Display(Name = "REFRIGERADO", GroupName = "Plant Reports", Description = "Refrigerated")]
        Refrigerated = 3
    }
}
