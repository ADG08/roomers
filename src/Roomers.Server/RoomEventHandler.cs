using Roomers.Domain;
using System.Collections.Concurrent;

namespace Roomers.Server
{
    public class RoomEventHandler
    {
        private readonly RoomRegistry _registry;
        private readonly TimeProvider _time;
        private readonly ConcurrentDictionary<string, ITimer> _timers = new();
        private readonly ILogger<RoomEventHandler> _logger;
        private readonly IRoomNotifier _notifier;

        public RoomEventHandler(RoomRegistry registry, TimeProvider time, ILogger<RoomEventHandler> logger, IRoomNotifier notifier)
        {
            _registry = registry;
            _time = time;
            _logger = logger;
            _notifier = notifier;
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
                    ClassStarted e => room.StartCourse(e.TeacherId, _time.GetUtcNow().AddSeconds(e.DurationSeconds)),
                    ClassEnded e => room.EndCourse(e.TeacherId),
                    StudentActive => true,
                    _ => false
                };

                if (accepted)
                {
                    UpdateTimer(ev, room);

                    if (ev is not StudentActive)
                        _notifier.RoomChanged(room.GetSnapshot());
                }
            }

            if (!accepted)
                error = "Action refusée par la salle";

            return accepted;
        }
        private void UpdateTimer(RoomEvent ev, Room room)
        {
            switch (ev)
            {
                case ClassStarted e:
                    StartTimer(room, e.Room, e.DurationSeconds);
                    break;
                case ClassEnded:
                case TeacherLeft:
                    StopTimer(ev.Room);
                    break;
            }
        }

        private void StartTimer(Room room, string roomName, int seconds)
        {
            StopTimer(roomName);
            ITimer timer = _time.CreateTimer(
                _ => OnCourseExpired(room, roomName),
                null,
                TimeSpan.FromSeconds(seconds),
                Timeout.InfiniteTimeSpan);
            _timers[roomName] = timer;
        }

        private void StopTimer(string roomName)
        {
            if (_timers.TryRemove(roomName, out ITimer? timer))
                timer.Dispose();
        }

        private void OnCourseExpired(Room room, string roomName)
        {
            bool expired;
            lock (room)
            {
                expired = room.ExpireCourse();
                StopTimer(roomName);

                if (expired)
                    _notifier.RoomChanged(room.GetSnapshot());
            }

            if (expired)
                _logger.LogInformation("Cours termine dans {Room} (temps ecoule)", roomName);
        }
    }
}