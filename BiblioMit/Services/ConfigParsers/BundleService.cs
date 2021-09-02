using BiblioMit.Models.VM;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace BiblioMit.Services
{
    public static class Bundler
    {
        private static Collection<BundleConfig> Bundles { get; set; }
        public static Collection<BundleConfig> LoadJson()
        {
            using StreamReader r = new("bundleconfig.json");
            string json = r.ReadToEnd();
            Bundles = JsonConvert.DeserializeObject<Collection<BundleConfig>>(json);
            return Bundles;
        }
        public static IEnumerable<BundleConfig> GetBundles(string lib)
        {
            return Bundles.Where(m => m.OutputFileName.Contains($"/{lib}.", StringComparison.InvariantCulture));
        }
    }
}
