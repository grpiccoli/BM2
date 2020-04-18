using System.ComponentModel.DataAnnotations;

namespace BiblioMit.Models
{
    public enum Ear
    {
        [Display(Name = "Absent")]
        Absent = 0,
        [Display(Name = "Rare")]
        Rare = 1,
        [Display(Name = "Scarce")]
        Scarce = 2,
        [Display(Name = "Regular")]
        Regular = 3,
        [Display(Name = "Abundant")]
        Abundant = 4,
        [Display(Name = "Very Abundant")]
        VeryAbundant = 5,
        [Display(Name = "Extremely Abundant")]
        ExtremelyAbundant = 6,
        [Display(Name = "Hyper Abundant")]
        HyperAbundant = 7
    }
}
