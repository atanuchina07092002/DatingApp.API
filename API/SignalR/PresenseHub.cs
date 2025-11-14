using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    /// <summary>
    /// this hub is created for online and ofline status
    /// </summary>
    [Authorize]
    public class PresenseHub :Hub
    {
        public override async Task OnConnectedAsync()
        {
             await Clients.Others.SendAsync("UserOnline",Context.User?.GetUserAsync());//Send message all connected users exceptcurrent one
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {

            await Clients.Others.SendAsync("UserOfline", Context.User?.GetUserAsync());
            await  base.OnDisconnectedAsync(exception);
        }
    }
}
