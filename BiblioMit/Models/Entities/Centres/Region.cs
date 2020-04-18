using BiblioMit.Extensions;
using System.Collections.Generic;

namespace BiblioMit.Models
{
    public class Region : Locality
    {
        //[Display(Name = "Código Único Territorial")]
        //[DatabaseGenerated(DatabaseGeneratedOption.None)]
        //public int Id { get; set; }

        //[Display(Name = "Superficie")]
        //public int Surface { get; set; }

        //[Display(Name = "Población 2002")]
        //public int Pop2002 { get; set; }

        //[Display(Name = "Población 2010")]
        //public int Pop2010 { get; set; }

        //[Display(Name = "Nombre de Región")]
        //public string Name { get; set; }
        public virtual ICollection<Province> Provinces { get; } = new List<Province>();
        public string GetRomanId() => (Id - 100).ToRomanNumeral();
        public string GetFullName() => $"{GetRomanId()} {Name}";
        //public virtual ICollection<Polygon> Polygons { get; } = new List<Polygon>();
    }
}
