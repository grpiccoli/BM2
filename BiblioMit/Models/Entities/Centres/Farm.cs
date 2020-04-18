using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BiblioMit.Models.Entities.Centres
{
    public class Farm : Psmb
    {
        [Display(Name = "PSMB area")]
        public int? PsmbAreaId { get; set; }
        public virtual PsmbArea PsmbArea { get; set; }
        [Display(Name = "Invoice number", Description = "National Registry of Aquaculture (RNA)")]
        public int? RnaInvoice { get; set; }
        public virtual ICollection<Larvae> Larvaes { get; } = new List<Larvae>();
        public virtual ICollection<Seed> Seeds { get; } = new List<Seed>();
        public virtual ICollection<Spawning> Spawnings { get; } = new List<Spawning>();
        public virtual ICollection<Analysis> Analyses { get; } = new List<Analysis>();
    }
}
