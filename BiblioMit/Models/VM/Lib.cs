using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BiblioMit.Models.VM
{
    public class LibManLibrary
    {
        public string Library { get; set; }
        public string Destination { get; set; }
        public Collection<string> Files { get; } = new Collection<string>();
        public string Provider { get; set; }
    }
}
