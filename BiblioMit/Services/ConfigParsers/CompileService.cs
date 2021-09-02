using BiblioMit.Models.VM;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace BiblioMit.Services
{
    public static class WebCompiler
    {
        private static Collection<Compile> Compiles { get; set; }
        public static Collection<Compile> LoadJson()
        {
            using StreamReader r = new("compilerconfig.json");
            string json = r.ReadToEnd();
            Compiles = JsonConvert.DeserializeObject<Collection<Compile>>(json);
            return Compiles;
        }
        public static IEnumerable<Compile> GetBundles(string lib)
        {
            return Compiles.Where(m => m.OutputFile.Contains($"/{lib}.", StringComparison.InvariantCulture));
        }
    }
}
