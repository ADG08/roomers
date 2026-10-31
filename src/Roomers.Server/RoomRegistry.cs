using Roomers.Domain;

namespace Roomers.Server
{
    public record RoomConfig(string Name, int MaxCapacity);

    public class RoomRegistry
    {
        private readonly Dictionary<string, Room> _rooms = new();
        public RoomRegistry(IEnumerable<RoomConfig> roomConfigs)
        {
            foreach (RoomConfig config in roomConfigs)
            {
                _rooms.Add(config.Name, new Room(config.Name, config.MaxCapacity));
            }
        }

        public bool TryGet(string name, out Room? room)
        {
            return _rooms.TryGetValue(name, out room);
        }
    }
}
