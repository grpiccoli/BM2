using System.Collections.Generic;

namespace BiblioMit.Models
{
    public class CatchmentArea
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int PolygonId { get; set; }
        public virtual Polygon Polygon { get; set; }
        public virtual ICollection<Commune> Communes { get; } = new List<Commune>();
    }
}
