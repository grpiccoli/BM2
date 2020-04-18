using System.ComponentModel.DataAnnotations;

namespace BiblioMit.Models
{
    public enum ContactStatus
    {
        [Display(Name = "Submitted")]
        Submitted,
        [Display(Name = "Approved")]
        Approved,
        [Display(Name = "Rejected")]
        Rejected
    }
}
