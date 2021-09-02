using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BiblioMit.Data;
using BiblioMit.Models.Entities.Ads;
using Microsoft.AspNetCore.Authorization;
using BiblioMit.Services.Interfaces;

namespace BiblioMit.Controllers
{
    [Authorize(Roles = nameof(RoleData.Administrator), Policy = nameof(UserClaims.Banners))]
    public class BannersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBannerService _banner;

        public BannersController(
            ApplicationDbContext context,
            IBannerService banner)
        {
            _banner = banner;
            _context = context;
        }

        // GET: Banners
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var carousel = await _banner.GetCarouselAsync(false).ConfigureAwait(false);
            return View(carousel);
            //var lang = Request.Cookies[".AspNetCore.Culture"][^2..];
            //var banners = await _context.Banners
            //    .Include(b => b.Imgs)
            //    .Include(b => b.Texts)
            //        .ThenInclude(b => b.Btns)
            //    .Include(b => b.Rgbs)
            //    .ToListAsync().ConfigureAwait(false);
            //return View((banners, lang));
        }

        // GET: Banners/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var banner = await _context.Banners
                .FirstOrDefaultAsync(m => m.Id == id).ConfigureAwait(false);
            if (banner == null)
            {
                return NotFound();
            }

            return View(banner);
        }

        // GET: Banners/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Banners/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,MaskAngle")] Banner banner)
        {
            if (ModelState.IsValid)
            {
                _context.Add(banner);
                await _context.SaveChangesAsync().ConfigureAwait(false);
                return RedirectToAction(nameof(Index));
            }
            return View(banner);
        }

        // GET: Banners/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var banner = await _context.Banners.FindAsync(id).ConfigureAwait(false);
            if (banner == null)
            {
                return NotFound();
            }
            return View(banner);
        }

        // POST: Banners/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MaskAngle")] Banner banner)
        {
            if (banner is null) throw new ArgumentNullException(nameof(banner));
            if (id != banner.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(banner);
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BannerExists(banner.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(banner);
        }

        // GET: Banners/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var banner = await _context.Banners
                .FirstOrDefaultAsync(m => m.Id == id).ConfigureAwait(false);
            if (banner == null)
            {
                return NotFound();
            }

            return View(banner);
        }

        // POST: Banners/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var banner = await _context.Banners.FindAsync(id).ConfigureAwait(false);
            _context.Banners.Remove(banner);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return RedirectToAction(nameof(Index));
        }

        private bool BannerExists(int id)
        {
            return _context.Banners.Any(e => e.Id == id);
        }
    }
}
