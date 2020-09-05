using BiblioMit.Data;
using BiblioMit.Extensions;
using BiblioMit.Models;
using BiblioMit.Models.Entities.Digest;
using DocumentFormat.OpenXml.Presentation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

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
        private IQueryable<DeclarationDate> GetDates(DeclarationType tp, int reg) =>
            tp switch
            {
                DeclarationType.Seed =>
                    GetDates(reg).Where(a => a.SernapescaDeclaration.Discriminator == tp && ((SeedDeclaration)a.SernapescaDeclaration).OriginId == 1),
                DeclarationType.Production =>
                    GetDates(reg).Where(a => a.SernapescaDeclaration.Discriminator == tp && a.ProductionType != ProductionType.Unknown && a.ItemType != Item.Product),
                _ => GetDates(reg).Where(a => a.SernapescaDeclaration.Discriminator == tp)
            };
        private IQueryable<DeclarationDate> GetDates(int reg) =>
            _context.DeclarationDates.Where(a => a.SernapescaDeclaration.OriginPsmb.Commune.Province.RegionId == reg);
        private IQueryable<DeclarationDate> GetDates(DeclarationType tp, int reg, DateTime start_dt, DateTime end_dt, DateTime start_dt_1, DateTime end_dt_1) =>
            GetDates(tp, reg).Where(a => (a.Date >= start_dt && a.Date <= end_dt) || (a.Date >= start_dt_1 && a.Date <= end_dt_1));
        private IQueryable<DeclarationDate> GetDates(DeclarationType tp, int reg, DateTime start_dt, DateTime end_dt) =>
            GetDates(tp, reg).Where(a => a.Date >= start_dt && a.Date <= end_dt);
        private IQueryable<DeclarationDate> GetDates(DeclarationType tp, Config config) =>
            config.Before ?
            GetDates(tp, config.Reg, config.Start, config.End, config.StartBefore, config.EndBefore) :
            GetDates(tp, config.Reg, config.Start, config.End);
        private IQueryable<IGrouping<string, DeclarationDate>> GetDatesCommunes(DeclarationType tp, Config config) =>
            GetDates(tp, config)
            .GroupBy(c => c.SernapescaDeclaration.OriginPsmb.Commune.Name)
            .OrderBy(g => g.Key);
        private IQueryable<IGrouping<string, DeclarationDate>> GetDatesProvinces(DeclarationType tp, Config config) =>
            GetDates(tp, config).GroupBy(c => c.SernapescaDeclaration.OriginPsmb.Commune.Province.Name)
            .OrderBy(g => g.Key);
        private IQueryable<IGrouping<int, DeclarationDate>> GetDatesMonths(DeclarationType tp, Config config) =>
            GetDates(tp, config).GroupBy(c => c.Date.Month).OrderBy(g => g.Key);
        private IQueryable<PlanktonAssay> GetAssays(Config config) =>
            GetAssays(config, config.StartBefore, config.EndBefore);
        private IQueryable<PlanktonAssay> GetAssays(Config config, DateTime startBefore, DateTime endBefore) =>
            config.Before ?
            GetAssays(config.Reg).Where(a =>
                (a.SamplingDate >= config.Start && a.SamplingDate <= config.End)
                || (a.SamplingDate >= startBefore && a.SamplingDate <= endBefore)) :
            GetAssays(config.Reg).Where(a => a.SamplingDate >= config.Start && a.SamplingDate <= config.End);
        private IQueryable<PlanktonAssay> GetAssays(int reg) => _context.PlanktonAssays.Where(a => a.Psmb.Commune.Province.RegionId == reg);
        [AllowAnonymous]
        public async Task<JsonResult> GetXlsx(int year, int start, int end)
        {
            var config = new Config(year, start, end)
            {
                Before = true
            };
            var pre = $"Total {config.Start.ToString("MMM", CultureInfo.InvariantCulture)}-{config.End.ToString("MMM", CultureInfo.InvariantCulture)}";
            var co = "Comuna";
            var pro = "Provincia";

            var graphs = new List<object>();
            var temp = new List<object>();
            var sal = new List<object>();

            graphs.AddRange(Enumerable.Range(1, 4).Select(tipo => GetDatesCommunes((DeclarationType)tipo, config)
                    .Select(comuna => new Dictionary<string, object>
                    {
                    { co, comuna.Key },
                    { pro, comuna.FirstOrDefault().SernapescaDeclaration.OriginPsmb.Commune.Province.Name },
                    { $"{pre} {config.YearBefore}", (int)Math.Round(comuna.Sum(a => a.Date.Year == config.YearBefore ? a.Weight : 0)) },
                    { $"{pre} {year}", (int)Math.Round(comuna.Sum(a => a.Date.Year == year ? a.Weight : 0)) }
                    })));

            var tmp = await GetAssays(config).ToListAsync().ConfigureAwait(false);

            var comunas = tmp.GroupBy(c => c.Psmb.Commune).OrderBy(o => o.Key.Name);

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
                if (comuna.Any(c => c.SamplingDate.Year == config.YearBefore))
                {
                    if (comuna.Any(c => c.Temperature.HasValue))
                    {
                        scyr_1 = Math.Round(comuna.Where(a => a.SamplingDate.Year == config.YearBefore && a.Salinity.HasValue)
                        .Average(a => a.Salinity.Value), 2);
                    }
                    if (comuna.Any(c => c.Salinity.HasValue))
                    {
                        cyr_1 = Math.Round(comuna.Where(a => a.SamplingDate.Year == config.YearBefore && a.Temperature.HasValue)
                        .Average(a => a.Temperature.Value), 2);
                    }
                }
                temp.Add(new Dictionary<string, object>
            {
                { co, comuna.Key.Name },
                { pro, comuna.Key.Province.Name },
                { $"{pre} {config.YearBefore}", cyr_1 },
                { $"{pre} {year}", cyr }
            });
                sal.Add(new Dictionary<string, object>
            {
                { co, comuna.Key.Name },
                { pro, comuna.Key.Province.Name },
                { $"{pre} {config.YearBefore}", scyr_1 },
                { $"{pre} {year}", scyr }
            });
            }
            graphs.Add(temp);
            graphs.Add(sal);
            return Json(graphs);
        }
        [AllowAnonymous]
        public JsonResult GetProvincias(int tipo, int year, int start, int end)
        {
            var config = new Config(year, start, end);

            return tipo switch
            {
                (int)DeclarationType.Temperature => Json(GetAssays(config)
                .Where(a => a.Temperature.HasValue)
                    .GroupBy(c => c.Psmb.Commune.Province.Name)
                    .OrderBy(o => o.Key)
                    .Select(provincia => new
                    {
                        provincia = provincia.Key,
                        ton = Math.Round(provincia
                        .Average(a => a.Temperature.Value), 2)
                    })),

                (int)DeclarationType.Salinity => Json(GetAssays(config)
                .Where(a => a.Salinity.HasValue)
                    .GroupBy(c => c.Psmb.Commune.Province.Name)
                    .OrderBy(o => o.Key)
                    .Select(provincia => new
                    {
                        provincia = provincia.Key,
                        ton = Math.Round(provincia
                        .Average(a => a.Salinity.Value), 2)
                    })),

                (int)DeclarationType.Production => Json(GetDatesProvinces((DeclarationType)tipo, config)
                    .Select(provi => new
                    {
                        provincia = provi.Key,
                        ton = (int)Math.Round(provi.Sum(a => a.Weight)),
                        //id = provi.FirstOrDefault().SernapescaDeclaration.OriginPsmb.Commune.ProvinceId,
                        subs = new List<object>
                        {
                        new {
                            provincia = "Congelado",
                            ton = (int)Math.Round(provi.Sum(p => p.ProductionType == ProductionType.Frozen ? p.Weight : 0))
                        },
                        new {
                            provincia = "Conserva",
                            ton = (int)Math.Round(provi.Sum(p => p.ProductionType == ProductionType.Preserved ? p.Weight : 0))
                        },
                        new {
                            provincia = "Refrigerado",
                            ton = (int)Math.Round(provi.Sum(p => p.ProductionType == ProductionType.Refrigerated ? p.Weight : 0))
                        }
                        //,
                        //new { provincia = "Desconocido",
                        //ton = (int)Math.Round(provi.Where(p => p.TipoProduccion == ProductionType.Desconocido).Sum(a => a.Peso)) }
                    }
                    })),

                    _ => Json(GetDatesProvinces((DeclarationType)tipo, config)
                    .Select(provincia => new { provincia = provincia.Key, ton = (int)Math.Round(provincia.Sum(a => a.Weight)) }))
            };
        }
        [AllowAnonymous]
        public JsonResult GetComunas(int tipo, int year, int start, int end, bool tb)
        {
            if (!tb && tipo == (int)DeclarationType.Production) tipo += 10;

            var config = new Config(year, start, end)
            {
                Before = true
            };

            return tipo switch
            {
                (int)DeclarationType.Temperature =>

                Json(GetAssays(config)
                .Where(c => c.SamplingDate.Year == year || c.SamplingDate.Year == config.YearBefore)
                .Where(c => c.Temperature.HasValue)
                .GroupBy(c => c.Psmb.Commune.Name).OrderBy(o => o.Key)
                .Select(comuna =>
                    new
                    {
                        comuna = comuna.Key,
                        lastyr =
                            Math.Round(comuna.Average(a => a.SamplingDate.Year == config.YearBefore ? a.Temperature : null).Value, 2),
                        year = Math.Round(comuna.Average(a => a.SamplingDate.Year == year ? a.Temperature : null).Value, 2)
                    })),

                (int)DeclarationType.Salinity =>

                Json(GetAssays(config)
                .Where(c => c.SamplingDate.Year == year || c.SamplingDate.Year == config.YearBefore)
                .Where(c => c.Salinity.HasValue)
                .GroupBy(c => c.Psmb.Commune.Name).OrderBy(o => o.Key)
                .Select(comuna =>
                    new
                    {
                        comuna = comuna.Key,
                        lastyr = Math.Round(comuna.Average(a => a.SamplingDate.Year == config.YearBefore ? a.Salinity : null).Value, 2),
                        year = Math.Round(comuna.Average(a => a.SamplingDate.Year == year ? a.Salinity : null).Value, 2)
                    })),

                (int)DeclarationType.Production =>

                Json(GetDatesCommunes(DeclarationType.Production, config)
                        .Select(comuna => new
                        {
                            comuna = comuna.Key,
                            aa_congelado = (int)Math.Round(comuna.Sum(a => a.Date.Year == year && a.ProductionType == ProductionType.Frozen ? a.Weight : 0)),
                            ab_conserva = (int)Math.Round(comuna.Sum(a => a.Date.Year == year && a.ProductionType == ProductionType.Preserved ? a.Weight : 0)),
                            ac_refrigerado = (int)Math.Round(comuna.Sum(a => a.Date.Year == year && a.ProductionType == ProductionType.Refrigerated ? a.Weight : 0)),
                            ba_congelado = (int)Math.Round(comuna.Sum(a => a.Date.Year == config.YearBefore && a.ProductionType == ProductionType.Frozen ? a.Weight : 0)),
                            bb_conserva = (int)Math.Round(comuna.Sum(a => a.Date.Year == config.YearBefore && a.ProductionType == ProductionType.Preserved ? a.Weight : 0)),
                            bc_refrigerado = (int)Math.Round(comuna.Sum(a => a.Date.Year == config.YearBefore && a.ProductionType == ProductionType.Refrigerated ? a.Weight : 0))
                    //var ad_desconicido = (int)Math.Round(cyr.Where(a => a.TipoProduccion == ProductionType.Desconocido).Sum(a => a.Peso));
                    //var bd_desconocido = (int)Math.Round(cyr_1.Where(a => a.TipoProduccion == ProductionType.Desconocido).Sum(a => a.Peso));
                })),

                14 => Json(GetDatesCommunes(DeclarationType.Production, config)
                    .Select(comuna => new
                    {
                        comuna = comuna.Key,
                        year = (int)Math.Round(comuna.Sum(a => a.Date.Year == year ? a.Weight : 0)),
                        lastyr = (int)Math.Round(comuna.Sum(a => a.Date.Year == config.YearBefore ? a.Weight : 0))
                    })),

                _ => Json(GetDatesCommunes((DeclarationType)tipo, config)
                    .Select(comuna => new
                    {
                        comuna = comuna.Key,
                        year = (int)Math.Round(comuna.Sum(a => a.Date.Year == year ? a.Weight : 0)),
                        lastyr = (int)Math.Round(comuna.Sum(a => a.Date.Year == config.YearBefore ? a.Weight : 0))
                    }))
            };
        }

        [AllowAnonymous]
        public JsonResult GetMeses(int tipo, int year, int start, int end)
        {
            var config = new Config(year, start, end);

            return tipo switch
            {
                (int)DeclarationType.Temperature => Json(GetAssays(config)
                .Where(a => a.Temperature.HasValue)
                    .GroupBy(c => c.SamplingDate.Month)
                    .OrderBy(o => o.Key)
                    .Select(month => new
                    {
                        date = $"{year}-{month.Key}",
                        value = Math.Round(month.Average(a => a.Temperature.Value), 2)
                    })),

                (int)DeclarationType.Salinity => Json(GetAssays(config)
                .Where(a => a.Salinity.HasValue)
                .GroupBy(c => c.SamplingDate.Month)
                .OrderBy(o => o.Key)
                .Select(month => new
                {
                    date = $"{year}-{month.Key}",
                    value = Math.Round(month.Average(a => a.Salinity.Value), 2)
                })),

                (int)DeclarationType.Production => Json(GetDatesMonths(DeclarationType.Production, config)
                    .Select(month => new
                    {
                        date = $"{year}-{month.Key}",
                        congelado = (int)Math.Round(month.Sum(a => a.ProductionType == ProductionType.Frozen ? a.Weight : 0)),
                        conserva = (int)Math.Round(month.Sum(a => a.ProductionType == ProductionType.Preserved ? a.Weight : 0)),
                        refrigerado = (int)Math.Round(month.Sum(a => a.ProductionType == ProductionType.Refrigerated ? a.Weight : 0))
                        //, desconocido = (int)Math.Round(month.Where(a => a.TipoProduccion == ProductionType.Desconocido).Sum(a => a.Peso))
                    })),

                    _ => Json(GetDatesMonths((DeclarationType)tipo, config)
                    .Select(month => new
                    {
                        date = $"{year}-{month.Key}",
                        value = (int)Math.Round(month.Sum(a => a.Weight))
                    }))
            };
        }
        [AllowAnonymous]
        public ActionResult Index(int? yr, int? start, int? end, int? reg, int? ver, int? tp) =>
            RedirectToAction(nameof(Boletin), new { yr, start, end, reg, ver, tp });

        // GET: Boletin
        [AllowAnonymous]
        public ActionResult Boletin(int? yr, int? start, int? end, int? reg, int? ver, int? tp)
        {
            if (!reg.HasValue) reg = 10;
            if (!ver.HasValue) ver = 3;
            if (!tp.HasValue) tp = 1;

            var years = _context.DeclarationDates.Select(a => a.Date.Year).Distinct();

            if (!yr.HasValue && years != null) yr = years.Max();
            var months = _context.DeclarationDates.Where(a => a.Date.Year == yr).Select(a => a.Date.Month).Distinct();
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
        public JsonResult GetAttr(int tp)
        {
            var m = (DeclarationType)tp;
            return Json(new
            {
                Def = m.GetAttrDescription(),
                Group = m.GetAttrGroupName(),
                Name = m.GetAttrName(),
                Units = m.GetAttrPrompt()
            });
        }
        [AllowAnonymous]
        public JsonResult GetRange(int yr)
        {
            var years = _context.DeclarationDates.Select(a => a.Date.Year).Distinct();
            var months = _context.DeclarationDates.Where(a => a.Date.Year == yr).Select(a => a.Date.Month).Distinct();
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
        public ActionResult Details(int id) => View(id);

        // GET: Boletin/Create
        public ActionResult Create() => View();

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
        public ActionResult Edit(int id) => View(id);

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
        public ActionResult Delete(int id) => View(id);

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
    public class Config
    {
        public Config(int year, int start, int end)
        {
            DateTime.TryParseExact($"{start} {year}", "M yyyy", CultureInfo.GetCultureInfo("en-GB"), DateTimeStyles.None, out var start_dt);
            DateTime.TryParseExact($"{DateTime.DaysInMonth(year, end)} {end} {year}", "d M yyyy", CultureInfo.GetCultureInfo("en-GB"), DateTimeStyles.None, out var end_dt);
            Reg = 110;
            Year = year;
            Start = start_dt;
            End = end_dt;
            YearBefore = Year - 1;
            StartBefore = Start.AddYears(-1);
            EndBefore = End.AddYears(-1);
        }
        public int Reg { get; set; }
        public int Year { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public bool Before { get; set; }
        public int YearBefore { get; set; }
        public DateTime StartBefore { get; set; }
        public DateTime EndBefore { get; set; }
    }
}