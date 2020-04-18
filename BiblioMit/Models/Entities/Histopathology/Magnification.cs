using System.ComponentModel.DataAnnotations;

namespace BiblioMit.Models.Entities.Histopathology
{
    public enum Magnification
    {
        [Display(Name = "5X")]
        mag5x,
        [Display(Name = "10X")]
        mag10x,
        [Display(Name = "20X")]
        mag20x,
        [Display(Name = "40X")]
        mag40x,
        [Display(Name = "100X")]
        mag100x
    }
}
