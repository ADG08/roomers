using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;

namespace Roomers.Server.Tests
{
    public class RoomEventHandlerTests
    {
        private readonly RoomEventHandler _handler;
        private readonly FakeTimeProvider _time = new FakeTimeProvider();
        private readonly FakeRoomNotifier _notifier = new FakeRoomNotifier();

        public RoomEventHandlerTests()
        {
            RoomRegistry registry = new RoomRegistry(new[] { new RoomConfig("Salle 1", 15) });
            _handler = new RoomEventHandler(registry, _time, NullLogger<RoomEventHandler>.Instance, _notifier);
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

        [Fact]
        public void TryHandle_ClassStarted_CourseExpiresAfterDuration()
        {
            Guid teacher = Guid.NewGuid();
            RoomEvent first = new TeacherJoined("Salle 1", DateTime.UtcNow, teacher, "Professeur Morin");
            RoomEvent second = new StudentJoined("Salle 1", DateTime.UtcNow, Guid.NewGuid(), "Andre Domingues Ramos");
            RoomEvent third = new ClassStarted("Salle 1", DateTime.UtcNow, teacher, 300);

            Assert.True(_handler.TryHandle(first, out string? error1));
            Assert.True(_handler.TryHandle(second, out string? error2));
            Assert.True(_handler.TryHandle(third, out string? error3));

            RoomEvent fourth = new StudentJoined("Salle 1", DateTime.UtcNow, Guid.NewGuid(), "Retardataire");
            Assert.False(_handler.TryHandle(fourth, out string? error4));
            
            _time.Advance(TimeSpan.FromSeconds(300));

            Assert.True(_handler.TryHandle(fourth, out string? error5));
        }

        [Fact]
        public void TryHandle_StudentActive_ReturnsTrue()
        {
            Guid student = Guid.NewGuid();
            RoomEvent first = new StudentJoined("Salle 1", DateTime.UtcNow, student, "Andre Domingues Ramos");
            RoomEvent second = new StudentActive("Salle 1", DateTime.UtcNow, student);

            Assert.True(_handler.TryHandle(first, out string? error1));
            Assert.True(_handler.TryHandle(second, out string? error2));
            Assert.Null(error2);
        }

        [Fact]
        public void TryHandle_ClassEndedBeforeDuration_OldTimerDoesNotCloseNextCourse()
        {
            Guid teacher = Guid.NewGuid();
            RoomEvent first = new TeacherJoined("Salle 1", DateTime.UtcNow, teacher, "Professeur Morin");
            RoomEvent second = new StudentJoined("Salle 1", DateTime.UtcNow, Guid.NewGuid(), "Andre Domingues Ramos");
            RoomEvent third = new ClassStarted("Salle 1", DateTime.UtcNow, teacher, 300);
            RoomEvent fourth = new ClassEnded("Salle 1", DateTime.UtcNow, teacher);
            RoomEvent fifth = new StudentJoined("Salle 1", DateTime.UtcNow, Guid.NewGuid(), "Andre Domingues Ramos 2");
            RoomEvent sixth = new ClassStarted("Salle 1", DateTime.UtcNow, teacher, 300);
            RoomEvent seventh = new StudentJoined("Salle 1", DateTime.UtcNow, Guid.NewGuid(), "Retardataire");

            Assert.True(_handler.TryHandle(first, out string? error1));
            Assert.True(_handler.TryHandle(second, out string? error2));
            Assert.True(_handler.TryHandle(third, out string? error3));

            _time.Advance(TimeSpan.FromSeconds(200));

            Assert.True(_handler.TryHandle(fourth, out string? error4));
            Assert.True(_handler.TryHandle(fifth, out string? error5));
            Assert.True(_handler.TryHandle(sixth, out string? error6));

            _time.Advance(TimeSpan.FromSeconds(100));

            Assert.False(_handler.TryHandle(seventh, out string? error7));

            _time.Advance(TimeSpan.FromSeconds(200));

            Assert.True(_handler.TryHandle(seventh, out string? error8));
        }

        [Fact]
        public void TryHandle_AcceptedEvent_NotifiesRoomSnapshot()
        {
            Guid teacher = Guid.NewGuid();
            RoomEvent first = new TeacherJoined("Salle 1", DateTime.UtcNow, teacher, "Professeur Morin");

            Assert.True(_handler.TryHandle(first, out string? error1));

            Assert.Single(_notifier.Snapshots);
            Assert.Equal("Salle 1", _notifier.Snapshots[0].Name);
            Assert.NotNull(_notifier.Snapshots[0].Teacher);
            Assert.Equal(teacher, _notifier.Snapshots[0].Teacher.Id);
        }

        [Fact]
        public void TryHandle_RefusedEvent_DoesNotNotify()
        {
            RoomEvent first = new StudentLeft("Salle 1", DateTime.UtcNow, Guid.NewGuid());

            Assert.False(_handler.TryHandle(first, out string? error1));

            Assert.Empty(_notifier.Snapshots);
        }

        [Fact]
        public void TryHandle_CourseExpires_NotifiesUnlockedSnapshot()
        {
            Guid teacher = Guid.NewGuid();
            RoomEvent first = new TeacherJoined("Salle 1", DateTime.UtcNow, teacher, "Professeur Morin");
            RoomEvent second = new StudentJoined("Salle 1", DateTime.UtcNow, Guid.NewGuid(), "Andre Domingues Ramos");
            RoomEvent third = new ClassStarted("Salle 1", DateTime.UtcNow, teacher, 300);

            Assert.True(_handler.TryHandle(first, out string? error1));
            Assert.True(_handler.TryHandle(second, out string? error2));
            Assert.True(_handler.TryHandle(third, out string? error3));
            Assert.True(_notifier.Snapshots[^1].IsLocked);

            _time.Advance(TimeSpan.FromSeconds(300));

            Assert.False(_notifier.Snapshots[^1].IsLocked);
            Assert.Empty(_notifier.Snapshots[^1].Students);
        }

        [Fact]
        public void TryHandle_ClassStarted_NotifiesSnapshotWithEndTime()
        {
            Guid teacher = Guid.NewGuid();
            RoomEvent first = new TeacherJoined("Salle 1", DateTime.UtcNow, teacher, "Professeur Morin");
            RoomEvent second = new StudentJoined("Salle 1", DateTime.UtcNow, Guid.NewGuid(), "Andre Domingues Ramos");
            RoomEvent third = new ClassStarted("Salle 1", DateTime.UtcNow, teacher, 300);
            DateTimeOffset expected = _time.GetUtcNow().AddSeconds(300);

            Assert.True(_handler.TryHandle(first, out string? error1));
            Assert.True(_handler.TryHandle(second, out string? error2));
            Assert.True(_handler.TryHandle(third, out string? error3));

            Assert.Equal(expected, _notifier.Snapshots[^1].CourseEndsAt);
        }
    }
}