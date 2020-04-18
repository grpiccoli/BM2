using System.Collections.Generic;

namespace BiblioMit.Models.Entities.Centres
{
    public class Craft : Psmb
    {
        public virtual ICollection<SupplyDeclaration> SupplyDeclarations { get; } = new List<SupplyDeclaration>();
    }
}
