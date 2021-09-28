using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BiblioMit.Models.Entities.Ads
{
    public class Caption
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Color { get; set; }
        public VerticalPosition Position { get; set; }
        public ICollection<Btn> Btns { get; internal set; }
        public Lang Lang { get; set; }
    }
    public enum Lang
    {
        None,
        en,
        es
    }
    public enum VerticalPosition
    {
        [Display(Name = "Top", Description = "carousel-caption top")]
        Top,
        [Display(Name = "Middle", Description = "")]
        Middle,
        [Display(Name = "Bottom", Description = "carousel-caption")]
        Bottom
    }

}
