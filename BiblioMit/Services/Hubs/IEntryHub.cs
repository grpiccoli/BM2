using System.Threading.Tasks;

namespace BiblioMit.Services.Hubs
{
    public interface IEntryHub
    {
        Task SendAdded(string user, int message);
        Task SendLog(string user, string message);
        Task SendProgress(string user, double pgr);
        Task SendStatusWarning(string user);
        Task SendStatusDanger(string user);
        Task SendStatusInfo(string user);
        Task SendStatusSuccess(string user);
    }
}
