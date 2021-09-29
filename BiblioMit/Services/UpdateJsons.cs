using BiblioMit.Controllers;
using BiblioMit.Data;
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
using System.Linq;

namespace BiblioMit.Services
{
    public class UpdateJsons : IUpdateJsons, IDisposable
    {
        private bool _disposed;
        private readonly IWebHostEnvironment _environment;
        private readonly AmbientalController _ambiental;
        private readonly IStringLocalizer<AmbientalController> _localizer;
        private readonly ApplicationDbContext _context;
        private readonly JsonSerializerSettings _jsonSerializerSettings = new()
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
            _ambiental = new AmbientalController(_context, environment, _localizer);
            _jsonPath = Path.Combine(_environment.ContentRootPath, "json");
        }
        public void SeedUpdate()
        {
            WriteJson(nameof(_ambiental.CuencaData));
            WriteJson(nameof(_ambiental.ComunaData));
            WriteJson(nameof(_ambiental.OceanVarList));
            WriteJson(nameof(_ambiental.CuencaList));
            WriteJson(nameof(_ambiental.ComunaList));
            WriteJson(nameof(_ambiental.CustomVarList));
            WriteJson(nameof(_ambiental.TLList));

            WriteJson(nameof(_ambiental.RegionList));

            WriteJson(nameof(_ambiental.GetPhotos));

            CenterUpdate();
            PlanktonUpdate();
        }
        public void CenterUpdate()
        {
            WriteJson(nameof(_ambiental.ComunaResearchList));
            WriteJson(nameof(_ambiental.ComunaFarmList));
            WriteJson(nameof(_ambiental.ProvinciaResearchList));
            WriteJson(nameof(_ambiental.ProvinciaFarmList));
            WriteJson(nameof(_ambiental.InstitutionList));
            WriteJson(nameof(_ambiental.CompanyList));
            WriteJson(nameof(_ambiental.ResearchList));
            WriteJson(nameof(_ambiental.FarmList));
            WriteJson(nameof(_ambiental.ResearchData));
            WriteJson(nameof(_ambiental.FarmData));
        }
        public void PlanktonUpdate()
        {
            WriteJson(nameof(_ambiental.PsmbData));
            WriteJson(nameof(_ambiental.PsmbList));
            WriteJson(nameof(_ambiental.GroupVarList));
            WriteJson(nameof(_ambiental.GenusVarList));
            WriteJson(nameof(_ambiental.SpeciesVarList));
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
