using Roomers.Domain;

namespace Roomers.Server
{
    public class LogRoomNotifier : IRoomNotifier
    {
        private readonly ILogger<LogRoomNotifier> _logger;

        public LogRoomNotifier(ILogger<LogRoomNotifier> logger)
        {
            _logger = logger;
        }

        public void RoomChanged(RoomSnapshot snapshot)
        {
            _logger.LogInformation(
                "Etat de {Room} : verrouillee={IsLocked}, professeur={Teacher}, etudiants={Count}",
                snapshot.Name, snapshot.IsLocked, snapshot.Teacher?.Name ?? "aucun", snapshot.Students.Count);
        }
    }
}