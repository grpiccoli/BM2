using System.Collections.ObjectModel;

namespace BiblioMit.Models.ViewModels
{
    public static class SoftTissuesData
    {
        public static Collection<Tissue> Tissues { get; } = new Collection<Tissue>
                                                            {
                                                                Tissue.DigestiveGland,
                                                                Tissue.Foot,
                                                                Tissue.Gill,
                                                                Tissue.Gonad,
                                                                Tissue.Intestine,
                                                                Tissue.Mantle,
                                                                Tissue.Nephridium,
                                                                Tissue.PlicateMembrane,
                                                                Tissue.StyleSac,
                                                                Tissue.Tubules
                                                            };
    }
}
