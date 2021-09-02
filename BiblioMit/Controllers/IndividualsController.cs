using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BiblioMit.Data;
using BiblioMit.Models;
using System.Linq;
using System.Threading.Tasks;

namespace BiblioMit.Controllers
{
    [Authorize]
    public class IndividualsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public IndividualsController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Samplings
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var individuals = await _context.Individuals
                    .Include(s => s.Sampling)
                        .ThenInclude(s => s.Centre)
                .ToListAsync().ConfigureAwait(false);
            return View(individuals);
        }

        // GET: Samplings/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var individual = await _context.Individuals
                .Include(s => s.Sampling)
                .SingleOrDefaultAsync(m => m.Id == id).ConfigureAwait(false);
            if (individual == null)
            {
                return NotFound();
            }

            return View(individual);
        }

        // GET: Samplings/Create
        [HttpGet]
        public IActionResult Create()
        {
            ViewData["SamplingId"] = new SelectList(_context.Samplings, "Id", "Id");
            return View();
        }

        // POST: Samplings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Create([Bind("Id,SamplingId,Sex,Maturity")] Individual individual)
        {
            if (individual == null) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Add(individual);
                await _context.SaveChangesAsync().ConfigureAwait(false);
                return RedirectToAction("Index");
            }
            ViewData["SamplingId"] = new SelectList(_context.Samplings, "Id", "Id", individual.SamplingId);
            return View(individual);
        }

        // GET: Samplings/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var individual = await _context.Individuals.SingleOrDefaultAsync(m => m.Id == id).ConfigureAwait(false);
            if (individual == null)
            {
                return NotFound();
            }
            ViewData["SamplingId"] = new SelectList(_context.Samplings, "Id", "Id", individual.SamplingId);
            return View(individual);
        }

        // POST: Samplings/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Edit(int id, [Bind("Id,SamplingId,Sex,Maturity")] Individual individual)
        {
            if (individual == null || id != individual.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(individual);
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IndividualExists(individual.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }
            ViewData["SamplingId"] = new SelectList(_context.Samplings, "Id", "Id", individual.SamplingId);
            return View(individual);
        }

        // GET: Samplings/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var individual = await _context.Individuals
                .Include(s => s.Sampling)
                .SingleOrDefaultAsync(m => m.Id == id).ConfigureAwait(false);
            if (individual == null)
            {
                return NotFound();
            }

            return View(individual);
        }

        // POST: Samplings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var individual = await _context.Individuals.SingleOrDefaultAsync(m => m.Id == id).ConfigureAwait(false);
            _context.Individuals.Remove(individual);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return RedirectToAction("Index");
        }

        private bool IndividualExists(int id)
        {
            return _context.Individuals.Any(e => e.Id == id);
        }


    }
}