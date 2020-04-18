using System.Collections.Generic;

namespace BiblioMit.Models.Entities.Centres
{
    public class Plant : Psmb
    {
        public bool? Certifiable { get; set; }
        public virtual ICollection<PlantProduct> Products { get; } = new List<PlantProduct>();
        public virtual ICollection<SupplyDeclaration> SupplyDeclarations { get; } = new List<SupplyDeclaration>();
        public virtual ICollection<ProductionDeclaration> ProductionDeclarations { get; } = new List<ProductionDeclaration>();
    }
}
