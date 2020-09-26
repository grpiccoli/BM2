using BiblioMit.Controllers;
using BiblioMit.Data;
using BiblioMit.Models.VM;
using BiblioMit.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace BiblioMit.Services
{
    public class UpdateJsons : IUpdateJsons, IDisposable
    {
        private bool _disposed;
        private readonly IWebHostEnvironment _environment;
        private readonly AmbientalController _ambiental;
        private readonly IStringLocalizer<AmbientalController> _localizer;
        private readonly ApplicationDbContext _context;
        private readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
        private readonly string _jsonPath;
        public UpdateJsons(
            IWebHostEnvironment environment,
            ApplicationDbContext context,
            IStringLocalizer<AmbientalController> localizer
            )
        {
            _context = context;
            _environment = environment;
            _localizer = localizer;
            _ambiental = new AmbientalController(_context, _localizer);
            _jsonPath = Path.Combine(_environment.ContentRootPath, "json");
        }
        public void SeedUpdate()
        {
            WriteJson(nameof(AmbientalController.CuencaData));
            WriteJson(nameof(AmbientalController.ComunaData));
            WriteJson(nameof(AmbientalController.OceanVarList));
            WriteJson(nameof(AmbientalController.CuencaList));
            WriteJson(nameof(AmbientalController.ComunaList));
            WriteJson(nameof(AmbientalController.TLList));

            WriteJson(nameof(AmbientalController.RegionList));
            CenterUpdate();
            PlanktonUpdate();
        }
        public void CenterUpdate()
        {
            WriteJson(nameof(AmbientalController.ComunaResearchList));
            WriteJson(nameof(AmbientalController.ComunaFarmList));
            WriteJson(nameof(AmbientalController.ProvinciaResearchList));
            WriteJson(nameof(AmbientalController.ProvinciaFarmList));
            WriteJson(nameof(AmbientalController.InstitutionList));
            WriteJson(nameof(AmbientalController.CompanyList));
            WriteJson(nameof(AmbientalController.ResearchList));
            WriteJson(nameof(AmbientalController.FarmList));
            WriteJson(nameof(AmbientalController.ResearchData));
            WriteJson(nameof(AmbientalController.FarmData));
        }
        public void PlanktonUpdate()
        {
            WriteJson(nameof(AmbientalController.PsmbData));
            WriteJson(nameof(AmbientalController.PsmbList));
            WriteJson(nameof(AmbientalController.GroupVarList));
            WriteJson(nameof(AmbientalController.GenusVarList));
            WriteJson(nameof(AmbientalController.SpeciesVarList));
        }
        private void WriteJson(string function)
        {
            var name = CultureInfo.InvariantCulture.TextInfo.ToLower(function);
            Type magicType = _ambiental.GetType();
            MethodInfo magicMethod = magicType.GetMethod(function);
            var result = (JsonResult)magicMethod.Invoke(_ambiental, Array.Empty<object>());
            var json = JsonConvert.SerializeObject(result.Value, Formatting.None, _jsonSerializerSettings);
            File.WriteAllText(Path.Combine(_jsonPath, CultureInfo.CurrentUICulture.TwoLetterISOLanguageName, $"{name}.json"), json);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _context?.Dispose();
                _ambiental?.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.

            _disposed = true;
        }
    }
}
