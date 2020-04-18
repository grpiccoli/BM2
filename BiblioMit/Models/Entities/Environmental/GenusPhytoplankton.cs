using BiblioMit.Extensions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BiblioMit.Models.Entities.Environmental
{
    public class GenusPhytoplankton
    {
        public int Id { get; set; }
        public string Name { get; private set; }
        [Required]
        public string NormalizedName { get; private set; }
        public void SetName(string value)
        {
            NormalizedName = value;
            Name = value?.ToString().FirstCharToUpper();
        }
        [Required]
        public int GroupId { get; set; }
        public virtual PhylogeneticGroup Group { get; set; }
        public virtual ICollection<SpeciesPhytoplankton> SpeciesPhytoplanktons { get; } = new List<SpeciesPhytoplankton>();
    }
}
