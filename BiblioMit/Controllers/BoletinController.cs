using BiblioMit.Data;
using BiblioMit.Extensions;
using BiblioMit.Models;
using BiblioMit.Models.Entities.Digest;
using BiblioMit.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace BiblioMit.Views
{
    [Authorize]
    public class BoletinController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BoletinController(
            ApplicationDbContext context
            )
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public PartialViewResult Download()
        {
            var model = new Dictionary<int, List<string>>
            {
                { 2018, new List<string>{ "ENE-MAR", "ABR-JUN", "JUL-SEP" } }
            };
            return PartialView("_Download", model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult Download(string src)
        {
            var url = $"{Request.Scheme}://{Request.Host.Value}/files/Boletin/BOLETIN-{src}.pdf";
            if (Url.IsLocalUrl(url))
                return Redirect(url);
            else return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public JsonResult GetXlsx(int year, int start, int end)
        {
            var reg = 10;
            var yr_1 = year - 1;
            var feature = HttpContext.Features.Get<IRequestCultureFeature>();
            var lang = feature.RequestCulture.Culture.TwoLetterISOLanguageName.ToUpperInvariant();
            DateTime.TryParseExact($"{start} {year}", "M yyyy", CultureInfo.GetCultureInfo("en-GB"), DateTimeStyles.None, out var start_dt);
            DateTime.TryParseExact($"{DateTime.DaysInMonth(year, end)} {end} {year}", "d M yyyy", CultureInfo.GetCultureInfo("en-GB"), DateTimeStyles.None, out var end_dt);
            DateTime.TryParseExact($"{start} {yr_1}", "M yyyy", CultureInfo.GetCultureInfo("en-GB"), DateTimeStyles.None, out var start_dt_1);
            DateTime.TryParseExact($"{DateTime.DaysInMonth(yr_1, end)} {end} {yr_1}", "d M yyyy", CultureInfo.GetCultureInfo("en-GB"), DateTimeStyles.None, out var end_dt_1);

            var pre = $"Total {start_dt.ToString("MMM", CultureInfo.InvariantCulture)}-{end_dt.ToString("MMM", CultureInfo.InvariantCulture)}";
            var co = "Comuna";
            var pro = "Provincia";

            var graphs = new List<object>();
            var temp = new List<object>();
            var sal = new List<object>();
            var tipos = new int[] { (int)DeclarationType.Seed, (int)DeclarationType.Harvest, (int)DeclarationType.Supply, (int)DeclarationType.Production };
            foreach (var tipo in tipos)
            {
                var tmp = new List<object>();
                var tp = (DeclarationType)tipo;

                var comuns = _context.SernapescaDeclarations
                    .Include(c => c.Centre)
                    .ThenInclude(c => c.Commune)
                    .ThenInclude(c => c.Province)
                    .Where(a =>
                    a.Dato == tp && a.OriginId == 1
                    && a.Centre.Commune.Province.RegionId == reg
                    && ((a.Date >= start_dt && a.Date <= end_dt)
                    || (a.Date >= start_dt_1 && a.Date <= end_dt_1)))
                    .ToList()
                    .GroupBy(c => c.Centre.Commune).OrderBy(o => o.Key.Name);

                foreach (var comuna in comuns)
                {
                    var cyr = (int)Math.Round(comuna.Where(a => a.Date.Year == year).Sum(a => a.Weight));
                    var cyr_1 = (int)Math.Round(comuna.Where(a => a.Date.Year == yr_1).Sum(a => a.Weight));
                    tmp.Add(new Dictionary<string, object>
                    {
                        { co, comuna.Key.Name },
                        { pro, comuna.Key.Province.Name },
                        { $"{pre} {yr_1}", cyr_1 },
                        { $"{pre} {year}", cyr }
                    });
                }
                graphs.Add(tmp);
            }

            var comunas = _context.PlanktonAssays
                    .Include(c => c.Psmb.Commune.Province)
                    .Where(a =>
                    a.Psmb.Commune.Province.RegionId == reg
                    //&& a.SamplingDate.HasValue 
                    && ((a.SamplingDate >= start_dt && a.SamplingDate <= end_dt)
                    || (a.SamplingDate >= start_dt_1 && a.SamplingDate <= end_dt_1)))
                    .ToList()
                    .GroupBy(c => c.Psmb.Commune).OrderBy(o => o.Key.Name);

            foreach (var comuna in comunas)
            {
                double cyr = 0;
                double cyr_1 = 0;
                double scyr = 0;
                double scyr_1 = 0;
                if (comuna.Any(c => c.SamplingDate.Year == year))
                {
                    if (comuna.Any(c => c.Temperature.HasValue))
                    {
                        cyr = Math.Round(comuna.Where(a => a.SamplingDate.Year == year && a.Temperature.HasValue)
                            .Average(a => a.Temperature.Value), 2);
                    }
                    if (comuna.Any(c => c.Salinity.HasValue))
                    {
                        scyr = Math.Round(comuna.Where(a => a.SamplingDate.Year == year && a.Salinity.HasValue)
                        .Average(a => a.Salinity.Value), 2);
                    }
                }
                if (comuna.Any(c => c.SamplingDate.Year == yr_1))
                {
                    if (comuna.Any(c => c.Temperature.HasValue))
                    {
                        scyr_1 = Math.Round(comuna.Where(a => a.SamplingDate.Year == yr_1 && a.Salinity.HasValue)
                        .Average(a => a.Salinity.Value), 2);
                    }
                    if (comuna.Any(c => c.Salinity.HasValue))
                    {
                        cyr_1 = Math.Round(comuna.Where(a => a.SamplingDate.Year == yr_1 && a.Temperature.HasValue)
                        .Average(a => a.Temperature.Value), 2);
                    }
                }
                temp.Add(new Dictionary<string, object>
                {
                    { co, comuna.Key.Name },
                    { pro, comuna.Key.Province.Name },
                    { $"{pre} {yr_1}", cyr_1 },
                    { $"{pre} {year}", cyr }
                });
                sal.Add(new Dictionary<string, object>
                {
                    { co, comuna.Key.Name },
                    { pro, comuna.Key.Province.Name },
                    { $"{pre} {yr_1}", scyr_1 },
                    { $"{pre} {year}", scyr }
                });
            }
            graphs.Add(temp);
            graphs.Add(sal);
            return Json(graphs);
        }

        [AllowAnonymous]
        public JsonResult GetComunas(int tipo, int year, int start, int end, bool? tb)
        {
            if (!tb.HasValue) tb = false;
            var reg = 10;
            var yr_1 = year - 1;
            var feature = HttpContext.Features.Get<IRequestCultureFeature>();
            var lang = feature.RequestCulture.Culture.TwoLetterISOLanguageName.ToUpperInvariant();
            DateTime.TryParseExact($"{start} {year}", "M yyyy", CultureInfo.GetCultureInfo("en-GB"), DateTimeStyles.None, out var start_dt);
            DateTime.TryParseExact($"{DateTime.DaysInMonth(year, end)} {end} {year}", "d M yyyy", CultureInfo.GetCultureInfo("en-GB"), DateTimeStyles.None, out var end_dt);
            DateTime.TryParseExact($"{start} {yr_1}", "M yyyy", CultureInfo.GetCultureInfo("en-GB"), DateTimeStyles.None, out var start_dt_1);
            DateTime.TryParseExact($"{DateTime.DaysInMonth(yr_1, end)} {end} {yr_1}", "d M yyyy", CultureInfo.GetCultureInfo("en-GB"), DateTimeStyles.None, out var end_dt_1);

            var graphs = new List<object>();

            if (tipo > (int)DeclarationType.Production)
            {
                var ambientales = _context.PlanktonAssays
                    .Include(c => c.Psmb.Commune.Province)
                    .Where(a =>
                    a.Psmb.Commune.Province.RegionId == reg
                    //&& a.SamplingDate.HasValue
                    && ((a.SamplingDate >= start_dt && a.SamplingDate <= end_dt)
                    || (a.SamplingDate >= start_dt_1 && a.SamplingDate <= end_dt_1)));

                var temp = tipo == (int)DeclarationType.Temperature;

                IQueryable<IGrouping<Commune, PlanktonAssay>> comunas;

                comunas = ambientales.GroupBy(c => c.Psmb.Commune).OrderBy(o => o.Key.Name);

                foreach (var comuna in comunas)
                {
                    if (!comuna.Any(c => temp ? c.Temperature.HasValue : c.Salinity.HasValue)) { continue; }
                    double? cyr = null;
                    double? cyr_1 = null;
                    if (temp)
                    {
                        if (comuna.Any(c => c.SamplingDate.Year == year && c.Temperature.HasValue))
                        {
                            cyr = Math.Round(comuna.Where(a => a.SamplingDate.Year == year && a.Temperature.HasValue)
                                .Average(a => a.Temperature.Value), 2);
                        }
                        if (comuna.Any(c => c.SamplingDate.Year == yr_1 && c.Temperature.HasValue))
                        {
                            cyr_1 = Math.Round(comuna.Where(a => a.SamplingDate.Year == yr_1 && a.Temperature.HasValue)
                            .Average(a => a.Temperature.Value), 2);
                        }
                    }
                    else
                    {
                        if (comuna.Any(c => c.SamplingDate.Year == year && c.Salinity.HasValue))
                        {
                            cyr = Math.Round(comuna.Where(a => a.SamplingDate.Year == year && a.Salinity.HasValue)
                            .Average(a => a.Salinity.Value), 2);
                        }
                        if (comuna.Any(c => c.SamplingDate.Year == yr_1 && c.Salinity.HasValue))
                        {
                            cyr_1 = Math.Round(comuna.Where(a => a.SamplingDate.Year == yr_1 && a.Salinity.HasValue)
                            .Average(a => a.Salinity.Value), 2);
                        }
                    }
                    graphs.Add(new { comuna = comuna.Key.Name, lastyr = cyr_1, year = cyr });
                }
            }
            else if (tipo == (int)DeclarationType.Production && !tb.Value)
            {
                var tp = (DeclarationType)tipo;

                var planilla = _context.SernapescaDeclarations
                .Include(c => c.Centre.Commune.Province)
                .Where(a =>
                a.Dato == tp
                && a.ProductionType != ProductionType.Unknown
                && a.ItemType != Item.Product
                && a.Centre.Commune.Province.RegionId == reg
                && ((a.Date >= start_dt && a.Date <= end_dt)
                    || (a.Date >= start_dt_1 && a.Date <= end_dt_1)));

                IQueryable<IGrouping<Commune, SernapescaDeclaration>> comunas;

                comunas = planilla.GroupBy(c => c.Centre.Commune).OrderBy(o => o.Key.Name);

                foreach (var comuna in comunas)
                {
                    var cyr = comuna.Where(a => a.Date.Year == year);
                    var cyr_1 = comuna.Where(a => a.Date.Year == yr_1);
                    var aa_congelado = (int)Math.Round(cyr.Where(a => a.ProductionType == ProductionType.Frozen).Sum(a => a.Weight));
                    var ba_congelado = (int)Math.Round(cyr_1.Where(a => a.ProductionType == ProductionType.Frozen).Sum(a => a.Weight));
                    var ab_conserva = (int)Math.Round(cyr.Where(a => a.ProductionType == ProductionType.Preserved).Sum(a => a.Weight));
                    var bb_conserva = (int)Math.Round(cyr_1.Where(a => a.ProductionType == ProductionType.Preserved).Sum(a => a.Weight));
                    var ac_refrigerado = (int)Math.Round(cyr.Where(a => a.ProductionType == ProductionType.Refrigerated).Sum(a => a.Weight));
                    var bc_refrigerado = (int)Math.Round(cyr_1.Where(a => a.ProductionType == ProductionType.Refrigerated).Sum(a => a.Weight));
                    //var ad_desconicido = (int)Math.Round(cyr.Where(a => a.TipoProduccion == ProductionType.Desconocido).Sum(a => a.Peso));
                    //var bd_desconocido = (int)Math.Round(cyr_1.Where(a => a.TipoProduccion == ProductionType.Desconocido).Sum(a => a.Peso));
                    graphs.Add(new
                    {
                        comuna = comuna.Key.Name,
                        aa_congelado,
                        ab_conserva,
                        ac_refrigerado,
                        ba_congelado,
                        bb_conserva,
                        bc_refrigerado
                    });
                }

            }
            else
            {
                var tp = (DeclarationType)tipo;

                var planilla = tipo == (int)DeclarationType.Seed ?
                    _context.SernapescaDeclarations
                    .Include(c => c.Centre.Commune.Province)
                    .Where(a =>
                    a.Dato == tp && a.OriginId == 1
                    && a.Centre.Commune.Province.RegionId == reg
                    && ((a.Date >= start_dt && a.Date <= end_dt)
                    || (a.Date >= start_dt_1 && a.Date <= end_dt_1))) :
                    _context.SernapescaDeclarations
                    .Include(c => c.Centre.Commune.Province)
                    .Where(a =>
                    a.Dato == tp
                    && a.Centre.Commune.Province.RegionId == reg
                    && ((a.Date >= start_dt && a.Date <= end_dt)
                    || (a.Date >= start_dt_1 && a.Date <= end_dt_1)));

                IQueryable<IGrouping<Commune, SernapescaDeclaration>> comunas;

                comunas = planilla.GroupBy(c => c.Centre.Commune).OrderBy(o => o.Key.Name);

                foreach (var comuna in comunas)
                {
                    var cyr = (int)Math.Round(comuna.Where(a => a.Date.Year == year).Sum(a => a.Weight));
                    var cyr_1 = (int)Math.Round(comuna.Where(a => a.Date.Year == yr_1).Sum(a => a.Weight));
                    graphs.Add(new { comuna = comuna.Key.Name, year = cyr, lastyr = cyr_1 });
                }
            }
            return Json(graphs);
        }

        [AllowAnonymous]
        public JsonResult GetMeses(int tipo, int year, int start, int end)
        {
            var reg = 10;
            var feature = HttpContext.Features.Get<IRequestCultureFeature>();
            var lang = feature.RequestCulture.Culture.TwoLetterISOLanguageName.ToUpperInvariant();
            DateTime.TryParseExact($"{start} {year}", "M yyyy", CultureInfo.GetCultureInfo("en-GB"), DateTimeStyles.None, out var start_dt);
            DateTime.TryParseExact($"{DateTime.DaysInMonth(year, end)} {end} {year}", "d M yyyy", CultureInfo.GetCultureInfo("en-GB"), DateTimeStyles.None, out var end_dt);

            var graphs = new List<object>();

            if (tipo > (int)DeclarationType.Production)
            {
                var ambientales = _context.PlanktonAssays
                    .Include(c => c.Psmb.Commune.Province)
                    .Where(a =>
                    a.Psmb.Commune.Province.RegionId == reg
                    //&& a.SamplingDate.HasValue
                    && a.SamplingDate >= start_dt
                    && a.SamplingDate <= end_dt);

                var temp = tipo == (int)DeclarationType.Temperature;

                IQueryable<IGrouping<int, PlanktonAssay>> meses;

                meses = ambientales.GroupBy(c => c.SamplingDate.Month).OrderBy(o => o.Key);

                foreach (var month in meses)
                {
                    var value = Math.Round(month.Where(a => temp ? a.Temperature.HasValue : a.Salinity.HasValue)
                        .Average(a => temp ? a.Temperature.Value : a.Salinity.Value), 2);
                    graphs.Add(new { date = $"{year}-{month.Key}", value });
                }
            }
            else if (tipo == (int)DeclarationType.Production)
            {
                var tp = (DeclarationType)tipo;

                var planilla = _context.SernapescaDeclarations
                    .Include(c => c.Centre.Commune.Province)
                    .Where(a =>
                    a.Dato == tp
                    && a.ProductionType != ProductionType.Unknown
                    && a.ItemType != Item.Product
                    && a.Centre.Commune.Province.RegionId == reg
                    && a.Date >= start_dt
                    && a.Date <= end_dt);

                IQueryable<IGrouping<int, SernapescaDeclaration>> meses;

                meses = planilla.GroupBy(c => c.Date.Month).OrderBy(o => o.Key);

                foreach (var month in meses)
                {
                    var congelado = (int)Math.Round(month.Where(a => a.ProductionType == ProductionType.Frozen).Sum(a => a.Weight));
                    var conserva = (int)Math.Round(month.Where(a => a.ProductionType == ProductionType.Preserved).Sum(a => a.Weight));
                    var refrigerado = (int)Math.Round(month.Where(a => a.ProductionType == ProductionType.Refrigerated).Sum(a => a.Weight));
                    //var desconocido = (int)Math.Round(month.Where(a => a.TipoProduccion == ProductionType.Desconocido).Sum(a => a.Peso));
                    graphs.Add(new
                    {
                        date = $"{year}-{month.Key}",
                        congelado,
                        conserva,
                        refrigerado
                        //, desconocido
                    });
                }
            }
            else
            {
                var tp = (DeclarationType)tipo;

                var planilla = tipo == (int)DeclarationType.Seed ?
                    _context.SernapescaDeclarations
                    .Include(c => c.Centre.Commune.Province)
                    .Where(a =>
                    a.Dato == tp && a.OriginId == 1
                    && a.Centre.Commune.Province.RegionId == reg
                    && a.Date >= start_dt
                    && a.Date <= end_dt) :
                    _context.SernapescaDeclarations
                    .Include(c => c.Centre.Commune.Province)
                    .Where(a =>
                    a.Dato == tp
                    && a.Centre.Commune.Province.RegionId == reg
                    && a.Date >= start_dt
                    && a.Date <= end_dt);

                IQueryable<IGrouping<int, SernapescaDeclaration>> meses;

                meses = planilla.GroupBy(c => c.Date.Month).OrderBy(o => o.Key);

                foreach (var month in meses)
                {
                    var cyr = (int)Math.Round(month.Sum(a => a.Weight));
                    graphs.Add(new { date = $"{year}-{month.Key}", value = cyr });
                }
            }
            return Json(graphs);
        }

        [AllowAnonymous]
        public JsonResult GetProvincias(int tipo, int year, int start, int end)
        {
            var reg = 10;
            var feature = HttpContext.Features.Get<IRequestCultureFeature>();
            var lang = feature.RequestCulture.Culture.TwoLetterISOLanguageName.ToUpperInvariant();
            DateTime.TryParseExact($"{start} {year}", "M yyyy", CultureInfo.GetCultureInfo("en-GB"), DateTimeStyles.None, out var start_dt);
            DateTime.TryParseExact($"{DateTime.DaysInMonth(year, end)} {end} {year}", "d M yyyy",
                CultureInfo.GetCultureInfo("en-GB"), DateTimeStyles.None, out var end_dt);

            var graphs = new List<object>();

            //if (tipo > (int)Tipo.Producción)
            //{
            //    var ambientales = _context.EnsayoFitos
            //        .Include(c => c.Centre.Comuna.Provincia)
            //        .Where(a =>
            //        a.Centre.Comuna.Provincia.RegionId == reg
            //        && a.FechaMuestreo >= start_dt
            //        && a.FechaMuestreo <= end_dt);

            //    var temp = tipo == (int)Tipo.Temperatura;

            //    IQueryable<IGrouping<Provincia, EnsayoFito>> provincias;

            //    provincias = ambientales.GroupBy(c => c.Centre.Comuna.Provincia).OrderBy(o => o.Key.Name);

            //    foreach (var provincia in provincias)
            //    {
            //        var cyr = Math.Round(provincia.Where(a => temp ? a.Temperatura.HasValue : a.Salinidad.HasValue)
            //            .Average(a => temp ? a.Temperatura.Value : a.Salinidad.Value),2);
            //        graphs.Add(new { provincia = provincia.Key.Name, ton = cyr });
            //    }
            //}
            //else 
            if (tipo == (int)DeclarationType.Production)
            {
                var tp = (DeclarationType)tipo;

                var planilla = _context.SernapescaDeclarations
                    .Include(c => c.Centre.Commune.Province)
                    .Where(a =>
                    a.Dato == tp
                    && a.ProductionType != ProductionType.Unknown
                    && a.ItemType != Item.Product
                    && a.Centre.Commune.Province.RegionId == reg
                    && a.Date >= start_dt
                    && a.Date <= end_dt);

                IQueryable<IGrouping<Province, SernapescaDeclaration>> provincias;

                provincias = planilla.GroupBy(c => c.Centre.Commune.Province).OrderBy(o => o.Key.Name);

                var colors = new Dictionary<string, string>{
                    { "Palena", "#ff9e01" },
                    { "Llanquihue", "#ff6600" },
                    { "Chiloé", "#ff0f00" }
                };

                foreach (var provi in provincias)
                {
                    var cyr = (int)Math.Round(provi.Sum(a => a.Weight));
                    var cong = (int)Math.Round(provi.Where(p => p.ProductionType == ProductionType.Frozen).Sum(a => a.Weight));
                    var cons = (int)Math.Round(provi.Where(p => p.ProductionType == ProductionType.Preserved).Sum(a => a.Weight));
                    var refr = (int)Math.Round(provi.Where(p => p.ProductionType == ProductionType.Refrigerated).Sum(a => a.Weight));
                    //var desc = (int)Math.Round(provi.Where(p => p.TipoProduccion == ProductionType.Desconocido).Sum(a => a.Peso));
                    graphs.Add(new
                    {
                        provincia = provi.Key.Name,
                        ton = cyr,
                        color = colors[provi.Key.Name],
                        id = provi.Key.Id,
                        subs = new List<object>
                        {
                            new { provincia = "Congelado",
                            ton = cong },
                            new { provincia = "Conserva",
                            ton = cons },
                            new { provincia = "Refrigerado",
                            ton = refr }
                            //,
                            //new { provincia = "Desconocido",
                            //ton = desc }
                        }
                    });
                }
            }
            else
            {
                var tp = (DeclarationType)tipo;

                var planilla = tipo == (int)DeclarationType.Seed ?
                    _context.SernapescaDeclarations
                    .Include(c => c.Centre.Commune.Province)
                    .Where(a =>
                    a.Dato == tp && a.OriginId == 1
                    && a.Centre.Commune.Province.RegionId == reg
                    && a.Date>= start_dt
                    && a.Date<= end_dt) :
                    _context.SernapescaDeclarations
                    .Include(c => c.Centre.Commune.Province)
                    .Where(a =>
                    a.Dato == tp
                    && a.Centre.Commune.Province.RegionId == reg
                    && a.Date>= start_dt
                    && a.Date <= end_dt);

                IQueryable<IGrouping<Province, SernapescaDeclaration>> provincias;

                provincias = planilla.GroupBy(c => c.Centre.Commune.Province).OrderBy(o => o.Key.Name);

                foreach (var provincia in provincias)
                {
                    if (provincia.Any())
                    {
                        var cyr = (int)Math.Round(provincia.Sum(a => a.Weight));
                        graphs.Add(new { provincia = provincia.Key.Name, ton = cyr });
                    }
                }
            }
            return Json(graphs);
        }

        [AllowAnonymous]
        public ActionResult Index(int? yr, int? start, int? end, int? reg, int? ver, int? tp)
        {
            return RedirectToAction(nameof(Boletin), new { yr, start, end, reg, ver, tp });
        }

        // GET: Boletin
        [AllowAnonymous]
        public ActionResult Boletin(int? yr, int? start, int? end, int? reg, int? ver, int? tp)
        {
            if (!reg.HasValue) reg = 10;
            if (!ver.HasValue) ver = 3;
            if (!tp.HasValue) tp = 1;

            var years = new List<int>();
            var months = new List<int>();
            years.AddRange(_context.SernapescaDeclarations.Select(a => a.Date.Year).Distinct().ToList());

            if (!yr.HasValue && years != null) yr = years.Max();
            months.AddRange(_context.SernapescaDeclarations.Where(a => a.Date.Year == yr).Select(a => a.Date.Month).Distinct().ToList());
            if (!start.HasValue) start = months.Min();
            if (!end.HasValue) end = months.Max();

            ViewData["Year"] = new SelectList(
                from int y in years
                select new { Id = y, Name = y }, "Id", "Name", yr.Value);

            var meses = DateTimeFormatInfo.CurrentInfo.MonthNames;

            var all = Enumerable.Range(1, 12).ToArray();
            //var disabled = Enumerable.Range(end.Value + 1, 12);

            ViewData["Start"] = new List<SelectListItem>(
                from int m in all
                select new SelectListItem { Text = meses[m - 1], Value = m.ToString(CultureInfo.InvariantCulture), Disabled = !months.Contains(m), Selected = m == start.Value });

            ViewData["End"] = new List<SelectListItem>(
                from int m in all
                select new SelectListItem { Text = meses[m - 1], Value = m.ToString(CultureInfo.InvariantCulture), Disabled = !months.Contains(m) || start.Value >= m, Selected = m == end.Value });

            ViewData["Tp"] = new SelectList(
                from DeclarationType m in Enum.GetValues(typeof(DeclarationType))
                select new
                {
                    Id = (int)m,
                    Name = m.ToString()
                },
                "Id", "Name", tp);

            var centros = _context.Psmbs
                .Include(c => c.Commune)
                .ThenInclude(c => c.Province)
                .Where(c => c.Declarations.Any() && c.Commune.Province.RegionId == reg)
                .Select(c => c.Commune).Distinct();

            ViewData["Comunas"] = centros.OrderBy(c => c.Province.Name).ThenBy(c => c.Name)
                .Select(c => new string[] { c.Name, c.Province.Name });
            ViewData["Ver"] = ver;
            return View();
        }

        [AllowAnonymous]
        public JsonResult GetAttr(int tp, string lg)
        {
            var m = (DeclarationType)tp;
            return Json(new
            {
                Def = m.Localize("Description", lg),
                Group = m.Localize("GroupName", lg),
                Name = m.Localize("Name", lg),
                Units = m.Localize("Prompt", lg)
            });
        }

        [AllowAnonymous]
        public JsonResult GetRange(int yr)
        {
            var years = new List<int>();
            var months = new List<int>();

            months.AddRange(_context.SernapescaDeclarations
                .Where(a => a.Date.Year == yr).Select(a => a.Date.Month).Distinct().ToList());
            var start = months.Min();
            var end = months.Max();

            ViewData["Year"] = new SelectList(
                from int y in years
                select new { Id = y, Name = y }, "Id", "Name", yr);

            var meses = DateTimeFormatInfo.CurrentInfo.AbbreviatedMonthNames;

            var all = Enumerable.Range(1, 12).ToArray();
            //var disabled = Enumerable.Range(end.Value + 1, 12);

            var strt = new List<SelectListItem>(
                from int m in all
                select new SelectListItem { Text = meses[m - 1], Value = m.ToString(CultureInfo.InvariantCulture), Disabled = !months.Contains(m), Selected = m == start });

            var nd = new List<SelectListItem>(
                from int m in all
                select new SelectListItem { Text = meses[m - 1], Value = m.ToString(CultureInfo.InvariantCulture), Disabled = !months.Contains(m), Selected = m == end });

            return Json(new { start, end });
        }

        // GET: Boletin/Details/5
        public ActionResult Details(int id)
        {
            return View(id);
        }

        // GET: Boletin/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Boletin/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create(IFormCollection collection)
        //{
        //    if (collection == null)
        //    {
        //        throw new ArgumentNullException(nameof(collection));
        //    }

        //    try
        //    {
        //        // TODO: Add insert logic here

        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        // GET: Boletin/Edit/5
        public ActionResult Edit(int id)
        {
            return View(id);
        }

        // POST: Boletin/Edit/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit(int id, IFormCollection collection)
        //{
        //    if (collection == null)
        //    {
        //        throw new ArgumentNullException(nameof(collection));
        //    }
        //    // TODO: Add update logic here
        //    return RedirectToAction(nameof(Index));
        //}

        // GET: Boletin/Delete/5
        public ActionResult Delete(int id)
        {
            return View(id);
        }

        // POST: Boletin/Delete/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Delete(int id, IFormCollection collection)
        //{
        //    if (collection == null)
        //    {
        //        throw new ArgumentNullException(nameof(collection));
        //    }

        //    try
        //    {
        //        // TODO: Add delete logic here

        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}
    }
}