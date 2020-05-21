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
        private readonly string _dateFormat;
        public AmbientalController(
            ApplicationDbContext context,
            IStringLocalizer<AmbientalController> localizer)
        {
            _localizer = localizer;
            _context = context;
            _dateFormat = "yyyy-MM-dd";
        }
        [AllowAnonymous]
        public IActionResult CuencaList()
        {
            var singlabel = _localizer["Catchment Area"];
            return Json(
                new ChoicesGroup
                {
                    Label = _localizer["Catchment Areas"],
                    Choices = _context.CatchmentAreas.Select(c => new ChoicesItem
                    {
                        Value = c.Id.ToString(CultureInfo.InvariantCulture),
                        Label = $"{singlabel} {c.Name}"
                    })
                });
        }
        [AllowAnonymous]
        public IActionResult ComunaList()
        {
            var singlabel = _localizer["Catchment Area"];
            return Json(_context.CatchmentAreas
                .Select(c => new ChoicesGroup
                {
                    Label = $"{singlabel} {c.Name}",
                    Choices = c.Communes
                    .Select(com => new ChoicesItem
                    {
                        Value = com.Id.ToString(CultureInfo.InvariantCulture),
                        Label = $"{com.Name} {c.Name}"
                    })
                }));
        }
        public IActionResult PsmbList() =>
            Json(_context.Communes
                .Where(c => c.CatchmentAreaId.HasValue)
                .Select(c => new ChoicesGroup
                {
                    Label = c.Name,
                    Choices = c.Psmbs
                    .Where(p => p.PolygonId.HasValue && p.PlanktonAssays.Any())
                    .Select(p => new ChoicesItem
                    {
                        Value = p.Id.ToString(CultureInfo.InvariantCulture),
                        Label = $"{p.Code} {p.Name} {c.Name}"
                    })
                }));
        [AllowAnonymous]
        // GET: Arrivals
        public async Task<IActionResult> PSMBsList()
        {
            var singlabel = _localizer["Catchment Area"];
            var catchment = new List<ChoicesGroup>
            {
                new ChoicesGroup
                {
                    Label = _localizer["Catchment Areas"],
                    Choices = _context.CatchmentAreas.Select(c => new ChoicesItem
                    {
                        Value = c.Id.ToString(CultureInfo.InvariantCulture),
                        Label = $"{singlabel} {c.Name}"
                    })
                }
            };
            var communes = await _context.CatchmentAreas
                .Select(c => new ChoicesGroup
                {
                    Label = $"{singlabel} {c.Name}",
                    Choices = c.Communes
                    .Select(com => new ChoicesItem
                    {
                        Value = com.Id.ToString(CultureInfo.InvariantCulture),
                        Label = $"{com.Name} {c.Name}"
                    })
                }).ToListAsync().ConfigureAwait(false);
            var areas = new List<ChoicesGroup>();
            areas.AddRange(catchment);
            areas.AddRange(communes);
            if (User.Identity.IsAuthenticated)
            {
                var psmbs = await _context.Communes
                    .Where(c => c.CatchmentAreaId.HasValue)
                    .Select(c => new ChoicesGroup
                    {
                        Label = c.Name,
                        Choices = c.Psmbs
                        .Where(p => p.PolygonId.HasValue && p.PlanktonAssays.Any())
                        .Select(p => new ChoicesItem
                        {
                            Value = p.Id.ToString(CultureInfo.InvariantCulture),
                            Label = $"{p.Code} {p.Name} {c.Name}"
                        })
                    }).ToListAsync().ConfigureAwait(false);
                areas.AddRange(psmbs);
            }
            return Json(areas);
        }

        [AllowAnonymous]
        public JsonResult VariableList()
        {
            var variables = Variable.t.Enum2ChoicesGroup("v").FirstOrDefault();
            var group = _localizer["Group"];
            var groups = new ChoicesGroup 
            {
                Label = _localizer["Phylogenetic Groups (Cel/mL)"],
                Choices = _context.PhylogeneticGroups.Select(p => new ChoicesItem
                {
                    Value = "f" + p.Id.ToString(CultureInfo.InvariantCulture),
                    Label = $"{p.Name} ({group})"
                })
            };
            var genus = _localizer["Genus"];
            var orders = new ChoicesGroup
            {
                Label = _localizer["Genera (Cel/mL)"],
                Choices = _context.GenusPhytoplanktons.Select(p => new ChoicesItem
                {
                    Value = "g" + p.Id.ToString(CultureInfo.InvariantCulture),
                    Label = $"{p.Name} ({genus})"
                })
            };
            var sp = _localizer["Species"];
            var species = new ChoicesGroup
            {
                Label = _localizer["Species (Cel/mL)"],
                Choices = _context.SpeciesPhytoplanktons
                .Include(s => s.Genus)
                .Select(p => new ChoicesItem
                {
                    Value = "s" + p.Id.ToString(CultureInfo.InvariantCulture),
                    Label = $"{p.GetFullName()} ({sp})"
                })
            };
            return Json(new List<ChoicesGroup> { variables, groups, orders, species });
        }
        public async Task<JsonResult> TLData(int a, int psmb, int sp, int? t, int? l, int? rs, int? s, string start, string end) {
            var i = Convert.ToDateTime(start, CultureInfo.InvariantCulture);
            var f = Convert.ToDateTime(end, CultureInfo.InvariantCulture);
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
            IQueryable<AmData> selection = new List<AmData>() as IQueryable<AmData>;
            switch (a)
            {
                case 11:
                    if (t.HasValue)
                    {
                        var range = t.Value % 10;
                        var db = _context.Tallas as IQueryable<Talla>;
                        if(range != 8) db = db.Where(tl => tl.Range == (Range)range);
                        if (psmb != 23) db = db.Where(tl => tl.SpecieSeed.Seed.Farm.Code == psmbs[psmb]);
                        if (sp != 34) db = db.Where(tl => tl.SpecieSeed.SpecieId == sps[sp]);
                        selection = db.Select(tl => new AmData { 
                            Date = tl.SpecieSeed.Seed.Date.ToString(_dateFormat,CultureInfo.InvariantCulture),
                        Value = tl.Proportion });
                    }
                    break;
                case 12:
                    if (l.HasValue)
                    {
                        var type = l.Value % 10;
                        var db = _context.Larvas as IQueryable<Larva>;
                        if (type != 3) db = db.Where(tl => tl.LarvaType == (LarvaType)type);
                        if (psmb != 23) db = db.Where(tl => tl.Larvae.Farm.Code == psmbs[psmb]);
                        if (sp != 34) db = db.Where(tl => tl.SpecieId == sps[sp]);
                        selection = db.Select(tl => new AmData
                        {
                            Date = tl.Larvae.Date.ToString(_dateFormat, CultureInfo.InvariantCulture),
                            Value = tl.Count
                        });
                    }
                    break;
                case 13:
                    if (s.HasValue)
                    {
                        var db = _context.Spawnings as IQueryable<Spawning>;
                        if (psmb != 23) db = db.Where(tl => tl.Farm.Code == psmbs[psmb]);
                        selection = db.Select(tl => new AmData
                        {
                            Date = tl.Date.ToString(_dateFormat, CultureInfo.InvariantCulture),
                            Value = s.Value == 70 ? tl.FemaleIG : tl.MaleIG
                        });
                    }
                    break;
                case 14:
                    if (true)
                    {
                        var db = _context.SpecieSeeds as IQueryable<SpecieSeed>;
                        if (psmb != 23) db = db.Where(tl => tl.Seed.Farm.Code == psmbs[psmb]);
                        if (sp != 34) db = db.Where(tl => tl.SpecieId == sps[sp]);
                        selection = db.Select(tl => new AmData
                        {
                            Date = tl.Seed.Date.ToString(_dateFormat, CultureInfo.InvariantCulture),
                            Value = tl.Capture
                        });
                    }
                    break;
                case 15:
                    if (rs.HasValue)
                    {
                        var stage = rs.Value % 10;
                        var db = _context.ReproductiveStages.Where(tl => tl.Stage == (Stage)stage);
                        if (psmb != 23) db = db.Where(tl => tl.Spawning.Farm.Code == psmbs[psmb]);
                        selection = db.Select(tl => new AmData
                        {
                            Date = tl.Spawning.Date.ToString(_dateFormat, CultureInfo.InvariantCulture),
                            Value = tl.Proportion
                        });
                    }
                    break;
                case 16:
                    if (s.HasValue)
                    {
                        var db = _context.Spawnings as IQueryable<Spawning>;
                        if (psmb != 23) db = db.Where(tl => tl.Farm.Code == psmbs[psmb]);
                        selection = db.Select(tl => new AmData
                        {
                            Date = tl.Date.ToString(_dateFormat, CultureInfo.InvariantCulture),
                            Value = s.Value == 70 ? tl.FemaleProportion : tl.MaleProportion
                        });
                    }
                    break;
                case 17:
                    if (true)
                    {
                        var db = _context.SpecieSeeds as IQueryable<SpecieSeed>;
                        if (psmb == 23) db = db.Where(tl => tl.Seed.Farm.Code == psmbs[psmb]);
                        if (psmb == 23) db = db.Where(tl => tl.SpecieId == sps[sp]);
                        selection = db.Select(tl => new AmData
                        {
                            Date = tl.Seed.Date.ToString(_dateFormat, CultureInfo.InvariantCulture),
                            Value = tl.Proportion
                        });
                    }
                    break;
            }
            var list = await selection.ToListAsync().ConfigureAwait(false);
            return Json(list.GroupBy(tl => tl.Date)
            .OrderBy(g => g.Key)
            .Select(g =>
                new AmData
                {
                    Date = g.Key,
                    Value = Math.Round(g.Select(gg => gg.Value).Average())
                }));
        }
        public JsonResult TLList() =>
            Json(new List<object>
            {
                new ChoicesGroup
                {
                    Label = _localizer["Analysis"],
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
        public IActionResult CuencaData() =>
            Json(_context.CatchmentAreas
                    .Select(c => new GMapPolygon
                    {
                        Id = c.Id,
                        Name = $"{_localizer["Catchment Area"]} {c.Name}",
                        Code = c.Id.ToString(CultureInfo.InvariantCulture),
                        Position = new[]{c.Polygon.Vertices.Select(o =>
                        new GMapCoordinate
                        {
                            Lat = o.Latitude,
                            Lng = o.Longitude
                        })}
                    }));
        [AllowAnonymous]
        public IActionResult ComunaData()
        {
            var com = _localizer["Commune"];
            return Json(_context.Communes
                    .Where(c => c.CatchmentAreaId.HasValue)
                    .Select(c =>
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
                }));
        }
        public IActionResult PsmbData() =>
            Json(_context.PsmbAreas
                    .Where(c => c.Commune.CatchmentAreaId.HasValue && c.PolygonId.HasValue && c.PlanktonAssays.Any())
                    .Select(c => new GMapPolygon
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
                    }));
        public IActionResult SearchPlanktonAssays() => View();
        public IActionResult BuscarInformes(int id, string start, string end)
        {
            var i = Convert.ToDateTime(start, CultureInfo.InvariantCulture);
            var f = Convert.ToDateTime(end, CultureInfo.InvariantCulture);
            var plankton = _context.PlanktonAssays.Where(p => p.SamplingDate >= i && p.SamplingDate <= f);
            if (id < 4)
            {
                plankton = plankton.Where(p => p.Psmb.Commune.CatchmentAreaId == id);
            }
            else if (id < 100_000)
            {
                plankton = plankton.Where(p => p.PsmbId == id);
            }
            else
            {
                plankton = plankton.Where(p => p.Psmb.CommuneId == id);
            }
            return Json(plankton.Select(p => new { p.Id, p.SamplingDate, p.Temperature, p.Oxigen, p.Ph, p.Salinity }));
        }
        //private static AmData SelectData(
        //    IGrouping<DateTime, PlanktonAssay> g,
        //    int var, char id)
        //{
        //    if (g == null) return null;
        //    var response = new AmData
        //    {
        //        Date = g.Key.ToString(CultureInfo.InvariantCulture)
        //    };
        //    if (!(id == 'v' && var != 4))
        //    {
        //        var cs = id switch { 
        //            'f' => g.SelectMany(m => m.Phytoplanktons
        //            .Where(p => p.Species.Genus.GroupId == var).Select(p => p.C)),
        //            'g' => g.SelectMany(m => m.Phytoplanktons
        //            .Where(p => p.Species.GenusId == var).Select(p => p.C)),
        //            's' => g.SelectMany(m => m.Phytoplanktons
        //            .Where(p => p.SpeciesId == var).Select(p => p.C)),
        //            _ => g.SelectMany(m => m.Phytoplanktons.Select(p => p.C))
        //        };
        //        if (cs.Any())
        //        {
        //            //response.Id = string.Join(", ", tmp.Select(m => m.Id));
        //            response.Value = Math.Round(cs.Average(), 2);
        //            return response;
        //        }
        //    }
        //    else
        //    {
        //        string attr = var switch
        //        {
        //            0 => "Temperature",
        //            1 => "Ph",
        //            2 => "Oxigen",
        //            3 => "Salinity",
        //            _ => ""
        //        };
        //        if(g.Any(m => m[attr] != null))
        //        {
        //            //response.Id = string.Join(", ", g.Select(m => m.Id));
        //            response.Value = Math.Round(g.Average(m => (double?)m[attr]).Value, 2);
        //            return response;
        //        }
        //    }
        //    return null;
        //}
        [AllowAnonymous]
        public async Task<IActionResult> Data(int area, string var, string start, string end)
        {
            if(string.IsNullOrWhiteSpace(var)) throw new ArgumentException(_localizer["var doesn't contain id"]);
            var type = var[0];
            var parsed = int.TryParse(var.Substring(1), out int id);
            if (!parsed) throw new ArgumentException(_localizer["var doesn't contain id"]);
            IQueryable<AmData> selection;
            var i = Convert.ToDateTime(start, CultureInfo.InvariantCulture);
            var f = Convert.ToDateTime(end, CultureInfo.InvariantCulture);
            if (!(type == 'v' && id != 4))
            {
                var phyto = _context.Phytoplanktons.Where(e => e.PlanktonAssay.SamplingDate >= i && e.PlanktonAssay.SamplingDate <= f);
                phyto = type switch
                {
                    'f' => phyto
                    .Where(p => p.Species.Genus.GroupId == id),
                    'g' => phyto
                    .Where(p => p.Species.GenusId == id),
                    's' => phyto
                    .Where(p => p.SpeciesId == id),
                    _ => phyto
                };
                if (area < 4)
                {
                    phyto = phyto.Where(e => e.PlanktonAssay.Psmb.Commune.CatchmentAreaId == area);
                }
                else if (area < 100_000)
                {
                    phyto = phyto.Where(e => e.PlanktonAssay.PsmbId == area);
                }
                else
                {
                    phyto = phyto.Where(e => e.PlanktonAssay.Psmb.CommuneId == area);
                }
                selection = phyto.Select(p => new AmData 
                { 
                    Date = p.PlanktonAssay.SamplingDate.ToString(_dateFormat, CultureInfo.InvariantCulture), 
                    Value = p.C 
                });
            }
            else
            {
                var assays = _context.PlanktonAssays.Where(e => e.SamplingDate >= i && e.SamplingDate <= f);
                if (area < 4)
                {
                    assays = assays.Where(e => e.Psmb.Commune.CatchmentAreaId == area);
                }
                else if (area < 100_000)
                {
                    assays = assays.Where(e => e.PsmbId == area);
                }
                else
                {
                    assays = assays.Where(e => e.Psmb.CommuneId == area);
                }
                selection = id switch
                {
                    //ph
                    1 => assays
                    .Where(a => a.Ph.HasValue)
                    .Select(a => new AmData { Date = a.SamplingDate.ToString(_dateFormat, CultureInfo.InvariantCulture), Value = a.Ph.Value }),
                    //ox
                    2 => assays
                    .Where(a => a.Oxigen.HasValue)
                    .Select(a => new AmData { Date = a.SamplingDate.ToString(_dateFormat, CultureInfo.InvariantCulture), Value = a.Oxigen.Value }),
                    //sal
                    3 => assays
                    .Where(a => a.Salinity.HasValue)
                    .Select(a => new AmData { Date = a.SamplingDate.ToString(_dateFormat, CultureInfo.InvariantCulture), Value = a.Salinity.Value }),
                    //temp
                    _ => assays
                    .Where(a => a.Temperature.HasValue)
                    .Select(a => new AmData { Date = a.SamplingDate.ToString(_dateFormat, CultureInfo.InvariantCulture), Value = a.Temperature.Value })
                };
            }
            var ensayos = await selection.ToListAsync().ConfigureAwait(false);
            return Json(ensayos.GroupBy(e => e.Date)
                .OrderBy(g => g.Key)
                .Select(g => new AmData { 
                    Date = g.Key, 
                    Value = Math.Round(g.Select(i => i.Value).Average(), 2) 
                }));
        }
        //public async Task<IActionResult> Data(int area, string var, string start, string end, int id)
        //{
        //    var data = await Data(area, var, start, end, id).ConfigureAwait(false);
        //    return Json(data);
        //}
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ProducesResponseType(typeof(JsonResult), StatusCodes.Status200OK)]
        //public async Task<IActionResult> GraphData(int area, string var, string start, string end)
        //{
        //    var data = await Data(area, var, start, end).ConfigureAwait(false);           
        //    return Json(data);
        //}
        [AllowAnonymous]
        public IActionResult Graph()
        {
            ViewData["start"] = _context.PlanktonAssays.Min(e => e.SamplingDate)
                .ToString(_dateFormat, CultureInfo.InvariantCulture);
            ViewData["end"] = _context.PlanktonAssays.Max(e => e.SamplingDate)
                .ToString(_dateFormat, CultureInfo.InvariantCulture);
            return View();
        }
    }
}