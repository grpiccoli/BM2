using System.Collections.Generic;

namespace BiblioMit.Models.Entities.Centres
{
    public class Plant : Psmb
    {
        public bool? Certifiable { get; set; }
        public virtual ICollection<PlantProduct> Products { get; } = new List<PlantProduct>();
    }
}
