using System;
using System.IO;
using System.Security.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BiblioMit.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StaticController : ControllerBase
    {
        [AllowAnonymous, HttpGet("json/{lang?}/{name?}")]
        [ResponseCache(Duration = 60)]
        public IActionResult GetJson(string lang, string name)
        {
            if (name == null) throw new ArgumentNullException($"argument name {name} cannot be null");
            var invariant = name.ToUpperInvariant();
            if (invariant.Contains("FARM", StringComparison.Ordinal) 
                || invariant.Contains("PSMB", StringComparison.Ordinal)
                || invariant.Contains("SPECIE", StringComparison.Ordinal)
                || invariant.Contains("GENUS", StringComparison.Ordinal)
                || invariant.Contains("TL", StringComparison.Ordinal))
            {
                if (!User.Identity.IsAuthenticated) throw new AuthenticationException($"Please log in to view this content {name}");
            }
            var file = Path.Combine(Directory.GetCurrentDirectory(),
                "json", lang, name);
            //var physical = System.IO.File.Exists(file) ? file
            //    : Path.Combine(Directory.GetCurrentDirectory(), DefaultStaticMiddleware.DefaultImagePath);
            return PhysicalFile(file, "application/json");
        }
    }
}
