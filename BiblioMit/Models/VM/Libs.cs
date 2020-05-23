using System.Collections.Generic;

namespace BiblioMit.Models.VM
{
    public class Libs
    {
        public double Version { get; set; }
        public string DefaultProvider { get; set; }
        public List<LibManLibrary> Libraries { get; } = new List<LibManLibrary>();
    }
}
