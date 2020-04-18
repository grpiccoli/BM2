using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BiblioMit.Data;
using BiblioMit.Models;
using BiblioMit.Extensions;
using Microsoft.AspNetCore.Authorization;

namespace BiblioMit.Controllers
{
    [Authorize]
    public class ColumnasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ColumnasController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int? pg, int? rpp, string srt, bool? asc, string[] val) =>
            RedirectToAction(nameof(Columnas), new { pg, rpp, srt, asc, val });
        // GET: Columnas
        public async Task<IActionResult> Columnas(int? pg, int? rpp, string srt,
            bool? asc, string[] val)
        {
            if (pg == null) pg = 1;
            if (rpp == null) rpp = 20;
            if (string.IsNullOrEmpty(srt)) srt = "ExcelId";
            if (asc == null) asc = true;

            bool _asc = asc.Value;

            var pre = _context.Registries.Pre();
            var sort = _context.Registries.FilterSort(srt);
            ViewData = _context.Registries.ViewData(pre, pg, rpp, srt, asc, val);
            var Filters = ViewData["Filters"] as IDictionary<string, List<string>>;

            var applicationDbContext = _asc ?
                pre
                .OrderBy(x => sort.GetValue(x))
                //.Skip((pg.Value - 1) * rpp.Value).Take(rpp.Value)
                .Include(c => c.InputFile) :
                pre
                .OrderByDescending(x => sort.GetValue(x))
                //.Skip((pg.Value - 1) * rpp.Value).Take(rpp.Value)
                .Include(c => c.InputFile);

            ViewData["ExcelId"] = new MultiSelectList(
                from InputFile e in _context.InputFiles
                select new
                { e.Id, e.ClassName }, "Id", "ClassName", Filters.ContainsKey("ExcelId") ?
                Filters["ExcelId"] : null);

            return View(await applicationDbContext.ToListAsync().ConfigureAwait(false));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Editar(string planilla, string atributo, string columna, string conversion)
        {
            var model = await _context.Registries
                .SingleOrDefaultAsync(c => c.InputFile.ClassName == planilla && c.NormalizedAttribute == atributo.ToUpperInvariant()).ConfigureAwait(false);
            model.Description = string.IsNullOrWhiteSpace(columna) ? null : columna;
            model.Operation = string.IsNullOrWhiteSpace(conversion) ? null : conversion;
            _context.Registries.Update(model);
            var result = await _context.SaveChangesAsync().ConfigureAwait(false);

            return Json(result);
        }
    }
}
