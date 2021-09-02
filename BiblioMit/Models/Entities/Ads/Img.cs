namespace BiblioMit.Models.Entities.Ads
{
    public class Img
    {
        public int Id { get; set; }
        public Size Size { get; set; }
        public string FileName { get; set; }
    }
    public enum Size
    {
        None,
        xs = 576,
        sm = 768,
        md = 992,
        lg = 1200,
        xl = 1400,
        xxl = 3800
    }
}
