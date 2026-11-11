using Roomers.Domain;

namespace Roomers.Server
{
    public interface IRoomNotifier
    {
        void RoomChanged(RoomSnapshot snapshot);
    }
}