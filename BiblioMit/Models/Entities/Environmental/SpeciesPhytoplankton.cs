using BiblioMit.Models.Entities.Environmental;
using Microsoft.Build.Framework;
using System.Collections.Generic;
using System.Globalization;

namespace BiblioMit.Models
{
    public class SpeciesPhytoplankton
    {
        public int Id { get; set; }
        public string Name { get; private set; }
        public string NormalizedName { get; private set; }
        public void SetName(string value)
        {
            NormalizedName = value;
            TextInfo text = CultureInfo.InvariantCulture.TextInfo;
            Name = text.ToLower(value);
        }
        //public string GetFullName()
        //{
        //    var n = string.IsNullOrWhiteSpace(Name) ? "sp" : Name;
        //    return $"{Genus.Name} {n}";
        //}
        [Required]
        public int GenusId { get; set; }
        public virtual GenusPhytoplankton Genus { get; set; }
        public virtual ICollection<Phytoplankton> Phytoplanktons { get; } = new List<Phytoplankton>();
    }
}
