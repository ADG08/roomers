using Roomers.Server;

namespace Roomers.Server.Tests
{
    public class EventParserTests
    {
        private string StudentId = Guid.NewGuid().ToString();
        private string TeacherId = Guid.NewGuid().ToString();

        private static string Json(string type, string extra)
        {
            return "{\"type\":\"" + type + "\",\"room\":\"Salle 1\",\"ts\":\"2026-10-27T13:30:00Z\"" + extra + "}";
        }

        private static void AssertError(string line)
        {
            bool ok = EventParser.TryParse(line, out RoomEvent? ev, out string? error);

            Assert.False(ok);
            Assert.Null(ev);
            Assert.NotNull(error);
        }


        [Fact]
        public void Parse_ValidStudentJoined_ReturnsStudentJoinedEvent()
        {
            string line = Json("student_joined", $",\"student_id\":\"{StudentId}\",\"name\":\"Andre\"");

            Assert.True(EventParser.TryParse(line, out RoomEvent? ev, out string? error));
            StudentJoined e = Assert.IsType<StudentJoined>(ev);
            Assert.Equal("Salle 1", e.Room);
            Assert.Equal(Guid.Parse(StudentId), e.StudentId);
            Assert.Equal("Andre", e.Name);
        }

        [Fact]
        public void Parse_ValidStudentLeft_ReturnsStudentLeftEvent()
        {
            string line = Json("student_left", $",\"student_id\":\"{StudentId}\"");

            Assert.True(EventParser.TryParse(line, out RoomEvent? ev, out string? error));
            StudentLeft e = Assert.IsType<StudentLeft>(ev);
            Assert.Equal(Guid.Parse(StudentId), e.StudentId);
        }

        [Fact]
        public void Parse_ValidStudentActive_ReturnsStudentActiveEvent()
        {
            string line = Json("student_active", $",\"student_id\":\"{StudentId}\"");

            Assert.True(EventParser.TryParse(line, out RoomEvent? ev, out string? error));
            StudentActive e = Assert.IsType<StudentActive>(ev);
            Assert.Equal(Guid.Parse(StudentId), e.StudentId);
        }

        [Fact]
        public void Parse_ValidTeacherJoined_ReturnsTeacherJoinedEvent()
        {
            string line = Json("teacher_joined", $",\"teacher_id\":\"{TeacherId}\",\"name\":\"Professeur Morin\"");

            Assert.True(EventParser.TryParse(line, out RoomEvent? ev, out string? error));
            TeacherJoined e = Assert.IsType<TeacherJoined>(ev);
            Assert.Equal(Guid.Parse(TeacherId), e.TeacherId);
            Assert.Equal("Professeur Morin", e.Name);
        }

        [Fact]
        public void Parse_ValidTeacherLeft_ReturnsTeacherLeftEvent()
        {
            string line = Json("teacher_left", $",\"teacher_id\":\"{TeacherId}\"");

            Assert.True(EventParser.TryParse(line, out RoomEvent? ev, out string? error));
            TeacherLeft e = Assert.IsType<TeacherLeft>(ev);
            Assert.Equal(Guid.Parse(TeacherId), e.TeacherId);
        }

        [Fact]
        public void Parse_ValidClassStarted_ReturnsDurationSeconds()
        {
            string line = Json("class_started", $",\"teacher_id\":\"{TeacherId}\",\"duration_seconds\":3600");

            Assert.True(EventParser.TryParse(line, out RoomEvent? ev, out string? error));
            ClassStarted e = Assert.IsType<ClassStarted>(ev);
            Assert.Equal(Guid.Parse(TeacherId), e.TeacherId);
            Assert.Equal(3600, e.DurationSeconds);
        }

        [Fact]
        public void Parse_ValidClassEnded_ReturnsClassEndedEvent()
        {
            string line = Json("class_ended", $",\"teacher_id\":\"{TeacherId}\"");

            Assert.True(EventParser.TryParse(line, out RoomEvent? ev, out string? error));
            ClassEnded e = Assert.IsType<ClassEnded>(ev);
            Assert.Equal(Guid.Parse(TeacherId), e.TeacherId);
        }

        [Fact]
        public void Parse_InvalidJson_ReturnsError()
        {
            AssertError("{pas du json");
        }

        [Fact]
        public void Parse_EmptyLine_ReturnsError()
        {
            AssertError("");
        }

        [Fact]
        public void Parse_UnknownType_ReturnsError()
        {
            AssertError(Json("student_flying", $",\"student_id\":\"{StudentId}\""));
        }

        [Fact]
        public void Parse_MissingType_ReturnsError()
        {
            AssertError($"{{\"room\":\"Salle 1\",\"ts\":\"2026-10-27T13:30:00Z\",\"student_id\":\"{StudentId}\"}}");
        }

        [Fact]
        public void Parse_InvalidGuid_ReturnsError()
        {
            AssertError(Json("student_left", ",\"student_id\":\"pas-un-guid\""));
        }

        [Fact]
        public void Parse_StudentJoinedWithoutStudentId_ReturnsError()
        {
            AssertError(Json("student_joined", ",\"name\":\"Andre\""));
        }

        [Fact]
        public void Parse_StudentJoinedWithoutName_ReturnsError()
        {
            AssertError(Json("student_joined", $",\"student_id\":\"{StudentId}\""));
        }

        [Fact]
        public void Parse_MissingRoom_ReturnsError()
        {
            AssertError($"{{\"type\":\"student_left\",\"ts\":\"2026-10-27T13:30:00Z\",\"student_id\":\"{StudentId}\"}}");
        }

        [Fact]
        public void Parse_EmptyGuid_ReturnsError()
        {
            AssertError(Json("student_left", ",\"student_id\":\"00000000-0000-0000-0000-000000000000\""));
        }

        [Fact]
        public void Parse_ClassStartedWithZeroDuration_ReturnsError()
        {
            AssertError(Json("class_started", $",\"teacher_id\":\"{TeacherId}\",\"duration_seconds\":0"));
        }

        [Fact]
        public void Parse_ClassStartedWithNegativeDuration_ReturnsError()
        {
            AssertError(Json("class_started", $",\"teacher_id\":\"{TeacherId}\",\"duration_seconds\":-10"));
        }
    }
}