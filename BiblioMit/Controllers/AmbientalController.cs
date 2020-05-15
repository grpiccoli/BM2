using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BiblioMit.Data;
using BiblioMit.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Dynamic;
using Range = BiblioMit.Models.Range;
using System.Threading.Tasks;
using BiblioMit.Models.Entities.Semaforo;
using BiblioMit.Blazor;
using BiblioMit.Extensions;
using Microsoft.Extensions.Localization;

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
        public IActionResult ExportMenu()
        {
            var images = new ExportMenu
            {
                Label = _localizer["Image"],
                Menu = new List<IExport>
                {
                    new ExportItem { Type = "jpg", Label = "JPG" }
                }
            };
            var datos = new ExportMenu
            {
                Label = _localizer["Data"],
                Menu = new List<IExport>
                {
                    new ExportItem { Type = "pdf", Label = "PDF" }
                }
            };
            if (User.Identity.IsAuthenticated)
            {
                images.Menu = images.Menu.Concat(new List<IExport>
                {
                    new ExportItem { Type = "png", Label = "PNG" },
                    new ExportItem { Type = "gif", Label = "GIF" },
                    new ExportItem { Type = "svg", Label = "SVG" }
                });
                datos.Menu = datos.Menu.Concat(new List<IExport>
                {
                    new ExportItem { Type = "json", Label = "JSON" },
                    new ExportItem { Type = "csv", Label = "CSV" },
                    new ExportItem { Type = "xlsx", Label = "XLSX" } 
                });
            }
            var model = new List<ExportMenu>
            {
                new ExportMenu
                {
                    Label = "...",
                    Menu = new List<IExport>
                    {
                        images,
                        datos,
                        new ExportItem { Label = _localizer["Print"], Type = "print" }
                    }
                }
            };
            return Json(model);
        }

        [AllowAnonymous]
        // GET: Arrivals
        public async Task<IActionResult> PSMBList()
        {
            var areas = await _context.CatchmentAreas
                .Include(c => c.Communes)
                .Select(c => new ChoicesGroup
                {
                    Id = c.Id + 1,
                    Label = $"{_localizer["Catchment Area"]} {c.Name}",
                    Choices = c.Communes.Select(com => new ChoicesItem
                    {
                        Value = com.Id.ToString(CultureInfo.InvariantCulture),
                        Label = com.Name
                    })
                }).ToListAsync().ConfigureAwait(false);
            areas.Add(new ChoicesGroup
                {
                    Id = 1,
                    Label = _localizer["Catchment Areas"],
                    Choices = _context.CatchmentAreas.Select(c => new ChoicesItem
                    {
                        Value = c.Id.ToString(CultureInfo.InvariantCulture),
                        Label = c.Name
                    })
                });
            if (User.Identity.IsAuthenticated)
            {
                var psmbs = _context.CatchmentAreas
                    .Include(p => p.Communes)
                        .ThenInclude(c => c.Psmbs
                        .Where(p => p.Discriminator == Models.Entities.Centres.PsmbType.PsmbArea
                        && p.PolygonId.HasValue))
                        .Select(c => new ChoicesGroup 
                        { 
                            Id = c.Id + 1,
                            Label = $"{_localizer["Catchment Area"]} {c.Name}",
                            Choices = c.Communes.SelectMany(com => com.Psmbs.Select(p => new ChoicesItem 
                            { 
                                Value = (p.Id * 100).ToString(CultureInfo.InvariantCulture),
                                Label = string.Join(" ",(new object[]{p.Id, p.Name, com.Name}).Where(o => o != null))
                            }))
                        });
                areas.AddRange(psmbs);
            }
            return Json(areas);
        }

        [AllowAnonymous]
        public JsonResult VariableList()
        {
            var result = Variable.t.Enum2ChoicesGroup();
            var groups = new ChoicesGroup 
            {
                Label = _localizer["Phylogenetic Groups (Cel/mL)"],
                Id = 3,
                Choices = _context.PhylogeneticGroups.Select(p => new ChoicesItem
                {
                    Value = p.Id.ToString(CultureInfo.InvariantCulture),
                    Label = p.Name
                })
            };
            var orders = new ChoicesGroup
            {
                Label = _localizer["Genus (Cel/mL)"],
                Id = 4,
                Choices = _context.GenusPhytoplanktons.Select(p => new ChoicesItem
                {
                    Value = p.Id.ToString(CultureInfo.InvariantCulture),
                    Label = p.Name
                })
            };
            var species = new ChoicesGroup
            {
                Label = _localizer["Species (Cel/mL)"],
                Id = 5,
                Choices = _context.SpeciesPhytoplanktons
                .Include(s => s.Genus)
                .Select(p => new ChoicesItem
                {
                    Value = p.Id.ToString(CultureInfo.InvariantCulture),
                    Label = p.GetName()
                })
            };
            return Json(result.Concat(new List<ChoicesGroup> { groups, orders, species }));
        }
        public JsonResult TLData(int a, int psmb, int sp, int? t, int? l, int? rs, int? s, string start, string end) {
            var i = Convert.ToDateTime(start, CultureInfo.InvariantCulture);
            var f = Convert.ToDateTime(end, CultureInfo.InvariantCulture);
            IEnumerable<ExpandoObject> data = new List<ExpandoObject>();
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
                        var date1 = Enumerable.Range(0, 1 + f.Subtract(i).Days)
                        .Select(offset => new Talla { SpecieSeed = new SpecieSeed { Seed = new Seed { Date = i.AddDays(offset) } } });
                        var ensayos1 = _context.Tallas
                        .Include(tl => tl.SpecieSeed)
                        .ThenInclude(ss => ss.Seed)
                        .Where(tl =>
                        (psmb == 23 || tl.SpecieSeed.Seed.FarmId == psmbs[psmb])
                        && (sp == 34 || tl.SpecieSeed.SpecieId == sps[sp])
                        && tl.Range == (Range)(t.Value % 10))
                        .ToList();
                        ensayos1.AddRange(date1);
                        data = ensayos1
                        .GroupBy(tl => tl.SpecieSeed.Seed.Date.Date)
                        .OrderBy(g => g.Key)
                        .Select(g =>
                        {
                            dynamic expando = new ExpandoObject();
                            expando.date = g.Key.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                            var cs = g.Where(gg => gg.Id != 0).Select(m => m.Proportion);
                            ((IDictionary<string, object>)expando)
                            .Add($"{a}_{psmb}_{sp}_{t.Value}", cs.Any() ? (double?)Math.Round(cs.Average()) : null);
                            return (ExpandoObject)expando;
                        });
                    }
                    break;
                case 12:
                    if (l.HasValue)
                    {
                        var date2 = Enumerable.Range(0, 1 + f.Subtract(i).Days)
                        .Select(offset => new Larva { Larvae = new Larvae { Date = i.AddDays(offset) } });
                        var ensayos2 = _context.Larvas
                        .Include(tl => tl.Larvae)
                        .Where(tl =>
                        (psmb == 23 || tl.Larvae.FarmId == psmbs[psmb])
                        && (sp == 34 || tl.SpecieId == sps[sp])
                        && tl.LarvaType == (LarvaType)(l.Value % 10))
                        .ToList();
                        ensayos2.AddRange(date2);
                        data = ensayos2
                        .GroupBy(tl => tl.Larvae.Date.Date)
                        .OrderBy(g => g.Key)
                        .Select(g =>
                        {
                            dynamic expando = new ExpandoObject();
                            expando.date = g.Key.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                            var cs = g.Where(gg => gg.Id != 0).Select(m => m.Count);
                            ((IDictionary<string, object>)expando)
                            .Add($"{a}_{psmb}_{sp}_{l.Value}", cs.Any() ? (double?)Math.Round(cs.Average()) : null);
                            return (ExpandoObject)expando;
                        });
                    }
                    break;
                case 13:
                    if (s.HasValue)
                    {
                        var date3 = Enumerable.Range(0, 1 + f.Subtract(i).Days)
                        .Select(offset => new Spawning { Date = i.AddDays(offset) });
                        var ensayos3 = _context.Spawnings
                        .Where(tl =>
                        (psmb == 23 || tl.FarmId == psmbs[psmb]))
                        .ToList();
                        ensayos3.AddRange(date3);
                        data = ensayos3
                        .GroupBy(tl => tl.Date.Date)
                        .OrderBy(g => g.Key)
                        .Select(g =>
                        {
                            dynamic expando = new ExpandoObject();
                            expando.date = g.Key.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                            var cs = g.Where(gg => gg.Id != 0).Select(m => s.Value == 70 ? m.FemaleIG : m.MaleIG);
                            ((IDictionary<string, object>)expando)
                            .Add($"{a}_{psmb}_{sp}_{s.Value}", cs.Any() ? (double?)Math.Round(cs.Average()) : null);
                            return (ExpandoObject)expando;
                        });
                    }
                    break;
                case 14:
                    var date4 = Enumerable.Range(0, 1 + f.Subtract(i).Days)
                    .Select(offset => new SpecieSeed { Seed = new Seed { Date = i.AddDays(offset) } });
                    var ensayos4 = _context.SpecieSeeds
                    .Include(ss => ss.Seed)
                    .Where(tl =>
                    (psmb == 23 || tl.Seed.FarmId == psmbs[psmb])
                    && (sp == 34 || tl.SpecieId == sps[sp]))
                    .ToList();
                    ensayos4.AddRange(date4);
                    data = ensayos4
                    .GroupBy(tl => tl.Seed.Date.Date)
                    .OrderBy(g => g.Key)
                    .Select(g =>
                    {
                        dynamic expando = new ExpandoObject();
                        expando.date = g.Key.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                        var cs = g.Where(gg => gg.Id != 0).Select(m => m.Capture);
                        ((IDictionary<string, object>)expando)
                        .Add($"{a}_{psmb}_{sp}", cs.Any() ? (double?)Math.Round(cs.Average()) : null);
                        return (ExpandoObject)expando;
                    });
                    break;
                case 15:
                    if (rs.HasValue)
                    {
                        var date5 = Enumerable.Range(0, 1 + f.Subtract(i).Days)
                        .Select(offset => new ReproductiveStage { Spawning = new Spawning { Date = i.AddDays(offset) } });
                        var ensayos5 = _context.ReproductiveStages
                        .Where(tl =>
                        (psmb == 23 || tl.Spawning.FarmId == psmbs[psmb])
                        && tl.Stage == (Stage)(rs.Value % 10))
                        .ToList();
                        ensayos5.AddRange(date5);
                        data = ensayos5
                        .GroupBy(tl => tl.Spawning.Date.Date)
                        .OrderBy(g => g.Key)
                        .Select(g =>
                        {
                            dynamic expando = new ExpandoObject();
                            expando.date = g.Key.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                            var cs = g.Where(gg => gg.Id != 0).Select(m => m.Proportion);
                            ((IDictionary<string, object>)expando)
                            .Add($"{a}_{psmb}_{sp}_{rs.Value}", cs.Any() ? (double?)Math.Round(cs.Average()) : null);
                            return (ExpandoObject)expando;
                        });
                    }
                    break;
                case 16:
                    if (s.HasValue)
                    {
                        var date6 = Enumerable.Range(0, 1 + f.Subtract(i).Days)
                        .Select(offset => new Spawning { Date = i.AddDays(offset) });
                        var ensayos6 = _context.Spawnings
                        .Where(tl =>
                        (psmb == 23 || tl.FarmId == psmbs[psmb]))
                        .ToList();
                        ensayos6.AddRange(date6);
                        data = ensayos6
                        .GroupBy(tl => tl.Date.Date)
                        .OrderBy(g => g.Key)
                        .Select(g =>
                        {
                            dynamic expando = new ExpandoObject();
                            expando.date = g.Key.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                            var cs = g.Where(gg => gg.Id != 0).Select(m => s.Value == 70 ? m.FemaleProportion : m.MaleProportion);
                            ((IDictionary<string, object>)expando)
                            .Add($"{a}_{psmb}_{sp}_{s.Value}", cs.Any() ? (double?)Math.Round(cs.Average()) : null);
                            return (ExpandoObject)expando;
                        });
                    }
                    break;
                case 17:
                    var date7 = Enumerable.Range(0, 1 + f.Subtract(i).Days)
                    .Select(offset => new SpecieSeed { Seed = new Seed { Date = i.AddDays(offset) } });
                    var ensayos7 = _context.SpecieSeeds
                    .Include(ss => ss.Seed)
                    .Where(tl =>
                    (psmb == 23 || tl.Seed.FarmId == psmbs[psmb])
                    && (sp == 34 || tl.SpecieId == sps[sp]))
                    .ToList();
                    ensayos7.AddRange(date7);
                    data = ensayos7
                    .GroupBy(tl => tl.Seed.Date.Date)
                    .OrderBy(g => g.Key)
                    .Select(g =>
                    {
                        dynamic expando = new ExpandoObject();
                        expando.date = g.Key.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                        var cs = g.Where(gg => gg.Id != 0).Select(m => m.Proportion);
                        ((IDictionary<string, object>)expando)
                        .Add($"{a}_{psmb}_{sp}", cs.Any() ? (double?)Math.Round(cs.Average()) : null);
                        return (ExpandoObject)expando;
                    });
                    break;
            }
            return Json(data);
        }
        public JsonResult TLList() =>
            Json(new List<object>
            {
                new
                {
                    label = "Análisis",
                    id = 1,
                    choices = new List<object>{
                        new {
                            value = "14",
                            label = "Captura por Especie"
                        },
                        new {
                            value = "17",
                            label = "% Especie"
                        },
                        new {
                            value = "11",
                            label = "% Talla por Especie"
                        },
                        new {
                            value = "12",
                            label = "Larvas"
                        },
                        new {
                            value = "13",
                            label = "IG Reproductores"
                        },
                        new {
                            value = "15",
                            label = "% Estado Reproductivo"
                        },
                        new {
                            value = "16",
                            label = "% Sexo"
                        }
                    }
                },
                new
                {
                    label = "PSMBs",
                    id = 2,
                    choices = new List<object>{
                        new {
                            value = "20",
                            label = "10219 Quetalco"
                        },
                        new {
                            value = "21",
                            label = "10220 Vilipulli"
                        },
                        new {
                            value = "22",
                            label = "10431 Carahue"
                        },
                        new {
                            value = "23",
                            label = "Todos los PSMBs"
                        }
                    }
                },
                new
                {
                    label = "Especies",
                    id = 3,
                    choices = new List<object>{
                        new {
                            value = "31",
                            label = "Chorito (<i>Mytilus chilensis</i>)"
                        },
                        new {
                            value = "32",
                            label = "Cholga (<i>Aulacomya atra</i>)"
                        },
                        new {
                            value = "33",
                            label = "Choro (<i>Choromytilus chorus</i>)"
                        },
                        new {
                            value = "33",
                            label = "Todas las especies"
                        }
                    }
                },
                new
                {
                    label = "Tallas (%)",
                    id = 4,
                    choices = new List<object>{
                        new {
                            value = "40",
                            label = "0 - 1 (mm)"
                        },
                        new {
                            value = "41",
                            label = "1 - 2 (mm)"
                        },
                        new {
                            value = "42",
                            label = "2 - 5 (mm)"
                        },
                        new {
                            value = "43",
                            label = "5 - 10 (mm)"
                        },
                        new {
                            value = "44",
                            label = "10 - 15 (mm)"
                        },
                        new {
                            value = "45",
                            label = "15 - 20 (mm)"
                        },
                        new {
                            value = "46",
                            label = "20 - 25 (mm)"
                        },
                        new {
                            value = "47",
                            label = "25 - 30 (mm)"
                        },
                        new {
                            value = "48",
                            label = "Todas las tallas"
                        },

                    }
                },
                new
                {
                    label = "Tipo de Larva (conteo)",
                    id = 5,
                    choices = new List<object>{
                        new {
                            value = "50",
                            label = "Larva D (D)"
                        },
                        new {
                            value = "51",
                            label = "Larva umbonada (U)"
                        },
                        new {
                            value = "52",
                            label = "Larva con ojo (O)"
                        },
                        new {
                            value = "53",
                            label = "Larvas Totales"
                        }
                    }
                },
                new
                {
                    label = "Estado reproductivo (%)",
                    id = 6,
                    choices = new List<object>{
                        new {
                            value = "60",
                            label = "En madurez"
                        },
                        new {
                            value = "61",
                            label = "Maduro"
                        },
                        new {
                            value = "62",
                            label = "Desovado"
                        },
                        new {
                            value = "63",
                            label = "En desove"
                        }
                    }
                },
                new
                {
                    label = "Sexo",
                    id = 7,
                    choices = new List<object>{
                        new {
                            value = "70",
                            label = "Hembra"
                        },
                        new {
                            value = "71",
                            label = "Macho"
                        }
                    }
                }
            });
        [AllowAnonymous]
        // GET: Arrivals
        public async Task<IActionResult> MapData()
        {
            var cuencas = await _context.CatchmentAreas
                .Include(c => c.Polygon)
                    .ThenInclude(p => p.Vertices)
                    .ToListAsync().ConfigureAwait(false);
            var map = cuencas
                .Select(c =>
                {
                    var pol = new GMapPolygon
                    {
                        Id = c.Id,
                        Name = $"{_localizer["Catchment Area"]} {c.Name}",
                        Region = "Los Lagos"
                    };
                    pol.Position.Add(c.Polygon.Vertices.OrderBy(o => o.Order).Select(o =>
                        new GMapCoordinate
                        {
                            Lat = o.Latitude,
                            Lng = o.Longitude
                        }));
                    return pol;
                });

            var comuna = await _context.Communes
                .Include(c => c.Province)
                .Include(c => c.Polygons)
                    .ThenInclude(c => c.Vertices)
                    .Where(c => c.CatchmentAreaId.HasValue && c.Polygons.Any())
                    .ToListAsync().ConfigureAwait(false);

            map = map.Concat(comuna.Select(c =>
            {
                var pol = new GMapPolygon
                {
                    Id = c.Id,
                    Name = $"{_localizer["Commune"]} {c.Name}",
                    Provincia = c.Province.Name,
                    Region = "Los Lagos"
                };
                pol.Position.AddRange(c.Polygons
                    .Select(p => p.Vertices.OrderBy(o => o.Order).Select(o =>
                    new GMapCoordinate
                    {
                        Lat = o.Latitude,
                        Lng = o.Longitude
                    })).ToList());
                return pol;
            }));
            if (User.Identity.IsAuthenticated)
            {
                var psmb = await _context.PsmbAreas
                    .Include(p => p.Commune)
                        .ThenInclude(p => p.Province)
                    .Include(p => p.Polygon)
                        .ThenInclude(p => p.Vertices)
                    .Where(c => c.PolygonId.HasValue && c.Commune.CatchmentAreaId.HasValue)
                    .ToListAsync().ConfigureAwait(false);
                map = map.Concat(psmb
                    .Select(c =>
                    {
                        var pol = new GMapPolygon
                        {
                            Id = c.Id * 100,
                            Name = "PSMB " + c.Name,
                            Comuna = c.Commune.Name,
                            Provincia = c.Commune.Province.Name,
                            Region = "Los Lagos"
                        };
                        pol.Position.Add(c.Polygon
                        .Vertices.OrderBy(o => o.Order).Select(o =>
                        new GMapCoordinate
                        {
                            Lat = o.Latitude,
                            Lng = o.Longitude
                        }));
                        return pol;
                    }));
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
            if (g.Count() > 1)
            {
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
            var mod = area / 100_000;
            if(mod > 1) area /= 100;
            assays = mod switch
            {
                0 => assays
                        .Where(e => e.Psmb.Commune.CatchmentAreaId == area),
                1 => assays
                        .Where(e => e.Psmb.CommuneId == area),
                _ => assays
                        .Where(e => e.PsmbId == area)
            };
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
    public class AmData
    {
        public string Date { get; set; }
        public double Value { get; set; }
    }
    public class GMapCoordinate
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }
    public class GMapPolygon
    {
        public List<IEnumerable<GMapCoordinate>> Position { get; } = new List<IEnumerable<GMapCoordinate>>();
        public int Id { get; set; }
        public string Name { get; set; }
        public string Comuna { get; set; }
        public string Provincia { get; set; }
        public string Region { get; set; }
    }
    public class IExport
    {
        public string Label { get; set; }
    }
    public class ExportMenu : IExport
    {
        public IEnumerable<IExport> Menu { get; set; }
    }
    public class ExportItem : IExport
    {
        public string Type { get; set; }
    }
    public class IChoices
    {
        public string Label { get; set; }
    }
    public class ChoicesItem : IChoices
    {
        public string Value { get; set; }
    }
    public class ChoicesGroup : IChoices
    {
        public int Id { get; set; }
        public IEnumerable<ChoicesItem> Choices { get; set; }
    }
}