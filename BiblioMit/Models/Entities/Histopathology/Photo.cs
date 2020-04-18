using BiblioMit.Models.Entities.Histopathology;

namespace BiblioMit.Models
{
    public class Photo
    {
        public int Id { get; set; }
        //Parent Id
        public int IndividualId { get; set; }
        //Related Parent Entities
        public virtual Individual Individual { get; set; }
        //ATT
        public string Key { get; set; }
        public string Comment { get; set; }
        public Magnification Magnification { get; set; }
    }
}
