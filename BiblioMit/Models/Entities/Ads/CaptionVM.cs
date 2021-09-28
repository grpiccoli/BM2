using System.Drawing;

namespace BiblioMit.Models.Entities.Ads
{
    public class CaptionVM
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public Color Color { get; set; }
        public VerticalPosition Position { get; set; }
        public Lang Lang { get; set; }
    }
}
