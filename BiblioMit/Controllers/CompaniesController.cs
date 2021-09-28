using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BiblioMit.Data;
using BiblioMit.Models;
using Microsoft.AspNetCore.Authorization;
using System;
using BiblioMit.Services;
using System.Globalization;
using BiblioMit.Extensions;

namespace BiblioMit.Controllers
{
    [Authorize(Policy = nameof(UserClaims.Institutions))]
    public class CompaniesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CompaniesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Companies
        [Authorize(Roles = nameof(RoleData.Administrator))]
        [Authorize(Roles = nameof(RoleData.Editor))]
        [Authorize(Roles = nameof(RoleData.Guest))]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Companies
                .Include(c => c.Psmbs)
                    .ThenInclude(c => c.Polygon)
                .AsNoTracking()
                .ToListAsync().ConfigureAwait(false));
        }

        // GET: Companies/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var company = await _context.Companies
                .Include(c => c.Psmbs)
                    .ThenInclude(c => c.Contacts)
                .Include(c => c.Psmbs)
                    .ThenInclude(c => c.Commune)
                        .ThenInclude(c => c.Province)
                            .ThenInclude(c => c.Region)
                .SingleOrDefaultAsync(m => m.Id == id).ConfigureAwait(false);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        // GET: Companies/Create
        [Authorize(Roles = nameof(RoleData.Administrator))]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Companies/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [Authorize(Roles = nameof(RoleData.Administrator))]
        public async Task<IActionResult> Create([Bind("Id,BsnssName,Acronym")] CompanyViewModel company)
        {
            if (company == null) return NotFound();
            if (ModelState.IsValid)
            {
                var rut = company.RUT.RUTUnformat();
                if (rut.HasValue)
                {
                    Company corp = new()
                    {
                        Id = rut.Value.rut
                    };
                    corp.SetBusinessName(company.BsnssName);
                    corp.SetAcronym(company.Acronym);
                    _context.Add(corp);
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                    return RedirectToAction("Index");
                }
            }
            return View(company);
        }

        // GET: Companies/Edit/5
        [Authorize(Roles = nameof(RoleData.Administrator))]
        [Authorize(Roles = nameof(RoleData.Editor))]
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var company = await _context.Companies.SingleOrDefaultAsync(m => m.Id == id).ConfigureAwait(false);
            if (company == null)
            {
                return NotFound();
            }
            var comp = new CompanyViewModel
            {
                RUT = company.Id.RUTFormat(),
                BsnssName = company.BusinessName
            };
            return View(comp);
        }

        // POST: Companies/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [Authorize(Roles = nameof(RoleData.Administrator))]
        [Authorize(Roles = nameof(RoleData.Editor))]
        public async Task<IActionResult> Edit(int id, [Bind("Id,BsnssName,Acronym")] Company company)
        {
            if (company == null || id != company.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(company);
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompanyExists(company.Id))
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
            return View(company);
        }

        // GET: Companies/Delete/5
        [Authorize(Roles = nameof(RoleData.Administrator))]
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var company = await _context.Companies
                .SingleOrDefaultAsync(m => m.Id == id).ConfigureAwait(false);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        // POST: Companies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [Authorize(Roles = nameof(RoleData.Administrator))]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var company = await _context.Companies.SingleOrDefaultAsync(m => m.Id == id).ConfigureAwait(false);
            _context.Companies.Remove(company);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return RedirectToAction("Index");
        }

        private bool CompanyExists(int id)
        {
            return _context.Companies.Any(e => e.Id == id);
        }
    }
}
