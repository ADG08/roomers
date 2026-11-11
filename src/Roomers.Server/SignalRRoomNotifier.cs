using Microsoft.AspNetCore.SignalR;
using Roomers.Domain;

namespace Roomers.Server
{
    public class SignalRRoomNotifier : IRoomNotifier
    {
        private readonly IHubContext<RoomsHub> _hub;
        private readonly ILogger<SignalRRoomNotifier> _logger;

        public SignalRRoomNotifier(IHubContext<RoomsHub> hub, ILogger<SignalRRoomNotifier> logger)
        {
            _hub = hub;
            _logger = logger;
        }

        public void RoomChanged(RoomSnapshot snapshot)
        {
            _ = SendAsync(snapshot);
        }

        private async Task SendAsync(RoomSnapshot snapshot)
        {
            try
            {
                await _hub.Clients.All.SendAsync("RoomChanged", snapshot);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Envoi du snapshot de {Room} impossible", snapshot.Name);
            }
        }
    }
}