using Microsoft.AspNetCore.SignalR;

namespace Roomers.Server
{
    public class RoomsHub : Hub
    {
        private readonly RoomRegistry _registry;

        public RoomsHub(RoomRegistry registry)
        {
            _registry = registry;
        }

        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("RoomsState", _registry.GetSnapshots());
            await base.OnConnectedAsync();
        }
    }
}