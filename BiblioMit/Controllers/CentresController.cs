﻿using BiblioMit.Authorization;
using BiblioMit.Data;
using BiblioMit.Models;
using BiblioMit.Models.Entities.Centres;
using BiblioMit.Models.VM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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
        public IActionResult GetMap(PsmbType type, int[] c, int[] i)
        {
            if (c == null || i == null) throw new ArgumentNullException($"Argument c {c} and i {i} cannot be null");
            var list = type switch
            {
                PsmbType.Farm => GetEntitiesAsync<Farm>(c, i),
                PsmbType.ResearchCentre => GetEntitiesAsync<ResearchCentre>(c, i),
                _ => GetEntitiesAsync<Psmb>(c, i)
            };
            return Json(list.Select(f =>
                    new GMapPolygonCentre
                    {
                        Id = f.Code,
                        Name = f.Name,
                        BusinessName = f.Company.BusinessName,
                        Rut = f.Company.Id,
                        Comuna = f.Commune.Name,
                        ComunaId = f.CommuneId.Value,
                        Provincia = f.Commune.Province.Name,
                        Region = f.Commune.Province.Region.Name,
                        Code = f.Code,
                        Position = f.Polygon.Vertices.OrderBy(o => o.Order).Select(o =>
                        new GMapCoordinate
                        {
                            Lat = o.Latitude,
                            Lng = o.Longitude
                        })
                    }));
        }
        private IEnumerable<Psmb> GetPsmbs<TEntity>(int[] c, int[] i) 
            where TEntity : Psmb
        {
            ViewData["c"] = string.Join(",", c);
            ViewData["i"] = string.Join(",", i);
            return GetEntitiesAsync<TEntity>(c, i);
        }
        private IEnumerable<TEntity> GetEntitiesAsync<TEntity>(int[] c, int[] i)
            where TEntity : Psmb
        {
            var centres = _context.Set<TEntity>()
                        .Where(a => a.PolygonId.HasValue
                && a.CommuneId.HasValue
                && a.CompanyId.HasValue);
            //if (map) centres = centres.Include(a => a.Polygon).ThenInclude(a => a.Vertices);
            //var list = await centres.ToListAsync().ConfigureAwait(false);
            foreach(var sc in c)
            {
                centres = centres.Where(a => a.CommuneId.Value == sc);
            }
            foreach (var si in i)
            {
                centres = centres.Where(a => a.CommuneId.Value == si);
            }
            return centres;
        }
        public IActionResult Producers(int[] c, int[] i) => View(GetPsmbs<Farm>(c, i));
        // GET: Centres
        [AllowAnonymous]
        public IActionResult Research(int[] c, int[] i) => View(GetPsmbs<ResearchCentre>(c, i));
        // GET: Centres/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var centre = await _context.Farms
                .Include(c => c.Company)
                .Include(c => c.Polygon)
                .Include(c => c.Contacts)
                .SingleOrDefaultAsync(m => m.Id == id).ConfigureAwait(false);
            if (centre == null) return NotFound();
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
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
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
            if (!isAuthorized) return RedirectToAction("AccessDenied", "Account");
            if (id == null) return NotFound();
            var centre = await _context.Farms.SingleOrDefaultAsync(m => m.Id == id).ConfigureAwait(false);
            if (centre == null) return NotFound();
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
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [Authorize(Roles = "Editor,Administrador", Policy = "Centros")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ComunaId,Type,Url,Acronym,CompanyId,Name,Address")] ResearchCentre centre)
        {
            var isAuthorized = User.IsInRole(Constants.ContactAdministratorsRole);
            if (!isAuthorized) return RedirectToAction("AccessDenied", "Account");
            if (centre == null || id != centre.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(centre);
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CentreExists(centre.Id)) return NotFound();
                    else throw;
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
            if (!isAuthorized) return RedirectToAction("AccessDenied", "Account");
            if (id == null) return NotFound();
            var centre = await _context.Farms
                .Include(c => c.Company)
                .Include(c => c.Commune)
                    .ThenInclude(c => c.Province)
                    .ThenInclude(c => c.Region)
                .SingleOrDefaultAsync(m => m.Id == id).ConfigureAwait(false);
            if (centre == null) return NotFound();
            return View(centre);
        }

        // POST: Centres/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [Authorize(Roles = "Administrador", Policy = "Centros")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var isAuthorized = User.IsInRole(Constants.ContactAdministratorsRole);
            if (!isAuthorized) return RedirectToAction("AccessDenied", "Account");
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
