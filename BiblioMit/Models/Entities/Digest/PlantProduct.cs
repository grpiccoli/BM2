using BiblioMit.Models.Entities.Centres;

namespace BiblioMit.Models
{
    public class PlantProduct
    {
        public int PlantId { get; set; }
        public virtual Plant Plant { get; set; }
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }
    }
}
