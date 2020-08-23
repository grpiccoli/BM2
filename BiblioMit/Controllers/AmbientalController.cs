﻿using System.Collections.Generic;
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
using Schema.NET;

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
        private IQueryable<ChoicesItem> CuencaChoices()
        {
            var singlabel = _localizer["Catchment Area"] + " ";
            return _context.CatchmentAreas.Select(c => new ChoicesItem
            {
                Value = c.Id,
                Label = singlabel + c.Name,
                Selected = c.Id == 1
            });
        }
        private IQueryable<ChoicesItem> CommuneChoices() => _context.CatchmentAreas.SelectMany(c => c.Communes
            .Select(com => new ChoicesItem
            {
                Value = com.Id,
                Label = com.Name + " " + c.Name,
                Selected = false
            }));
        private IQueryable<ChoicesItem> PsmbChoices() => _context.CatchmentAreas.SelectMany(c => c.Communes
        .SelectMany(com => com.Psmbs.Where(p => p.PolygonId.HasValue && p.PlanktonAssays.Any())
        .Select(p => new ChoicesItem
        {
            Value = p.Id,
            Label = p.Code + " " + p.Name + " " + c.Name,
            Selected = false
        })));
        private IQueryable<ChoicesItem> PublicAreaChoices() => CuencaChoices().Union(CommuneChoices());
        private IQueryable<ChoicesItem> PrivateAreaChoices() => PublicAreaChoices().Union(PsmbChoices());
        [AllowAnonymous]
        public IActionResult PublicAreasList() => Json(PublicAreaChoices());
        public IActionResult PrivateAreasList() => Json(PrivateAreaChoices());
        [AllowAnonymous]
        public IActionResult CuencaList()
        {
            var singlabel = _localizer["Catchment Area"] + " ";
            return Json(new ChoicesGroup
            {
                    Label = _localizer["Catchment Areas"],
                    Choices = _context.CatchmentAreas.Select(c => new ChoicesItem
                    {
                        Value = c.Id,
                        Label = singlabel + c.Name
                    })
                });
        }
        [AllowAnonymous]
        public IActionResult RegionList() => Json(new ChoicesGroup
            {
                Label = _localizer["Regions"],
                Choices = _context.Regions
                    .Select(com => new ChoicesItem
                    {
                        Value = com.Id,
                        Label = com.Name
                    })
            });
        [AllowAnonymous]
        public IActionResult ProvinciaList() => Json(Provincia());
        private IQueryable<ChoicesGroup> Provincia() =>
            _context.Regions.Select(c => new ChoicesGroup
            {
                Label = c.Name,
                Choices = c.Provinces
                    .Select(com => new ChoicesItem
                    {
                        Value = com.Id,
                        Label = com.Name + ", " + c.Name
                    })
            });
        [AllowAnonymous]
        public IActionResult ComunaList()
        {
            var singlabel = _localizer["Catchment Area"] + " ";
            return Json(Comuna(singlabel));
        }
        private IQueryable<ChoicesGroup> Comuna(string singlabel) =>
            _context.CatchmentAreas.Select(c => new ChoicesGroup
            {
                Label = singlabel + c.Name,
                Choices = c.Communes
                    .Select(com => new ChoicesItem
                    {
                        Value = com.Id,
                        Label = com.Name + " " + c.Name
                    })
            });
        public IActionResult PsmbList() => Json(_context.Communes
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
        public IActionResult ProvinciaFarmList() => Json(Provincia());
        public IActionResult ComunaFarmList()
        {
            var singlabel = _localizer["Catchment Area"] + " ";
            return Json(Comuna(singlabel));
        }
        [AllowAnonymous]
        public IActionResult ProvinciaResearchList() => Json(_context.Regions
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
        [AllowAnonymous]
        public IActionResult ComunaResearchList() => Json(_context.Provinces
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
        public IActionResult FarmData() => Json(SelectPsmbs(_context.PsmbAreas
            .Where(c => c.PolygonId.HasValue)));
        [AllowAnonymous]
        public IActionResult ResearchData() => Json(_context.ResearchCentres
                    .Where(c => c.PolygonId.HasValue)
                    .Select(c => new GMapPolygon
                    {
                        Id = c.Id,
                        Name = c.Name + " (" + c.Acronym + ")",
                        Comuna = c.Commune.Name,
                        Provincia = c.Commune.Province.Name,
                        Region = c.Commune.Province.Region.Name,
                        Position = c.Polygon
                        .Vertices.Select(o => new GMapCoordinate
                        {
                            Lat = o.Latitude,
                            Lng = o.Longitude
                        })
                    }));
        public IActionResult FarmList() => Json(_context.Farms
                    .Where(p => p.PolygonId.HasValue)
                    .Select(p => new ChoicesItem
                    {
                        Value = p.Id,
                        Label = p.Code + " " + p.Name
                    }));
        public IActionResult CompanyList() => Json(_context.Companies
                    .Where(p => p.Id > 900_000 && p.Psmbs.Any(f => f.Discriminator == Models.Entities.Centres.PsmbType.Farm && f.PolygonId.HasValue))
                    .Select(p => new ChoicesItem
                    {
                        Value = p.Id,
                        Label = p.BusinessName + " (" + p.GetRUT() + ")"
                    }));
        [AllowAnonymous]
        public IActionResult ResearchList() => Json(_context.ResearchCentres
                    .Where(p => p.PolygonId.HasValue)
                    .Select(p => new ChoicesItem
                    {
                        Value = p.Id,
                        Label =  p.Name + " (" + p.Acronym + ")"
                    }));
        [AllowAnonymous]
        public IActionResult InstitutionList() => Json(_context.Companies
                    .Where(p => p.Psmbs.Any(p => p.Discriminator == Models.Entities.Centres.PsmbType.ResearchCentre))
                    .Select(p => new ChoicesItem
                    {
                        Value = p.Id,
                        Label = p.BusinessName + " (" + p.Acronym + ")"
                    }));
        [AllowAnonymous]
        public JsonResult OceanVarList() => Json(Variable.t.Enum2ChoicesGroup("v").FirstOrDefault());
        [AllowAnonymous]
        public JsonResult GroupVarList()
        {
            var group = " (" + _localizer["Group"] + ")";
            return Json(new ChoicesGroup
            {
                Label = _localizer["Phylogenetic Groups (Cel/mL)"],
                Choices = _context.PhylogeneticGroups.Select(p => new ChoicesItem
                {
                    Value = "f" + p.Id,
                    Label = p.Name + group
                })
            });
        }
        public JsonResult GenusVarList()
        {
            var genus = " (" + _localizer["Genus"] + ")";
            return Json(new ChoicesGroup
            {
                Label = _localizer["Genera (Cel/mL)"],
                Choices = _context.GenusPhytoplanktons.Select(p => new ChoicesItem
                {
                    Value = "g" + p.Id,
                    Label = p.Name + genus
                })
            });
        }
        public JsonResult SpeciesVarList()
        {
            var sp = " (" + _localizer["Species"] + ")";
            var species = new ChoicesGroup
            {
                Label = _localizer["Species (Cel/mL)"],
                Choices = _context.SpeciesPhytoplanktons
                .Select(p => new ChoicesItem
                {
                    Value = "s" + p.Id,
                    Label = p.Genus.Name + " " + p.Name + sp
                })
            };
            return Json(species);
        }
        public async Task<JsonResult> TLData(int a, int psmb, int sp, int? v, string start, string end)
        {
            //1 analysis, 2 psmb, 3 species, 4 size, 5 larva type, 6 rep stg, 7 sex
            if (a != 14 && a != 17 && !v.HasValue) throw new ArgumentNullException(_localizer["error"]);
            if ((a == 13 || a == 15 || a == 16) && sp != 31) return null;
            var i = Convert.ToDateTime(start, CultureInfo.InvariantCulture);
            var f = Convert.ToDateTime(end, CultureInfo.InvariantCulture);
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
                        selection = db.Select(tl => new AmData { 
                            Date = tl.SpecieSeed.Seed.Date.ToString(_dateFormat,CultureInfo.InvariantCulture),
                        Value = tl.Proportion });
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
                        selection = db.Select(tl => new AmData
                        {
                            Date = tl.Larvae.Date.ToString(_dateFormat, CultureInfo.InvariantCulture),
                            Value = tl.Count
                        });
                    }
                    break;
                //ig reproductores
                case 13:
                    if (v.HasValue)
                    {
                        var db = _context.Spawnings as IQueryable<Spawning>;
                        if (psmb != 23) db = db.Where(tl => tl.Farm.Code == psmbs[psmb]);
                        selection = db.Select(tl => new AmData
                        {
                            Date = tl.Date.ToString(_dateFormat, CultureInfo.InvariantCulture),
                            Value = v.Value == 70 ? tl.FemaleIG : tl.MaleIG
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
                        selection = db.Select(tl => new AmData
                        {
                            Date = tl.Seed.Date.ToString(_dateFormat, CultureInfo.InvariantCulture),
                            Value = tl.Capture
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
                        selection = db.Select(tl => new AmData
                        {
                            Date = tl.Spawning.Date.ToString(_dateFormat, CultureInfo.InvariantCulture),
                            Value = tl.Proportion
                        });
                    }
                    break;
                //% sex
                case 16:
                    if (v.HasValue)
                    {
                        var db = _context.Spawnings as IQueryable<Spawning>;
                        if (psmb != 23) db = db.Where(tl => tl.Farm.Code == psmbs[psmb]);
                        selection = db.Select(tl => new AmData
                        {
                            Date = tl.Date.ToString(_dateFormat, CultureInfo.InvariantCulture),
                            Value = v.Value == 70 ? tl.FemaleProportion : tl.MaleProportion
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
            .Select(g => new AmData
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
        [AllowAnonymous]
        public JsonResult CuencaData()
        {
            var title = _localizer["Catchment Area"] + " ";
            return Json(_context.CatchmentAreas.Select(c => new GMapPolygon
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
        [AllowAnonymous]
        public JsonResult ComunaData()
        {
            var title = _localizer["Commune"] + " ";
            return Json(_context.Communes
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
        public JsonResult PsmbData() => Json(SelectPsmbs(_context.PsmbAreas
                    .Where(c => c.Commune.CatchmentAreaId.HasValue && c.PolygonId.HasValue && c.PlanktonAssays.Any())));
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