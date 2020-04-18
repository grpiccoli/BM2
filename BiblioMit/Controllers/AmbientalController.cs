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

namespace BiblioMit.Controllers
{
    [Authorize]
    public class AmbientalController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AmbientalController(ApplicationDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        public IActionResult ExportMenu()
        {
            var model = User.Identity.IsAuthenticated ? 
            new List<object>
            {
                new
                {
                    label = "...",
                    menu = new List<object>
                    {
                        new
                        {
                            label = "Imagen",
                            menu = new List<object>
                            {
                                new { type = "png", label = "PNG" },
                                new { type = "jpg", label = "JPG" },
                                new { type = "gif", label = "GIF" },
                                new { type = "svg", label = "SVG" },
                                new { type = "pdf", label = "PDF" }
                            }
                        },
                        new
                        {
                            label = "Datos",
                            menu = new List<object>
                            {
                                new { type = "json", label = "JSON" },
                                new { type = "csv", label = "CSV" },
                                new { type = "xlsx", label = "XLSX" }
                            }
                        },
                        new { label = "Imprimir", type = "print" }
                    }
                }
            }:
            new List<object>
            {
                new
                {
                    label = "...",
                    menu = new List<object>
                    {
                        new
                        {
                            label = "Imagen",
                            menu = new List<object>
                            {
                                new { type = "jpg", label = "JPG" },
                            }
                        },
                        new { label = "Imprimir", type = "print" }
                    }
                }
            };

            return Json(model);
        }

        [AllowAnonymous]
        // GET: Arrivals
        public async Task<IActionResult> PSMBList()
        {
            var areas = new List<object>
            {
                new
                {
                    label = "Cuencas",
                    id = 1,
                    choices = new List<object>
                    {
                        new
                        {
                            value = "1",
                            label = "Norte",
                            selected = true
                        },
                        new
                        {
                            value = "3",
                            label = "Centro"
                        },
                        new
                        {
                            value = "2",
                            label = "Sur"
                        }
                    }
                }
            };

            var areasList = await _context.Communes
                .Include(c => c.CatchmentArea)
                //.Include(c => c.Polygons)
                //.Include(c => c.Centres)
                //    .ThenInclude(c => c.EnsayoFitos)
                //.Where(c => c.Polygons.Any() && c.Centres.Any(e => e.EnsayoFitos.Any()))
                .ToListAsync().ConfigureAwait(false);

            areas.AddRange(areasList
                .GroupBy(c => c.CatchmentAreaId)
                .Select(g => new
                {
                    label = "Cuenca " + g.First().CatchmentArea.Name,
                    id = g.Key + 1,
                    choices = g
                    .Select(i => new
                    {
                        value = i.Id.ToString(CultureInfo.InvariantCulture),
                        label = $"{i.Name}"
                    })
                }));

            if (User.Identity.IsAuthenticated)
            {
                var psmbsList = await _context.PsmbAreas
                    .Include(p => p.Commune)
                        .ThenInclude(c => c.CatchmentArea)
                    //.Include(p => p.Coordinates)
                    //.Include(p => p.Centres)
                    //    .ThenInclude(c => c.EnsayoFitos)
                    //.Where(c => c.Coordinates.Any() && c.Centres.Any(e => e.EnsayoFitos.Any()))
                .ToListAsync().ConfigureAwait(false);

                areas.AddRange(psmbsList
                .GroupBy(c => c.Commune.CatchmentAreaId)
                .Select(g => new
                {
                    label = "Cuenca " + g.First().Commune.CatchmentArea.Name,
                    id = g.Key + 1,
                    choices = g
                    .Select(i => new
                    {
                        value = (i.Id * 100).ToString(CultureInfo.InvariantCulture),
                        label = $"{i.Id} {i.Name}, {i.Commune.Name}"
                    })
                }));
            }
            return Json(areas);
        }

        [AllowAnonymous]
        public JsonResult VariableList()
        {
            var fito = new List<object>
            {
                new
                {
                    value = "phy",
                    label = "Fitoplancton Total"
                }
            };
            fito.AddRange(_context.PhylogeneticGroups.Select(p => new
            {
                value = p.NormalizedName,
                label = $"{p.Name} Total"
            }));

            var result = new List<object>
            {
                new
                {
                    label = "Variables Oceanográficas",
                    id = 1,
                    choices = new List<object>
                    {
                        new
                        {
                            value = "t",
                            label = "Temperatura",
                            selected = true
                        },
                        new
                        {
                            value = "ph",
                            label = "pH"
                        },
                        new
                        {
                            value = "sal",
                            label = "Salinidad"
                        },
                        new
                        {
                            value = "o2",
                            label = "Oxígeno"
                        }
                    }
                },
                new
                {
                    label = "Fitoplancton",
                    id = 2,
                    choices = fito
                }
            };
            return Json(result);
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
        public IActionResult MapData()
        {
            var psmb = _context.PsmbAreas
                .Include(p => p.Farms)
                .Include(p => p.Polygon)
                    .ThenInclude(p => p.Vertices)
                .Where(c => c.PolygonId.HasValue && (c.PlanktonAssays.Any() || c.Farms.Any(e => e.PlanktonAssays.Any())))
                .Select(c => new
                {
                    position = c.Polygon.Vertices.OrderBy(o => o.Order).Select(o => new
                    {
                        lat = o.Latitude,
                        lng = o.Longitude
                    }),
                    id = c.Id * 100,
                    name = "PSMB " + c.Name,
                    comuna = c.Commune.Name,
                    provincia = c.Commune.Province.Name,
                    region = c.Commune.Province.Region.Name
                });

            var comuna = _context.Communes
                .Where(c => c.Polygons.Any() 
                && c.Psmbs.Any(e => e.PlanktonAssays.Any()))
                .Select(c => new
                {
                    position = c.Polygons.Select(p => p.Vertices.OrderBy(o => o.Order).Select(o => new
                    {
                        lat = o.Latitude,
                        lng = o.Longitude
                    })),
                    c.Id,
                    name = "Comuna " + c.Name,
                    provincia = c.Province.Name,
                    region = c.Province.Region.Name
                });

            var cuencas = _context.CatchmentAreas
                .Select(c => new
                {
                    position = c.Polygon.Vertices.OrderBy(o => o.Order).Select(o => new
                    {
                        lat = o.Latitude,
                        lng = o.Longitude
                    }),
                    c.Id,
                    name = "Cuenca " + c.Name,
                    region = "Los Lagos"
                });

            var map = new List<object>();
            map.AddRange(cuencas);
            map.AddRange(comuna);
            if (User.Identity.IsAuthenticated) 
            map.AddRange(psmb);

            return Json(map);
        }
        [AllowAnonymous]
        public IActionResult FitoData(int psmb, string var, string start, string end)
        {
            var i = Convert.ToDateTime(start, CultureInfo.InvariantCulture);
            var f = Convert.ToDateTime(end, CultureInfo.InvariantCulture);
            var date = Enumerable.Range(0, 1 + f.Subtract(i).Days)
                .Select(offset => new PlanktonAssay { SamplingDate = i.AddDays(offset) });
            IEnumerable<ExpandoObject> data = new List<ExpandoObject>() { };
            var ensayos = new List<PlanktonAssay>();
            if (psmb < 100) // Cuenca
            {
                ensayos = _context.PlanktonAssays
                    .Where(e => e.Psmb.Commune.CatchmentAreaId == psmb 
                    //&& e.SamplingDate.HasValue
                    && e.SamplingDate >= i && e.SamplingDate <= f)
                    .Include(e => e.Phytoplanktons)
                        .ThenInclude(f => f.Species)
                                .ThenInclude(f => f.Genus)
                            .ThenInclude(f => f.Group)
                    .ToList();
            }
            else if (psmb < 20_000) //Comuna
            {
                ensayos = _context.PlanktonAssays
                        .Where(e => e.Psmb.CommuneId == psmb
                        //&& e.SamplingDate.HasValue
                        && e.SamplingDate >= i && e.SamplingDate <= f)
                        .Include(e => e.Phytoplanktons)
                            .ThenInclude(f => f.Species)
                                .ThenInclude(f => f.Genus)
                                .ThenInclude(f => f.Group)
                        .ToList();
            }
            else //PSMB * 100
            {
                var psmbs = psmb / 100;
                ensayos = _context.PlanktonAssays
                        .Where(e => e.PsmbId == psmbs
                        //&& e.SamplingDate.HasValue
                        && e.SamplingDate >= i && e.SamplingDate <= f)
                        .Include(e => e.Phytoplanktons)
                            .ThenInclude(f => f.Species)
                                .ThenInclude(f => f.Genus)
                                    .ThenInclude(f => f.Group)
                        .ToList();
            }
            ensayos.AddRange(date);
            data = var switch
            {
                "phy" => ensayos
                        .GroupBy(e => e.SamplingDate.Date)
                        .OrderBy(g => g.Key)
                        .Select(g =>
                        {
                            dynamic expando = new ExpandoObject();
                            expando.date = g.Key.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                            var cs = g.Where(m => m.Phytoplanktons != null)
                            .SelectMany(m => m.Phytoplanktons.Select(p => p.C));
                            ((IDictionary<string, object>)expando)
                            .Add($"{var}_{psmb}", cs.Any() ? (double?)Math.Round(cs.Average(), 2) : null);
                            return (ExpandoObject)expando;
                        }),
                _ => ensayos
                     .GroupBy(e => e.SamplingDate.Date)
                     .OrderBy(g => g.Key)
                     .Select(g =>
                     {
                         dynamic expando = new ExpandoObject();
                         expando.date = g.Key.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                         var cs = g.Where(m => m.Phytoplanktons != null 
                         && m.Phytoplanktons.Any(p => p.Species.Genus.Group.NormalizedName.Equals(var, StringComparison.Ordinal)))
                         .SelectMany(m => m.Phytoplanktons
                         .Where(p => p.Species.Genus.Group.NormalizedName.Equals(var, StringComparison.Ordinal)).Select(p => p.C));
                         ((IDictionary<string, object>)expando)
                         .Add($"{var}_{psmb}", cs.Any() ?
                         (double?)Math.Round(cs.Average(), 2) : null);
                         return (ExpandoObject)expando;
                     }),//var id = Convert.ToInt16(var, CultureInfo.InvariantCulture);
            };
            return Json(data);
        }
        //[HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ProducesResponseType(typeof(JsonResult), StatusCodes.Status200OK)]
        public IActionResult GraphData(int psmb, string var, string start, string end)
        {
            var i = Convert.ToDateTime(start, CultureInfo.InvariantCulture);
            var f = Convert.ToDateTime(end, CultureInfo.InvariantCulture);
            var date = Enumerable.Range(0, 1 + f.Subtract(i).Days)
                .Select(offset => new PlanktonAssay { SamplingDate = i.AddDays(offset) });
            var index = new Dictionary<string, string>
            {
                { "t", "Temperatura" },
                { "ph", "Ph" },
                { "sal", "Salinidad" },
                { "o2", "Oxigeno" }
            };
            var ensayos = new List<PlanktonAssay>();
            if (psmb < 100) // Cuenca
            {
                ensayos = _context.PlanktonAssays
                        .Where(e => e.Psmb.Commune.CatchmentAreaId == psmb 
                        //&& e.SamplingDate.HasValue
                        && e.SamplingDate >= i && e.SamplingDate <= f)
                        .ToList();
            }
            else if (psmb < 20_000) //Comuna
            {
                ensayos = _context.PlanktonAssays
                        .Where(e => e.Psmb.CommuneId == psmb
                        //&& e.SamplingDate.HasValue
                        && e.SamplingDate >= i && e.SamplingDate <= f)
                        .ToList();
            }
            else //PSMB * 100
            {
                var psmbs = psmb / 100;
                ensayos = _context.PlanktonAssays
                        .Where(e => e.PsmbId == psmbs
                        //&& e.SamplingDate.HasValue
                        && e.SamplingDate >= i && e.SamplingDate <= f)
                        .ToList();
            }

            ensayos.AddRange(date);
            var data = ensayos
            .GroupBy(e => e.SamplingDate.Date)
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                dynamic expando = new ExpandoObject();
                expando.date = g.Key.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                ((IDictionary<string, object>)expando)
                .Add($"{var}_{psmb}", g.Any(m => m[index[var]] != null) ? (double?)Math.Round(g.Average(m => (double?)m[index[var]]).Value, 2) : null);
                return (ExpandoObject)expando;
            }).ToList();
            
            return Json(data);
        }
        [AllowAnonymous]
        public IActionResult Map(int[] c, int[] i)
        {
            var selc = c.ToList();
            var seli = i.ToList();

            TextInfo textInfo = new CultureInfo("es-CL", false).TextInfo;

            ViewData["c"] = string.Join(",", c);
            ViewData["i"] = string.Join(",", i);

            var centres = _context.PsmbAreas
                .Include(a => a.Polygon)
                .Include(a => a.Commune)
                    .ThenInclude(a => a.Province)
                    .ThenInclude(a => a.Region)
                .Where(a => a.Farms.Any() && a.PolygonId.HasValue);

            return View(centres);
        }

        public JsonResult GetSpecies(string groupId) =>
            Json(_context.Phytoplanktons
                .Include(f => f.Species)
                    .ThenInclude(s => s.Genus)
                .Where(p => p.Species.Genus.Group.NormalizedName.Equals(groupId, StringComparison.Ordinal))
                .Select(p => new {
                    name = p.Species,
                    id = p.Species,
                    icon = "",
                    unit = "(Cel/mL)",
                    group = p.Species.Genus.GroupId
                }).Distinct());
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