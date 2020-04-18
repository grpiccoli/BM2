using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BiblioMit.Models
{
    public class Email : Indexed
    {
        public int Id { get; set; }
        [Required]
        public string Address { get; set; }
        public virtual ICollection<PlanktonAssayEmail> PlanktonAssayEmails { get; } = new List<PlanktonAssayEmail>();
    }
}
