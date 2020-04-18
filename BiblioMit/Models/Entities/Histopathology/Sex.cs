using System.ComponentModel.DataAnnotations;

namespace BiblioMit.Models.Entities.Histopathology
{
    public enum Sex
    {
        [Display(Name = "Male")]
        Male,
        [Display(Name = "Female")]
        Female,
        [Display(Name = "Hermaphrodite")]
        Hermaphrodite,
        [Display(Name = "Undetermined")]
        Undetermined
    }
}
