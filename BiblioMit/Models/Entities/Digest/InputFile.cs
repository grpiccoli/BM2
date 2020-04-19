using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BiblioMit.Models
{
    public class InputFile
    {
        public int Id { get; set; }
        [Display(Name = "Código interno archivo")]
        public string ClassName { get; set; }
        public virtual ICollection<Registry> Registries { get; } = new List<Registry>();
    }
}
