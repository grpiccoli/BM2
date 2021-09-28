using BiblioMit.Models.Entities.Histopathology;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BiblioMit.Models
{
    public class Individual
    {
        //Ids
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Display(Name = "No")]
        public int Id { get; set; }
        [Display(Name = "Sample Id")]
        public int SamplingId { get; set; }
        //Parent
        [Display(Name = "Sample")]
        public virtual Sampling Sampling { get; set; }
        //ATT
        [Display(Name = "Sex")]
        public Sex Sex { get; set; }
        [Display(Name = "Maturity")]
        public Maturity Maturity { get; set; }
        [Display(Name = "Length (mm)")]
        public int Length { get; set; }
        [Display(Name = "Comment")]
        public string Comment { get; set; }
        [Display(Name = "Number")]
        public int Number { get; set; }
        [Display(Name = "Tag")]
        public string Tag { get; set; }
        [Display(Name = "Depth")]
        public int? Depth { get; set; }
        [Display(Name = "Adipogranular cells")]
        public Adg? ADG { get; set; }
        //CHILD
        [Display(Name = "Valves")]
        public virtual ICollection<Valve> Valves { get; } = new List<Valve>();
        [Display(Name = "Soft Tissue")]
        public virtual ICollection<Soft> Softs { get; } = new List<Soft>();
        public virtual ICollection<Photo> Photos { get; } = new List<Photo>();
    }
}
