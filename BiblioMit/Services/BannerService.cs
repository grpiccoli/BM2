using BiblioMit.Data;
using BiblioMit.Extensions;
using BiblioMit.Models.Entities.Ads;
using BiblioMit.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BiblioMit.Services
{
    public class BannerService : IBannerService
    {
        private readonly ApplicationDbContext _context;
        private readonly IStringLocalizer<BannerService> _localizer;
        private const string _carouselId = "introCarousel";
        public BannerService(
            IStringLocalizer<BannerService> localizer,
            ApplicationDbContext context
            )
        {
            _context = context;
            _localizer = localizer;
        }
        public IQueryable<Banner> GetBanners() =>
            _context.Banners
            .Include(b => b.Imgs)
            .Include(b => b.Texts)
                .ThenInclude(b => b.Btns)
            .Include(b => b.Rgbs)
            .Include(b => b.Payments);
        public async Task<List<Banner>> GetBannersAsync() =>
            await GetBanners()
            .ToListAsync()
            .ConfigureAwait(false);
        public async Task<IList<Banner>> GetBannersShuffledAsync() =>
            (await GetBannersAsync().ConfigureAwait(false)).Shuffle();
        public async Task<Carousel> GetCarouselAsync(bool activeOnly, bool shuffle)
        {
            var model = shuffle ? await GetBannersShuffledAsync().ConfigureAwait(false)
                : await GetBannersAsync().ConfigureAwait(false);

            if (activeOnly)
            {
                var active = model.Where(b => b.Active()).ToList();
                if(active.Count != 0)
                {
                    model = active;
                }
            }
            else
            {
                //.Where(b => b.Payments.Any())
                model = model.ToList();
            }

            var modelo = new Carousel();

            foreach (var item in model.Select((value, i) => new { i, value }))
            {
                var mask = "background-color: rgba(0, 0, 0, 0.6)";
                var active = item.i == 0 ? "active" : "";
                var lang = _localizer["en"].Value;
                var text = item.value.Texts.FirstOrDefault(t => t.Lang.ToString() == lang);
                if (text == null) continue;
                modelo.Indicators.Add(GetCarouselButton(item.i,active));
                var btns = "";
                if (item.value.Rgbs != null && item.value.Rgbs.Any())
                {
                    if (item.value.Rgbs.Count > 1 && string.IsNullOrWhiteSpace(item.value.MaskAngle))
                    {
                        var rgbas = string.Join(",", item.value.Rgbs.Select(r => $"rgba({r.R}, {r.G}, {r.B}, 0.6)"));
                        mask = $"background: linear-gradient({item.value.MaskAngle}, {rgbas})";
                    }
                    else
                    {
                        var first = item.value.Rgbs.First();
                        mask = $"background-color: rgba({first.R}, {first.G}, {first.B}, 0.6)";
                    }
                }
                if (text.Btns != null && text.Btns.Any())
                {
                    btns = string.Join("", text.Btns.Select(b => @$"<a id=""{b.Id}"" class=""btn-banner btn btn-outline-light btn-lg m-2"" href=""{b.Uri}""
role=""button"" rel=""nofollow"" target=""_blank"">{b.Title}</a>"));
                }
                mask = $@".banner-{item.i} .mask{{{mask};}}";
                if (!string.IsNullOrWhiteSpace(text.Color))
                    mask += $@"[id=""{text.Id}""]{{color:{text.Color} !important}}";
                modelo.Styles.Add(string.Join(" ", item.value.Imgs.Select(i => $@"@media (max-width: {(int)i.Size}px){{ 
.banner-{item.i}{{background-image: url('../Home/GetBanner?f={i.FileName}');}}}}")) + mask);
                modelo.Items.Add(@$"<div class=""carousel-item banner-{item.i} {active}"">
                     <div class=""mask"">
                           <div class=""d-flex justify-content-center align-items-center h-100"">
                                <div id=""{text.Id}"" data-lang=""{text.Lang}"" data-position=""{text.Position.GetAttrName()}"" class=""caption text-white text-center {text.Position.GetAttrDescription()} d-none d-md-block"">
                                     <h1 class=""mb-3"">{text.Title}</h1>
                                     <h5 class=""mb-4"">{text.Subtitle}</h5>
                                        {btns}
                                </div>
                           </div>
                      </div>
                   </div>");
            }
            return modelo;
        }
        public string GetCarouselButton(int id, string active) => 
            @$"<button type=""button"" data-bs-target=""#{_carouselId}"" data-bs-slide-to=""{id}"" class=""{active}""></button>";
    }
    public class Carousel
    {
        public ICollection<string> Indicators { get; internal set; } = new List<string>();
        public ICollection<string> Items { get; internal set; } = new List<string>();
        public ICollection<string> Styles { get; internal set; } = new List<string>();
    }
}
