using BiblioMit.Services.Interfaces;
using System.Threading.Tasks;

namespace BiblioMit.Services
{
    public class PlanktonService : IPlanktonService
    {
        private readonly IPuppet _puppet;
        public PlanktonService(IPuppet puppet)
        {
            _puppet = puppet;
        }
        public static Task SignIn()
        {

            return Task.CompletedTask;
        }
    }
}
