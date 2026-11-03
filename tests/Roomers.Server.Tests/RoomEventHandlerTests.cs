namespace Roomers.Server.Tests
{
    public class RoomEventHandlerTests
    {
        private readonly RoomEventHandler _handler;

        public RoomEventHandlerTests()
        {
            RoomRegistry registry = new RoomRegistry(new[] { new RoomConfig("Salle 1", 15) });
            _handler = new RoomEventHandler(registry);
        }

        [Fact]
        public void TryHandle_UnknownRoom_ReturnsFalse()
        {
            RoomEvent ev = new TeacherJoined("Salle X", DateTime.UtcNow, Guid.NewGuid(), "Professeur Morin");

            bool ok = _handler.TryHandle(ev, out string? error);

            Assert.False(ok);
            Assert.NotNull(error);
        }

        [Fact]
        public void TryHandle_TeacherJoined_ReturnsTrueAndSecondTeacherIsRefused()
        {
            RoomEvent first = new TeacherJoined("Salle 1", DateTime.UtcNow, Guid.NewGuid(), "Professeur Morin");
            RoomEvent second = new TeacherJoined("Salle 1", DateTime.UtcNow, Guid.NewGuid(), "Professeur Dubois");

            Assert.True(_handler.TryHandle(first, out string? error1));
            Assert.False(_handler.TryHandle(second, out string? error2));
            Assert.NotNull(error2);
        }

        [Fact]
        public void TryHandle_StudentJoinedTwice_SecondReturnsFalse()
        {
            RoomEvent first = new TeacherJoined("Salle 1", DateTime.UtcNow, Guid.NewGuid(), "Professeur Morin");

            Guid student = Guid.NewGuid();
            RoomEvent second = new StudentJoined("Salle 1", DateTime.UtcNow, student, "Andre Domingues Ramos");
            RoomEvent third = new StudentJoined("Salle 1", DateTime.UtcNow, student, "Andre Domingues Ramos 2");

            Assert.True(_handler.TryHandle(first, out string? error1));
            Assert.True(_handler.TryHandle(second, out string? error2));
            Assert.False(_handler.TryHandle(third, out string? error3));
        }

        [Fact]
        public void TryHandle_StudentLeft_ReturnsTrue()
        {
            Guid student = Guid.NewGuid();

            RoomEvent first = new StudentJoined("Salle 1", DateTime.UtcNow, student, "Andre Domingues Ramos");
            RoomEvent second = new StudentLeft("Salle 1", DateTime.UtcNow, student);

            Assert.True(_handler.TryHandle(first, out string? error1));
            Assert.True(_handler.TryHandle(second, out string? error2));
        }

        [Fact]
        public void TryHandle_StudentLeftNotInRoom_ReturnsFalse()
        {
            RoomEvent first = new StudentLeft("Salle 1", DateTime.UtcNow, Guid.NewGuid());
            Assert.False(_handler.TryHandle(first, out string? error1));
        }

        [Fact]
        public void TryHandle_ClassStartedByTeacher_ReturnsTrue()
        {
            Guid teacher = Guid.NewGuid();
            RoomEvent first = new TeacherJoined("Salle 1", DateTime.UtcNow, teacher, "Professeur Morin");
            RoomEvent second = new StudentJoined("Salle 1", DateTime.UtcNow, Guid.NewGuid(), "Andre Domingues Ramos");
            RoomEvent third = new StudentJoined("Salle 1", DateTime.UtcNow, Guid.NewGuid(), "Andre Domingues Ramos 2");
            RoomEvent fourth = new ClassStarted("Salle 1", DateTime.UtcNow, teacher, 300);
            RoomEvent fifth = new StudentJoined("Salle 1", DateTime.UtcNow, Guid.NewGuid(), "Andre Domingues Ramos 3");

            Assert.True(_handler.TryHandle(first, out string? error1));
            Assert.True(_handler.TryHandle(second, out string? error2));
            Assert.True(_handler.TryHandle(third, out string? error3));
            Assert.True(_handler.TryHandle(fourth, out string? error4));
            Assert.False(_handler.TryHandle(fifth, out string? error5));
        }

        [Fact]
        public void TryHandle_ClassStartedByStudent_ReturnsFalse()
        {
            Guid firstStudent = Guid.NewGuid();
            RoomEvent first = new TeacherJoined("Salle 1", DateTime.UtcNow, Guid.NewGuid(), "Professeur Morin");
            RoomEvent second = new StudentJoined("Salle 1", DateTime.UtcNow, firstStudent, "Andre Domingues Ramos");
            RoomEvent third = new StudentJoined("Salle 1", DateTime.UtcNow, Guid.NewGuid(), "Andre Domingues Ramos 2");
            RoomEvent fourth = new ClassStarted("Salle 1", DateTime.UtcNow, firstStudent, 300);


            Assert.True(_handler.TryHandle(first, out string? error1));
            Assert.True(_handler.TryHandle(second, out string? error2));
            Assert.True(_handler.TryHandle(third, out string? error3));
            Assert.False(_handler.TryHandle(fourth, out string? error4));
        }

        [Fact]
        public void TryHandle_ClassEnded_ReopensRoom()
        {
            Guid teacher = Guid.NewGuid();
            RoomEvent first = new TeacherJoined("Salle 1", DateTime.UtcNow, teacher, "Professeur Morin");
            RoomEvent second = new StudentJoined("Salle 1", DateTime.UtcNow, Guid.NewGuid(), "Andre Domingues Ramos");
            RoomEvent third = new StudentJoined("Salle 1", DateTime.UtcNow, Guid.NewGuid(), "Andre Domingues Ramos 2");
            RoomEvent fourth = new ClassStarted("Salle 1", DateTime.UtcNow, teacher, 300);
            RoomEvent fifth = new ClassEnded("Salle 1", DateTime.UtcNow, teacher);
            RoomEvent sixth = new StudentJoined("Salle 1", DateTime.UtcNow, Guid.NewGuid(), "Andre Domingues Ramos 3");


            Assert.True(_handler.TryHandle(first, out string? error1));
            Assert.True(_handler.TryHandle(second, out string? error2));
            Assert.True(_handler.TryHandle(third, out string? error3));
            Assert.True(_handler.TryHandle(fourth, out string? error4));
            Assert.True(_handler.TryHandle(fifth, out string? error5));
            Assert.True(_handler.TryHandle(sixth, out string? error6));
        }

        [Fact]
        public void TryHandle_TeacherLeftDuringCourse_ReopensRoom()
        {
            Guid teacher = Guid.NewGuid();
            RoomEvent first = new TeacherJoined("Salle 1", DateTime.UtcNow, teacher, "Professeur Morin");
            RoomEvent second = new StudentJoined("Salle 1", DateTime.UtcNow, Guid.NewGuid(), "Andre Domingues Ramos");
            RoomEvent third = new StudentJoined("Salle 1", DateTime.UtcNow, Guid.NewGuid(), "Andre Domingues Ramos 2");
            RoomEvent fourth = new ClassStarted("Salle 1", DateTime.UtcNow, teacher, 300);
            RoomEvent fifth = new TeacherLeft("Salle 1", DateTime.UtcNow, teacher);
            RoomEvent sixth = new StudentJoined("Salle 1", DateTime.UtcNow, Guid.NewGuid(), "Andre Domingues Ramos 3");


            Assert.True(_handler.TryHandle(first, out string? error1));
            Assert.True(_handler.TryHandle(second, out string? error2));
            Assert.True(_handler.TryHandle(third, out string? error3));
            Assert.True(_handler.TryHandle(fourth, out string? error4));
            Assert.True(_handler.TryHandle(fifth, out string? error5));
            Assert.True(_handler.TryHandle(sixth, out string? error6));
        }
    }
}