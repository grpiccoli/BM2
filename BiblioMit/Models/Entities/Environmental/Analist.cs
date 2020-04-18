using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BiblioMit.Models
{
    public class Analist : Indexed
    {
        public int Id { get; set; }
        [Required]
        public string NormalizedName { get; set; }
        public virtual ICollection<PlanktonAssay> PlanktonAssays { get; } = new List<PlanktonAssay>();
    }
}
