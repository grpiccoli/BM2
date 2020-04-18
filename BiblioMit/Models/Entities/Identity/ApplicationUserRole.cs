using Microsoft.AspNetCore.Identity;

namespace BiblioMit.Models
{
    public class ApplicationUserRole : IdentityUserRole<string>
    {
        public string RoleAssigner { get; set; }
    }
}
