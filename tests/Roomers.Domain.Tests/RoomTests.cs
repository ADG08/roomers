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
    }
}
