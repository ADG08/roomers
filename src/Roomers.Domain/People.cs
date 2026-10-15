namespace Roomers.Domain
{
    public class People
    {
        public Guid Id { get; private set; }
        private string _name = string.Empty;
        private PeopleType _type;

        public People(Guid id, string name, PeopleType type)
        {
            Id = id;
            _name = name;
            _type = type;
        }

        public bool IsTeacher()
        {
            return _type == PeopleType.Teacher;
        }
    }
}
