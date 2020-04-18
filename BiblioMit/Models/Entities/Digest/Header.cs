using BiblioMit.Extensions;
using System.ComponentModel.DataAnnotations;

namespace BiblioMit.Models.Entities.Digest
{
    public class Header
    {
        public int Id { get; set; }
        [Display(Name = "Texto del encabezado de dato a extraer")]
        public int RegistryId { get; set; }
        public virtual Registry Registry { get; set; }
        public string Name { get; private set; }
        public void SetName(string value)
        {
            NormalizedName = value?.ToString().CleanCell();
            Name = value;
        }
        public string NormalizedName { get; private set; }
    }
}
