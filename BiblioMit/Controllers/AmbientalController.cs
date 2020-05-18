using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BiblioMit.Data;
using BiblioMit.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using Range = BiblioMit.Models.Range;
using System.Threading.Tasks;
using BiblioMit.Models.Entities.Semaforo;
using BiblioMit.Blazor;
using BiblioMit.Extensions;
using Microsoft.Extensions.Localization;
using BiblioMit.Models.VM;

namespace BiblioMit.Controllers
{
    [Authorize]
    public class AmbientalController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IStringLocalizer<AmbientalController> _localizer;
        public AmbientalController(
            ApplicationDbContext context,
            IStringLocalizer<AmbientalController> localizer)
        {
            _localizer = localizer;
            _context = context;
        }
        [AllowAnonymous]
        // GET: Arrivals
        public async Task<IActionResult> PSMBList()
        {
            var areas = new List<ChoicesGroup>
            {
                new ChoicesGroup
                {
                    Id = 4,
                    Label = _localizer["Catchment Areas"],
                    Choices = _context.CatchmentAreas.Select(c => new ChoicesItem
                    {
                        Value = c.Id.ToString(CultureInfo.InvariantCulture),
                        Label = $"{_localizer["Catchment Area"]} {c.Name}"
                    })
                }
            };
            areas.AddRange(await _context.CatchmentAreas
                .Include(c => c.Communes)
                .Select(c => new ChoicesGroup
                {
                    Id = c.Id,
                    Label = $"{_localizer["Catchment Area"]} {c.Name}",
                    Choices = c.Communes
                    .Select(com => new ChoicesItem
                    {
                        Value = com.Id.ToString(CultureInfo.InvariantCulture),
                        Label = $"{com.Name} {c.Name}"
                    })
                }).ToListAsync().ConfigureAwait(false));
            if (User.Identity.IsAuthenticated)
            {
                var psmbLst = await _context.PsmbAreas
                    .Include(p => p.Commune)
                    .Where(p => p.PlanktonAssays.Count > 0 && p.PolygonId.HasValue && p.Commune.CatchmentAreaId.HasValue)
                    .ToListAsync().ConfigureAwait(false);
                var psmbs = psmbLst.GroupBy(p => p.Commune)
                        .Select(c => new ChoicesGroup 
                        { 
                            Id = c.Key.Id,
                            Label = c.Key.Name,
                            Choices = c
                            .Select(p => new ChoicesItem 
                            {
                                Value = p.Id.ToString(CultureInfo.InvariantCulture),
                                Label = $"{p.Code} {p.Name} {c.Key.Name}"
                            })
                        });
                areas.AddRange(psmbs);
            }
            return Json(areas);
        }

