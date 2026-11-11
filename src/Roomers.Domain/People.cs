namespace Roomers.Domain
{
    public class People
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        private PeopleType _type;

        public People(Guid id, string name, PeopleType type)
        {
            Id = id;
            Name = name;
            _type = type;
        }

        public bool IsTeacher()
        {
            return _type == PeopleType.Teacher;
        }
    }
}
