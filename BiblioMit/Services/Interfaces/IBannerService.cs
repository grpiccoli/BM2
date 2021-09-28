using BiblioMit.Models.Entities.Ads;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BiblioMit.Services.Interfaces
{
    public interface IBannerService
    {
        IQueryable<Banner> GetBanners();
        Task<List<Banner>> GetBannersAsync();
        Task<IList<Banner>> GetBannersShuffledAsync();
        Task<Carousel> GetCarouselAsync(bool activeOnly = true, bool shuffle = true);
    }
}
