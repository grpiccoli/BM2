using System.ComponentModel.DataAnnotations;

namespace BiblioMit.Models
{
    public enum Item
    {
        [Display(Name = "PRODUCTO")]
        Product = 0,
        [Display(Name = "RECURSO")]
        Resource = 1
    }
}
