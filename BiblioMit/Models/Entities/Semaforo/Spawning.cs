using BiblioMit.Models.Entities.Centres;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BiblioMit.Models
{
    public class Spawning
    {
        public int Id { get; set; }
        public int FarmId { get; set; }
        public virtual Farm Farm { get; set; }
        public DateTime Date { get; set; }
        [Range(0,100)]
        [Display(Description = "%")]
        public int MaleProportion { get; set; }
        [Range(0, 100)]
        [Display(Description = "%")]
        public int FemaleProportion { get; set; }
        [Display(Description = "%")]
        public double MaleIG { get; set; }
        [Display(Description = "%")]
        public double FemaleIG { get; set; }
        public virtual ICollection<ReproductiveStage> Stage { get; } = new List<ReproductiveStage>();
    }
}
