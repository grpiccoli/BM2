using System.ComponentModel.DataAnnotations;

namespace BiblioMit.Models
{
    public class Phytoplankton
    {
        public int PlanktonAssayId { get; set; }
        public virtual PlanktonAssay PlanktonAssay { get; set; }
        public int SpeciesId { get; set; }
        public virtual SpeciesPhytoplankton Species { get; set; }
        [Display(Name = "Relative Abundance Scale", ShortName = "E.A.R.")]
        public Ear? EAR { get; set; }
        [Display(Name = "Toxicity concentration", Description = "cel/mL", ShortName = "C.")]
        public double C { get; set; }
    }
}
