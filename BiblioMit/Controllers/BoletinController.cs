using BiblioMit.Data;
using BiblioMit.Extensions;
using BiblioMit.Models;
using BiblioMit.Models.Entities.Digest;
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
        private IQueryable<DeclarationDate> GetDates(DeclarationType tp, int reg)
        {
            var dates = _context.DeclarationDates
                .Include(c => c.SernapescaDeclaration)
                .ThenInclude(d => d.OriginPsmb)
                .ThenInclude(o => o.Commune)
                    .ThenInclude(c => c.Province)
                .Where(a =>
                a.SernapescaDeclaration.Discriminator == tp
                && a.SernapescaDeclaration.OriginPsmb.Commune.Province.RegionId == reg);
            if (tp == DeclarationType.Seed)
            {
                dates = dates.Where(a => ((SeedDeclaration)a.SernapescaDeclaration).OriginId == 1);
            }
            else if (tp == DeclarationType.Production)
            {
                dates = dates.Where(a =>
                a.ProductionType != ProductionType.Unknown
                && a.ItemType != Item.Product);
            }
            return dates;
        }
        private async Task<List<DeclarationDate>> GetDates(DeclarationType tp, Config conf)
        {
            return conf.Before ?
                await GetDates(tp, conf.Reg, conf.Start, conf.End, conf.StartBefore(), conf.EndBefore()).ConfigureAwait(false) :
                await GetDates(tp, conf.Reg, conf.Start, conf.End).ConfigureAwait(false);
        }
        private async Task<List<DeclarationDate>> GetDates(DeclarationType tp, int reg, DateTime start_dt, DateTime end_dt)
        {
            var dates = GetDates(tp, reg);
            dates = dates.Where(a => a.Date >= start_dt && a.Date <= end_dt);
            return await dates.ToListAsync().ConfigureAwait(false);
        }
        private async Task<List<DeclarationDate>> GetDates(DeclarationType tp, int reg, DateTime start_dt, DateTime end_dt, DateTime start_dt_1, DateTime end_dt_1)
        {
            var dates = GetDates(tp, reg);
            dates = dates.Where(a => (a.Date >= start_dt && a.Date <= end_dt)
                || (a.Date >= start_dt_1 && a.Date <= end_dt_1));
            return await dates.ToListAsync().ConfigureAwait(false);
        }
        private async Task<List<PlanktonAssay>> GetAssays(Config config)
        {
            var planktons = _context.PlanktonAssays
                .Include(c => c.Psmb.Commune)
                    .ThenInclude(c => c.Province)
                .Where(a =>
                a.Psmb.Commune.Province.RegionId == config.Reg);
            planktons = config.Before ? planktons.Where(a =>
                (a.SamplingDate >= config.Start && a.SamplingDate <= config.End)
                || (a.SamplingDate >= config.StartBefore() && a.SamplingDate <= config.EndBefore())) :
                planktons.Where(a =>
                a.SamplingDate >= config.Start && a.SamplingDate <= config.End);
            return await planktons.ToListAsync().ConfigureAwait(false);
        }
        private static Config GetConfig(int year, int start, int end)
        {
            DateTime.TryParseExact($"{start} {year}", "M yyyy", CultureInfo.GetCultureInfo("en-GB"), DateTimeStyles.None, out var start_dt);
            DateTime.TryParseExact($"{DateTime.DaysInMonth(year, end)} {end} {year}", "d M yyyy", CultureInfo.GetCultureInfo("en-GB"), DateTimeStyles.None, out var end_dt);
            return new Config
            {
                Reg = 10,
                Year = year,
                Start = start_dt,
                End = end_dt
            };
        }
        [AllowAnonymous]
        public async Task<JsonResult> GetXlsx(int year, int start, int end)
        {
            var config = GetConfig(year, start, end);
            config.Before = true;
            var pre = $"Total {config.Start.ToString("MMM", CultureInfo.InvariantCulture)}-{config.End.ToString("MMM", CultureInfo.InvariantCulture)}";
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

                var dateslst = await GetDates(tp, config).ConfigureAwait(false);
                var comuns = dateslst
                    .GroupBy(c => c.SernapescaDeclaration.OriginPsmb.Commune).OrderBy(o => o.Key.Name);

                foreach (var comuna in comuns)
                {
                    var cyr = (int)Math.Round(comuna.Where(a => a.Date.Year == year).Sum(a => a.Weight));
                    var cyr_1 = (int)Math.Round(comuna.Where(a => a.Date.Year == config.YearBefore()).Sum(a => a.Weight));
                    tmp.Add(new Dictionary<string, object>
                    {
                        { co, comuna.Key.Name },
                        { pro, comuna.Key.Province.Name },
                        { $"{pre} {config.YearBefore()}", cyr_1 },
                        { $"{pre} {year}", cyr }
                    });
                }
                graphs.Add(tmp);
            }

            var planktonslst = await GetAssays(config).ConfigureAwait(false);
            var comunas = planktonslst.GroupBy(c => c.Psmb.Commune).OrderBy(o => o.Key.Name);

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
                if (comuna.Any(c => c.SamplingDate.Year == config.YearBefore()))
                {
                    if (comuna.Any(c => c.Temperature.HasValue))
                    {
                        scyr_1 = Math.Round(comuna.Where(a => a.SamplingDate.Year == config.YearBefore() && a.Salinity.HasValue)
                        .Average(a => a.Salinity.Value), 2);
                    }
                    if (comuna.Any(c => c.Salinity.HasValue))
                    {
                        cyr_1 = Math.Round(comuna.Where(a => a.SamplingDate.Year == config.YearBefore() && a.Temperature.HasValue)
                        .Average(a => a.Temperature.Value), 2);
                    }
                }
                temp.Add(new Dictionary<string, object>
                {
                    { co, comuna.Key.Name },
                    { pro, comuna.Key.Province.Name },
                    { $"{pre} {config.YearBefore()}", cyr_1 },
                    { $"{pre} {year}", cyr }
                });
                sal.Add(new Dictionary<string, object>
                {
                    { co, comuna.Key.Name },
                    { pro, comuna.Key.Province.Name },
                    { $"{pre} {config.YearBefore()}", scyr_1 },
                    { $"{pre} {year}", scyr }
                });
            }
            graphs.Add(temp);
            graphs.Add(sal);
            return Json(graphs);
        }
        [AllowAnonymous]
        public async Task<JsonResult> GetProvincias(int tipo, int year, int start, int end)
        {
            var config = GetConfig(year, start, end);

            var graphs = new List<object>();

            if (tipo > (int)DeclarationType.Production)
            {
                var temp = tipo == (int)DeclarationType.Temperature;

                var ambientaleslst = await GetAssays(config).ConfigureAwait(false);

                var provincias = ambientaleslst.GroupBy(c => c.Psmb.Commune.Province).OrderBy(o => o.Key.Name);

                foreach (var provincia in provincias)
                {
                    var cyr = Math.Round(provincia.Where(a => temp ? a.Temperature.HasValue : a.Salinity.HasValue)
                        .Average(a => temp ? a.Temperature.Value : a.Salinity.Value), 2);
                    graphs.Add(new { provincia = provincia.Key.Name, ton = cyr });
                }
            }
            else if (tipo == (int)DeclarationType.Production)
            {
                var tp = (DeclarationType)tipo;

                var planilla = await GetDates(tp, config).ConfigureAwait(false);

                var provincias = planilla.GroupBy(c => c.SernapescaDeclaration.OriginPsmb.Commune.Province).OrderBy(o => o.Key.Name);

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

                var planilla = await GetDates(tp, config).ConfigureAwait(false);

                var provincias = planilla.GroupBy(c => c.SernapescaDeclaration.OriginPsmb.Commune.Province).OrderBy(o => o.Key.Name);

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
        public async Task<JsonResult> GetComunas(int tipo, int year, int start, int end, bool? tb)
        {
            if (!tb.HasValue) tb = false;
            var config = GetConfig(year, start, end);
            config.Before = true;

            var graphs = new List<object>();

            if (tipo > (int)DeclarationType.Production)
            {
                var temp = tipo == (int)DeclarationType.Temperature;

                var ambientaleslst = await GetAssays(config).ConfigureAwait(false);

                var comunas = ambientaleslst.GroupBy(c => c.Psmb.Commune).OrderBy(o => o.Key.Name);

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
                        if (comuna.Any(c => c.SamplingDate.Year == config.YearBefore() && c.Temperature.HasValue))
                        {
                            cyr_1 = Math.Round(comuna.Where(a => a.SamplingDate.Year == config.YearBefore() && a.Temperature.HasValue)
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
                        if (comuna.Any(c => c.SamplingDate.Year == config.YearBefore() && c.Salinity.HasValue))
                        {
                            cyr_1 = Math.Round(comuna.Where(a => a.SamplingDate.Year == config.YearBefore() && a.Salinity.HasValue)
                            .Average(a => a.Salinity.Value), 2);
                        }
                    }
                    graphs.Add(new { comuna = comuna.Key.Name, lastyr = cyr_1, year = cyr });
                }
            }
            else if (tipo == (int)DeclarationType.Production && !tb.Value)
            {
                var tp = (DeclarationType)tipo;

                var planillalst = await GetDates(tp, config).ConfigureAwait(false);

                var comunas = planillalst.GroupBy(c => c.SernapescaDeclaration.OriginPsmb.Commune).OrderBy(o => o.Key.Name);

                foreach (var comuna in comunas)
                {
                    var cyr = comuna.Where(a => a.Date.Year == year);
                    var cyr_1 = comuna.Where(a => a.Date.Year == config.YearBefore());
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

                var dateslst = await GetDates(tp, config).ConfigureAwait(false);

                var comunas = dateslst.GroupBy(c => c.SernapescaDeclaration.OriginPsmb.Commune).OrderBy(o => o.Key.Name);

                foreach (var comuna in comunas)
                {
                    var cyr = (int)Math.Round(comuna.Where(a => a.Date.Year == year).Sum(a => a.Weight));
                    var cyr_1 = (int)Math.Round(comuna.Where(a => a.Date.Year == config.YearBefore()).Sum(a => a.Weight));
                    graphs.Add(new { comuna = comuna.Key.Name, year = cyr, lastyr = cyr_1 });
                }
            }
            return Json(graphs);
        }

        [AllowAnonymous]
        public async Task<JsonResult> GetMeses(int tipo, int year, int start, int end)
        {
            var config = GetConfig(year, start, end);

            var graphs = new List<object>();

            if (tipo > (int)DeclarationType.Production)
            {
                var ambientaleslst = await GetAssays(config).ConfigureAwait(false);

                var meses = ambientaleslst.GroupBy(c => c.SamplingDate.Month).OrderBy(o => o.Key);

                var temp = tipo == (int)DeclarationType.Temperature;

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

                var planillalst = await GetDates(tp, config).ConfigureAwait(false);

                var meses = planillalst.GroupBy(c => c.Date.Month).OrderBy(o => o.Key);

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

                var planilla = await GetDates(tp, config).ConfigureAwait(false);

                var meses = planilla.GroupBy(c => c.Date.Month).OrderBy(o => o.Key);

                foreach (var month in meses)
                {
                    var cyr = (int)Math.Round(month.Sum(a => a.Weight));
                    graphs.Add(new { date = $"{year}-{month.Key}", value = cyr });
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
            years.AddRange(_context.DeclarationDates.Select(a => a.Date.Year).Distinct().ToList());

            if (!yr.HasValue && years != null) yr = years.Max();
            months.AddRange(_context.DeclarationDates.Where(a => a.Date.Year == yr).Select(a => a.Date.Month).Distinct().ToList());
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
            var years = new List<int>();
            var months = new List<int>();

            months.AddRange(_context.DeclarationDates
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
    public class Config
    {
        public int Reg { get; set; }
        public int Year { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public bool Before { get; set; }
        public int YearBefore() => Year - 1;
        public DateTime StartBefore() => Start.AddYears(-1);
        public DateTime EndBefore() => End.AddYears(-1);
    }
}