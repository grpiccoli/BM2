using BiblioMit.Data;
using BiblioMit.Extensions;
using BiblioMit.Models;
using BiblioMit.Models.Entities.Digest;
using BiblioMit.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BiblioMit.Controllers
{
    [Authorize]
    public class EntriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly IImport _import;
        private readonly ITableToExcel _tableToExcel;
        private readonly IStringLocalizer<EntriesController> _localizer;

        public EntriesController(ApplicationDbContext context,
            IImport import,
            IStringLocalizer<EntriesController> localizer,
            IServiceProvider serviceProvider,
            UserManager<ApplicationUser> userManager,
            ITableToExcel tableToExcel)
        {
            _localizer = localizer;
            _tableToExcel = tableToExcel;
            _import = import;
            _context = context;
            _userManager = userManager;
            _serviceProvider = serviceProvider;
        }

        // GET: Entries
        [HttpGet]
        public IActionResult Index(int? id, int? pg, int? rpp, string srt,
            bool? asc, string[] val)
        {
            if (!pg.HasValue) pg = 1;
            if (!rpp.HasValue) rpp = 20;
            if (string.IsNullOrWhiteSpace(srt)) srt = "Date";
            if (!asc.HasValue) asc = false;

            var pre = _context.SernapescaEntries.Pre();
            var sort = _context.SernapescaEntries.FilterSort(srt);
            ViewData = _context.SernapescaEntries.ViewData(pre, pg, rpp, srt, asc, val);
            var Filters = ViewData["Filters"] as IDictionary<string, List<string>>;

            var applicationDbContext = asc.Value ?
                pre
                .Include(e => e.ApplicationUser)
                .ToList()
                .OrderBy(x => sort.GetValue(x))
                .Skip((pg.Value - 1) * rpp.Value).Take(rpp.Value)
                :
                pre
                .Include(e => e.ApplicationUser)
                .ToList()
                .OrderByDescending(x => sort.GetValue(x))
                .Skip((pg.Value - 1) * rpp.Value).Take(rpp.Value);

            ViewData["Processing"] = id;

            //if(Filters["Tipo"] == null || Filters["Tipo"].Count() == 0)
            //{
            Filters["Tipo"] = new List<string> { "Semilla", "Cosecha", "Abastecimiento", "Producción" };
            //}

            ViewData[nameof(DeclarationType)] = DeclarationType.Supply.Enum2MultiSelect();

            ViewData["Date"] = string.Format(CultureInfo.CurrentCulture, "'{0}'",
                string.Join("','", _context.SernapescaEntries.Select(v => v.Date.Date.ToString("yyyy-M-d", CultureInfo.CurrentCulture)).Distinct().ToList()));

            return View(applicationDbContext);
        }

        // GET: Entries/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entry = await _context.SernapescaEntries
                .Include(e => e.ApplicationUser)
                .SingleOrDefaultAsync(m => m.Id == id)
                .ConfigureAwait(false);
            if (entry == null)
            {
                return NotFound();
            }

            return View(entry);
        }
        [HttpGet]
        public IActionResult Output(int id)
        {
            var model = _context.SernapescaEntries.FirstOrDefault(e => e.Id == id);
            return PartialView("_Output", model);
        }
        // GET: Entries/Create
        [HttpGet]
        public IActionResult CreateFito()
        {
            return View();
        }
        [HttpPost]
        [Produces("application/json")]
        [ValidateAntiForgeryToken]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> CreateFito(IFormFile qqfile)
        {
            if (qqfile == null) return Json(new { success = false, error = "error file null" });

            if (qqfile.Length > 0)
            {
                try
                {
                    var result = await _import.AddAsync(qqfile).ConfigureAwait(false);
                    return Json(new { success = true, error = string.Empty });
                }
                catch
                {
                    throw;
                    //return Json(new { success = false, error = ex.Message });
                }
            }
            throw new ArgumentNullException($"Argument {qqfile} has length 0");
            //return Json(new { success = false, error = "qqfile length 0" });
        }

        // GET: Entries/Create
        [HttpGet]
        public IActionResult Create()
        {
            ViewData[nameof(DeclarationType)] = DeclarationType.Supply.Name2Select();

            return View();
        }
        // POST: Entries/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[RequestFormSizeLimit(valueCountLimit: 200000, Order = 1)]
        [ValidateAntiForgeryToken]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Create([Bind("InputFile,DeclarationType")] SernapescaEntry entry)
        {
            if (ModelState.IsValid)
            {
                if (entry?.InputFile == null) return View(nameof(Create));

                entry.ApplicationUserId = _userManager.GetUserId(User);
                entry.IP = Request.HttpContext.Connection.RemoteIpAddress.ToString();
                entry.FileName = entry.InputFile.FileName;
                entry.Date = DateTime.Now;
                entry.Success = false;
                await _context.Entries.AddAsync(entry).ConfigureAwait(false);
                await _context.SaveChangesAsync()
                    .ConfigureAwait(false);

                var result = string.Empty;

                Stream stream = entry.InputFile.OpenReadStream();
                using ExcelPackage package = new(stream);
                var t = entry.DeclarationType switch
                {
                    DeclarationType.Seed => await _import.ReadAsync<SeedDeclaration>(package, entry).ConfigureAwait(false),
                    DeclarationType.Harvest => await _import.ReadAsync<HarvestDeclaration>(package, entry).ConfigureAwait(false),
                    DeclarationType.Supply => await _import.ReadAsync<SupplyDeclaration>(package, entry).ConfigureAwait(false),
                    DeclarationType.Production => await _import.ReadAsync<ProductionDeclaration>(package, entry).ConfigureAwait(false),
                    _ => Task.FromException(new ArgumentException(_localizer["Error input format"]))
                };
                return RedirectToAction(nameof(Index), new { id = entry.Id });
            }
            var Filters = new Dictionary<string, List<string>>
            {
                ["Tipo"] = DeclarationType.Supply.Enum2ListNames().ToList()
            };

            ViewData[nameof(DeclarationType)] = DeclarationType.Supply.Enum2MultiSelect(Filters[typeof(DeclarationType).ToString()], "Name");
            return View(entry);
        }
        // GET: Entries/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entry = await _context.SernapescaEntries.SingleOrDefaultAsync(m => m.Id == id)
                .ConfigureAwait(false);
            if (entry == null)
            {
                return NotFound();
            }
            ViewData["ApplicationUserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id", entry.ApplicationUserId);
            return View(entry);
        }

        // POST: Entries/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Edit(int id, 
            [Bind("Id,ApplicationUserId,IP,ProcessStart,ProcessTime,Stage,DeclarationType")] SernapescaEntry entry)
        {
            if (id != entry?.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(entry);
                    await _context.SaveChangesAsync()
                        .ConfigureAwait(false);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EntryExists(entry.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ApplicationUserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id", entry.ApplicationUserId);
            return View(entry);
        }
        // GET: Entries/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entry = await _context.SernapescaEntries
                .Include(e => e.ApplicationUser)
                .SingleOrDefaultAsync(m => m.Id == id)
                .ConfigureAwait(false);
            if (entry == null)
            {
                return NotFound();
            }

            return View(entry);
        }
        // POST: Entries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entry = await _context.SernapescaEntries.SingleOrDefaultAsync(m => m.Id == id).ConfigureAwait(false);
            _context.SernapescaEntries.Remove(entry);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return RedirectToAction(nameof(Index));
        }
        private bool EntryExists(int id)
        {
            return _context.SernapescaEntries.Any(e => e.Id == id);
        }
    }
}