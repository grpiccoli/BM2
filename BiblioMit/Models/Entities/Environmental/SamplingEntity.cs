using Microsoft.Build.Framework;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BiblioMit.Models
{
    public class SamplingEntity : Indexed
    {
        public int Id { get; set; }
        [Required, DisallowNull]
        public string NormalizedName { get; set; }
        public virtual ICollection<PlanktonAssay> PlanktonAssays { get; } = new List<PlanktonAssay>();
    }
}
