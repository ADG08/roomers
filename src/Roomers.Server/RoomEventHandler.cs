using Roomers.Domain;

namespace Roomers.Server
{
    public class RoomEventHandler
    {
        private readonly RoomRegistry _registry;

        public RoomEventHandler(RoomRegistry registry)
        {
            _registry = registry;
        }

        public bool TryHandle(RoomEvent ev, out string? error)
        {
            error = null;

            if (!_registry.TryGet(ev.Room, out Room? room) || room == null)
            {
                error = $"Salle inconnue : {ev.Room}";
                return false;
            }

            bool accepted;
            lock (room)
            {
                accepted = ev switch
                {
                    TeacherJoined e => room.JoinRoom(new People(e.TeacherId, e.Name, PeopleType.Teacher)),
                    TeacherLeft e => room.LeaveRoom(e.TeacherId),
                    StudentJoined e => room.JoinRoom(new People(e.StudentId, e.Name, PeopleType.Student)),
                    StudentLeft e => room.LeaveRoom(e.StudentId),
                    ClassStarted e => room.StartCourse(e.TeacherId),
                    ClassEnded e => room.EndCourse(e.TeacherId),
                    _ => false
                };
            }

            if (!accepted)
                error = "Action refusée par la salle";

            return accepted;
        }
    }
}