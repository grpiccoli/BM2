using BiblioMit.Models.VM;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace BiblioMit.Services
{
    public static class Libman
    {
        private static Libs Libs { get; set; } = new Libs();

        public static Libs LoadJson()
        {
            using StreamReader r = new StreamReader("libman.json");
            string json = r.ReadToEnd();
            Libs = JsonConvert.DeserializeObject<Libs>(json);
            var os = Environment.OSVersion.Platform.ToString();
            if ("Win32NT" != os)
            {
                foreach(var lib in Libs.Libraries)
                {
                    //unpkg as default!!!
                    if(string.IsNullOrWhiteSpace(lib.Provider))
                    {
                        var prefix = $"https://unpkg.com/{lib.Library}/";
                        foreach(var file in lib.Files)
                        {
                            using var process = new Process
                            {
                                StartInfo = new ProcessStartInfo
                                {
                                    FileName = "wget",
                                    Arguments = $"{prefix}/{file} -O {lib.Destination}/{file}",
                                    UseShellExecute = false,
                                    RedirectStandardOutput = true,
                                    RedirectStandardError = true
                                }
                            };
                            var s = string.Empty;
                            var e = string.Empty;
                            process.OutputDataReceived += (sender, data) => s += data.Data;
                            process.ErrorDataReceived += (sender, data) => e += data.Data;
                            process.Start();
                            process.BeginOutputReadLine();
                            process.BeginErrorReadLine();
                            process.WaitForExit();
                            process.Close();
                        }
                    }
                }
            }
            return Libs;
        }

        public static LibManLibrary GetLibs(string lib)
        {
            var libs = Libs.Libraries.FirstOrDefault(m => 
            m.Library.StartsWith($"{lib}@", StringComparison.Ordinal) || m.Library.StartsWith($"{lib}/", StringComparison.Ordinal));
            return libs;
        }
    }
}
