using System.ComponentModel.DataAnnotations;

namespace BiblioMit.Models
{
    public enum Position
    {
        [Display(Name = "Research")]
        Research,
        [Display(Name = "Secretary")]
        Secretary,
        [Display(Name = "Management")]
        Management
    }
}
