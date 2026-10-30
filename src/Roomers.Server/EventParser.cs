using System.Text.Json;

namespace Roomers.Server
{
    public abstract record RoomEvent(string Room, DateTime Ts);

    public record StudentJoined(string Room, DateTime Ts, Guid StudentId, string Name) : RoomEvent(Room, Ts);
    public record StudentLeft(string Room, DateTime Ts, Guid StudentId) : RoomEvent(Room, Ts);
    public record StudentActive(string Room, DateTime Ts, Guid StudentId) : RoomEvent(Room, Ts);
    public record TeacherJoined(string Room, DateTime Ts, Guid TeacherId, string Name) : RoomEvent(Room, Ts);
    public record TeacherLeft(string Room, DateTime Ts, Guid TeacherId) : RoomEvent(Room, Ts);
    public record ClassStarted(string Room, DateTime Ts, Guid TeacherId, int DurationSeconds) : RoomEvent(Room, Ts);
    public record ClassEnded(string Room, DateTime Ts, Guid TeacherId) : RoomEvent(Room, Ts);

    public static class EventParser
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        public static bool TryParse(string line, out RoomEvent? ev, out string? error)
        {
            ev = null;
            error = null;

            try
            {
                using JsonDocument doc = JsonDocument.Parse(line);
                string? type = doc.RootElement.GetProperty("type").GetString();

                ev = type switch
                {
                    "student_joined" => doc.RootElement.Deserialize<StudentJoined>(Options),
                    "student_left" => doc.RootElement.Deserialize<StudentLeft>(Options),
                    "student_active" => doc.RootElement.Deserialize<StudentActive>(Options),
                    "teacher_joined" => doc.RootElement.Deserialize<TeacherJoined>(Options),
                    "teacher_left" => doc.RootElement.Deserialize<TeacherLeft>(Options),
                    "class_started" => doc.RootElement.Deserialize<ClassStarted>(Options),
                    "class_ended" => doc.RootElement.Deserialize<ClassEnded>(Options),
                    _ => null
                };

                if (ev == null)
                {
                    error = $"Type inconnu : {type}";
                }
                else
                {
                    error = Validate(ev);
                    if (error != null)
                        ev = null;
                }
            }
            catch (Exception e) when (e is JsonException or KeyNotFoundException or InvalidOperationException)
            {
                error = e.Message;
            }

            return ev != null;
        }

        private static string? Validate(RoomEvent ev)
        {
            if (string.IsNullOrWhiteSpace(ev.Room))
                return "room manquant";

            return ev switch
            {
                StudentJoined e => ValidateId(e.StudentId) ?? ValidateName(e.Name),
                StudentLeft e => ValidateId(e.StudentId),
                StudentActive e => ValidateId(e.StudentId),
                TeacherJoined e => ValidateId(e.TeacherId) ?? ValidateName(e.Name),
                TeacherLeft e => ValidateId(e.TeacherId),
                ClassStarted e => ValidateId(e.TeacherId)
                    ?? (e.DurationSeconds <= 0 ? "duration_seconds doit etre superieur a 0" : null),
                ClassEnded e => ValidateId(e.TeacherId),
                _ => null
            };
        }

        private static string? ValidateId(Guid id)
        {
            return id == Guid.Empty ? "identifiant manquant ou vide" : null;
        }

        private static string? ValidateName(string? name)
        {
            return string.IsNullOrWhiteSpace(name) ? "name manquant" : null;
        }
    }
}