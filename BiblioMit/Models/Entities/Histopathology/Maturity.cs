using System.ComponentModel.DataAnnotations;

namespace BiblioMit.Models.Entities.Histopathology
{
    public enum Maturity
    {
        [Display(Name = "Developing")]
        Developing,
        [Display(Name = "Mature")]
        Mature,
        [Display(Name = "Spawning")]
        Spawning,
        [Display(Name = "Post-spawning")]
        PostSpawning,
        [Display(Name = "Undetermined")]
        Undetermined
    }
}
