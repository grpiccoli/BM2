using System.Collections.Generic;
using System.Linq;

namespace BiblioMit.Models
{
    public class Polygon
    {
        public int Id { get; set; }
        public int? LocalityId { get; set; }
        public virtual Locality Locality { get; set; }
        public int? PsmbId { get; set; }
        public virtual Psmb Psmb { get; set; }
        public int? CatchmentAreaId { get; set; }
        public virtual CatchmentArea CatchmentArea { get; set; }
        public virtual ICollection<Coordinate> Vertices { get; } = new List<Coordinate>();
        public double GetSurface()
        {
            // Add the first point to the end.
            int num_points = Vertices.Count;
            Coordinate[] pts = new Coordinate[num_points + 1];
            Vertices.CopyTo(pts, 0);
            pts[num_points] = Vertices.First();
            // Get the areas.
            double area = 0;
            for (int i = 0; i < num_points; i++)
            {
                area +=
                    (pts[i + 1].Latitude - pts[i].Latitude) *
                    (pts[i + 1].Longitude + pts[i].Longitude) / 2;
            }
            // Return the result.
            return area;
        }
    }
}
