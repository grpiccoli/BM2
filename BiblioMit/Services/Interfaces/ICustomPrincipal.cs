using System.Security.Principal;

namespace BiblioMit.Services.Interfaces
{
    public interface ICustomPrincipal : IPrincipal
    {
        int Id { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
    }
}
