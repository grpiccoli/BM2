using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BiblioMit.Data;
using BiblioMit.Models.Entities.Ads;
using Microsoft.AspNetCore.Authorization;
using BiblioMit.Services.Interfaces;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Drawing;

namespace BiblioMit.Controllers
{
    [Authorize(Roles = nameof(RoleData.Administrator), Policy = nameof(UserClaims.Banners))]
    public class BannersController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ApplicationDbContext _context;
        private readonly IBannerService _banner;

        public BannersController(
            ApplicationDbContext context,
            IWebHostEnvironment environment,
            IBannerService banner)
        {
            _banner = banner;
            _environment = environment;
            _context = context;
        }

        // GET: Banners
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var carousel = await _banner.GetCarouselAsync(false, false).ConfigureAwait(false);
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
                return NotFound();

            var banner = await _context.Banners
                .FirstOrDefaultAsync(m => m.Id == id).ConfigureAwait(false);
            if (banner == null)
                return NotFound();

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
                return NotFound();

            var banner = await _context.Banners.FindAsync(id).ConfigureAwait(false);
            if (banner == null)
                return NotFound();
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
                return NotFound();

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
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(banner);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCaption(int id, [Bind("Id,Title,Subtitle,Position,Lang,Color")] CaptionVM caption)
        {
            if (caption is null) throw new ArgumentNullException(nameof(caption));
            if (id != caption.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var newCaption = new Caption
                    {
                        Id = caption.Id,
                        Title = caption.Title,
                        Subtitle = caption.Subtitle,
                        Position = caption.Position,
                        Lang = caption.Lang,
                        Color = ColorTranslator.ToHtml(caption.Color)
                    };
                    _context.Update(newCaption);
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BannerExists(caption.Id))
                        return NotFound();
                    else
                        throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBtn(int id, [Bind("Id,Title,Uri")] Btn btn)
        {
            if (btn is null) throw new ArgumentNullException(nameof(btn));
            if (id != btn.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(btn);
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BannerExists(btn.Id))
                        return NotFound();
                    else
                        throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        private string UploadedFile(ImgVM model)
        {
            string uniqueFileName = null;

            if (model.FileName != null)
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "BannerImgs");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.FileName.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using var fileStream = new FileStream(filePath, FileMode.Create);
                model.FileName.CopyTo(fileStream);
            }
            return uniqueFileName;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddImg([Bind("Size,FileName")] ImgVM img)
        {
            if (img is null) throw new ArgumentNullException(nameof(img));
            if (ModelState.IsValid)
            {
                try
                {
                    string uniqueFileName = UploadedFile(img);

                    var edit = _context.Imgs.FirstOrDefault(i => i.Size == img.Size);
                    if(edit != null)
                    {
                        edit.FileName = uniqueFileName;
                        _context.Update(edit);
                    }
                    else
                    {
                        Img newimg = new()
                        {
                            Size = img.Size,
                            FileName = uniqueFileName
                        };
                        _context.Imgs.Add(newimg);
                    }
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        // GET: Banners/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var banner = await _context.Banners
                .FirstOrDefaultAsync(m => m.Id == id).ConfigureAwait(false);
            if (banner == null)
                return NotFound();

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
