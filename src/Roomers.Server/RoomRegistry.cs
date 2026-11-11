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

        public IReadOnlyList<RoomSnapshot> GetSnapshots()
        {
            List<RoomSnapshot> snapshots = new List<RoomSnapshot>();

            foreach (Room room in _rooms.Values)
            {
                lock (room)
                {
                    snapshots.Add(room.GetSnapshot());
                }
            }

            return snapshots;
        }
    }
}
