using System.Threading;
using System.Threading.Tasks;

namespace BiblioMit.Services.Interfaces
{
    public interface IPlanktonService
    {
        Task PullRecordsAsync(CancellationToken stoppingToken);
    }
}
