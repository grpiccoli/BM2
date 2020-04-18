using BiblioMit.Data;
using BiblioMit.Extensions;
using BiblioMit.Models;
using Microsoft.AspNetCore.SignalR;
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

namespace BiblioMit.Services
{
    public class ImportService : IImport
    {
        private string Declaration { get; set; }
        private Registry PhytoStart { get; set; }
        private Registry PhytoEnd { get; set; }
        private int StartRow { get; set; } = 0;
        private Dictionary<string, Dictionary<string, int>> InSet { get; set; } = new Dictionary<string, Dictionary<string, int>>();
        private MethodInfo FirstOrDefaultAsyncMethod { get; set; }
        private InputFile InputFile { get; set; }
        private List<Tdata> Tdatas { get; set; }
        [SuppressMessage("Performance", "CA1802:Use literals where appropriate", Justification = "includes circular definition")]
        private static readonly BindingFlags BindingFlags = BindingFlags.Instance | BindingFlags.Public;
        private List<PropertyInfo> FieldInfos { get; } = new List<PropertyInfo>();
        //private const string encoding = "Windows-1252";
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<EntryHub> _hubContext;
        private readonly IStringLocalizer<ImportService> _localizer;
        private readonly ITableToExcel _tableToExcel;
        public ImportService(ApplicationDbContext context,
            IHubContext<EntryHub> hubContext,
            ITableToExcel tableToExcel,
            IStringLocalizer<ImportService> localizer)
        {
            _tableToExcel = tableToExcel;
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
                if(!Directory.Exists(pwd))
                    throw new DirectoryNotFoundException(_localizer[$"directory {pwd} not found"]);
                var logs = Path.Combine(pwd, "LOGS");
                if (Directory.Exists(logs))
                {
                    var di = new DirectoryInfo(logs);
                    foreach (var file in di.GetFiles("*log"))
                        file.Delete();
                }
                Directory.CreateDirectory(logs);
                foreach (var file in files)
                {
                    await AddEntryAsync(file, pwd, logs).ConfigureAwait(false);
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
        public async Task<Task> ReadAsync(ExcelPackage package, SernapescaEntry entry, DeclarationType tipo)
        {
            if (entry == null || package == null)
                throw new ArgumentNullException($"Arguments Entry {entry} and package {package} cannot be null");
            double pgr = 0;
            Declaration = entry.DeclarationType.ToString();
            var planillas = new List<SernapescaDeclaration>();

            var msg = string.Empty;

            var resultInit = await Init<SernapescaDeclaration>().ConfigureAwait(false);

            var pgrTotal = 100;

            pgr += 4;

            var pgrReadWrite = (pgrTotal - pgr) / 6;

            var pgrRow = pgrReadWrite / package.Workbook.Worksheets.Where(w => w.Dimension != null).Sum(w => w.Dimension.Rows);

            var status = "info";

            foreach (var worksheet in package.Workbook.Worksheets.Where(w => w.Dimension != null))
            {
                if (worksheet == null) break;
                int rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++)
                {
                    if (worksheet.Cells[row, 1].Value == null)
                    {
                        msg = $">W: Fila '{row}' en hoja '{worksheet.Name}' Está vacía.";

                        entry.OutPut += msg;

                        await _hubContext
                            .Clients.All
                            .SendAsync("Update", "log", msg)
                            .ConfigureAwait(false);

                        status = "warning";
                        await _hubContext
                            .Clients.All
                            .SendAsync("Update", "status", status)
                            .ConfigureAwait(false);

                        continue;
                    }
                    var item = Activator.CreateInstance<SernapescaDeclaration>();

                    item.Row = row;

                    item.Sheet = worksheet.Name;

                    foreach (var d in Tdatas)
                    {
                        MethodInfo method = typeof(ImportService).GetMethod("GetFromExcel")
                            //.MakeGenericMethod(d.FieldType);
                            .MakeGenericMethod(typeof(ImportService));
                        object value = null;
                        value = method.Invoke(value, new object[] { worksheet, d.Q, null });
                        if (value == null)
                        {
                            status = "danger";

                            msg =
                                $">ERROR: Columna '{d.Q}' no encontrada en hoja '{worksheet.Name}'. Verificar archivo.\n0 registros procesados.";

                            entry.OutPut += msg;

                            _context.Update(entry);
                            await _context.SaveChangesAsync()
                                .ConfigureAwait(false);

                            await _hubContext
                            .Clients.All
                            .SendAsync("Update", "log", msg)
                            .ConfigureAwait(false);

                            await _hubContext
                                .Clients.All
                                .SendAsync("Update", "status", status)
                                .ConfigureAwait(false);

                            return Task.CompletedTask;
                        }
                        if (d.Operation != null)
                        {
                            NCalc.Expression e = new NCalc.Expression((value + d.Operation).Replace(",", ".", StringComparison.InvariantCultureIgnoreCase));
                            item[d.Name] = e.Evaluate();
                        }
                        else
                            item[d.Name] = value;
                        Debug.WriteLine($"column:{d.Name}");
                    }

                    //EXTRA STEPS
                    if (item.Date == DateTime.MinValue)
                        item.Date = new DateTime(item.Year, item.Month, 1);

                    var test = (item.Date).ToString("MMyyyy", new CultureInfo("es-CL"));

                    var n = item.DeclarationNumber;

                    item.Discriminator = entry.DeclarationType.ToString();

                    var dt = (int)entry.DeclarationType;

                    var test2 = string.Format(new CultureInfo("es-CL"), "{0}{1}{2}",
                        n, dt, test);

                    item.Id = Convert.ToInt64(test2, new CultureInfo("es-CL"));

                    planillas.Add(item);

                    pgr += pgrRow;

                    await _hubContext
                        .Clients.All
                        .SendAsync("Update", "progress", pgr)
                        .ConfigureAwait(false);
                    await _hubContext
                        .Clients.All
                        .SendAsync("Update", "status", status)
                        .ConfigureAwait(false);

                    Debug.WriteLine($"row:{item.Row} sheet:{item.Sheet}");
                }
            }

            entry.Min = planillas.Min(p => p.Date);
            entry.Max = planillas.Max(p => p.Date);

            var registros = planillas.GroupBy(p => p.Id);

            var pgrP = pgrReadWrite * 5 / registros.Count();

            var datos = new List<SernapescaDeclaration>();
            var origenes = new List<Origin>();
            var centros = new List<Psmb>();
            var updates = new List<SernapescaDeclaration>();

            foreach (var r in registros)
            {
                var dato = r.First();
                dato.Discriminator = string.IsNullOrEmpty(dato.Discriminator) ? dato.Discriminator : entry.DeclarationType.ToString();
                dato.Weight = r.Sum(p => p.Weight);
                dato.Row = r.Sum(p => p.Row);

                var find = await _context.SernapescaDeclarations.FindAsync(dato.Id).ConfigureAwait(false);

                if (find == null)
                {
                    if (dato.Discriminator == "SeedDeclaration" && !origenes.Any(o => o.Id == dato.OriginId))
                    {
                        var orig = await _context.Origins.FindAsync(dato.OriginId).ConfigureAwait(false);

                        if (orig == null)
                        {
                            if (dato.OriginId.HasValue && dato.Origen != null)
                            {
                                var origen = new Origin
                                {
                                    Id = dato.OriginId.Value,
                                    Name = dato.Origen
                                };

                                origenes.Add(origen);
                            }
                            else
                            {
                                msg = $">W: Origen no existe en archivo." +
                                    $">Declaración de {dato.Discriminator} N°{dato.Id}, con fecha {dato.Date}, " +
                                    $"en hoja {dato.Sheet}, filas {dato.Rows} no pudieron ser procesadas.\n" +
                                    $">Verificar archivo.";

                                entry.OutPut += msg;
                                entry.Observations++;

                                await _hubContext
                                .Clients.All
                                .SendAsync("Update", "log", msg)
                                .ConfigureAwait(false);

                                status = "warning";
                                await _hubContext
                                    .Clients.All
                                    .SendAsync("Update", "status", status)
                                    .ConfigureAwait(false);
                                continue;
                            }
                        }
                    }

                    //if (!centros.Any(o => o.Id == dato.ConsessionId))
                    //{
                    //    var parents = _context.Psmbs.Where(p => p.Id == dato.ConsessionId);

                    //    if (parents.Any())
                    //    {
                    //        var comuna = _context.Communes.FirstOrDefault(c =>
                    //        c.Name == dato.CommuneName);
                    //        if (comuna != null)
                    //        {
                    //            var centre = new Craft
                    //            {
                    //                Id = dato.ConsessionId,
                    //                CommuneId = comuna.Id,
                    //                WaterBody = WaterBody.Ocean
                    //            };
                    //            centros.Add(centre);
                    //        }
                    //        else
                    //        {
                    //            msg = $">W: Comuna {dato.CommuneName} no existe en base de datos." +
                    //                $">Declaración de {dato.Discriminator} N°{dato.Id}, con fecha {dato.Date}, " +
                    //                $"en hoja {dato.Sheet}, filas {dato.Rows} no pudieron ser procesadas.\n" +
                    //                $">Verificar archivo.";

                    //            entry.OutPut += msg;
                    //            entry.Observations++;

                    //            await _hubContext
                    //            .Clients.All
                    //            .SendAsync("Update", "log", msg)
                    //            .ConfigureAwait(false);

                    //            status = "warning";
                    //            await _hubContext
                    //                .Clients.All
                    //                .SendAsync("Update", "status", status)
                    //                .ConfigureAwait(false);

                    //            continue;
                    //        }
                    //    }
                    //}

                    datos.Add(dato);
                    entry.Added++;
                    await _hubContext
                        .Clients.All
                        .SendAsync("Update", "agregada", entry.Added)
                        .ConfigureAwait(false);
                }
                else
                {
                    //var updated = find.AddChanges(dato);
                    //if (updated != find)
                    //{
                    //    updates.Add(updated);
                    //    entry.Updated++;
                    //    await _hubContext
                    //        .Clients.All
                    //        .SendAsync("Update", "agregada", entry.Added)
                    //        .ConfigureAwait(false);
                    //}
                }

                pgr += pgrP;

                await _hubContext.Clients.All
                    .SendAsync("Update", "progress", pgr)
                    .ConfigureAwait(false);
                await _hubContext.Clients.All
                    .SendAsync("Update", "status", status)
                    .ConfigureAwait(false);
            }
            await _context.Psmbs.AddRangeAsync(centros).ConfigureAwait(false);
            await _context.Origins.AddRangeAsync(origenes).ConfigureAwait(false);
            await _context.SernapescaDeclarations.AddRangeAsync(datos).ConfigureAwait(false);
            _context.SernapescaDeclarations.UpdateRange(updates);
            status = "success";

            await _hubContext.Clients.All
                .SendAsync("Update", "progress", 100)
                .ConfigureAwait(false);
            await _hubContext.Clients.All
                .SendAsync("Update", "status", status)
                .ConfigureAwait(false);

            msg = $">{entry.Added} añadidos" + (entry.Updated != 0 ? $"y {entry.Updated} registros actualizados " : " ") + "exitosamente.";
            entry.OutPut += msg;
            entry.Success = true;
            await _hubContext.Clients.All.SendAsync("Update", "log", msg)
                .ConfigureAwait(false);

            _context.SernapescaEntries.Update(entry);

            await _context.SaveChangesAsync().ConfigureAwait(false);

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
            if(type == typeof(PlanktonAssay))
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
            }
            FieldInfos.AddRangeOverride(type.GetProperties(BindingFlags)
                .Where(f =>
                f.GetCustomAttribute<ParseSkipAttribute>() == null 
                //&& (!tipo.HasValue 
                //|| (tipo.Value == DeclarationType.Seed && f.GetCustomAttribute<SemillaSkipAttribute>() == null)
                //|| f.GetCustomAttribute<ProduccionSkipAttribute>() == null)
                ));
            Tdatas = FieldInfos.Select(async dt =>
            {
                var var = await _context.Registries
                .Include(r => r.Headers)
                .FirstOrDefaultAsync(c => c.InputFileId == InputFile.Id 
                && c.NormalizedAttribute == dt.Name.ToUpperInvariant()).ConfigureAwait(false);
                if (var == null)
                    throw new EvaluationException(dt.Name);
                if (string.IsNullOrWhiteSpace(var.Description))
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
                    Operation = var.Operation,
                    DecimalPlaces = var.DecimalPlaces,
                    DecimalSeparator = var.DecimalSeparator,
                    DeleteAfter2ndNegative = var.DeleteAfter2ndNegative
                };
                data.Q.AddRangeOverride(var.Headers.Select(h => h.NormalizedName));
                return data;
            }).Select(t => t.Result).Where(t => t != null).ToList();
            FirstOrDefaultAsyncMethod = typeof(EntityFrameworkQueryableExtensions).GetMethods(BindingFlags.Static | BindingFlags.Public)
                .FirstOrDefault(m => m.Name == nameof(EntityFrameworkQueryableExtensions.FirstOrDefaultAsync) && m.GetParameters().Length == 3);
            if (FirstOrDefaultAsyncMethod == null)
            {
                throw new Exception(_localizer[$"Cannot find \"System.Linq.FirstOrDefault\" method."]);
            }
            return Task.CompletedTask;
        }
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        private async Task<Task> AddEntryAsync(string file, string pwd, string logsd)
        {
            var dest = "ERROR";
            var dir = Path.GetFileName(Path.GetDirectoryName(file));
            var logFile = Path.Combine(logsd, $"{dir}_{Path.GetFileName(file)}.log");
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
            item.Name = Regex.Replace(item.Name, @"[^A-Z0-9 ]", "");
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
            var pareaId = await ParseArea(item).ConfigureAwait(false);
            if (pareaId.HasValue)
            {
                item.PsmbId = pareaId.Value;
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
            var psmb = await ParsePsmb(item).ConfigureAwait(false);
            if (item.PsmbId == 0 && psmb == null)
                throw new Exception($"No se pudo encontrar un Psmb o Centro válido para la declaración {item.Id} con fecha {item.SamplingDate}");
            item.Psmb = await ParsePsmb(item).ConfigureAwait(false);
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
                double? ce = matrix.GetValue(3, row).ParseDouble();
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
                        if (genusSp.Count == 1) genusSp.Add(null);
                        SpeciesPhytoplankton specie = await _context.SpeciesPhytoplanktons
                            .FirstOrDefaultAsync(s => s.Genus.NormalizedName == genusSp[0] && s.NormalizedName == genusSp[1]).ConfigureAwait(false);
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
        private string GetKey<T>(T item, string key) where T : IHasBasicIndexer => item[key] as string;
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
        private async Task<Station> ParseEstacion(string text, PlanktonAssay item)
        {
            if (text == null) return null;
            text = Regex.Replace(text, @"[^A-Z0-9 ]|ESTA?C?I?O?N? *", "");
            text = Regex.Replace(text, @"\s+(\b[\w']{1,4}\b)", "$1");
            text = Regex.Replace(text, @"\bE([\w']{1,4}\b)", "$1");
            text = Regex.Replace(text, @"\s{,2}", " ").Trim();
            return await FindParsedAsync<Station>(item, nameof(Station.NormalizedName), text).ConfigureAwait(false);
        }
        private async Task<SamplingEntity> ParseEntidadMuestreadora(string text, PlanktonAssay item)
        {
            if (text == null) return null;
            text = Regex.Replace(text, @"[^A-Z\s]", "");
            return await FindParsedAsync<SamplingEntity>(item, nameof(SamplingEntity.NormalizedName), text).ConfigureAwait(false);
        }
        private async Task<Laboratory> ParseLaboratorio(string text, PlanktonAssay item)
        {
            if (text == null) return null;
            text = Regex.Replace(text, @"[^A-Z\s]", "");
            return await FindParsedAsync<Laboratory>(item, nameof(Laboratory.NormalizedName), text).ConfigureAwait(false);
        }
        private async Task<Analist> ParseAnalista(string text, PlanktonAssay item)
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
            return await FindParsedAsync<Analist>(item, nameof(Analist.NormalizedName), text).ConfigureAwait(false);
        }
        private async Task<Phone> ParseTelefono(string text, PlanktonAssay item)
        {
            if (text == null) return null;
            text = Regex.Replace(text, @"[^0-9\-\/\(\)\s]", "");
            text = Regex.Replace(text, @"\(\)", "");
            return await FindParsedAsync<Phone>(item, nameof(Phone.Number), text).ConfigureAwait(false);
        }
        private async Task<List<PlanktonAssayEmail>> ParseEmails(string text, int? id)
        {
            var results = new List<PlanktonAssayEmail>();
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
        private async Task<object> GetValue(string val, Tdata data, PlanktonAssay item)
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
                nameof(DeclarationType) => val.ParseTipo(),
                nameof(Enum) => Enum.Parse(DataTableExtensions
                        .GetEnumType(data.FieldName), val),
                nameof(Station) => await ParseEstacion(val, item).ConfigureAwait(false),
                nameof(SamplingEntity) => await ParseEntidadMuestreadora(val, item).ConfigureAwait(false),
                nameof(Laboratory) => await ParseLaboratorio(val, item).ConfigureAwait(false),
                nameof(PlanktonAssayEmail) => await ParseEmails(val, item.Id).ConfigureAwait(false),
                nameof(Analist) => await ParseAnalista(val, item).ConfigureAwait(false),
                nameof(Phone) => await ParseTelefono(val, item).ConfigureAwait(false),
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
    }
}