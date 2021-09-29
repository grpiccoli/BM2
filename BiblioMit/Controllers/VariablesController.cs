using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BiblioMit.Data;
using BiblioMit.Models.Entities.Variables;

namespace BiblioMit.Controllers
{
    public class VariablesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VariablesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Variables
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Variables.Include(v => v.Psmb).Include(v => v.VariableType);
            return View(await applicationDbContext.ToListAsync().ConfigureAwait(false));
        }

        // GET: Variables/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var variable = await _context.Variables
                .Include(v => v.Psmb)
                .Include(v => v.VariableType)
                .FirstOrDefaultAsync(m => m.Id == id).ConfigureAwait(false);
            if (variable == null)
            {
                return NotFound();
            }

            return View(variable);
        }

        // GET: Variables/Create
        [HttpGet]
        public IActionResult Create()
        {
            ViewData["PsmbId"] = new SelectList(_context.Psmbs, "Id", "Id");
            ViewData["VariableTypeId"] = new SelectList(_context.VariableTypes, "Id", "Id");
            return View();
        }

        // POST: Variables/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,PsmbId,VariableTypeId,Value,Date")] Variable variable)
        {
            if (ModelState.IsValid)
            {
                _context.Add(variable);
                await _context.SaveChangesAsync().ConfigureAwait(false);
                return RedirectToAction(nameof(Index));
            }
            ViewData["PsmbId"] = new SelectList(_context.Psmbs, "Id", "Id", variable?.PsmbId);
            ViewData["VariableTypeId"] = new SelectList(_context.VariableTypes, "Id", "Id", variable.VariableTypeId);
            return View(variable);
        }

        // GET: Variables/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var variable = await _context.Variables.FindAsync(id).ConfigureAwait(false);
            if (variable == null)
            {
                return NotFound();
            }
            ViewData["PsmbId"] = new SelectList(_context.Psmbs, "Id", "Id", variable.PsmbId);
            ViewData["VariableTypeId"] = new SelectList(_context.VariableTypes, "Id", "Id", variable.VariableTypeId);
            return View(variable);
        }

        // POST: Variables/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PsmbId,VariableTypeId,Value,Date")] Variable variable)
        {
            if (id != variable?.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(variable);
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VariableExists(variable.Id))
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
            ViewData["PsmbId"] = new SelectList(_context.Psmbs, "Id", "Id", variable.PsmbId);
            ViewData["VariableTypeId"] = new SelectList(_context.VariableTypes, "Id", "Id", variable.VariableTypeId);
            return View(variable);
        }

        // GET: Variables/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var variable = await _context.Variables
                .Include(v => v.Psmb)
                .Include(v => v.VariableType)
                .FirstOrDefaultAsync(m => m.Id == id).ConfigureAwait(false);
            if (variable == null)
            {
                return NotFound();
            }

            return View(variable);
        }

        // POST: Variables/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var variable = await _context.Variables.FindAsync(id).ConfigureAwait(false);
            _context.Variables.Remove(variable);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return RedirectToAction(nameof(Index));
        }

        private bool VariableExists(int id)
        {
            return _context.Variables.Any(e => e.Id == id);
        }
    }
}
