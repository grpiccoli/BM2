using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BiblioMit.Models
{
    public class Origin : Indexed
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<SeedDeclaration> Seeds { get; } = new List<SeedDeclaration>();
    }
}
