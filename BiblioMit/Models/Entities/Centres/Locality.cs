using BiblioMit.Extensions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;

namespace BiblioMit.Models
{
    public abstract class Locality
    {
        [Display(Name = "Unique Territorial Code CUT")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string Name { get; private set; }
        public void SetName(string value)
        {
            Name = value;
            NormalizedName = value?.ToString().RemoveDiacritics().ToUpperInvariant();
        }
        public LocalityType Discriminator { get; set; }
        public string NormalizedName { get; private set; }
        public virtual ICollection<Polygon> Polygons { get; } = new List<Polygon>();
        public virtual ICollection<Census> Censuses { get; } = new List<Census>();
        public double GetSurface() =>
            Polygons.Sum(p => p.GetSurface());
        //Código único territorial
        public string GetCUT() =>
            Id.ToString(CultureInfo.InvariantCulture).Remove(0, 1);
    }
    public enum LocalityType
    {
        Region = 1,
        Province = 2,
        Commune = 3
    }
}
