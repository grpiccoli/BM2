using BiblioMit.Models.Entities.Digest;
using System.ComponentModel.DataAnnotations.Schema;

namespace BiblioMit.Models
{
    public class HarvestDeclaration : NotProduction
    {
        [NotMapped]
        public string CompanyName { get; set; }
        [NotMapped]
        public int CompanyId { get; set; }
    }
}
