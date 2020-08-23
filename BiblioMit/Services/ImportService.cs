using BiblioMit.Data;
using BiblioMit.Extensions;
using BiblioMit.Models;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using NCalc;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DataTableExtensions = BiblioMit.Extensions.DataTableExtensions;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using AngleSharp.Text;
using BiblioMit.Models.Entities.Digest;
using BiblioMit.Models.Entities.Centres;
using System.Linq.Expressions;
using BiblioMit.Models.Entities.Environmental;
using BiblioMit.Services.Hubs;
using Microsoft.AspNetCore.Identity;

namespace BiblioMit.Services
{
    public class ImportService : IImport
    {
        private string Declaration { get; set; }
        private List<string> Headers { get; set; }
        private Registry PhytoStart { get; set; }
        private Registry PhytoEnd { get; set; }
        private int StartRow { get; set; }
        private Tdata PhytoTData { get; set; }
        private Dictionary<string, Dictionary<string, int>> InSet { get; set; } = new Dictionary<string, Dictionary<string, int>>();
        private MethodInfo FirstOrDefaultAsyncMethod { get; set; }
        private InputFile InputFile { get; set; }
        private List<Tdata> Tdatas { get; set; }
        [SuppressMessage("Performance", "CA1802:Use literals where appropriate", Justification = "includes circular definition")]
        private static readonly BindingFlags BindingFlags = BindingFlags.Instance | BindingFlags.Public;
        private List<PropertyInfo> FieldInfos { get; } = new List<PropertyInfo>();
        private List<Commune> Communes { get; set; }
        //private const string encoding = "Windows-1252";
        private readonly ApplicationDbContext _context;
        private readonly IEntryHub _hubContext;
        private readonly IStringLocalizer<ImportService> _localizer;
        private readonly ITableToExcel _tableToExcel;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContext;
        public ImportService(ApplicationDbContext context,
            IEntryHub hubContext,
            IHttpContextAccessor httpContext,
            UserManager<ApplicationUser> userManager,
            ITableToExcel tableToExcel,
            IStringLocalizer<ImportService> localizer)
        {
            _httpContext = httpContext;
            _tableToExcel = tableToExcel;
            _userManager = userManager;
            _localizer = localizer;
            _hubContext = hubContext;
            _context = context;
        }
        public async Task<Task> AddRangeAsync(string pwd, IEnumerable<string> files)
        {
            if (files == null) throw new ArgumentNullException(_localizer[$"argument files cannot be null {files}"]);
            var resultInit = await Init<PlanktonAssay>().ConfigureAwait(false);
            if (resultInit.IsCompletedSuccessfully)
            {
                foreach (var file in files)
                {
                    await AddEntryAsync(file, pwd).ConfigureAwait(false);
                }
                return Task.CompletedTask;
            }
            throw new ArgumentException(_localizer["Error Initializing ImportService"]);
        }
        public async Task<Task> AddAsync(string file)
        {
            using var stream = File.OpenRead(file);
            return await AddAsync(stream).ConfigureAwait(false);
        } //polymorphism convertion
        public async Task<Task> AddAsync(IFormFile file) =>
            await AddAsync(file?.OpenReadStream()).ConfigureAwait(false); //polymorphism convertion
        public async Task<Task> AddAsync(Stream file)
        {
            var resultInit = await Init<PlanktonAssay>().ConfigureAwait(false);
            if (resultInit.IsCompletedSuccessfully)
            {
                var matrix = await _tableToExcel.HtmlTable2Matrix(file).ConfigureAwait(false);
                return await AddEntryAsync(matrix).ConfigureAwait(false);
            }
            throw new ArgumentException(_localizer[$"Unknown Error"]);
        }
        public async Task<Task> AddFilesAsync(string pwd)
        {
            if (!Directory.Exists(pwd))
                throw new DirectoryNotFoundException(_localizer[$"directory {pwd} not found"]);
            var logs = Path.Combine(pwd, "LOGS");
            if (Directory.Exists(logs))
            {
                var di = new DirectoryInfo(logs);
                foreach (var file in di.GetFiles("*log"))
                    file.Delete();
            }
            Directory.CreateDirectory(logs);
            string filesPath = Path.Combine(pwd, "plankton");
            if (!Directory.Exists(filesPath))
                throw new DirectoryNotFoundException(_localizer[$"directory {filesPath} not found"]);
            IEnumerable<string> files = Directory.GetDirectories(filesPath).SelectMany(d => Directory.GetFiles(d));
            if (files.Any())
            {
                Task response1 = await AddRangeAsync(pwd, files).ConfigureAwait(false);
                if (!response1.IsCompletedSuccessfully) throw new Exception(_localizer["Error when adding Plankton records"]);
            }
            var context = _httpContext.HttpContext;
            var userId = string.Empty;
            if (context != null)
            {
                var contextUser = context.User;
                var user = await _userManager.GetUserAsync(contextUser).ConfigureAwait(false);
                userId = user.Id;
            }
            else
            {
                var user = await _context.ApplicationUsers.FirstOrDefaultAsync(u => u.UserName == "WebMaster").ConfigureAwait(false);
                userId = user.Id;
            }
            for (var i = 1; i < 5; i++)
            {
                string dt = ((DeclarationType)i).ToString();
                string dir = Path.Combine(pwd, "declaration", dt);
                if (!Directory.Exists(dir)) continue;
                IEnumerable<string> ds = Directory.GetFiles(dir);
                foreach(var d in ds)
                {
                    var logFile = Path.Combine(logs, $"{dt}_{Path.GetFileNameWithoutExtension(d)}.log");
                    using var log = new StreamWriter(logFile);
                    var entry = new SernapescaEntry
                    {
                        ApplicationUserId = userId,
                        FileName = Path.GetFileNameWithoutExtension(d),
                        Date = DateTime.Now,
                        Success = false,
                        DeclarationType = (DeclarationType)i
                    };
                    await _context.Entries.AddAsync(entry).ConfigureAwait(false);
                    await _context.SaveChangesAsync()
                        .ConfigureAwait(false);
                    var fs = File.OpenRead(d);
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    using ExcelPackage package = new ExcelPackage(fs);
                    try 
                    {
                        var result = i switch
                        {
                            1 => await ReadAsync<SeedDeclaration>(package, entry).ConfigureAwait(false),
                            2 => await ReadAsync<HarvestDeclaration>(package, entry).ConfigureAwait(false),
                            3 => await ReadAsync<SupplyDeclaration>(package, entry).ConfigureAwait(false),
                            _ => await ReadAsync<ProductionDeclaration>(package, entry).ConfigureAwait(false)
                        };
                    }
                    catch (ArgumentException de)
                    {
                        log.WriteLine($"File: {d}");
                        log.WriteLine($"DNE: {de}");
                        throw;
                    }
                    catch (Exception e)
                    {
                        log.WriteLine($"File: {d}");
                        log.WriteLine($"E: {e}");
                        throw;
                    }
                }
            }
            return Task.CompletedTask;
        }
        public async Task<Task> ReadAsync<T>(ExcelPackage package, SernapescaEntry entry)
            where T : SernapescaDeclaration
        {
            if (entry == null || package == null)
                throw new ArgumentNullException($"Arguments Entry {entry} and package {package} cannot be null");

            Task resultInit = await Init<T>().ConfigureAwait(false);
            if (!resultInit.IsCompletedSuccessfully) throw new ArgumentException(_localizer[$"Unknown Error"]);
            double pgr = 0;
            IEnumerable<ExcelWorksheet> worksheets = package.Workbook.Worksheets.Where(w => w.Dimension != null);
            double rows = worksheets.Sum(w => w.Dimension.Rows);
            double pgrRow = 100 / rows;
            string msg = string.Empty;
            HttpContext context = _httpContext.HttpContext;
            List<int> datesIds = new List<int>();
            string userId = string.Empty;
            if (context != null)
            {
                userId = context.User.Identity.Name;
            }
            else
            {
                userId = "nofeed";
            }
            foreach (ExcelWorksheet worksheet in worksheets)
            {
                if (worksheet == null) continue;
                int rowCount = worksheet.Dimension.Rows;
                ExcelRange headers = worksheet.Cells["1:1"];
                Headers = headers.Select(s => s.Value.ToString().CleanCell()).ToList();
                for (int row = 2; row <= rowCount; row++)
                {
                    if (worksheet.Cells[row, 1].Value == null)
                    {
                        msg = $">W: Fila '{row}' en hoja '{worksheet.Name}' Está vacía.";
                        entry.OutPut += msg;
                        await _hubContext.SendLog(userId, msg)
                            .ConfigureAwait(false);
                        await _hubContext.SendStatusWarning(userId)
                            .ConfigureAwait(false);
                        continue;
                    }
                    int last = Tdatas.Count - 1;
                    T item = Activator.CreateInstance<T>();
                    item.EntryId = entry.Id;
                    for (int d = 0; d < last; d++)
                    {
                        object value = await GetValue(worksheet, d, row, item).ConfigureAwait(false);
                        if (value == null)
                        {
                            if (Tdatas[d].Name == nameof(SeedDeclaration.Origin)) continue;
                            msg =
                                $">ERROR: Columna '{string.Join(",", Tdatas[d].Q)}' no encontrada en hoja '{worksheet.Name}'. Verificar archivo.\n0 registros procesados.";
                            entry.OutPut += msg;
                            _context.Update(entry);
                            await _context.SaveChangesAsync()
                                .ConfigureAwait(false);
                            await _hubContext.SendLog(userId, msg)
                            .ConfigureAwait(false);
                            await _hubContext.SendStatusDanger(userId)
                                .ConfigureAwait(false);
                            return Task.CompletedTask;
                        }
                        else
                        {
                            item[Tdatas[d].Name] = value;
                            Debug.WriteLine($"column:{Tdatas[d].Name}");
                        }
                    }
                    //Psmb
                    object psmb = await GetValue(worksheet, last, row, item).ConfigureAwait(false);
                    //Discriminator
                    item.Discriminator = entry.DeclarationType;
                    var dbSet = _context.Set<T>();
                    T find = await dbSet
                        .FirstOrDefaultAsync(t => t.DeclarationNumber == item.DeclarationNumber).ConfigureAwait(false);
                    if (find == null)
                    {
                        await dbSet.AddAsync(item).ConfigureAwait(false);
                        await _context.SaveChangesAsync().ConfigureAwait(false);
                        entry.Added++;
                    }
                    else
                    {
                        item.Id = find.Id;
                        if (find != item)
                        {
                            find.AddChanges(item);
                            dbSet.Update(find);
                            await _context.SaveChangesAsync().ConfigureAwait(false);
                            entry.Updated++;
                        }
                    }
                    if (item.Discriminator == DeclarationType.Production)
                    {
                        var rawMaterial = item[nameof(ProductionDeclaration.RawOrProd)].ToString()
                            .ToUpperInvariant().Contains("M", StringComparison.Ordinal);
                        var date = await _context.DeclarationDates
                            .FirstOrDefaultAsync(t => t.SernapescaDeclarationId == item.Id
                            && t.Date == item.Date
                            && t.RawMaterial == rawMaterial
                            && t.ProductionType == (ProductionType)item[nameof(ProductionDeclaration.ProductionType)]
                            && t.ItemType == (Item)item[nameof(ProductionDeclaration.ItemType)]).ConfigureAwait(false);
                        if(date == null)
                        {
                            date = new DeclarationDate
                            {
                                SernapescaDeclarationId = item.Id,
                                Date = item.Date,
                                Weight = item.Weight,
                                RawMaterial = item[nameof(ProductionDeclaration.RawOrProd)].ToString()
                                .ToUpperInvariant().Contains("M", StringComparison.Ordinal),
                                ProductionType = (ProductionType)item[nameof(ProductionDeclaration.ProductionType)],
                                ItemType = (Item)item[nameof(ProductionDeclaration.ItemType)]
                            };
                            await _context.DeclarationDates.AddAsync(date).ConfigureAwait(false);
                        }
                        else
                        {
                            date.Weight += item.Weight;
                            _context.DeclarationDates.Update(date);
                        }
                    }
                    else
                    {
                        var date = await _context.DeclarationDates
                            .FirstOrDefaultAsync(d => d.SernapescaDeclarationId == item.Id && d.Date == item.Date)
                            .ConfigureAwait(false);
                        if(date == null)
                        {
                            date = new DeclarationDate
                            {
                                SernapescaDeclarationId = item.Id,
                                Date = item.Date,
                                Weight = item.Weight
                            };
                            await _context.DeclarationDates.AddAsync(date).ConfigureAwait(false);
                        }
                        else
                        {
                            if (!datesIds.Contains(date.Id))
                            {
                                date.Weight = item.Weight;
                            }
                            else
                            {
                                date.Weight += item.Weight;
                            }
                            _context.DeclarationDates.Update(date);
                        }
                        datesIds.Add(date.Id);
                    }
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                    if (entry.Min > item.Date) entry.Min = item.Date;
                    if (entry.Max < item.Date) entry.Max = item.Date;
                    pgr += pgrRow;
                    await _hubContext.SendProgress(userId, pgr)
                        .ConfigureAwait(false);
                    await _hubContext.SendStatusInfo(userId)
                        .ConfigureAwait(false);
                    Debug.WriteLine($"row:{row} sheet:{worksheet.Name}");
                }
            }
            msg = $">{entry.Added} añadidos" + (entry.Updated != 0 ? $"y {entry.Updated} registros actualizados " : " ") + "exitosamente.";
            entry.OutPut += msg;
            entry.Success = true;
            await _hubContext.SendLog(userId, msg).ConfigureAwait(false);
            _context.SernapescaEntries.Update(entry);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            await _hubContext.SendProgress(userId, 100).ConfigureAwait(false);
            await _hubContext.SendStatusSuccess(userId).ConfigureAwait(false);

            return Task.CompletedTask;
        }
        private async Task<Task> Init<T>()
        {
            var type = typeof(T);
            Declaration = type.Name;
            InputFile = await _context.InputFiles
                .FirstOrDefaultAsync(e => e.ClassName == Declaration).ConfigureAwait(false);
            if (InputFile == null)
                throw new MissingFieldException(_localizer?[$"ExcelFile {Declaration} not present in DataBase"]);
            FieldInfos.AddRangeOverride(type.GetProperties(BindingFlags)
                .Where(f =>
                f.GetCustomAttribute<ParseSkipAttribute>() == null));
            var registries = _context.Registries
                .Include(r => r.Headers)
                .Where(c => c.InputFileId == InputFile.Id);
            Tdatas = FieldInfos.Select(async dt =>
            {
                var r = await registries
                .FirstOrDefaultAsync(c => c.NormalizedAttribute == dt.Name.ToUpperInvariant())
                .ConfigureAwait(false);
                if (r == null)
                    throw new EvaluationException(dt.Name);
                if (string.IsNullOrWhiteSpace(r.Description))
                    return null;
                var t = dt.PropertyType;
                if (t.IsClass)
                {
                    InSet[t.Name] = new Dictionary<string, int>();
                }
                if (t.IsGenericType)
                {
                    var def = t.GetGenericTypeDefinition();
                    if (def == typeof(Nullable<>))
                    {
                        t = Nullable.GetUnderlyingType(t);
                    }
                    else if (def == typeof(ICollection<>))
                    {
                        t = t.GetGenericArguments().Single();
                    }
                }
                var data = new Tdata
                {
                    FieldName = t.Name,
                    Name = dt.Name,
                    Operation = r.Operation,
                    DecimalPlaces = r.DecimalPlaces,
                    DecimalSeparator = r.DecimalSeparator,
                    DeleteAfter2ndNegative = r.DeleteAfter2ndNegative
                };
                data.Q.AddRangeOverride(r.Headers.Select(h => h.NormalizedName));
                return data;
            }).Select(t => t.Result).Where(t => t != null).ToList();
            FirstOrDefaultAsyncMethod = typeof(EntityFrameworkQueryableExtensions).GetMethods(BindingFlags.Static | BindingFlags.Public)
                .FirstOrDefault(m => m.Name == nameof(EntityFrameworkQueryableExtensions.FirstOrDefaultAsync) && m.GetParameters().Length == 3);
            if (FirstOrDefaultAsyncMethod == null)
            {
                throw new Exception(_localizer[$"Cannot find \"System.Linq.FirstOrDefault\" method."]);
            }
            if (type == typeof(PlanktonAssay))
            {
                PhytoStart = await _context.Registries
                    .Include(r => r.Headers)
                    .FirstOrDefaultAsync(r => r.NormalizedAttribute == nameof(PhytoStart).ToUpperInvariant())
                    .ConfigureAwait(false);
                PhytoEnd = await _context.Registries
                    .Include(r => r.Headers)
                    .FirstOrDefaultAsync(r => r.NormalizedAttribute == nameof(PhytoEnd).ToUpperInvariant())
                    .ConfigureAwait(false);
                if (PhytoStart == null || PhytoEnd == null)
                    throw new MissingFieldException(_localizer?[$"Columna Inicio or Fin not defined in DataBase"]);
                InSet[nameof(Email)] = new Dictionary<string, int>();
                InSet[nameof(SpeciesPhytoplankton)] = new Dictionary<string, int>();
                InSet[nameof(GenusPhytoplankton)] = new Dictionary<string, int>();
                InSet[nameof(PhylogeneticGroup)] = new Dictionary<string, int>();
                InSet[nameof(Psmb)] = new Dictionary<string, int>();
                var regsp = await _context.Registries.FirstOrDefaultAsync(r => r.Attribute == nameof(Phytoplankton)).ConfigureAwait(false);
                PhytoTData = new Tdata
                {
                    Operation = regsp.Operation,
                    DecimalPlaces = regsp.DecimalPlaces,
                    DecimalSeparator = regsp.DecimalSeparator,
                    DeleteAfter2ndNegative = regsp.DeleteAfter2ndNegative
                };
            }
            //else
            //{
            //    Tdatas.MoveLast(d => d.Name == nameof(SernapescaDeclaration.OriginPsmb));
            //}
            return Task.CompletedTask;
        }
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        private async Task<Task> AddEntryAsync(string file, string pwd)
        {
            var dest = "ERROR";
            var dir = Path.GetFileName(Path.GetDirectoryName(file));
            var logFile = Path.Combine(pwd, "LOGS", $"{dir}_{Path.GetFileName(file)}.log");
            using var log = new StreamWriter(logFile);
            try
            {
                var matrix = await _tableToExcel.HtmlTable2Matrix(file).ConfigureAwait(false);
                var result = await AddEntryAsync(matrix).ConfigureAwait(false);
                dest = "OK";
            }
            catch (DuplicateNameException de)
            {
                log.WriteLine($"File: {file}");
                log.WriteLine($"DNE: {de}");
                dest = "OK";
            }
            catch (Exception e)
            {
                log.WriteLine($"File: {file}");
                log.WriteLine($"E: {e}");
            }
            try
            {
                var newf = $"{pwd}/{dest}/{dir}";
                if (!Directory.Exists(newf))
                {
                    Directory.CreateDirectory(newf);
                }
                newf += $"/{Path.GetFileName(file)}";
                if (File.Exists(newf))
                    File.Delete(newf);
                File.Move(file, newf);
                log.Close();
            }
            catch (IOException e)
            {
                log.WriteLine($"IOE: {e}");
                log.Close();
            }
            return Task.CompletedTask;
        }
        private async Task<Psmb> ParsePsmb(PlanktonAssay item)
        {
            Farm newFarm = new Farm();
            if(item.Name != null)
            item.Name = Regex.Replace(item.Name, @"[^A-Z0-9 ]", "");
            if(item.Acronym != null)
            item.Acronym = Regex.Replace(item.Acronym, @"[^A-Z]", "");
            if (item.FarmCode.HasValue)
            {
                var farmcode = item.FarmCode.Value.ToString(CultureInfo.InvariantCulture);
                if (InSet[nameof(Psmb)].ContainsKey(farmcode))
                {
                    item.PsmbId = InSet[nameof(Psmb)][farmcode];
                    return null;
                }
                Farm tmp = await _context.Farms
                    .FirstOrDefaultAsync(f => f.Code == item.FarmCode.Value).ConfigureAwait(false);
                if (tmp != null)
                {
                    bool update = false;
                    if (!string.IsNullOrWhiteSpace(item.Name) && item.Name != tmp.Name)
                    {
                        tmp.SetName(item.Name);
                        update = true;
                    }
                    if (!tmp.PsmbAreaId.HasValue)
                    {
                        var areaId = await ParseAreaCodeOnly(item).ConfigureAwait(false);
                        if (areaId.HasValue)
                        {
                            tmp.PsmbAreaId = areaId.Value;
                            update = true;
                        }
                    }
                    if (update)
                    {
                        _context.Farms.Update(tmp);
                        await _context.SaveChangesAsync().ConfigureAwait(false);
                    }
                    InSet[nameof(Psmb)].Add(farmcode, tmp.Id);
                    item.PsmbId = tmp.Id;
                    return null;
                }
                newFarm.Code = item.FarmCode.Value;
            }
            else if (!string.IsNullOrWhiteSpace(item.Acronym))
            {
                if (InSet[nameof(Psmb)].ContainsKey(item.Acronym))
                {
                    item.PsmbId = InSet[nameof(Psmb)][item.Acronym];
                    return null;
                }
                var tmp = await _context.Farms
                    .FirstOrDefaultAsync(f => f.Acronym == item.Acronym).ConfigureAwait(false);
                if (tmp != null)
                {
                    bool update = false;
                    if (!string.IsNullOrWhiteSpace(item.Name) && tmp.Name != item.Name)
                    {
                        tmp.SetName(item.Name);
                        update = true;
                    }
                    if (!tmp.PsmbAreaId.HasValue)
                    {
                        var areaId = await ParseAreaCodeOnly(item).ConfigureAwait(false);
                        if (areaId.HasValue)
                        {
                            tmp.PsmbAreaId = areaId.Value;
                            update = true;
                        }
                    }
                    if (update)
                    {
                        _context.Farms.Update(tmp);
                        await _context.SaveChangesAsync().ConfigureAwait(false);
                    }
                    InSet[nameof(Psmb)].Add(item.Acronym, tmp.Id);
                    item.PsmbId = tmp.Id;
                    return null;
                }
                var tma = await _context.PsmbAreas
                    .FirstOrDefaultAsync(f => f.Acronym == item.Acronym).ConfigureAwait(false);
                if(tma != null)
                {
                    if (!string.IsNullOrWhiteSpace(item.Name) && tma.Name != item.Name)
                    {
                        tma.SetName(item.Name);
                        _context.PsmbAreas.Update(tma);
                        await _context.SaveChangesAsync().ConfigureAwait(false);
                    }
                    InSet[nameof(Psmb)].Add(item.Acronym, tma.Id);
                    item.PsmbId = tma.Id;
                    return null;
                }
                newFarm.SetAcronym(item.Acronym);
            }
            if (item.AreaCode.HasValue && item.FarmCode.HasValue && item.AreaCode.Value == item.FarmCode.Value)
            {
                var areacode = item.AreaCode.Value.ToString(CultureInfo.InvariantCulture);
                if (InSet[nameof(Psmb)].ContainsKey(areacode))
                {
                    item.PsmbId = InSet[nameof(Psmb)][areacode];
                    return null;
                }
                var tmp = await _context.PsmbAreas
                    .FirstOrDefaultAsync(f => f.Code == item.AreaCode.Value).ConfigureAwait(false);
                if (tmp != null)
                {
                    bool update = false;
                    if (!string.IsNullOrWhiteSpace(item.Name) && tmp.Name != item.Name)
                    {
                        tmp.SetName(item.Name);
                        update = true;
                    }
                    if (!string.IsNullOrWhiteSpace(item.Acronym) && tmp.Acronym != item.Acronym)
                    {
                        tmp.SetAcronym(item.Acronym);
                        update = true;
                    }
                    if (update)
                    {
                        _context.PsmbAreas.Update(tmp);
                        await _context.SaveChangesAsync().ConfigureAwait(false);
                    }
                    InSet[nameof(Psmb)].Add(areacode, tmp.Id);
                    item.PsmbId = tmp.Id;
                    return null;
                }
            }
            if (!string.IsNullOrWhiteSpace(item.Name))
            {
                if (InSet[nameof(Psmb)].ContainsKey(item.Name))
                {
                    item.PsmbId = InSet[nameof(Psmb)][item.Name];
                    return null;
                }
                var psmbs = await _context.Psmbs
                .Where(f => f.NormalizedName == item.Name && (f.Discriminator == PsmbType.PsmbArea || f.Discriminator == PsmbType.Farm)).ToListAsync().ConfigureAwait(false);
                var tmps = new List<Psmb>();
                foreach (var tmp in psmbs)
                    if (await _context.PlanktonAssays.AnyAsync(p => p.SamplingEntityId == item.SamplingEntityId
                         && p.PsmbId == tmp.Id).ConfigureAwait(false))
                        tmps.Add(tmp);
                if (tmps.Count == 1)
                {
                    var tmp = tmps[0];
                    bool update = false;
                    if (!string.IsNullOrWhiteSpace(item.Acronym) && tmp.Acronym != item.Acronym)
                    {
                        tmp.SetAcronym(item.Acronym);
                        update = true;
                    }
                    if (tmp.Discriminator == PsmbType.Farm)
                    {
                        var farm = (Farm)tmp;
                        if (!farm.PsmbAreaId.HasValue)
                        {
                            var areaId = await ParseAreaCodeOnly(item).ConfigureAwait(false);
                            if (areaId.HasValue)
                            {
                                farm.PsmbAreaId = areaId.Value;
                                _context.Farms.Update(farm);
                                await _context.SaveChangesAsync().ConfigureAwait(false);
                            }
                        }
                    }
                    if (update)
                    {
                        _context.Psmbs.Update(tmp);
                        await _context.SaveChangesAsync().ConfigureAwait(false);
                    }
                    InSet[nameof(Psmb)].Add(item.Name, tmp.Id);
                    item.PsmbId = tmp.Id;
                    return null;
                }
                newFarm.SetName(item.Name);
            }
            if (item.AreaCode.HasValue)
            {
                var areacode = item.AreaCode.Value.ToString(CultureInfo.InvariantCulture);
                if (InSet[nameof(Psmb)].ContainsKey(areacode))
                {
                    item.PsmbId = InSet[nameof(Psmb)][areacode];
                    return null;
                }
                var tmp = await _context.Farms
                    .FirstOrDefaultAsync(f => f.Code == item.AreaCode.Value).ConfigureAwait(false);
                if (tmp != null)
                {
                    bool update = false;
                    if (!string.IsNullOrWhiteSpace(item.Name) && tmp.Name != item.Name)
                    {
                        tmp.SetName(item.Name);
                        update = true;
                    }
                    if (!string.IsNullOrWhiteSpace(item.Acronym) && tmp.Acronym != item.Acronym)
                    {
                        tmp.SetAcronym(item.Acronym);
                        update = true;
                    }
                    if (update)
                    {
                        _context.Farms.Update(tmp);
                        await _context.SaveChangesAsync().ConfigureAwait(false);
                    }
                    InSet[nameof(Psmb)].Add(areacode, tmp.Id);
                    item.PsmbId = tmp.Id;
                    return null;
                }
            }
            if (item.AreaCode.HasValue || item.FarmCode.HasValue)
            {
                var pareaId = await ParseArea(item).ConfigureAwait(false);
                if (pareaId.HasValue)
                {
                    item.PsmbId = pareaId.Value;
                    return null;
                }
                newFarm.Code = item.AreaCode.Value;
            }
            if (newFarm.Code != 0)
            {
                var areaId = await ParseAreaCodeOnly(item).ConfigureAwait(false);
                if (areaId.HasValue)
                {
                    newFarm.PsmbAreaId = areaId.Value;
                }
                return newFarm;
            }
            return null;
        }
        private async Task<int?> ParseAreaCodeOnly(PlanktonAssay item)
        {
            if (item.AreaCode.HasValue)
            {
                var tmp = await _context.PsmbAreas
                    .FirstOrDefaultAsync(p => p.Code == item.AreaCode.Value).ConfigureAwait(false);
                if (tmp != null)
                {
                    return tmp.Id;
                }
            }
            return null;
        }
        private async Task<int?> ParseArea(PlanktonAssay item)
        {
            if (item.AreaCode.HasValue)
            {
                var tmp = await _context.PsmbAreas
                    .FirstOrDefaultAsync(p => p.Code == item.AreaCode.Value).ConfigureAwait(false);
                if (tmp != null)
                {
                    bool update = false;
                    if (!string.IsNullOrWhiteSpace(item.Name) && tmp.Name != item.Name)
                    {
                        tmp.SetName(item.Name);
                        update = true;
                    }
                    if (!string.IsNullOrWhiteSpace(item.Acronym) && tmp.Acronym != item.Acronym)
                    {
                        tmp.SetAcronym(item.Acronym);
                        update = true;
                    }
                    if (update)
                    {
                        _context.PsmbAreas.Update(tmp);
                        await _context.SaveChangesAsync().ConfigureAwait(false);
                    }
                    return tmp.Id;
                }
            }
            if (item.FarmCode.HasValue)
            {
                var tmp = await _context.PsmbAreas
                    .FirstOrDefaultAsync(p => p.Code == item.FarmCode.Value).ConfigureAwait(false);
                if (tmp != null)
                {
                    bool update = false;
                    if (!string.IsNullOrWhiteSpace(item.Name) && tmp.Name != item.Name)
                    {
                        tmp.SetAcronym(item.Name);
                        update = true;
                    }
                    if (!string.IsNullOrWhiteSpace(item.Acronym) && tmp.Acronym != item.Acronym)
                    {
                        tmp.SetAcronym(item.Acronym);
                        update = true;
                    }
                    if (update)
                    {
                        _context.PsmbAreas.Update(tmp);
                        await _context.SaveChangesAsync().ConfigureAwait(false);
                    }
                    return tmp.Id;
                }
            }
            return null;
        }
        private async Task<Task> AddEntryAsync(Dictionary<(int, int), string> matrix)
        {
            if (matrix == null) throw new ArgumentNullException($"matrix null error: {matrix}");
            if(!(matrix.ContainsKey((1, StartRow))
                && PhytoStart.Headers.Any(h => h.NormalizedName == matrix.GetValue(1, StartRow))))
                StartRow = matrix.SearchHeaders(PhytoStart.Headers.Select(h => h.NormalizedName).ToList()).Item2;
            int end = matrix.SearchHeaders(PhytoEnd.Headers.Select(h => h.NormalizedName).ToList()).Item2;
            if (end == 0 || StartRow == 0) 
                throw new InputFormatterException($"start {PhytoStart.Description} or end {PhytoEnd.Description} not found");
            PlanktonAssay item = Activator.CreateInstance<PlanktonAssay>();
            //GET Id
            var d = 0;
            var id = await GetValue(matrix, d).ConfigureAwait(false);
            if(id == null)
                throw new FormatException(_localizer[$"Archivo presenta errores no se encontró Id {string.Join("; ", Tdatas[d].Q)}"]);
            item.Id = (int)id;
            d++;
            //GET Sampling Date
            var samplingDate = await GetValue(matrix, d).ConfigureAwait(false);
            if (samplingDate == null)
                throw new FormatException(_localizer[$"Archivo presenta errores no se encontró Fecha de Muestreo en {id} {string.Join("; ", Tdatas[d].Q)}"]);
            item.SamplingDate = (DateTime)samplingDate;
            //get other values
            for (d++; d < Tdatas.Count; d++)
            {
                var value = await GetValue(matrix, d, item).ConfigureAwait(false);
                if (value != null) item[Tdatas[d].Name] = value;
            }
            //check db for centre || psmb
            item.Psmb = await ParsePsmb(item).ConfigureAwait(false);
            if (item.PsmbId == 0 && item.Psmb == null)
                throw new Exception($"No se pudo encontrar un Psmb o Centro válido para la declaración {item.Id} con fecha {item.SamplingDate}");
            //Get Phytos
            int groupId = 0;
            var speciesInFile = new HashSet<string>();
            var fitos = new Dictionary<string, Phytoplankton>();
            for (int row = StartRow + 1; row < end; row++)
            {
                string val = matrix.GetValue(1, row);
                if (val == null || val.Contains("TOTAL", StringComparison.Ordinal)) continue;
                var fullName = val.CleanScientificName();
                if (string.IsNullOrWhiteSpace(fullName)) continue;
                List<string> genusSp = fullName.SplitSpaces().ToList();
                double? ce = matrix.GetValue(3, row)
                    .ParseDouble(
                    PhytoTData.DecimalPlaces, PhytoTData.DecimalSeparator, 
                    PhytoTData.DeleteAfter2ndNegative, PhytoTData.Operation);
                if (ce.HasValue)
                {
                    if (ce == 0) continue;
                    Ear? e = (Ear?)matrix.GetValue(2, row).ParseInt();
                    SpeciesPhytoplankton sp = new SpeciesPhytoplankton();
                    if (!InSet[nameof(SpeciesPhytoplankton)].ContainsKey(fullName))
                    {
                        if (!InSet[nameof(GenusPhytoplankton)].ContainsKey(genusSp[0]))
                        {
                            var genus = await _context.GenusPhytoplanktons
                                .FirstOrDefaultAsync(s => s.NormalizedName == genusSp[0]).ConfigureAwait(false);
                            if (genus == null)
                            {
                                genus = new GenusPhytoplankton
                                {
                                    GroupId = groupId
                                };
                                genus.SetName(genusSp[0]);
                                await _context.GenusPhytoplanktons.AddAsync(genus).ConfigureAwait(false);
                                await _context.SaveChangesAsync().ConfigureAwait(false);
                            }
                            InSet[nameof(GenusPhytoplankton)].Add(genus.NormalizedName, genus.Id);
                        }
                        int genusId = InSet[nameof(GenusPhytoplankton)][genusSp[0]];
                        if (genusSp.Count == 1) genusSp.Add("SP");
                        SpeciesPhytoplankton specie = await _context.SpeciesPhytoplanktons
                            .FirstOrDefaultAsync(s => 
                            s.Genus.NormalizedName == genusSp[0] 
                            && s.NormalizedName == genusSp[1]).ConfigureAwait(false);
                        if (specie == null)
                        {
                            specie = new SpeciesPhytoplankton
                            {
                                GenusId = genusId
                            };
                            specie.SetName(genusSp[1]);
                            sp = specie;
                        }
                        else
                        {
                            InSet[nameof(SpeciesPhytoplankton)].Add(fullName, specie.Id);
                            sp = specie;
                        }
                    }
                    else
                    {
                        sp.Id = InSet[nameof(SpeciesPhytoplankton)][fullName];
                    }
                    if (speciesInFile.Contains(fullName) && fitos.ContainsKey(fullName))
                            fitos[fullName].AddToPhyto(ce.Value, e);
                    else if(sp.Id != 0)
                    {
                        Phytoplankton fito = await _context.Phytoplanktons
                                .FirstOrDefaultAsync(f => f.PlanktonAssayId == item.Id && f.SpeciesId == sp.Id)
                                .ConfigureAwait(false);
                        if (fito == null)
                        {
                            fitos.Add(fullName, new Phytoplankton
                            {
                                SpeciesId = sp.Id,
                                C = ce.Value,
                                PlanktonAssayId = item.Id,
                                EAR = e
                            });
                        }
                        else
                        {
                            fito.C = ce.Value;
                            fito.EAR = e;
                            fitos.Add(fullName, fito);
                        }
                    }
                    else
                    {
                        fitos.Add(fullName, new Phytoplankton
                        {
                            Species = sp,
                            C = ce.Value,
                            PlanktonAssayId = item.Id,
                            EAR = e
                        });
                    }
                    speciesInFile.Add(fullName);
                }
                else
                {
                    string grp = genusSp[0];
                    if (!InSet[nameof(PhylogeneticGroup)].ContainsKey(grp))
                    {
                        var group = await _context.PhylogeneticGroups
                        .FirstOrDefaultAsync(g => g.NormalizedName == grp).ConfigureAwait(false);
                        if (group == null)
                        {
                            group = new PhylogeneticGroup();
                            group.SetName(grp);
                            await _context.PhylogeneticGroups.AddAsync(group).ConfigureAwait(false);
                            await _context.SaveChangesAsync().ConfigureAwait(false);
                        }
                        InSet[nameof(PhylogeneticGroup)].Add(grp, group.Id);
                    }
                    groupId = InSet[nameof(PhylogeneticGroup)][grp];
                }
            }
            PlanktonAssay ensayoFito = await _context.PlanktonAssays
                .Include(p => p.Phytoplanktons)
                .Include(p => p.Emails)
                    .ThenInclude(p => p.Email)
                .Include(p => p.Station)
                .FirstOrDefaultAsync(e => e.Id == item.Id)
                .ConfigureAwait(false);
            List<bool> toUpdate = new List<bool>
            {
                item.Analist != null,
                item.Emails.Any(e => e.Email != null),
                item.Laboratory != null,
                item.Phone != null,
                item.Phytoplanktons.Any(p => p.Species != null),
                item.Psmb != null,
                item.SamplingEntity != null,
                item.Station != null
            };
            if (ensayoFito == null)
            {
                if (fitos.Any())
                    item.Phytoplanktons.AddRangeOverride(fitos.Values);
                await _context.PlanktonAssays.AddAsync(item).ConfigureAwait(false);
                await _context.SaveChangesAsync().ConfigureAwait(false);
                UpdateInSet(item, toUpdate);
            }
            else if (ensayoFito != item)
            {
                item.Id = ensayoFito.Id;
                ////get ids from entities
                ensayoFito.AddChanges(item);
                if (fitos.Any())
                    ensayoFito.Phytoplanktons.AddRangeOverride(fitos.Values);
                _context.PlanktonAssays.Update(ensayoFito);
                await _context.SaveChangesAsync().ConfigureAwait(false);
                UpdateInSet(ensayoFito, toUpdate);
            }
            return Task.CompletedTask;
        }
        private void UpdateInSet(PlanktonAssay item, List<bool> toUpdate)
        {
            if (toUpdate[0])
                AddToInset<Analist>(item, nameof(Analist.NormalizedName));
            if (toUpdate[1])
                InSet[nameof(Email)].AddRangeNewOnly(item.Emails.Select(s => new { s.Email.Address, s.EmailId })
                    .ToDictionary(v => v.Address, v => v.EmailId));
            if (toUpdate[2])
                AddToInset<Laboratory>(item, nameof(Laboratory.NormalizedName));
            if (toUpdate[3])
                AddToInset<Phone>(item, nameof(Phone.Number));
            if (toUpdate[4])
                InSet[nameof(SpeciesPhytoplankton)].AddRangeNewOnly(item.Phytoplanktons.Select(s => new { s.Species.NormalizedName, s.SpeciesId })
                    .ToDictionary(v => v.NormalizedName, v => v.SpeciesId));
            if (toUpdate[5])
                AddToInset<Psmb>(item, nameof(Psmb.NormalizedName));
            if (toUpdate[6])
                AddToInset<SamplingEntity>(item, nameof(SamplingEntity.NormalizedName));
            if (toUpdate[7])
                AddToInset<Station>(item, nameof(Station.NormalizedName));
        }
        private void AddToInset<T>(PlanktonAssay item, string key) where T : IHasBasicIndexer
        {
            var name = typeof(T).Name;
            var id = (int?)item[$"{name}Id"];
            T entity = (T)item[name];
            if (!InSet[name].ContainsKey(GetKey(entity, key)) && id.HasValue)
                InSet[name].Add(GetKey(entity, key), id.Value);
        }
        private static string GetKey<T>(T item, string key) where T : IHasBasicIndexer => item[key] as string;
        private async Task<TEntity> FindParsedAsync<TEntity>(Indexed item, string attribute, string normalized) where TEntity : class, IHasBasicIndexer
        {
            //return null if arguments null
            if (string.IsNullOrWhiteSpace(normalized)) return null;
            //if inset saved Id only and return null
            var entityType = typeof(TEntity);
            var name = entityType.Name;
            if (InSet[name].ContainsKey(normalized))
            {
                item[$"{name}Id"] = InSet[name][normalized];
                return null;
            }
            //get dbset
            var dbSet = _context.Set<TEntity>();
            if (dbSet == null)
            {
                throw new Exception($"{entityType} DB Contexts doesn't contains collection for this type.");
            }
            //get method
            var firstOrDefaultAsyncMethod = FirstOrDefaultAsyncMethod.MakeGenericMethod(entityType);
            //build expression
            ParameterExpression parameter = System.Linq.Expressions.Expression.Parameter(entityType, "x");
            MemberExpression property = System.Linq.Expressions.Expression.Property(parameter, attribute);
            ConstantExpression rightSide = System.Linq.Expressions.Expression.Constant(normalized);
            BinaryExpression operation = System.Linq.Expressions.Expression.Equal(property, rightSide);
            Type delegateType = typeof(Func<,>).MakeGenericType(entityType, typeof(bool));
            LambdaExpression predicate = System.Linq.Expressions.Expression.Lambda(delegateType, operation, parameter);

            TEntity element = (TEntity)await firstOrDefaultAsyncMethod.InvokeAsync(null, new object[] { dbSet, predicate, default }).ConfigureAwait(false);

            if (element == null)
            {
                element = Activator.CreateInstance<TEntity>();
                element[attribute] = normalized;
                return element;
            }
            else
            {
                var id = (int)element["Id"];
                InSet[name].Add(normalized, id);
                item[$"{name}Id"] = id;
                return null;
            }
        }
        private async Task<Station> ParseEstacion(string text, Indexed item)
        {
            if (text == null) return null;
            text = Regex.Replace(text, @"[^A-Z0-9 ]|ESTA?C?I?O?N? *", "");
            text = Regex.Replace(text, @"\s+(\b[\w']{1,4}\b)", "$1");
            text = Regex.Replace(text, @"\bE([\w']{1,4}\b)", "$1");
            text = Regex.Replace(text, @"\s{,2}", " ").Trim();
            return await FindParsedAsync<Station>(item, nameof(Station.NormalizedName), text).ConfigureAwait(false);
        }
        private async Task<Origin> ParseOrigin(string text, Indexed item)
        {
            if (text == null) return null;
            int id = (int)item[nameof(SeedDeclaration.OriginId)];
            string name = nameof(Origin);
            if (InSet[name].ContainsKey(text))
            {
                item[$"{name}Id"] = InSet[name][text];
                return null;
            }
            Origin origin = await _context.Origins
                .FindAsync(id).ConfigureAwait(false);
            if(origin == null)
            {
                return new Origin
                {
                    Id = id,
                    Name = text
                };
            }
            return null;
        }
        private async Task<SamplingEntity> ParseEntidadMuestreadora(string text, Indexed item)
        {
            if (text == null) return null;
            text = Regex.Replace(text, @"[^A-Z\s]", "");
            return await FindParsedAsync<SamplingEntity>(item, nameof(SamplingEntity.NormalizedName), text).ConfigureAwait(false);
        }
        private async Task<Laboratory> ParseLaboratorio(string text, Indexed item)
        {
            if (text == null) return null;
            text = Regex.Replace(text, @"[^A-Z\s]", "");
            return await FindParsedAsync<Laboratory>(item, nameof(Laboratory.NormalizedName), text).ConfigureAwait(false);
        }
        private async Task<Analist> ParseAnalista(string text, Indexed item)
        {
            if (text == null) return null;
            text = Regex.Replace(text, @"[^A-Z\s]|\b[\w']{1,3}\b", "");
            var names = text.SplitSpaces();
            if (!names.Any() || names.Length == 1) 
                return null;
            if (names.Length == 3)
            {
                text = $"{names[0]} {names.Last()}";
            }
            else if (names.Length > 3)
            {
                text = $"{names[0]} {names.TakeLast(2).First()}";
            }
            return await FindParsedAsync<Analist>(item, nameof(Analist.NormalizedName), text)
                .ConfigureAwait(false);
        }
        private async Task<Psmb> ParseOriginPsmb(string text, SernapescaDeclaration item)
        {
            var id = text.ParseInt();
            if (id.HasValue)
            {
                var psmb = await _context.Psmbs.FirstOrDefaultAsync(p => p.Code == id)
                    .ConfigureAwait(false);
                if(psmb != null)
                {
                    item.OriginPsmbId = psmb.Id;
                    return null;
                }
                var newpsmb = new Craft
                {
                    Code = id.Value
                };
                if ((int)item.Discriminator < 2)
                {
                    psmb = await _context.Psmbs.FirstOrDefaultAsync(p => p.NormalizedName == text)
                        .ConfigureAwait(false);
                    if(psmb != null)
                    {
                        item.OriginPsmbId = psmb.Id;
                        return null;
                    }
                    newpsmb.SetName(text);
                }
                newpsmb.WaterBody = WaterBody.Ocean;
                if (item.CommuneName == "METROPOLITANA")
                {
                    newpsmb.CommuneId = 113;
                    await _context.Psmbs.AddAsync(newpsmb).ConfigureAwait(false);
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                    item.OriginPsmbId = newpsmb.Id;
                }
                else
                {
                    var comuna = _context.Communes.FirstOrDefault(c =>
                    c.NormalizedName == item.CommuneName);
                    if (comuna != null)
                    {
                        newpsmb.CommuneId = comuna.Id;
                        await _context.Psmbs.AddAsync(newpsmb).ConfigureAwait(false);
                        await _context.SaveChangesAsync().ConfigureAwait(false);
                        item.OriginPsmbId = newpsmb.Id;
                    }
                }
            }
            return null;
        }
        private async Task<Phone> ParseTelefono(string text, Indexed item)
        {
            if (text == null) return null;
            text = Regex.Replace(text, @"[^0-9\-\/\(\)\s]", "");
            text = Regex.Replace(text, @"\(\)", "");
            return await FindParsedAsync<Phone>(item, nameof(Phone.Number), text).ConfigureAwait(false);
        }
        private async Task<List<PlanktonAssayEmail>> ParseEmails(string text, Indexed item)
        {
            var results = new List<PlanktonAssayEmail>();
            var i = item[nameof(PlanktonAssay.Id)];
            var id = i as int?;
            if (text == null || !id.HasValue) return null;
            var normalizedList = Regex.Replace(text, @"[^A-Z0-9_\-\.\@;]", "")
                .Split(";");
            foreach(var email in normalizedList)
            {
                if (!Regex.IsMatch(email, @"^([A-Z0-9_\-\.]+)@([A-Z0-9_\-\.]+)\.([A-Z]{2,5})$")) continue;
                var emailensayo = new PlanktonAssayEmail
                {
                    PlanktonAssayId = id.Value
                };
                emailensayo.Email = await FindParsedAsync<Email>(emailensayo, nameof(Email.Address), email).ConfigureAwait(false);
                results.Add(emailensayo);
            }
            return results;
        }
        private async Task<object> GetValue(ExcelWorksheet worksheet, int d, int row, Indexed item = null)
        {
            var data = Tdatas[d];
            if (worksheet == null || !data.Q.Any()) return null;
            if (data.LastColumn == -1 || Headers.Count <= data.LastColumn + 1 || !data.Q.Contains(Headers[data.LastColumn]))
            {
                data.LastColumn = Headers.GetColumnByNames(data.Q);
            }
            ExcelRange vl = worksheet.Cells[row, data.LastColumn + 1];
            if (vl != null && vl.Value != null)
            {
                string value = vl.Value.ToString().CleanCell();
                return await GetValue(value, data, item)
                    .ConfigureAwait(false);
            }
            return null;
        }
        private async Task<object> GetValue(Dictionary<(int, int), string> matrix, int d, PlanktonAssay item = null)
        {
            var data = Tdatas[d];
            if (matrix == null || !data.Q.Any()) return null;
            if (data.LastPosition != (0, 0) && matrix.ContainsKey(data.LastPosition))
            {
                var lpHeader = matrix[data.LastPosition];
                if(!data.Q.Any(r => r == lpHeader))
                {
                    data.LastPosition = matrix.SearchHeaders(data.Q);
                }
            }
            else
            {
                data.LastPosition = matrix.SearchHeaders(data.Q);
            }
            var valuePos = (data.LastPosition.Item1 + 1, data.LastPosition.Item2);
            if (matrix.ContainsKey(valuePos))
            {
                string value = matrix[valuePos];
                return await GetValue(value, data, item).ConfigureAwait(false);
            }
            return null;
        }
        private async Task<object> GetValue(string val, Tdata data, Indexed item)
        {
            if (data.FieldName == null) return null;
            return data.FieldName switch
            {
                nameof(Int32) => val.ParseInt(data.DeleteAfter2ndNegative, data.Operation),
                nameof(Double) => val.ParseDouble(data.DecimalPlaces, data.DecimalSeparator,
                    data.DeleteAfter2ndNegative, data.Operation),
                nameof(Boolean) => val[0] != 'N' || val[0] != 'F',
                nameof(DateTime) => val.ParseDateTime(),
                nameof(ProductionType) => val.ParseProductionType(),
                nameof(Item) => val.ParseItem(),
                nameof(Enum) => Enum.Parse(DataTableExtensions.GetEnumType(data.FieldName), val),
                nameof(Station) => await ParseEstacion(val, item).ConfigureAwait(false),
                nameof(Laboratory) => await ParseLaboratorio(val, item).ConfigureAwait(false),
                nameof(SamplingEntity) => await ParseEntidadMuestreadora(val, item).ConfigureAwait(false),
                nameof(PlanktonAssayEmail) => await ParseEmails(val, item).ConfigureAwait(false),
                nameof(Analist) => await ParseAnalista(val, item).ConfigureAwait(false),
                nameof(Phone) => await ParseTelefono(val, item).ConfigureAwait(false),
                nameof(Origin) => await ParseOrigin(val, item).ConfigureAwait(false),
                nameof(Psmb) => await ParseOriginPsmb(val, (SernapescaDeclaration)item).ConfigureAwait(false),
                _ => val
            };
        }
    }
    public class Tdata
    {
        public string Name { get; set; }
        public string FieldName { get; set; }
        public List<string> Q { get; } = new List<string>();
        public string Operation { get; set; }
        public int? DecimalPlaces { get; set; }
        public char? DecimalSeparator { get; set; }
        public bool? DeleteAfter2ndNegative { get; set; }
        public (int, int) LastPosition { get; set; } = (0, 0);
        public int LastColumn { get; set; } = -1;
    }
}