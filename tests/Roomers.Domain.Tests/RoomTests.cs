namespace Roomers.Domain.Tests
{
    public class RoomTests
    {
        const int Capacity = 15;
        private readonly Room _room = new Room("Salle 1", Capacity);

        [Fact]
        public void JoinRoom_NullPeople_ReturnsFalse()
        {
            People? people = null;
            Assert.False(_room.JoinRoom(people));
        }

        [Fact]
        public void JoinRoom_SamePersonTwice_ReturnsFalse()
        {
            People student = new People(Guid.NewGuid(), "Andre Domingues Ramos", PeopleType.Student);

            Assert.True(_room.JoinRoom(student));
            Assert.False(_room.JoinRoom(student));
        }

        [Fact]
        public void JoinRoom_RoomFull_ReturnsFalse()
        {
            People teacher = new People(Guid.NewGuid(), "Professeur Morin", PeopleType.Teacher);
            Assert.True(_room.JoinRoom(teacher));

            for (int i = 0; i < Capacity - 1; i++)
            {
                People p = new People(Guid.NewGuid(), $"Student {i}", PeopleType.Student);
                Assert.True(_room.JoinRoom(p));
            }

            People student = new People(Guid.NewGuid(), "Andre Domingues Ramos", PeopleType.Student);
            Assert.False(_room.JoinRoom(student));
        }

        [Fact]
        public void JoinRoom_StudentTakingLastSeatWithoutTeacher_ReturnsFalse()
        {
            for (int i = 0; i < Capacity - 1; i++)
            {
                People p = new People(Guid.NewGuid(), $"Student {i}", PeopleType.Student);
                Assert.True(_room.JoinRoom(p));
            }

            People student = new People(Guid.NewGuid(), "Andre Domingues Ramos", PeopleType.Student);
            Assert.False(_room.JoinRoom(student));
        }

        [Fact]
        public void JoinRoom_TeacherInEmptyRoom_ReturnsTrue()
        {
            People teacher = new People(Guid.NewGuid(), "Professeur Morin", PeopleType.Teacher);

            Assert.True(_room.JoinRoom(teacher));
        }

        [Fact]
        public void JoinRoom_SecondTeacher_ReturnsFalse()
        {
            People teacher = new People(Guid.NewGuid(), "Professeur Morin", PeopleType.Teacher);
            People teacher2 = new People(Guid.NewGuid(), "Professeur Dubois", PeopleType.Teacher);

            Assert.True(_room.JoinRoom(teacher));
            Assert.False(_room.JoinRoom(teacher2));
        }

        [Fact]
        public void JoinRoom_StudentInEmptyRoom_ReturnsTrue()
        {
            People student = new People(Guid.NewGuid(), "Andre Domingues Ramos", PeopleType.Student);
            Assert.True(_room.JoinRoom(student));
        }

        [Fact]
        public void Constructor_CapacityBelowMinimum_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Room("Salle 2", 5));
        }

        [Fact]
        public void StartCourse_TeacherInside_ReturnsTrueAndLocksRoom()
        {
            People teacher = new People(Guid.NewGuid(), "Professeur Morin", PeopleType.Teacher);
            Assert.True(_room.JoinRoom(teacher));

            People student = new People(Guid.NewGuid(), "Andre Domingues Ramos", PeopleType.Student);
            Assert.True(_room.JoinRoom(student));

            Assert.True(_room.StartCourse(teacher.Id));
            Assert.True(_room.IsLocked());

        }

        [Fact]
        public void StartCourse_Student_ReturnsFalse()
        {
            People student = new People(Guid.NewGuid(), "Andre Domingues Ramos", PeopleType.Student);
            Assert.True(_room.JoinRoom(student));

            People p = new People(Guid.NewGuid(), $"Student 1", PeopleType.Student);
            Assert.True(_room.JoinRoom(p));

            Assert.False(_room.StartCourse(student.Id));
            Assert.False(_room.IsLocked());
        }

        [Fact]
        public void StartCourse_AlreadyStarted_ReturnsFalse()
        {
            People teacher = new People(Guid.NewGuid(), "Professeur Morin", PeopleType.Teacher);
            Assert.True(_room.JoinRoom(teacher));

            People student = new People(Guid.NewGuid(), "Andre Domingues Ramos", PeopleType.Student);
            Assert.True(_room.JoinRoom(student));

            Assert.True(_room.StartCourse(teacher.Id));
            Assert.True(_room.IsLocked());

            Assert.False(_room.StartCourse(teacher.Id));
        }

        [Fact]
        public void EndCourse_AfterStart_RemovesStudentsKeepsTeacher()
        {
            People teacher = new People(Guid.NewGuid(), "Professeur Morin", PeopleType.Teacher);
            Assert.True(_room.JoinRoom(teacher));

            People student = new People(Guid.NewGuid(), "Andre Domingues Ramos", PeopleType.Student);
            Assert.True(_room.JoinRoom(student));
            Assert.False(_room.JoinRoom(student));


            Assert.True(_room.StartCourse(teacher.Id));
            Assert.True(_room.IsLocked());

            Assert.True(_room.EndCourse(teacher.Id));
            Assert.False(_room.IsLocked());

            Assert.False(_room.JoinRoom(teacher));
            Assert.True(_room.JoinRoom(student));
        }

        [Fact]
        public void EndCourse_NotStarted_ReturnsFalse()
        {
            People teacher = new People(Guid.NewGuid(), "Professeur Morin", PeopleType.Teacher);
            Assert.True(_room.JoinRoom(teacher));

            People student = new People(Guid.NewGuid(), "Andre Domingues Ramos", PeopleType.Student);
            Assert.True(_room.JoinRoom(student));

            Assert.False(_room.EndCourse(teacher.Id));
        }

        [Fact]
        public void StartCourse_TeacherAlone_ReturnsFalse()
        {
            People teacher = new People(Guid.NewGuid(), "Professeur Morin", PeopleType.Teacher);
            Assert.True(_room.JoinRoom(teacher));

            Assert.False(_room.StartCourse(teacher.Id));
        }

        [Fact]
        public void StartCourse_UnknownId_ReturnsFalse()
        {
            People teacher = new People(Guid.NewGuid(), "Professeur Morin", PeopleType.Teacher);
            Assert.True(_room.JoinRoom(teacher));

            People student = new People(Guid.NewGuid(), "Andre Domingues Ramos", PeopleType.Student);
            Assert.True(_room.JoinRoom(student));

            Assert.False(_room.StartCourse(Guid.NewGuid()));
        }

        [Fact]
        public void JoinRoom_DuringCourse_ReturnsFalse()
        {
            People teacher = new People(Guid.NewGuid(), "Professeur Morin", PeopleType.Teacher);
            Assert.True(_room.JoinRoom(teacher));

            People student = new People(Guid.NewGuid(), "Andre Domingues Ramos", PeopleType.Student);
            Assert.True(_room.JoinRoom(student));

            Assert.True(_room.StartCourse(teacher.Id));
            Assert.True(_room.IsLocked());

            People p = new People(Guid.NewGuid(), $"Student 1", PeopleType.Student);
            Assert.False(_room.JoinRoom(p));
        }

        [Fact]
        public void EndCourse_Student_ReturnsFalse()
        {
            People teacher = new People(Guid.NewGuid(), "Professeur Morin", PeopleType.Teacher);
            Assert.True(_room.JoinRoom(teacher));

            People student = new People(Guid.NewGuid(), "Andre Domingues Ramos", PeopleType.Student);
            Assert.True(_room.JoinRoom(student));

            Assert.True(_room.StartCourse(teacher.Id));
            Assert.True(_room.IsLocked());

            Assert.False(_room.EndCourse(student.Id));
        }

        [Fact]
        public void EndCourse_UnknownId_ReturnsFalse()
        {
            People teacher = new People(Guid.NewGuid(), "Professeur Morin", PeopleType.Teacher);
            Assert.True(_room.JoinRoom(teacher));

            People student = new People(Guid.NewGuid(), "Andre Domingues Ramos", PeopleType.Student);
            Assert.True(_room.JoinRoom(student));

            Assert.True(_room.StartCourse(teacher.Id));
            Assert.True(_room.IsLocked());

            Assert.False(_room.EndCourse(Guid.NewGuid()));
        }

        [Fact]
        public void LeaveRoom_PersonInside_ReturnsTrue()
        {
            People teacher = new People(Guid.NewGuid(), "Professeur Morin", PeopleType.Teacher);
            Assert.True(_room.JoinRoom(teacher));

            People student = new People(Guid.NewGuid(), "Andre Domingues Ramos", PeopleType.Student);
            Assert.True(_room.JoinRoom(student));

            Assert.True(_room.LeaveRoom(student.Id));
            Assert.False(_room.LeaveRoom(student.Id));
        }

        [Fact]
        public void LeaveRoom_PersonNotInside_ReturnsFalse()
        {
            People teacher = new People(Guid.NewGuid(), "Professeur Morin", PeopleType.Teacher);
            Assert.True(_room.JoinRoom(teacher));

            People student = new People(Guid.NewGuid(), "Andre Domingues Ramos", PeopleType.Student);
            Assert.True(_room.JoinRoom(student));

            Assert.False(_room.LeaveRoom(Guid.NewGuid()));
        }

        [Fact]
        public void LeaveRoom_DuringCourse_ReturnsTrue()
        {
            People teacher = new People(Guid.NewGuid(), "Professeur Morin", PeopleType.Teacher);
            Assert.True(_room.JoinRoom(teacher));

            People student = new People(Guid.NewGuid(), "Andre Domingues Ramos", PeopleType.Student);
            Assert.True(_room.JoinRoom(student));

            Assert.True(_room.StartCourse(teacher.Id));
            Assert.True(_room.IsLocked());

            Assert.True(_room.LeaveRoom(student.Id));
        }
    }
}
