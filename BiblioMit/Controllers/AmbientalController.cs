using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BiblioMit.Data;
using BiblioMit.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using Range = BiblioMit.Models.Range;
using BiblioMit.Models.Entities.Semaforo;
using BiblioMit.Blazor;
using BiblioMit.Extensions;
using Microsoft.Extensions.Localization;
using BiblioMit.Models.VM;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using BiblioMit.Models.VM.AmbientalVM;

namespace BiblioMit.Controllers
{
    [Authorize]
    public class AmbientalController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IStringLocalizer<AmbientalController> _localizer;
        private readonly string _dateFormat;
        private readonly IWebHostEnvironment _environment;
        public AmbientalController(
            ApplicationDbContext context,
            IWebHostEnvironment environment,
            IStringLocalizer<AmbientalController> localizer)
        {
            _localizer = localizer;
            _context = context;
            _environment = environment;
            _dateFormat = "yyyy-MM-dd";
        }
        [HttpGet]
        public IActionResult GetContent([Bind("Name,Code,Commune,Province,Area,Lang")]Content model)
        {
            return PartialView("_GetContent", model);
        }
        [HttpGet]
        public IActionResult PullPlankton()
        {
            var file = Path.Combine(_environment.ContentRootPath, "html", "PullRecords.html");
            var htmlString = System.IO.File.ReadAllLines(file);
            return View("PullPlankton", string.Join("",htmlString));
        }
        private IQueryable<ChoicesItem> CuencaChoices()
        {
            var singlabel = _localizer["Catchment Area"] + " ";
            return _context.CatchmentAreas
                .AsNoTracking()
                .Select(c => new ChoicesItemSelected
            {
                Value = c.Id,
                Label = singlabel + c.Name,
                Selected = c.Id == 1
            });
        }
        private IQueryable<ChoicesItem> CommuneChoices() => _context.CatchmentAreas
            .AsNoTracking()
            .SelectMany(c => c.Communes
            .Select(com => new ChoicesItem
            {
                Value = com.Id,
                Label = com.Name + " " + c.Name
            }));
        private IQueryable<ChoicesItem> PsmbChoices() => _context.CatchmentAreas
            .AsNoTracking()
            .SelectMany(c => c.Communes
        .SelectMany(com => com.Psmbs.Where(p => p.PolygonId.HasValue && p.PlanktonAssays.Any())
        .Select(p => new ChoicesItem
        {
            Value = p.Id,
            Label = p.Code + " " + p.Name + " " + c.Name
        })));
        private IQueryable<ChoicesItem> PublicAreaChoices() => CuencaChoices().Union(CommuneChoices());
        private IQueryable<ChoicesItem> PrivateAreaChoices() => PublicAreaChoices().Union(PsmbChoices());
        [HttpGet]
        public IActionResult PublicAreasList() => Json(PublicAreaChoices());
        [HttpGet]
        public IActionResult PrivateAreasList() => Json(PrivateAreaChoices());
        [HttpGet]
        public IActionResult CuencaList()
        {
            var singlabel = _localizer["Catchment Area"] + " ";
            return Json(new ChoicesGroup
            {
                    Label = _localizer["Catchment Areas"],
                    Choices = _context.CatchmentAreas
                    .AsNoTracking()
                    .Select(c => new ChoicesItem
                    {
                        Value = c.Id,
                        Label = singlabel + c.Name
                    })
                });
        }
        [HttpGet]
        public IActionResult CustomVarList() => Json(new ChoicesGroup
        {
            Label = _localizer["Custom Variables"],
            Choices = _context.VariableTypes
            .AsNoTracking()
                    .Select(com => new ChoicesItem
                    {
                        Value = com.Id,
                        Label = com.Name + " (" + com.Units + ")"
                    })
        });
        [HttpGet]
        public IActionResult RegionList() => Json(new ChoicesGroup
            {
                Label = _localizer["Regions"],
                Choices = _context.Regions
            .AsNoTracking()
                    .Select(com => new ChoicesItem
                    {
                        Value = com.Id,
                        Label = com.Name
                    })
            });
        [HttpGet]
        public IActionResult ProvinciaList() => Json(Provincia());
        private IQueryable<ChoicesGroup> Provincia() =>
            _context.Regions
            .AsNoTracking()
            .Select(c => new ChoicesGroup
            {
                Label = c.Name,
                Choices = c.Provinces
                    .Select(com => new ChoicesItem
                    {
                        Value = com.Id,
                        Label = com.Name + ", " + c.Name
                    })
            });
        [HttpGet]
        public IActionResult ComunaList()
        {
            var singlabel = _localizer["Catchment Area"] + " ";
            return Json(Comuna(singlabel));
        }
        private IQueryable<ChoicesGroup> Comuna(string singlabel) =>
            _context.CatchmentAreas
            .AsNoTracking()
            .Select(c => new ChoicesGroup
            {
                Label = singlabel + c.Name,
                Choices = c.Communes
                    .Select(com => new ChoicesItem
                    {
                        Value = com.Id,
                        Label = com.Name + " " + c.Name
                    })
            });
        [HttpGet]
        public IActionResult PsmbList() => Json(_context.Communes
            .AsNoTracking()
                .Where(c => c.CatchmentAreaId.HasValue)
                .Select(c => new ChoicesGroup
                {
                    Label = c.Name,
                    Choices = c.Psmbs
                    .Where(p => p.PolygonId.HasValue && p.PlanktonAssays.Any())
                    .Select(p => new ChoicesItem
                    {
                        Value = p.Id,
                        Label = p.Code + " " + p.Name + " " + c.Name
                    })
                }));
        [HttpGet]
        public IActionResult ProvinciaFarmList() => Json(Provincia());
        [HttpGet]
        public IActionResult ComunaFarmList()
        {
            var singlabel = _localizer["Catchment Area"] + " ";
            return Json(Comuna(singlabel));
        }
        [HttpGet]
        public IActionResult ProvinciaResearchList() => Json(_context.Regions
            .AsNoTracking()
            .Select(c => new ChoicesGroup
            {
                Label = c.Name,
                Choices = c.Provinces
                .Where(c => c.Communes.Any(c => c.Psmbs.Any(p => p.Discriminator == Models.Entities.Centres.PsmbType.ResearchCentre)))
                        .Select(com => new ChoicesItem
                        {
                            Value = com.Id,
                            Label = com.Name + ", " + c.Name
                        })
            }));
        [HttpGet]
        public IActionResult ComunaResearchList() => Json(_context.Provinces
            .AsNoTracking()
            .Select(c => new ChoicesGroup
            {
                Label = c.Name,
                Choices = c.Communes
                .Where(c => c.Psmbs.Any(p => p.Discriminator == Models.Entities.Centres.PsmbType.ResearchCentre))
                        .Select(com => new ChoicesItem
                        {
                            Value = com.Id,
                            Label = com.Name + ", " + c.Name
                        })
            }));
        [HttpGet]
        public IActionResult FarmData() => Json(_context.PsmbAreas
            .AsNoTracking()
            .Where(c => c.PolygonId.HasValue && c.CommuneId.HasValue)
            .Select(c => new GMapPolygonCentre
            {
                Id = c.Id,
                Name = c.Code + " " + c.Name ?? "",
                Comuna = c.Commune.Name,
                ComunaId = c.CommuneId.Value,
                Provincia = c.Commune.Province.Name,
                Code = c.Code,
                //BusinessName = c.Company.BusinessName ?? "",
                //Rut = c.CompanyId.Value,
                Position = c.Polygon
                        .Vertices.Select(o => new GMapCoordinate
                        {
                            Lat = o.Latitude,
                            Lng = o.Longitude
                        })
            }));
        [HttpGet]
        public IActionResult ResearchData() => Json(_context.ResearchCentres
            .AsNoTracking()
                    .Where(c => c.PolygonId.HasValue && c.CommuneId.HasValue)
                    .Select(c => new GMapPolygonCentre
                    {
                        Id = c.Id,
                        Name = c.Name + " (" + c.Acronym + ")",
                        Comuna = c.Commune.Name,
                        ComunaId = c.CommuneId.Value,
                        Provincia = c.Commune.Province.Name,
                        Region = c.Commune.Province.Region.Name,
                        BusinessName = c.Company.BusinessName,
                        Rut = c.Company.Id,
                        Position = c.Polygon
                        .Vertices.Select(o => new GMapCoordinate
                        {
                            Lat = o.Latitude,
                            Lng = o.Longitude
                        })
                    }));
        [HttpGet]
        public IActionResult FarmList() => Json(_context.Farms
            .AsNoTracking()
                    .Where(p => p.PolygonId.HasValue)
                    .Select(p => new ChoicesItem
                    {
                        Value = p.Id,
                        Label = p.Code + " " + p.Name
                    }));
        [HttpGet]
        public IActionResult CompanyList() => Json(_context.Companies
            .AsNoTracking()
                    .Where(p => p.Id > 900_000 && p.Psmbs.Any(f => f.Discriminator == Models.Entities.Centres.PsmbType.Farm && f.PolygonId.HasValue))
                    .Select(p => new ChoicesItem
                    {
                        Value = p.Id,
                        Label = p.BusinessName + " (" + p.GetRUT() + ")"
                    }));
        [HttpGet]
        public IActionResult ResearchList() => Json(_context.ResearchCentres
            .AsNoTracking()
                    .Where(p => p.PolygonId.HasValue)
                    .Select(p => new ChoicesItem
                    {
                        Value = p.CompanyId,
                        Label =  p.Name + " (" + p.Acronym + ")"
                    }));
        [HttpGet]
        public IActionResult InstitutionList() => Json(_context.Companies
            .AsNoTracking()
                    .Where(p => p.Psmbs.Any(p => p.Discriminator == Models.Entities.Centres.PsmbType.ResearchCentre))
                    .Select(p => new ChoicesItem
                    {
                        Value = p.Id,
                        Label = p.BusinessName + " (" + p.Acronym + ")"
                    }));
        [HttpGet]
        public JsonResult OceanVarList() => Json(Variable.t.Enum2ChoicesGroup("v").FirstOrDefault());
        [HttpGet]
        public JsonResult GroupVarList()
        {
            var group = " (" + _localizer["Group"] + ")";
            return Json(new ChoicesGroup
            {
                Label = _localizer["Phylogenetic Groups (Cel/mL)"],
                Choices = _context.PhylogeneticGroups
                .AsNoTracking()
                .Select(p => new ChoicesItem
                {
                    Value = "f" + p.Id,
                    Label = p.Name + group
                })
            });
        }
        [HttpGet]
        public JsonResult GenusVarList()
        {
            var genus = " (" + _localizer["Genus"] + ")";
            return Json(new ChoicesGroup
            {
                Label = _localizer["Genera (Cel/mL)"],
                Choices = _context.GenusPhytoplanktons
                .AsNoTracking()
                .Select(p => new ChoicesItem
                {
                    Value = "g" + p.Id,
                    Label = p.Name + genus
                })
            });
        }
        [HttpGet]
        public JsonResult SpeciesVarList()
        {
            var sp = " (" + _localizer["Species"] + ")";
            var species = new ChoicesGroup
            {
                Label = _localizer["Species (Cel/mL)"],
                Choices = _context.SpeciesPhytoplanktons
                .AsNoTracking()
                .Select(p => new ChoicesItem
                {
                    Value = "s" + p.Id,
                    Label = p.Genus.Name + " " + p.Name + sp
                })
            };
            return Json(species);
        }
        [ResponseCache(Duration = 60 * 60, VaryByQueryKeys = new string[] { "*" })]
        [HttpGet]
        public JsonResult TLData(int a, int psmb, int sp, int? v
            //, DateTime start, DateTime end
            )
        {
            //1 analysis, 2 psmb, 3 species, 4 size, 5 larva type, 6 rep stg, 7 sex
            if (a != 14 && a != 17 && !v.HasValue) throw new ArgumentNullException(_localizer["error"]);
            if ((a == 13 || a == 15 || a == 16) && sp != 31) return null;
            var psmbs = new Dictionary<int,int>{
                //Quetalco
                {20, 101990},
                //Vilipulli
                {21, 101017},
                //Carahue
                {22, 103633}
            };
            //1 chorito, 2 cholga, 3 choro, 4 all
            var sps = new Dictionary<int, int>{
                {31, 1},
                {32, 2},
                {33, 3}
            };
            //TallaRange 0-7
            //LarvaType 0 D, 1 U, 2 O
            //0 101990 Quetalco, 1 101017 Vilipulli, 2 103633 Carahue 23 all
            IQueryable<AmData> selection = new List<AmData>() as IQueryable<AmData>;
            switch (a)
            {
                //size
                case 11:
                    if (v.HasValue)
                    {
                        var range = v.Value % 10;
                        var db = _context.Tallas as IQueryable<Talla>;
                        if(range != 8) db = db.Where(tl => tl.Range == (Range)range);
                        if (psmb != 23) db = db.Where(tl => tl.SpecieSeed.Seed.Farm.Code == psmbs[psmb]);
                        if (sp != 34) db = db.Where(tl => tl.SpecieSeed.SpecieId == sps[sp]);
                        selection = db
                            .GroupBy(tl => tl.SpecieSeed.Seed.Date.Date)
                            .OrderBy(g => g.Key)
                            .Select(g => new AmData
                            {
                                Date = g.Key.ToString(_dateFormat, CultureInfo.InvariantCulture),
                                Value = Math.Round(g.Average(tl => tl.Proportion))
                            });
                    }
                    break;
                //larvae
                case 12:
                    if (v.HasValue)
                    {
                        var type = v.Value % 10;
                        var db = _context.Larvas as IQueryable<Larva>;
                        if (type != 3) db = db.Where(tl => tl.LarvaType == (LarvaType)type);
                        if (psmb != 23) db = db.Where(tl => tl.Larvae.Farm.Code == psmbs[psmb]);
                        if (sp != 34) db = db.Where(tl => tl.SpecieId == sps[sp]);
                        selection = db
                            .GroupBy(tl => tl.Larvae.Date.Date)
                            .OrderBy(g => g.Key)
                            .Select(g => new AmData
                            {
                                Date = g.Key.ToString(_dateFormat, CultureInfo.InvariantCulture),
                                Value = Math.Round(g.Average(tl => tl.Count))
                            });
                    }
                    break;
                //ig reproductores
                case 13:
                    if (v.HasValue)
                    {
                        var db = _context.Spawnings as IQueryable<Spawning>;
                        if (psmb != 23) db = db.Where(tl => tl.Farm.Code == psmbs[psmb]);
                        selection = db
                            .GroupBy(tl => tl.Date.Date)
                            .OrderBy(g => g.Key)
                            .Select(g => new AmData
                            {
                                Date = g.Key.ToString(_dateFormat, CultureInfo.InvariantCulture),
                                Value = Math.Round(g.Average(tl => v.Value == 70 ? tl.FemaleIG : tl.MaleIG))
                            });
                    }
                    break;
                //capture
                case 14:
                    if (true)
                    {
                        var db = _context.SpecieSeeds as IQueryable<SpecieSeed>;
                        if (psmb != 23) db = db.Where(tl => tl.Seed.Farm.Code == psmbs[psmb]);
                        if (sp != 34) db = db.Where(tl => tl.SpecieId == sps[sp]);
                        selection = db
                            .GroupBy(tl => tl.Seed.Date.Date)
                            .OrderBy(g => g.Key)
                            .Select(g => new AmData
                            {
                                Date = g.Key.ToString(_dateFormat, CultureInfo.InvariantCulture),
                                Value = Math.Round(g.Average(tl => tl.Capture))
                            });
                    }
                    break;
                //rep stage
                case 15:
                    if (v.HasValue)
                    {
                        var stage = v.Value % 10;
                        var db = _context.ReproductiveStages.Where(tl => tl.Stage == (Stage)stage);
                        if (psmb != 23) db = db.Where(tl => tl.Spawning.Farm.Code == psmbs[psmb]);
                        selection = db
                            .GroupBy(tl => tl.Spawning.Date.Date)
                            .OrderBy(g => g.Key)
                            .Select(g => new AmData
                            {
                                Date = g.Key.ToString(_dateFormat, CultureInfo.InvariantCulture),
                                Value = Math.Round(g.Average(tl => tl.Proportion))
                            });
                    }
                    break;
                //% sex
                case 16:
                    if (v.HasValue)
                    {
                        var db = _context.Spawnings as IQueryable<Spawning>;
                        if (psmb != 23) db = db.Where(tl => tl.Farm.Code == psmbs[psmb]);
                        selection = db
                            .GroupBy(tl => tl.Date.Date)
                            .OrderBy(g => g.Key)
                            .Select(g => new AmData
                            {
                                Date = g.Key.ToString(_dateFormat, CultureInfo.InvariantCulture),
                                Value = Math.Round(g.Average(tl => v.Value == 70 ? tl.FemaleProportion : tl.MaleProportion))
                            });
                    }
                    break;
                //% species
                case 17:
                    if (true)
                    {
                        var db = _context.SpecieSeeds as IQueryable<SpecieSeed>;
                        if (psmb != 23) db = db.Where(tl => tl.Seed.Farm.Code == psmbs[psmb]);
                        if (sp != 34) db = db.Where(tl => tl.SpecieId == sps[sp]);
                        selection = db
                            .GroupBy(tl => tl.Seed.Date.Date)
                            .OrderBy(g => g.Key)
                            .Select(g => new AmData
                        {
                            Date = g.Key.ToString(_dateFormat, CultureInfo.InvariantCulture),
                            Value = Math.Round(g.Average(tl => tl.Proportion))
                        });
                    }
                    break;
            }
            return Json(selection);
        }
        [HttpGet]
        public JsonResult TLList() =>
            Json(new List<object>
            {
                new ChoicesGroup
                {
                    Label = _localizer["Analysis"],
                    Choices = new List<ChoicesItem>{
                        new ChoicesItem{
                            Value = 14,
                            Label = _localizer["Capture per Species"]
                        },
                        new ChoicesItem{
                            Value = 17,
                            Label = _localizer["% Species"]
                        },
                        new ChoicesItem{
                            Value = 11,
                            Label = _localizer["% Size per Species"]
                        },
                        new ChoicesItem{
                            Value = 12,
                            Label = _localizer["Larvae"]
                        },
                        new ChoicesItem{
                            Value = 13,
                            Label = _localizer["IG Reproductores"]
                        },
                        new ChoicesItem{
                            Value = 15,
                            Label = _localizer["% Reproductive Stage"]
                        },
                        new ChoicesItem{
                            Value = 16,
                            Label = _localizer["% Sex"]
                        }
                    }
                },
                new ChoicesGroup
                {
                    Label = "PSMBs",
                    Choices = new List<ChoicesItem>{
                        new ChoicesItem{
                            Value = 20,
                            Label = "10219 Quetalco"
                        },
                        new ChoicesItem{
                            Value = 21,
                            Label = "10220 Vilipulli"
                        },
                        new ChoicesItem{
                            Value = 22,
                            Label = "10431 Carahue"
                        },
                        new ChoicesItem{
                            Value = 23,
                            Label = _localizer["All PSMBs"]
                        }
                    }
                },
                new ChoicesGroup
                {
                    Label = _localizer["Species"],
                    Choices = new List<ChoicesItem>{
                        new ChoicesItem{
                            Value = 31,
                            Label = "Chorito (<i>Mytilus chilensis</i>)"
                        },
                        new ChoicesItem{
                            Value = 32,
                            Label = "Cholga (<i>Aulacomya atra</i>)"
                        },
                        new ChoicesItem{
                            Value = 33,
                            Label = "Choro (<i>Choromytilus chorus</i>)"
                        },
                        new ChoicesItem{
                            Value = 34,
                            Label = _localizer["All Species"]
                        }
                    }
                },
                new ChoicesGroup
                {
                    Label = _localizer["Size (%)"],
                    Choices = new List<ChoicesItem>{
                        new ChoicesItem{
                            Value = 40,
                            Label = "0 - 1 (mm)"
                        },
                        new ChoicesItem{
                            Value = 41,
                            Label = "1 - 2 (mm)"
                        },
                        new ChoicesItem{
                            Value = 42,
                            Label = "2 - 5 (mm)"
                        },
                        new ChoicesItem{
                            Value = 43,
                            Label = "5 - 10 (mm)"
                        },
                        new ChoicesItem{
                            Value = 44,
                            Label = "10 - 15 (mm)"
                        },
                        new ChoicesItem{
                            Value = 45,
                            Label = "15 - 20 (mm)"
                        },
                        new ChoicesItem{
                            Value = 46,
                            Label = "20 - 25 (mm)"
                        },
                        new ChoicesItem{
                            Value = 47,
                            Label = "25 - 30 (mm)"
                        },
                        new ChoicesItem{
                            Value = 48,
                            Label = _localizer["Todas las tallas"]
                        },
                    }
                },
                new ChoicesGroup
                {
                    Label = _localizer["Larva Type (count)"],
                    Choices = new List<ChoicesItem>{
                        new ChoicesItem{
                            Value = 50,
                            Label = _localizer["D-Larva"]
                        },
                        new ChoicesItem{
                            Value = 51,
                            Label = _localizer["Umbanate Larva"]
                        },
                        new ChoicesItem{
                            Value = 52,
                            Label = _localizer["Eyed Larva"]
                        },
                        new ChoicesItem{
                            Value = 53,
                            Label = _localizer["Total Larvae"]
                        }
                    }
                },
                new ChoicesGroup
                {
                    Label = _localizer["Estado reproductivo (%)"],
                    Choices = new List<ChoicesItem>{
                        new ChoicesItem{
                            Value = 60,
                            Label = _localizer["Maturing"]
                        },
                        new ChoicesItem{
                            Value = 61,
                            Label = _localizer["Mature"]
                        },
                        new ChoicesItem{
                            Value = 62,
                            Label = _localizer["Spawned"]
                        },
                        new ChoicesItem{
                            Value = 63,
                            Label = _localizer["Spawning"]
                        }
                    }
                },
                new ChoicesGroup
                {
                    Label = _localizer["Sex"],
                    Choices = new List<ChoicesItem>{
                        new ChoicesItem{
                            Value = 70,
                            Label = _localizer["Female"]
                        },
                        new ChoicesItem{
                            Value = 71,
                            Label = _localizer["Male"]
                        }
                    }
                }
            });
        [HttpGet]
        public JsonResult CuencaData()
        {
            var title = _localizer["Catchment Area"] + " ";
            return Json(_context.CatchmentAreas
                .AsNoTracking()
                .Select(c => new GMapPolygon
               {
                   Id = c.Id,
                   Name = title + c.Name,
                   Position = c.Polygon.Vertices.Select(o =>
                   new GMapCoordinate
                   {
                       Lat = o.Latitude,
                       Lng = o.Longitude
                   })
               }));
        }
        [HttpGet]
        public JsonResult ComunaData()
        {
            var title = _localizer["Commune"] + " ";
            return Json(_context.Communes
                .AsNoTracking()
                .Where(com => com.CatchmentAreaId.HasValue)
                .Select(com => new GMapMultiPolygon
                {
                    Id = com.Id,
                    Name = title + com.Name,
                    Provincia = com.Province.Name,
                    Position = com.Polygons
                    .Select(p => p.Vertices.Select(o => new GMapCoordinate
                    {
                        Lat = o.Latitude,
                        Lng = o.Longitude
                    }))
                }));
        }
        private static IQueryable<GMapPolygon> SelectPsmbs(IQueryable<PsmbArea> psmbs) =>
            psmbs.Select(c => new GMapPolygon
                                 {
                                     Id = c.Id,
                                     Name = c.Code + " " + c.Name,
                                     Comuna = c.Commune.Name,
                                     Provincia = c.Commune.Province.Name,
                                     Code = c.Code,
                                     Position = c.Polygon
                        .Vertices.Select(o => new GMapCoordinate
                        {
                            Lat = o.Latitude,
                            Lng = o.Longitude
                        })
                                 });
        [HttpGet]
        public JsonResult PsmbData() => Json(SelectPsmbs(_context.PsmbAreas
                    .AsNoTracking()
                    .Where(c => c.Commune.CatchmentAreaId.HasValue && c.PolygonId.HasValue && c.PlanktonAssays.Any())));
        [HttpGet]
        public IActionResult BuscarInformes(int id, string start, string end)
        {
            int order = id / 99_996 + 24_998 / 24_999;
            var i = Convert.ToDateTime(start, CultureInfo.InvariantCulture);
            var f = Convert.ToDateTime(end, CultureInfo.InvariantCulture).AddDays(1);
            var plankton = _context.PlanktonAssays.Where(p => p.SamplingDate >= i && p.SamplingDate <= f);
            plankton = order switch
            {
                0 => plankton.Where(e => e.Psmb.Commune.CatchmentAreaId == id),
                1 => plankton.Where(e => e.PsmbId == id),
                _ => plankton.Where(e => e.Psmb.CommuneId == id)
            };
            return Json(plankton.Select(p => new { p.Id, SamplingDate = p.SamplingDate.ToShortDateString(), p.Temperature, p.Oxigen, p.Ph, p.Salinity }));
        }
        [ResponseCache(Duration = 60 * 60, VaryByQueryKeys = new string[] { "*" })]
        [HttpGet]
        public IActionResult CustomData(int area, int typeid, DateTime start, DateTime end)
        {
            end = end.AddDays(1);
            return Json(_context.Variables
            .AsNoTracking()
            .Where(v => v.VariableTypeId == typeid && v.PsmbId == area && v.Date >= start && v.Date <= end)
        .GroupBy(e => e.Date)
        .OrderBy(g => g.Key)
        .Select(g => new AmData
        {
            Date = g.Key.ToString(_dateFormat, CultureInfo.InvariantCulture),
            Value = g.Average(p => p.Value)
        }));
        }
        [AllowAnonymous]
        [ResponseCache(Duration = 60 * 60, VaryByQueryKeys = new string[] { "*" })]
        [HttpGet]
        public IActionResult Data(int area, char type, int id, DateTime start, DateTime end)
        {
            end = end.AddDays(1);
            int order = 0;
            if (area > 3) order++;
            if (area > 100000) order++;

            if (!(type == 'v' && id != 4))
            {
                var phyto = _context.Phytoplanktons
                                .AsNoTracking()
                    .Where(e => e.PlanktonAssay.SamplingDate >= start && e.PlanktonAssay.SamplingDate <= end);
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
                phyto = order switch
                {
                    0 => phyto.Where(e => e.PlanktonAssay.Psmb.Commune.CatchmentAreaId == area),
                    1 => phyto.Where(e => e.PlanktonAssay.PsmbId == area),
                    _ => phyto.Where(e => e.PlanktonAssay.Psmb.CommuneId == area)
                };
                return Json(phyto
                    .GroupBy(e => e.PlanktonAssay.SamplingDate.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new AmData 
                    { 
                        Date = g.Key.ToString(_dateFormat, CultureInfo.InvariantCulture), 
                        Value = g.Average(p => p.C)
                    }));
            }
            else
            {
                var assays = _context.PlanktonAssays
                    .AsNoTracking()
                    .Where(e => e.SamplingDate >= start && e.SamplingDate <= end);
                assays = order switch
                {
                    0 => assays.Where(e => e.Psmb.Commune.CatchmentAreaId == area),
                    1 => assays.Where(e => e.PsmbId == area),
                    _ => assays.Where(e => e.Psmb.CommuneId == area)
                };
                return Json(id switch
                {
                    //ph
                    1 => assays
                    .Where(a => a.Ph.HasValue)
                    .GroupBy(e => e.SamplingDate.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new AmData { 
                        Date = g.Key.ToString(_dateFormat, CultureInfo.InvariantCulture), 
                        Value = Math.Round(g.Average(a => a.Ph.Value), 2) }),
                    //ox
                    2 => assays
                    .Where(a => a.Oxigen.HasValue)
                    .GroupBy(e => e.SamplingDate.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new AmData { Date = g.Key.ToString(_dateFormat, CultureInfo.InvariantCulture), Value = Math.Round(g.Average(a => a.Oxigen.Value), 2) }),
                    //sal
                    3 => assays
                    .Where(a => a.Salinity.HasValue)
                    .GroupBy(e => e.SamplingDate.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new AmData { Date = g.Key.ToString(_dateFormat, CultureInfo.InvariantCulture), Value = Math.Round(g.Average(a => a.Salinity.Value), 2) }),
                    //temp
                    _ => assays
                    .Where(a => a.Temperature.HasValue)
                    .GroupBy(e => e.SamplingDate.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new AmData { Date = g.Key.ToString(_dateFormat, CultureInfo.InvariantCulture), Value = Math.Round(g.Average(a => a.Temperature.Value), 2) })
                });
            }
        }
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Graph()
        {
            var minplank = _context.PlanktonAssays
                .AsNoTracking()
                .Min(e => e.SamplingDate);
            var maxplank = _context.PlanktonAssays
                .AsNoTracking()
                .Max(e => e.SamplingDate);
            if (_context.Variables.Any())
            {
                var mincustom = _context.Variables
                    .AsNoTracking()
                    .Min(e => e.Date);
                var maxcustom = _context.Variables
                    .AsNoTracking()
                    .Max(e => e.Date);
                var plankNewer = minplank > mincustom;
                ViewData["start"] = plankNewer ?
                    mincustom.ToString(_dateFormat, CultureInfo.InvariantCulture)
                    : minplank.ToString(_dateFormat, CultureInfo.InvariantCulture);
                ViewData["end"] = plankNewer ?
                    maxplank.ToString(_dateFormat, CultureInfo.InvariantCulture) :
                    maxcustom.ToString(_dateFormat, CultureInfo.InvariantCulture);
            }
            else
            {
                ViewData["start"] = minplank.ToString(_dateFormat, CultureInfo.InvariantCulture);
                ViewData["end"] = maxplank.ToString(_dateFormat, CultureInfo.InvariantCulture);
            }
            return View();
        }
        [HttpGet]
        public IActionResult GetPhotos()
        {
            var photos = _context.Photos
                            .Include(p => p.Individual)
                                .ThenInclude(i => i.Sampling)
                            .AsNoTracking()
                            .ToList();

            List<NanoGalleryElement> gallery = photos.Select(photo => new NanoGalleryElement
            {
                Src =  $"Photos/GetImg?f={photo.Key}&d=DB",
                Srct = $"Photos/GetImg?f={photo.Key}&d=DB/Thumbs",
                Title = photo.Comment,
                Id = $"{photo.IndividualId}{photo.Id}",
                AlbumId = photo.IndividualId.ToString(CultureInfo.InvariantCulture)
            }).ToList();

            gallery.AddRange(photos.Select(p => p.Individual)
            .Select(i => new NanoGalleryElement
            {
                Src = $"Photos/GetImg?f={i.Photos.First().Key}&d=DB",
                Srct = $"Photos/GetImg?f={i.Photos.First().Key}&d=DB/Thumbs",
                Title = i.Id.ToString(CultureInfo.InvariantCulture),
                Id = i.Id.ToString(CultureInfo.InvariantCulture),
                AlbumId = i.SamplingId.ToString(CultureInfo.InvariantCulture),
                Kind = "album"
            }));

            gallery.AddRange(photos.Select(p => p.Individual.Sampling)
            .Select(s => new NanoGalleryElement
            {
                Src = $"Photos/GetImg?f={s.Individuals.First().Photos.First().Key}&d=DB",
                Srct = $"Photos/GetImg?f={s.Individuals.First().Photos.First().Key}&d=DB/Thumbs",
                Title = s.Id.ToString(CultureInfo.InvariantCulture),
                Id = s.Id.ToString(CultureInfo.InvariantCulture),
                Kind = "album"
            }));

            return Json(gallery);
        }
    }
}