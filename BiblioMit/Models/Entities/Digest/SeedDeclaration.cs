using BiblioMit.Models.Entities.Digest;
using System.ComponentModel.DataAnnotations.Schema;

namespace BiblioMit.Models
{
    public class SeedDeclaration : NotProduction
    {
        public int OriginId { get; set; }
        public virtual Origin Origin { get; set; }
        [NotMapped]
        public string CompanyName { get; set; }
    }
}
