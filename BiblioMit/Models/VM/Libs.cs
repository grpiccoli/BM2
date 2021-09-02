using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BiblioMit.Models.VM
{
    public class Libs
    {
        public double Version { get; set; }
        public string DefaultProvider { get; set; }
        public Collection<LibManLibrary> Libraries { get; } = new Collection<LibManLibrary>();
    }
}
