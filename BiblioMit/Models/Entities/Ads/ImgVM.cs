using Microsoft.AspNetCore.Http;

namespace BiblioMit.Models.Entities.Ads
{
    public class ImgVM
    {
        public Size Size { get; set; }
        public IFormFile FileName { get; set; }
    }
}
