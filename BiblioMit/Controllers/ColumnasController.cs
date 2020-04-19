﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BiblioMit.Data;
using BiblioMit.Models;
using BiblioMit.Extensions;
using Microsoft.AspNetCore.Authorization;
using BiblioMit.Models.Entities.Digest;

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

            //bool _asc = asc.Value;

            var pre = _context.Registries.Pre();
            //var sort = _context.Registries.FilterSort(srt);
            ViewData = _context.Registries.ViewData(pre, pg, rpp, srt, asc, val);
            ViewData["ExcelId"] = new MultiSelectList(
                from InputFile e in _context.InputFiles
                select new
                { e.Id, e.ClassName }, "Id", "ClassName",
                ViewData["Filters"] is IDictionary<string, List<string>> Filters && Filters.ContainsKey("ExcelId") ?
                Filters["ExcelId"] : null);

            //var applicationDbContext = _asc ?
            //    pre
            //    .OrderBy(x => sort.GetValue(x))
            //    //.Skip((pg.Value - 1) * rpp.Value).Take(rpp.Value)
            //    .Include(c => c.InputFile) :
            //    pre
            //    .OrderByDescending(x => sort.GetValue(x))
            //    //.Skip((pg.Value - 1) * rpp.Value).Take(rpp.Value)
            //    .Include(c => c.InputFile);

            var regs = _context.Registries.Include(r => r.InputFile).Include(r => r.Headers);

            var two2five = User.Claims.Any(c => c.Value == "per" || c.Value == "sernapesca");
            var one = User.Claims.Any(c => c.Value == "intemit");

            if(one && two2five)
            {
                return View(await regs.Where(r => r.InputFileId < 6).ToListAsync().ConfigureAwait(false));
            }
            else if(two2five)
            {
                return View(await regs.Where(r => r.InputFileId > 1 && r.InputFileId < 6).ToListAsync().ConfigureAwait(false));
            }
            else if (one)
            {
                return View(await regs.Where(r => r.InputFileId == 1).ToListAsync().ConfigureAwait(false));
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Editar(int id, string description, string headers, string conversion)
        {
            var model = await _context.Registries
                .FindAsync(id).ConfigureAwait(false);
            var heads = _context.Headers.Where(h => h.RegistryId == id);
            model.Description = string.IsNullOrWhiteSpace(description) ? null : description;
            model.Operation = string.IsNullOrWhiteSpace(conversion) ? null : conversion;
            var all = headers?.Split(";;");
            if (all.Any())
            {
                var newh = all.Select(a => { var h = new Header(); h.SetName(a); return h; });
                _context.Headers.RemoveRange(heads);
                _context.Headers.AddRange(newh);
            }
            _context.Registries.Update(model);
            var result = await _context.SaveChangesAsync().ConfigureAwait(false);

            return Json(result);
        }
    }
}
