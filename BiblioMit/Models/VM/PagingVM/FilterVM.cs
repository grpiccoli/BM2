using System.Collections.Generic;

namespace BiblioMit.Models.ViewModels
{
    public class FilterVM
    {
        public FilterVM(ICollection<string> input)
        {
            Val = input;
        }
        public int Rpp { get; set; }

        public bool Asc { get; set; }

        public string Controller { get; set; }

        public string Action { get; set; }

        public string Srt { get; set; }

        public ICollection<string> Val { get; internal set;  }
    }
}
