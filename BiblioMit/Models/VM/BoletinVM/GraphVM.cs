using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BiblioMit.Models.ViewModels
{
    public class GraphVM
    {
        public Dictionary<string,
                        Dictionary<string, List<string>>> Graphs { get; } = new Dictionary<string, Dictionary<string, List<string>>>();

        public int Version { get; set; }

        public Collection<string> Reportes { get; } = new Collection<string>();

        public int Year { get; set; }

        public string Start { get; set; }

        public string End { get; set; }
    }
}
