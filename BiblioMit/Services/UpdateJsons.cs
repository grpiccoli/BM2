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
            _jsonPath = Path.Combine(_environment.ContentRootPath, "wwwroot", "json");
        }
        public void SeedUpdate()
        {
            WriteJson(nameof(AmbientalController.CuencaData));
            WriteJson(nameof(AmbientalController.ComunaData));
            WriteJson(nameof(AmbientalController.OceanVarList));
            WriteJson(nameof(AmbientalController.CuencaList));
            WriteJson(nameof(AmbientalController.ComunaList));
            WriteJson(nameof(AmbientalController.TLList));
            PlanktonUpdate();
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
            File.WriteAllText(Path.Combine(_jsonPath, $"{name}.json"), json);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

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
