using BiblioMit.Models.Entities.Semaforo;
using System.ComponentModel.DataAnnotations;

namespace BiblioMit.Models
{
    public class ReproductiveStage
    {
        public int Id { get; set; }
        public int SpawningId { get; set; }
        public virtual Spawning Spawning { get; set; }
        public Stage Stage { get; set; }
        [Range(0,100)]
        [Display(Description = "%")]
        public int Proportion { get; set; }
    }
}
