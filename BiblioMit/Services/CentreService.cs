using System;
using System.Collections.Generic;
using System.Linq;
using BiblioMit.Models;
using BiblioMit.Data;

namespace BiblioMit.Services
{
    public class CentreService : ICentre
    {
        private readonly ApplicationDbContext _context;

        public CentreService(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Psmb> GetFilteredCentres(int page, int rpp, string searchQuery)
        {
            if (string.IsNullOrEmpty(searchQuery))
            {
                var normalized = searchQuery?.ToUpperInvariant();
                return ( _context.Psmbs
                    .Where(c =>
                    c.Address.Contains(normalized, StringComparison.Ordinal) ||
                    c.Company.BusinessName.Contains(normalized, StringComparison.Ordinal) ||
                    c.Commune.Name.Contains(normalized, StringComparison.Ordinal))
                    .OrderBy(c => c.Id)
                    .ToList()
                    .GetRange(page * rpp - 1, rpp) );
            }
            else
            {
                return( _context.Psmbs
                    .OrderBy(c => c.Id)
                    .ToList()
                    .GetRange(page * rpp - 1, rpp) );
            }
        }
    }
}
