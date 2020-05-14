using BiblioMit.Services.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace BiblioMit.Services
{
    public class EntryHub : Hub, IEntryHub
    {
        private async Task Send(string user, SendFunction function, object message)
        {
            if (user != "nofeed") await Clients.User(user).SendAsync("Update", function.ToString(), message)
            .ConfigureAwait(false);
        }
        public async Task SendAdded(string user, int message) =>
            await Send(user, SendFunction.agregada, message).ConfigureAwait(false);
        public async Task SendLog(string user, string message) =>
            await Send(user, SendFunction.log, message).ConfigureAwait(false);
        private async Task SendStatus(string user, Status message) =>
            await Send(user, SendFunction.status, message.ToString()).ConfigureAwait(false);
        public async Task SendProgress(string user, double pgr) =>
            await Send(user, SendFunction.progress, pgr).ConfigureAwait(false);
        public async Task SendStatusWarning(string user) =>
            await SendStatus(user, Status.warning).ConfigureAwait(false);
        public async Task SendStatusDanger(string user) =>
            await SendStatus(user, Status.danger).ConfigureAwait(false);
        public async Task SendStatusInfo(string user) =>
            await SendStatus(user, Status.info).ConfigureAwait(false);
        public async Task SendStatusSuccess(string user) =>
            await SendStatus(user, Status.success).ConfigureAwait(false);
    }
    public enum Status
    {
        warning,danger,info,success
    }
    public enum SendFunction
    {
        log, status, progress, agregada
    }
}
