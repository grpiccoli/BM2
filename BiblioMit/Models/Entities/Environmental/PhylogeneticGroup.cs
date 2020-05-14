using BiblioMit.Extensions;
using BiblioMit.Models.Entities.Environmental;
using Microsoft.Build.Framework;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BiblioMit.Models
{
    public class PhylogeneticGroup
    {
        public int Id { get; set; }
        [DisallowNull]
        public string Name { get; private set; }
        [Required, DisallowNull]
        public string NormalizedName { get; private set; }
        public void SetName([DisallowNull]string value)
        {
            NormalizedName = value;
            Name = value.FirstCharToUpper();
        }
        public virtual ICollection<GenusPhytoplankton> GenusPhytoplanktons { get; } = new List<GenusPhytoplankton>();
    }
}
