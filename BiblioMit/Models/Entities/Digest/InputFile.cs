using System.Collections.Generic;

namespace BiblioMit.Models
{
    public class InputFile
    {
        public int Id { get; set; }
        public string ClassName { get; set; }
        public virtual ICollection<Registry> Registries { get; } = new List<Registry>();
    }
}
