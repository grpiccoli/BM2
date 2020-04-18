using System.Collections.Generic;

namespace BiblioMit.Models
{
    public class Origin
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<SeedDeclaration> Seeds { get; } = new List<SeedDeclaration>();
    }
}
