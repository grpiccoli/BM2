using BiblioMit.Extensions;
using BiblioMit.Models.Entities.Environmental;
using Microsoft.Build.Framework;
using System.Collections.Generic;

namespace BiblioMit.Models
{
    public class PhylogeneticGroup
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
        public virtual ICollection<GenusPhytoplankton> GenusPhytoplanktons { get; } = new List<GenusPhytoplankton>();
    }
}
