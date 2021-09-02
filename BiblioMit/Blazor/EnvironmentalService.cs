using BiblioMit.Data;
using BiblioMit.Extensions;
using GoogleMapsComponents.Maps;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace BiblioMit.Blazor
{
    public class EnvironmentalService : IEnvironmental
    {
        private readonly ApplicationDbContext _context;
        public EnvironmentalService(ApplicationDbContext context)
        {
            _context = context;
        }
        public DateTime GetMinDate() =>
            _context.PlanktonAssays.Min(e => e.SamplingDate);
        public DateTime GetMaxDate() =>
            _context.PlanktonAssays.Max(e => e.SamplingDate);
        public async Task<IReadOnlyCollection<SelectListItem>> GetPhytoplanktonOrders() =>
            await _context.PhylogeneticGroups
            .OrderBy(s => s.NormalizedName)
            .Select(p => new SelectListItem
            {
                Value = p.Id.ToString(CultureInfo.InvariantCulture),
                Text = $"{p.Name} Totales"
            }).ToListAsync().ConfigureAwait(false);
        public async Task<IReadOnlyCollection<SelectListItem>> GetPhytoplanktonGenus() =>
            await _context.GenusPhytoplanktons
            .OrderBy(s => s.NormalizedName)
            .Select(p => new SelectListItem
            {
                Value = p.Id.ToString(CultureInfo.InvariantCulture),
                Text = p.Name
            }).ToListAsync().ConfigureAwait(false);
        public async Task<IReadOnlyCollection<SelectListItem>> GetPhytoplanktonSp() =>
            await _context.SpeciesPhytoplanktons
            .Include(s => s.Genus)
            .Where(s => s.Name != null)
            .OrderBy(s => s.Genus.NormalizedName)
            .Select(p => new SelectListItem
            {
                Value = p.Id.ToString(CultureInfo.InvariantCulture),
                Text = $"{p.Genus.Name} {p.Name}"
            }).ToListAsync().ConfigureAwait(false);
        public async Task<IReadOnlyCollection<SelectListItem>> GetCommunes() =>
            await _context.Communes
            .Include(c => c.Province)
                .ThenInclude(p => p.Region)
            .Where(s => s.CatchmentAreaId.HasValue)
            .OrderBy(s => s.NormalizedName)
            .Select(p => new SelectListItem
            {
                Value = p.Id.ToString(CultureInfo.InvariantCulture),
                Text = p.GetFullName()
            }).ToListAsync().ConfigureAwait(false);
        public async Task<IReadOnlyCollection<SelectListItem>> GetPsmbs() =>
            await _context.PsmbAreas
            .Include(p => p.Commune)
                .ThenInclude(c => c.Province)
                    .ThenInclude(c => c.Region)
            .Where(s => s.PlanktonAssays.Any() || s.Farms.Any(f => f.PlanktonAssays.Any()))
            .OrderBy(s => s.NormalizedName)
            .Select(p => new SelectListItem
            {
                Value = p.Id.ToString(CultureInfo.InvariantCulture),
                Text = p.GetFullName()
            }).ToListAsync().ConfigureAwait(false);
        public async Task<IReadOnlyCollection<SelectListItem>> GetCatchments() =>
            await _context.CatchmentAreas
            .Select(p => new SelectListItem
            {
                Value = p.Id.ToString(CultureInfo.InvariantCulture),
                Text = p.Name
            }).ToListAsync().ConfigureAwait(false);
        public async Task<ChartData> GetData(
            DateTimeOffset start, 
            DateTimeOffset endDt, 
            LocationType type, 
            int locationId, 
            string locationName, 
            Variable variable)
        {
            var filtered = type switch
            {
                LocationType.Cuenca => _context.PlanktonAssays
                .Where(a =>
                a.Psmb.Commune.CatchmentAreaId == locationId
                && a.SamplingDate >= start
                && a.SamplingDate <= endDt),

                LocationType.Psmb => _context.PlanktonAssays
                .Where(a =>
                a.PsmbId == locationId
                && a.SamplingDate >= start
                && a.SamplingDate <= endDt),

                _ => _context.PlanktonAssays
                .Where(a =>
                a.Psmb.CommuneId == locationId
                && a.SamplingDate >= start
                && a.SamplingDate <= endDt)
            };
            var dots = variable switch
            {
                Variable.o2 => filtered.Where(a => a.Oxigen.HasValue)
                .Select(a => new Dot
                {
                    Date = a.SamplingDate,
                    Y = a.Oxigen.Value
                }),
                Variable.ph => filtered.Where(a => a.Ph.HasValue)
                .Select(a => new Dot
                {
                    Date = a.SamplingDate,
                    Y = a.Ph.Value
                }),
                Variable.sal => filtered.Where(a => a.Salinity.HasValue)
                .Select(a => new Dot
                {
                    Date = a.SamplingDate,
                    Y = a.Salinity.Value
                }),
                Variable.phy => filtered
                .Select(a => new Dot
                {
                    Date = a.SamplingDate,
                    Y = a.Phytoplanktons.Sum(p => p.C)
                }),
                _ => filtered.Where(a => a.Temperature.HasValue)
                .Select(a => new Dot
                {
                    Date = a.SamplingDate,
                    Y = a.Temperature.Value
                })
            };
            return new ChartData
            {
                Title = $"{variable} {locationName}",
                Dots = await dots.ToListAsync().ConfigureAwait(false)
            };
        }
        public async Task<ICollection<ChartData>> GetDatas(
            DateTimeOffset? start,
            DateTimeOffset? endDt,
            IList<Variable> variables,
            IList<SelectListItem> orders,
            IList<SelectListItem> genus,
            IList<SelectListItem> species,
            IList<SelectListItem> catchments,
            IList<SelectListItem> communes,
            IList<SelectListItem> psmbs)
        {
            var resp = new List<ChartData>();
            if (!start.HasValue || !endDt.HasValue) return null;
            if(variables != null && variables.Any())
                foreach (var v  in variables)
                {
                    resp.Add(await GetData(
                        start.Value,
                        endDt.Value,
                        LocationType.Cuenca,
                        1,
                        "Cuenca Norte",
                        v).ConfigureAwait(false));
                }
            return resp;
        }
        public async Task<IEnumerable<PolygonOptions>> GetCuencaPolygonsAsync()
        {
            var polygonsCom = await _context.CatchmentAreas
                .Include(p => p.Polygon)
                    .ThenInclude(p => p.Vertices)
                .ToListAsync().ConfigureAwait(false);
            return polygonsCom
                .Select(p => new PolygonOptions
                {
                    Paths = new List<IEnumerable<LatLngLiteral>>
                    {
                        p.Polygon.Vertices.OrderBy(v => v.Order)
                        .Select(v => new LatLngLiteral
                        {
                            Lat = v.Latitude,
                            Lng = v.Longitude
                        })
                    },
                    ZIndex = p.Id
                });
        }
        public async Task<IEnumerable<PolygonOptions>> GetCommunePolygonsAsync()
        {
            var polygonsCom = await _context.Communes
                .Include(p => p.Polygons)
                    .ThenInclude(p => p.Vertices)
                .Where(p => p.CatchmentAreaId.HasValue)
                .ToListAsync().ConfigureAwait(false);
            return polygonsCom
                .Select(p => new PolygonOptions 
                {
                    Paths = p.Polygons
                    .Select(g => g.Vertices.OrderBy(v => v.Order)
                    .Select(v => new LatLngLiteral 
                    { 
                        Lat = v.Latitude, 
                        Lng = v.Longitude 
                    })),
                    ZIndex = p.Id
                });
        }
        public async Task<IEnumerable<PolygonOptions>> GetPsmbPolygonsAsync()
        {
            var polygonsCom = await _context.PsmbAreas
                .Include(p => p.Polygon)
                    .ThenInclude(p => p.Vertices)
                .Where(p => p.PolygonId.HasValue)
                .ToListAsync().ConfigureAwait(false);
            return polygonsCom
                .Select(p => new PolygonOptions
                {
                    Paths = new List<IEnumerable<LatLngLiteral>>
                    {
                        p.Polygon.Vertices.OrderBy(v => v.Order)
                        .Select(v => new LatLngLiteral
                        {
                            Lat = v.Latitude,
                            Lng = v.Longitude
                        })
                    },
                    ZIndex = p.Id
                });
        }
    }
}
