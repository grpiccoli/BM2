using System;

namespace BiblioMit.Models
{
    public class Census
    {
        public int Id { get; set; }
        public DateTime Year { get; set; }
        public int Count { get; set; }
        public int LocationId { get; set; }
        public virtual Locality Locality { get; set; }
    }
}
