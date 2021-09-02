using System.Collections.Generic;
using System.Globalization;

namespace BiblioMit.Models
{
    public class CultureSwitcherModel
    {
        public CultureInfo CurrentUICulture { get; set; }
        public ICollection<CultureInfo> SupportedCultures { get; internal set; }
    }
}
