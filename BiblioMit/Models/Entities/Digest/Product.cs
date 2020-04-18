using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BiblioMit.Models
{
    public class Product
    {
        [Display(Name = "Product")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string NormalizedName { get; set; }
        public virtual ICollection<PlantProduct> Plants { get; } = new List<PlantProduct>();
    }
}
