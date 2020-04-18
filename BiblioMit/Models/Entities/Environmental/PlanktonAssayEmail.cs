using System.ComponentModel.DataAnnotations;

namespace BiblioMit.Models
{
    public class PlanktonAssayEmail : Indexed
    {
        [Required]
        public int EmailId { get; set; }
        public virtual Email Email { get; set; }
        [Required]
        public int PlanktonAssayId { get; set; }
        public virtual PlanktonAssay PlanktonAssay { get; set; }
    }
}
