using System.ComponentModel.DataAnnotations;

namespace BiblioMit.Models.Entities.Histopathology
{
    public enum Adg
    {
        [Display(Name = "1 Present", Description = "Sparse adipogranular cells observed")]
        Intensity1,

        [Display(Name = "2 Frequent", Description = "Dispersed throughout mantle tissues")]
        Intensity2,

        [Display(Name = "3 Abundant", Description = "Adipogranular cells (ADG) constitute the vast mayority of the vesiculosus tissue total volume")]
        Intensity3
    }
}
