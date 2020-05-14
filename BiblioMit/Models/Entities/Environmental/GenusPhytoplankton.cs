using BiblioMit.Extensions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace BiblioMit.Models.Entities.Environmental
{
    public class GenusPhytoplankton
    {
        public int Id { get; set; }
        [DisallowNull, Required]
        public string Name { get; private set; }
        [Required, DisallowNull]
        public string NormalizedName { get; private set; }
        public void SetName([DisallowNull]string value)
        {
            NormalizedName = value;
            Name = value.FirstCharToUpper();
        }
        [Required]
        public int GroupId { get; set; }
        public virtual PhylogeneticGroup Group { get; set; }
        public virtual ICollection<SpeciesPhytoplankton> SpeciesPhytoplanktons { get; } = new List<SpeciesPhytoplankton>();
    }
}
