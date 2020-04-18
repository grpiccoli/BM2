using BiblioMit.Models.Entities.Centres;
using System.Collections.Generic;

namespace BiblioMit.Models
{
    public class PsmbArea : Psmb
    {
        public virtual ICollection<Farm> Farms { get; } = new List<Farm>();
    }
}
