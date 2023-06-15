using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace smsmanager.refreshPage
{
    public class ServerHub : Hub
    {
        public async Task ReloadPageTable()
        {
            await Clients.All.SendAsync("reloadPageTable");
        }
    }
}
