using BiblioMit.Models.Entities.Centres;
using System;
using System.Collections.Generic;

namespace BiblioMit.Models
{
    public class Seed
    {
        public int Id { get; set; }
        public int FarmId { get; set; }
        public virtual Farm Farm { get; set; }
        public DateTime Date { get; set; }
        public DateTime DateCuelga { get; set; }
        public virtual ICollection<SpecieSeed> Specie { get; } = new List<SpecieSeed>();
    }
}
