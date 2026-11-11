using Roomers.Domain;

namespace Roomers.Server.Tests
{
    public class FakeRoomNotifier : IRoomNotifier
    {
        public List<RoomSnapshot> Snapshots { get; } = new List<RoomSnapshot>();

        public void RoomChanged(RoomSnapshot snapshot)
        {
            Snapshots.Add(snapshot);
        }
    }
}