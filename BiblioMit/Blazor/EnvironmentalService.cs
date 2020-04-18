using BiblioMit.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace BiblioMit.Blazor
{
    public class EnvironmentalService : IEnvironmental
    {
        private readonly ApplicationDbContext _context;
        public EnvironmentalService(ApplicationDbContext context) {
            _context = context;
        }
        public DateTime GetMinDate() =>
            _context.PlanktonAssays.Min(e => e.SamplingDate);
        public DateTime GetMaxDate() =>
            _context.PlanktonAssays.Max(e => e.SamplingDate);
        public IEnumerable<SelectListItem> GetPhytoplanktonOrders() =>
            _context.PhylogeneticGroups
            .OrderBy(s => s.NormalizedName)
            .Select(p => new SelectListItem
            {
                Value = p.Id.ToString(CultureInfo.InvariantCulture),
                Text = $"{p.Name} Totales"
            });
        public IEnumerable<SelectListItem> GetPhytoplanktonGenus() =>
            _context.GenusPhytoplanktons
            .OrderBy(s => s.NormalizedName)
            .Select(p => new SelectListItem
            {
                Value = p.Id.ToString(CultureInfo.InvariantCulture),
                Text = p.Name
            });
        public IEnumerable<SelectListItem> GetPhytoplanktonSp() =>
            _context.SpeciesPhytoplanktons
            .Include(s => s.Genus)
            .Where(s => s.Name != null)
            .OrderBy(s => s.Genus.NormalizedName)
            .Select(p => new SelectListItem
            {
                Value = p.Id.ToString(CultureInfo.InvariantCulture),
                Text = $"{p.Genus.Name} {p.Name}"
            });
        public IEnumerable<SelectListItem> GetCommunes() =>
            _context.Communes
            .Include(c => c.Province)
                .ThenInclude(p => p.Region)
            .Where(s => s.CatchmentAreaId.HasValue)
            .OrderBy(s => s.NormalizedName)
            .Select(p => new SelectListItem
            {
                Value = p.Id.ToString(CultureInfo.InvariantCulture),
                Text = p.GetFullName()
            });
        public IEnumerable<SelectListItem> GetPsmbs() =>
            _context.PsmbAreas
            .Include(p => p.Commune)
            .Where(s => s.PlanktonAssays.Any() || s.Farms.Any(f => f.PlanktonAssays.Any()))
            .OrderBy(s => s.NormalizedName)
            .Select(p => new SelectListItem
            {
                Value = p.Id.ToString(CultureInfo.InvariantCulture),
                Text = p.GetFullName()
            });
    }
}
