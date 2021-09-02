using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BiblioMit.Data;
using Microsoft.AspNetCore.Authorization;
using BiblioMit.Models;

namespace BiblioMit.Controllers
{
    [Authorize]
    public class ExcelsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExcelsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Excels
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View(await _context.InputFiles.Include(e => e.Registries).ToListAsync().ConfigureAwait(false));
        }

        // GET: Excels/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var excel = await _context.InputFiles
                .SingleOrDefaultAsync(m => m.Id == id).ConfigureAwait(false);
            if (excel == null)
            {
                return NotFound();
            }

            return View(excel);
        }

        // GET: Excels/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Excels/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Create([Bind("Id,Name")] InputFile excel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(excel);
                await _context.SaveChangesAsync().ConfigureAwait(false);
                return RedirectToAction(nameof(Index));
            }
            return View(excel);
        }

        // GET: Excels/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var excel = await _context.InputFiles.SingleOrDefaultAsync(m => m.Id == id).ConfigureAwait(false);
            if (excel == null)
            {
                return NotFound();
            }
            return View(excel);
        }

        // POST: Excels/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] InputFile excel)
        {
            if (excel == null || id != excel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(excel);
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExcelExists(excel.Id))
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
            return View(excel);
        }

        // GET: Excels/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var excel = await _context.InputFiles
                .SingleOrDefaultAsync(m => m.Id == id).ConfigureAwait(false);
            if (excel == null)
            {
                return NotFound();
            }

            return View(excel);
        }

        // POST: Excels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var excel = await _context.InputFiles.SingleOrDefaultAsync(m => m.Id == id).ConfigureAwait(false);
            _context.InputFiles.Remove(excel);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return RedirectToAction(nameof(Index));
        }

        private bool ExcelExists(int id)
        {
            return _context.InputFiles.Any(e => e.Id == id);
        }
    }
}
