using System;
using System.Collections.Generic;

namespace BiblioMit.Models.Entities.Environmental.Plancton
{
    public class PlanktonUser
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public virtual ICollection<PlanktonAssay> Assays { get; } = new List<PlanktonAssay>();
    }
}
