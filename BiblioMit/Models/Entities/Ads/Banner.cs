using System.Collections.Generic;
using System.Linq;

namespace BiblioMit.Models.Entities.Ads
{
    public class Banner
    {
        public int Id { get; set; }
        public ICollection<Img> Imgs { get; internal set; }
        public string MaskAngle { get; set; }
        public ICollection<Caption> Texts { get; internal set; }
        public ICollection<Rgb> Rgbs { get; internal set; }
        public virtual ICollection<Payment> Payments { get; internal set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
        public bool Active() => Payments.Any() && !Payments.Any(p => p.OverDue());
    }
}
