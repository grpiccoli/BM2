using BiblioMit.Authorization;
using BiblioMit.Data;
using BiblioMit.Extensions;
using BiblioMit.Models;
using BiblioMit.Models.Entities.Centres;
using BiblioMit.Models.VM.MapsVM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace BiblioMit.Controllers
{
    [Authorize]
    public class CentresController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CentresController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Centres
        [Authorize(Roles = "Invitado,Editor,Administrator",Policy ="Centros")]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Psmbs
                .Include(c => c.Company)
                .Include(c => c.Commune)
                    .ThenInclude(c => c.Province)
                    .ThenInclude(c => c.Region);
            return View(await applicationDbContext.ToListAsync().ConfigureAwait(false));
        }

        // GET: Centres
        public async Task<IActionResult> Centres(int[] i, int[] c, bool? r)
        {
            if (!r.HasValue) r = false;
            var comunas = _context.Communes
                .Include(o => o.Province)
                    .ThenInclude(o => o.Region)
                .Include(o => o.Psmbs)
                .Where(o => o.Psmbs.Any() && o.Id != 0);
            ViewData["comunas"] = comunas;
            var companies = r.Value ? 
                _context.Companies
                .Where(o => o.Acronym != null) :
                _context.Companies
                .Where(o => o.Acronym == null);
            ViewData["company"] = companies;
            ViewData["c"] = c;
            ViewData["i"] = i;
            ViewData["r"] = r.Value;
            ViewData["Title"] = r.Value ? "Centros I+D" : "Productores";
            ViewData["Main"] = r.Value ? "Centros de Investigación, Tecnología y desarrollo" : "Compañías Mitilicultoras";
            var model = await _context.Psmbs
                .Include(o => o.Company)
                .Include(o => o.Polygon)
                .Include(o => o.Commune)
                    .ThenInclude(o => o.Province)
                    .ThenInclude(o => o.Region)
                .Where(o =>
                o.Name == null &&
                c.Any() ? c.ToString().Contains(Convert.ToString(o.CommuneId, CultureInfo.InvariantCulture),
                StringComparison.InvariantCultureIgnoreCase) : true &&
                i.Any() ? i.ToString().Contains(Convert.ToString(o.CompanyId, CultureInfo.InvariantCulture),
                StringComparison.InvariantCultureIgnoreCase) : true)
                .ToListAsync().ConfigureAwait(false);

            return View(model);
        }

        public IActionResult Producers(int[] c, int[] i)
        {
            var selc = c.ToList();
            var seli = i.ToList();

            ViewData["comunas"] = from Commune u in _context.Communes
                .Include(a => a.Psmbs)
                .Include(a => a.Province)
                    .ThenInclude(a => a.Region)
                .Where(a => a.Psmbs.Any(b => b.Discriminator == PsmbType.Farm))
                    select new BSSVM {
                        Selected = selc.Contains(u.Id),
                        Subtext = u.Province.GetFullName(),
                        Value = u.Id,
                        Text = u.Name,
                        Tokens = string.Join(" ",u.Psmbs.Where(p => p.Discriminator == PsmbType.Farm)
                        .Select(k => k.Address))
                    };

            TextInfo textInfo = new CultureInfo("es-CL", false).TextInfo;

            ViewData["company"] = from Company u in _context.Companies
                .Where(a => a.Acronym != null)
                                  select new BSSVM
                                  {
                                      Tokens = u.BusinessName + string.Join(" ", u.Psmbs.Select(k => k.Address)),
                                      Selected = seli.Contains(u.Id),
                                      Subtext =
                                      $"({u.Acronym}) {u.Id}-{u.Id.RUTGetDigit()}",
                                      Value = u.Id,
                                      Text = textInfo.ToTitleCase(textInfo
                                      .ToLower(u.BusinessName.Substring(0, Math.Min(u.BusinessName.Length, 50)))),
                                      Hellip = u.BusinessName.Length > 50
                                  };

            ViewData["c"] = string.Join(",", c);
            ViewData["i"] = string.Join(",", i);

            var centres = _context.Farms
                .Include(a => a.Polygon)
                .Include(a => a.Company)
                .Include(a => a.Samplings)
                .Include(a => a.Commune)
                    .ThenInclude(a => a.Province)
                    .ThenInclude(a => a.Region)
                .Where(
                a => a.PolygonId.HasValue
                && selc.Any() && a.CommuneId.HasValue ? selc.Contains(a.CommuneId.Value) : true
                && seli.Any() && a.CompanyId.HasValue ? seli.Contains(a.CompanyId.Value) : true
                );

            return View(centres);
        }

        // GET: Centres
        [AllowAnonymous]
        public IActionResult Research(int[] c, int[] i)
        {
            var selc = c.ToList();
            var seli = i.ToList();

            ViewData["comunas"] = from Commune u in _context.Communes
                                                  .Include(a => a.Psmbs)
                                                  .Include(a => a.Province)
                                                  .ThenInclude(a => a.Region)
                                                  .Where(a => a.Psmbs.Any(b => b.Discriminator == PsmbType.ResearchCentre))
                          select new BSSVM {
                              Selected = selc.Contains(u.Id),
                              Subtext = u.Province.GetFullName(),
                              Value = u.Id,
                              Text = u.Name
                          };

            TextInfo textInfo = new CultureInfo("es-CL", false).TextInfo;

            ViewData["company"] = from Company u in _context.Companies
                .Where(a => a.Acronym != null)
                                  select new BSSVM {
                                      Icon = $"bib-{u.Acronym}-mono",
                                      Tokens = u.BusinessName,
                                      Selected = seli.Contains(u.Id),
                                      Subtext =
                                      $"({u.Acronym}) {u.Id}-{u.Id.RUTGetDigit()}",
                                      Value = u.Id,
                                      Text = textInfo.ToTitleCase(textInfo
                                      .ToLower(u.BusinessName.Substring(0, Math.Min(u.BusinessName.Length, 50)))),
                                      Hellip = u.BusinessName.Length > 50
                                    };

            ViewData["c"] = string.Join(",",c);
            ViewData["i"] = string.Join(",",i);

            var centres = _context.ResearchCentres
                .Include(a => a.Polygon)
                .Include(a => a.Company)
                .Include(a => a.Commune)
                    .ThenInclude(a => a.Province)
                    .ThenInclude(a => a.Region)
                .Where(a =>
                a.PolygonId.HasValue
                && selc.Any() && a.CommuneId.HasValue ? selc.Contains(a.CommuneId.Value) : true
                && seli.Any() && a.CompanyId.HasValue ? seli.Contains(a.CompanyId.Value) : true
                );

            var polygons = new List<object>();

            foreach (var g in centres)
            {
                polygons.Add(g.Polygon.Vertices.OrderBy(o => o.Order).Select(m => 
                new {
                    lat = m.Latitude,
                    lng = m.Longitude
                }));
            }



            return View(centres);
        }

        // GET: Centres/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var centre = await _context.Farms
                .Include(c => c.Company)
                .Include(c => c.Polygon)
                .Include(c => c.Contacts)
                .SingleOrDefaultAsync(m => m.Id == id).ConfigureAwait(false);
            if (centre == null)
            {
                return NotFound();
            }

            return View(centre);
        }

        // GET: Centres/Create
        [Authorize(Roles = "Administrador", Policy = "Centros")]
        public IActionResult Create()
        {
            var isAuthorized = User.IsInRole(Constants.ContactAdministratorsRole);

            if (!isAuthorized)
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            ViewData["ComunaId"] = new SelectList(_context.Communes, "Id", "Name");
            //var values = from CentreType e in Enum.GetValues(typeof(CentreType))
            //             select new { Id = e, Name = e.ToString() };
            //ViewData["Type"] = new SelectList(values, "Id", "Name");

            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Id");
            return View();
        }

        // POST: Centres/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador", Policy = "Centros")]
        public async Task<IActionResult> Create([Bind("Id,ComunaId,Type,Url,Acronym,CompanyId,Name,Address")] ResearchCentre centre)
        {
            if (centre == null) return NotFound();

            var isAuthorized = User.IsInRole(Constants.ContactAdministratorsRole);

            if(!isAuthorized)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (ModelState.IsValid)
            {
                _context.Add(centre);
                await _context.SaveChangesAsync().ConfigureAwait(false);
                return RedirectToAction("Index");
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Id", centre.CompanyId);
            return View(centre);
        }

        // GET: Centres/Edit/5
        [Authorize(Roles = "Editor,Administrador", Policy = "Centros")]
        public async Task<IActionResult> Edit(int? id)
        {
            var isAuthorized = User.IsInRole(Constants.ContactAdministratorsRole);

            if (!isAuthorized)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (id == null)
            {
                return NotFound();
            }

            var centre = await _context.Farms.SingleOrDefaultAsync(m => m.Id == id).ConfigureAwait(false);
            if (centre == null)
            {
                return NotFound();
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Id", centre.CompanyId);
            ViewData["ComunaId"] = new SelectList(_context.Communes, "Id", "Name", centre.CommuneId);
            //var values = from CentreType e in Enum.GetValues(typeof(CentreType))
            //             select new { Id = e, Name = e.ToString() };
            //ViewData["Type"] = new SelectList(values, "Id", "Name", centre.Type);
            return View(centre);
        }

        // POST: Centres/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Editor,Administrador", Policy = "Centros")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ComunaId,Type,Url,Acronym,CompanyId,Name,Address")] ResearchCentre centre)
        {
            var isAuthorized = User.IsInRole(Constants.ContactAdministratorsRole);

            if (!isAuthorized)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (centre == null || id != centre.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(centre);
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CentreExists(centre.Id))
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
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Id", centre.CompanyId);
            return View(centre);
        }

        // GET: Centres/Delete/5
        [Authorize(Roles = "Administrador", Policy = "Centros")]
        public async Task<IActionResult> Delete(int? id)
        {
            var isAuthorized = User.IsInRole(Constants.ContactAdministratorsRole);

            if (!isAuthorized)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (id == null)
            {
                return NotFound();
            }

            var centre = await _context.Farms
                .Include(c => c.Company)
                .Include(c => c.Commune)
                    .ThenInclude(c => c.Province)
                    .ThenInclude(c => c.Region)
                .SingleOrDefaultAsync(m => m.Id == id).ConfigureAwait(false);
            if (centre == null)
            {
                return NotFound();
            }

            return View(centre);
        }

        // POST: Centres/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador", Policy = "Centros")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var isAuthorized = User.IsInRole(Constants.ContactAdministratorsRole);

            if (!isAuthorized)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var centre = await _context.Farms.SingleOrDefaultAsync(m => m.Id == id).ConfigureAwait(false);
            _context.Farms.Remove(centre);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return RedirectToAction("Index");
        }

        private bool CentreExists(int id)
        {
            return _context.Farms.Any(e => e.Id == id);
        }
    }
}