        [AllowAnonymous]
        public JsonResult VariableList()
        {
            var result = Variable.t.Enum2ChoicesGroup();
            var group = _localizer["Group"];
            var groups = new ChoicesGroup 
            {
                Label = _localizer["Phylogenetic Groups (Cel/mL)"],
                Id = 3,
                Choices = _context.PhylogeneticGroups.Select(p => new ChoicesItem
                {
                    Value = p.Id.ToString(CultureInfo.InvariantCulture),
                    Label = $"{p.Name} ({group})"
                })
            };
            var genus = _localizer["Genus"];
            var orders = new ChoicesGroup
            {
                Label = _localizer["Genera (Cel/mL)"],
                Id = 4,
                Choices = _context.GenusPhytoplanktons.Select(p => new ChoicesItem
                {
                    Value = p.Id.ToString(CultureInfo.InvariantCulture),
                    Label = $"{p.Name} ({genus})"
                })
            };
            var sp = _localizer["Species"];
            var species = new ChoicesGroup
            {
                Label = _localizer["Species (Cel/mL)"],
                Id = 5,
                Choices = _context.SpeciesPhytoplanktons
                .Include(s => s.Genus)
                .Select(p => new ChoicesItem
                {
                    Value = p.Id.ToString(CultureInfo.InvariantCulture),
                    Label = $"{p.GetFullName()} ({sp})"
                })
            };
            return Json(result.Concat(new List<ChoicesGroup> { groups, orders, species }));
        }
        public async Task<JsonResult> TLData(int a, int psmb, int sp, int? t, int? l, int? rs, int? s, string start, string end) {
            var i = Convert.ToDateTime(start, CultureInfo.InvariantCulture);
            var f = Convert.ToDateTime(end, CultureInfo.InvariantCulture);
            var data = new List<AmData>();
            //a 1 semilla, 2 larva, 3 reproductor
            var psmbs = new Dictionary<int,int>{
                {20, 101990},
                {21, 101017},
                {22, 103633}
            };
            var sps = new Dictionary<int, int>{
                {31, 1},
                {32, 2},
                {33, 3}
            };
            //TallaRange 0-7
            //LarvaType 0 D, 1 U, 2 O
            //0 101990 Quetalco, 1 101017 Vilipulli, 2 103633 Carahue
            //1 chorito, 2 cholga, 3 choro
            switch (a)
            {
                case 11:
                    if (t.HasValue)
                    {
                        var range = t.Value % 10;
                        var db = _context.Tallas
                        .Include(tl => tl.SpecieSeed)
                        .ThenInclude(ss => ss.Seed) as IQueryable<Talla>;
                        if(range != 8) db = db.Where(tl => tl.Range == (Range)range);
                        if (psmb != 23) db = db.Where(tl => tl.SpecieSeed.Seed.Farm.Code == psmbs[psmb]);
                        if (sp != 34) db = db.Where(tl => tl.SpecieSeed.SpecieId == sps[sp]);
                        var list = await db.ToListAsync().ConfigureAwait(false);
                        data = list.GroupBy(tl => tl.SpecieSeed.Seed.Date)
                        .OrderBy(g => g.Key)
                        .Select(g =>
                        {
                            var expando = new AmData
                            {
                                Date = g.Key.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                            };
                            var cs = g.Where(gg => gg.Id != 0).Select(m => m.Proportion);
                            if (!cs.Any()) return null;
                            expando.Value = Math.Round(cs.Average());
                            return expando;
                        }).Where(d => d != null).ToList();
                    }
                    break;
                case 12:
                    if (l.HasValue)
                    {
                        var type = l.Value % 10;
                        var db = _context.Larvas
                        .Include(tl => tl.Larvae) as IQueryable<Larva>;
                        if (type != 3) db = db.Where(tl => tl.LarvaType == (LarvaType)type);
                        if (psmb != 23) db = db.Where(tl => tl.Larvae.Farm.Code == psmbs[psmb]);
                        if (sp != 34) db = db.Where(tl => tl.SpecieId == sps[sp]);
                        var list = await db.ToListAsync().ConfigureAwait(false);
                        data = list.GroupBy(tl => tl.Larvae.Date)
                        .OrderBy(g => g.Key)
                        .Select(g =>
                        {
                            var expando = new AmData
                            {
                                Date = g.Key.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                            };
                            var cs = g.Where(gg => gg.Id != 0).Select(m => m.Count);
                            if (!cs.Any()) return null;
                            expando.Value = Math.Round(cs.Average());
                            return expando;
                        }).Where(d => d != null).ToList();
                    }
                    break;
                case 13:
                    if (s.HasValue)
                    {
                        var db = _context.Spawnings as IQueryable<Spawning>;
                        if (psmb != 23) db = db.Where(tl => tl.Farm.Code == psmbs[psmb]);
                        var list = await db.ToListAsync().ConfigureAwait(false);
                        data = list.GroupBy(tl => tl.Date)
                        .OrderBy(g => g.Key)
                        .Select(g =>
                        {
                            var expando = new AmData
                            {
                                Date = g.Key.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                            };
                            var cs = g.Where(gg => gg.Id != 0).Select(m => s.Value == 70 ? m.FemaleIG : m.MaleIG);
                            if (!cs.Any()) return null;
                            expando.Value = Math.Round(cs.Average());
                            return expando;
                        }).Where(d => d != null).ToList();
                    }
                    break;
                case 14:
                    if (true)
                    {
                        var db = _context.SpecieSeeds
                        .Include(ss => ss.Seed) as IQueryable<SpecieSeed>;
                        if (psmb != 23) db = db.Where(tl => tl.Seed.Farm.Code == psmbs[psmb]);
                        if (sp != 34) db = db.Where(tl => tl.SpecieId == sps[sp]);
                        var list = await db.ToListAsync().ConfigureAwait(false);
                        data = list.GroupBy(tl => tl.Seed.Date)
                        .OrderBy(g => g.Key)
                        .Select(g =>
                        {
                            var expando = new AmData
                            {
                                Date = g.Key.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                            };
                            var cs = g.Where(gg => gg.Id != 0).Select(m => m.Capture);
                            if (!cs.Any()) return null;
                            expando.Value = Math.Round(cs.Average());
                            return expando;
                        }).Where(d => d != null).ToList();
                    }
                    break;
                case 15:
                    if (rs.HasValue)
                    {
                        var stage = rs.Value % 10;
                        var db = _context.ReproductiveStages
                        .Where(tl => tl.Stage == (Stage)stage);
                        if (psmb != 23) db = db.Where(tl => tl.Spawning.Farm.Code == psmbs[psmb]);
                        var list = await db.ToListAsync().ConfigureAwait(false);
                        data = list.GroupBy(tl => tl.Spawning.Date)
                        .OrderBy(g => g.Key)
                        .Select(g =>
                        {
                            var expando = new AmData
                            {
                                Date = g.Key.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                            };
                            var cs = g.Where(gg => gg.Id != 0).Select(m => m.Proportion);
                            if (!cs.Any()) return null;
                            expando.Value = Math.Round(cs.Average());
                            return expando;
                        }).Where(d => d != null).ToList();
                    }
                    break;
                case 16:
                    if (s.HasValue)
                    {
                        var db = _context.Spawnings as IQueryable<Spawning>;
                        if (psmb != 23) db = db.Where(tl => tl.Farm.Code == psmbs[psmb]);
                        var list = await db.ToListAsync().ConfigureAwait(false);
                        data = list.GroupBy(tl => tl.Date)
                        .OrderBy(g => g.Key)
                        .Select(g =>
                        {
                            var expando = new AmData
                            {
                                Date = g.Key.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                            };
                            var cs = g.Where(gg => gg.Id != 0).Select(m => s.Value == 70 ? m.FemaleProportion : m.MaleProportion);
                            if (!cs.Any()) return null;
                            expando.Value = Math.Round(cs.Average());
                            return expando;
                        }).Where(d => d != null).ToList();
                    }
                    break;
                case 17:
                    if (true)
                    {
                        var db = _context.SpecieSeeds
                        .Include(ss => ss.Seed) as IQueryable<SpecieSeed>;
                        if (psmb == 23) db = db.Where(tl => tl.Seed.Farm.Code == psmbs[psmb]);
                        if (psmb == 23) db = db.Where(tl => tl.SpecieId == sps[sp]);
                        var list = await db.ToListAsync().ConfigureAwait(false);
                        data = list.GroupBy(tl => tl.Seed.Date)
                        .OrderBy(g => g.Key)
                        .Select(g =>
                        {
                            var expando = new AmData
                            {
                                Date = g.Key.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                            };
                            var cs = g.Where(gg => gg.Id != 0).Select(m => m.Proportion);
                            if (!cs.Any()) return null;
                            expando.Value = Math.Round(cs.Average());
                            return expando;
                        }).Where(d => d != null).ToList();
                    }
                    break;
            }
            return Json(data);
        }
        public JsonResult TLList() =>
            Json(new List<object>
            {
                new ChoicesGroup
                {
                    Label = _localizer["Analysis"],
                    Id = 1,
                    Choices = new List<ChoicesItem>{
                        new ChoicesItem{
                            Value = "14",
                            Label = _localizer["Capture per Species"]
                        },
                        new ChoicesItem{
                            Value = "17",
                            Label = _localizer["% Species"]
                        },
                        new ChoicesItem{
                            Value = "11",
                            Label = _localizer["% Size per Species"]
                        },
                        new ChoicesItem{
                            Value = "12",
                            Label = _localizer["Larvae"]
                        },
                        new ChoicesItem{
                            Value = "13",
                            Label = _localizer["IG Reproductores"]
                        },
                        new ChoicesItem{
                            Value = "15",
                            Label = _localizer["% Reproductive Stage"]
                        },
                        new ChoicesItem{
                            Value = "16",
                            Label = _localizer["% Sex"]
                        }
                    }
                },
                new ChoicesGroup
                {
                    Label = "PSMBs",
                    Id = 2,
                    Choices = new List<ChoicesItem>{
                        new ChoicesItem{
                            Value = "20",
                            Label = "10219 Quetalco"
                        },
                        new ChoicesItem{
                            Value = "21",
                            Label = "10220 Vilipulli"
                        },
                        new ChoicesItem{
                            Value = "22",
                            Label = "10431 Carahue"
                        },
                        new ChoicesItem{
                            Value = "23",
                            Label = _localizer["All PSMBs"]
                        }
                    }
                },
                new ChoicesGroup
                {
                    Label = _localizer["Species"],
                    Id = 3,
                    Choices = new List<ChoicesItem>{
                        new ChoicesItem{
                            Value = "31",
                            Label = "Chorito (<i>Mytilus chilensis</i>)"
                        },
                        new ChoicesItem{
                            Value = "32",
                            Label = "Cholga (<i>Aulacomya atra</i>)"
                        },
                        new ChoicesItem{
                            Value = "33",
                            Label = "Choro (<i>Choromytilus chorus</i>)"
                        },
                        new ChoicesItem{
                            Value = "33",
                            Label = _localizer["All Species"]
                        }
                    }
                },
                new ChoicesGroup
                {
                    Label = _localizer["Size (%)"],
                    Id = 4,
                    Choices = new List<ChoicesItem>{
                        new ChoicesItem{
                            Value = "40",
                            Label = "0 - 1 (mm)"
                        },
                        new ChoicesItem{
                            Value = "41",
                            Label = "1 - 2 (mm)"
                        },
                        new ChoicesItem{
                            Value = "42",
                            Label = "2 - 5 (mm)"
                        },
                        new ChoicesItem{
                            Value = "43",
                            Label = "5 - 10 (mm)"
                        },
                        new ChoicesItem{
                            Value = "44",
                            Label = "10 - 15 (mm)"
                        },
                        new ChoicesItem{
                            Value = "45",
                            Label = "15 - 20 (mm)"
                        },
                        new ChoicesItem{
                            Value = "46",
                            Label = "20 - 25 (mm)"
                        },
                        new ChoicesItem{
                            Value = "47",
                            Label = "25 - 30 (mm)"
                        },
                        new ChoicesItem{
                            Value = "48",
                            Label = _localizer["Todas las tallas"]
                        },
                    }
                },
                new ChoicesGroup
                {
                    Label = _localizer["Larva Type (count)"],
                    Id = 5,
                    Choices = new List<ChoicesItem>{
                        new ChoicesItem{
                            Value = "50",
                            Label = _localizer["D-Larva"]
                        },
                        new ChoicesItem{
                            Value = "51",
                            Label = _localizer["Umbanate Larva"]
                        },
                        new ChoicesItem{
                            Value = "52",
                            Label = _localizer["Eyed Larva"]
                        },
                        new ChoicesItem{
                            Value = "53",
                            Label = _localizer["Total Larvae"]
                        }
                    }
                },
                new ChoicesGroup
                {
                    Label = _localizer["Estado reproductivo (%)"],
                    Id = 6,
                    Choices = new List<ChoicesItem>{
                        new ChoicesItem{
                            Value = "60",
                            Label = _localizer["Maturing"]
                        },
                        new ChoicesItem{
                            Value = "61",
                            Label = _localizer["Mature"]
                        },
                        new ChoicesItem{
                            Value = "62",
                            Label = _localizer["Spawned"]
                        },
                        new ChoicesItem{
                            Value = "63",
                            Label = _localizer["Spawning"]
                        }
                    }
                },
                new ChoicesGroup
                {
                    Label = _localizer["Sex"],
                    Id = 7,
                    Choices = new List<ChoicesItem>{
                        new ChoicesItem{
                            Value = "70",
                            Label = _localizer["Female"]
                        },
                        new ChoicesItem{
                            Value = "71",
                            Label = _localizer["Male"]
                        }
                    }
                }
            });
        [AllowAnonymous]
        // GET: Arrivals
        public async Task<IActionResult> MapData()
        {
            var cuenca = _localizer["Catchment Area"];
            var com = _localizer["Commune"];
            var map = await _context.CatchmentAreas
                .Include(c => c.Polygon)
                    .ThenInclude(p => p.Vertices)
                    .Select(c => new GMapPolygon
                    {
                        Id = c.Id,
                        Name = $"{cuenca} {c.Name}",
                        Code = c.Id.ToString(CultureInfo.InvariantCulture),
                        Position = new[]{c.Polygon.Vertices.Select(o =>
                        new GMapCoordinate
                        {
                            Lat = o.Latitude,
                            Lng = o.Longitude
                        })}
                    })
                    .ToListAsync().ConfigureAwait(false);
            map.AddRange(await _context.Communes
                .Include(c => c.Province)
                .Include(c => c.Polygons)
                    .ThenInclude(c => c.Vertices)
                    .Where(c => c.CatchmentAreaId.HasValue).Select(c =>
                new GMapPolygon
                {
                    Id = c.Id,
                    Name = $"{com} {c.Name}",
                    Provincia = c.Province.Name,
                    Code = c.GetCUT(),
                    Position = c.Polygons
                    .Select(p => p.Vertices.Select(o =>
                    new GMapCoordinate
                    {
                        Lat = o.Latitude,
                        Lng = o.Longitude
                    }))
                }).ToListAsync().ConfigureAwait(false));
            if (User.Identity.IsAuthenticated)
            {
                map.AddRange(await _context.PsmbAreas
                    .Include(p => p.Commune)
                        .ThenInclude(p => p.Province)
                    .Include(p => p.Polygon)
                        .ThenInclude(p => p.Vertices)
                    .Where(c => c.PlanktonAssays.Count > 0 && c.PolygonId.HasValue && c.Commune.CatchmentAreaId.HasValue).Select(c => new GMapPolygon
                    {
                        Id = c.Id,
                        Name = $"{c.Code} {c.Name}",
                        Comuna = c.Commune.Name,
                        Provincia = c.Commune.Province.Name,
                        Code = c.Code.ToString(CultureInfo.InvariantCulture),
                        Position = new[]{c.Polygon
                        .Vertices.Select(o => new GMapCoordinate
                            {
                                Lat = o.Latitude,
                                Lng = o.Longitude
                            }) }
                    }).ToListAsync().ConfigureAwait(false));
            }
            return Json(map);
        }
        private static AmData SelectData(
            IGrouping<DateTime, PlanktonAssay> g,
            string var, bool fito)
        {
            if (g == null) return null;
            var response = new AmData 
            { 
                Date = g.Key.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            };
            if (fito)
            {
                var tmp = g.Where(m => m.Phytoplanktons != null);
                if (tmp.Any())
                {
                    bool phy = var == "phy";
                    if (!phy)
                        tmp = tmp.Where(m => m.Phytoplanktons
                        .Any(p => p.Species.Genus.Group.NormalizedName
                        .Equals(var, StringComparison.Ordinal)));
                    if (phy || tmp.Any())
                    {
                        var cs = phy ?
                            tmp.SelectMany(m => m.Phytoplanktons.Select(p => p.C))
                            : tmp.SelectMany(m => m.Phytoplanktons
                            .Where(p => p.Species.Genus.Group.NormalizedName
                            .Equals(var, StringComparison.Ordinal)).Select(p => p.C));
                        if (cs.Any())
                        {
                            response.Value = Math.Round(cs.Average(), 2);
                            return response;
                        }
                    }
                }
            }
            else
            {
                string attr = var switch
                {
                    "t" => "Temperature",
                    "ph" => "Ph",
                    "sal" => "Salinity",
                    "o2" => "Oxigen",
                    _ => ""
                };
                if(g.Any(m => m[attr] != null))
                {
                    response.Value = Math.Round(g.Average(m => (double?)m[attr]).Value, 2);
                    return response;
                }
            }
            return null;
        }
        private async Task<IEnumerable<AmData>> Data(int area, string var, string start, string end, bool fito = false)
        {
            var assays = _context.PlanktonAssays as IQueryable<PlanktonAssay>;
            if(fito) assays = assays
                .Include(e => e.Phytoplanktons)
                    .ThenInclude(f => f.Species)
                        .ThenInclude(f => f.Genus)
                            .ThenInclude(f => f.Group);
            if(area < 4)
            {
                assays.Where(e => e.Psmb.Commune.CatchmentAreaId == area);
            }
            else if(area < 100_000)
            {
                assays.Where(e => e.PsmbId == area);
            }
            else
            {
                assays.Where(e => e.Psmb.CommuneId == area);
            }
            var i = Convert.ToDateTime(start, CultureInfo.InvariantCulture);
            var f = Convert.ToDateTime(end, CultureInfo.InvariantCulture);
            var ensayos = await assays
                .Where(e => e.SamplingDate >= i && e.SamplingDate <= f)
                .ToListAsync().ConfigureAwait(false);
            return ensayos.GroupBy(e => e.SamplingDate.Date).OrderBy(g => g.Key).Select(g => SelectData(g, var, fito)).Where(d => d != null);
        }
        [AllowAnonymous]
        public async Task<IActionResult> FitoData(int area, string var, string start, string end)
        {
            var data = await Data(area, var, start, end, true).ConfigureAwait(false);
            return Json(data);
        }
        //[HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ProducesResponseType(typeof(JsonResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> GraphData(int area, string var, string start, string end)
        {
            var data = await Data(area, var, start, end).ConfigureAwait(false);           
            return Json(data);
        }
        [AllowAnonymous]
        public IActionResult Graph()
        {
            ViewData["start"] = _context.PlanktonAssays.Min(e => e.SamplingDate)
                .ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            ViewData["end"] = _context.PlanktonAssays.Max(e => e.SamplingDate)
                .ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            return View();
        }
    }
}