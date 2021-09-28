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
    public class VariableTypesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VariableTypesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: VariableTypes
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View(await _context.VariableTypes.ToListAsync().ConfigureAwait(false));
        }

        // GET: VariableTypes/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var variableType = await _context.VariableTypes
                .FirstOrDefaultAsync(m => m.Id == id).ConfigureAwait(false);
            if (variableType == null)
            {
                return NotFound();
            }

            return View(variableType);
        }

        // GET: VariableTypes/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: VariableTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Units")] VariableType variableType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(variableType);
                await _context.SaveChangesAsync().ConfigureAwait(false);
                return RedirectToAction(nameof(Index));
            }
            return View(variableType);
        }

        // GET: VariableTypes/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var variableType = await _context.VariableTypes.FindAsync(id).ConfigureAwait(false);
            if (variableType == null)
            {
                return NotFound();
            }
            return View(variableType);
        }

        // POST: VariableTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Units")] VariableType variableType)
        {
            if (id != variableType?.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(variableType);
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VariableTypeExists(variableType.Id))
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
            return View(variableType);
        }

        // GET: VariableTypes/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var variableType = await _context.VariableTypes
                .FirstOrDefaultAsync(m => m.Id == id).ConfigureAwait(false);
            if (variableType == null)
            {
                return NotFound();
            }

            return View(variableType);
        }

        // POST: VariableTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var variableType = await _context.VariableTypes.FindAsync(id).ConfigureAwait(false);
            _context.VariableTypes.Remove(variableType);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return RedirectToAction(nameof(Index));
        }

        private bool VariableTypeExists(int id)
        {
            return _context.VariableTypes.Any(e => e.Id == id);
        }
    }
}
