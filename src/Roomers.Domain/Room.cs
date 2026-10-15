namespace Roomers.Domain
{
    public class Room
    {
        private readonly string _name = string.Empty;
        private RoomMode _mode = RoomMode.Open;
        private readonly int _maxCapacity = 0;
        private readonly List<People> _peoples = new List<People>();

        public Room(string name, int maxCapacity)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            _name = name;

            if (maxCapacity < 10)
                throw new ArgumentOutOfRangeException(nameof(maxCapacity));

            _maxCapacity = maxCapacity;
        }

        public bool JoinRoom(People p)
        {
            if (p == null)
                return false;

            if (this._mode == RoomMode.Locked)
                return false;

            if (FindPeople(p.Id) != null)
                return false;

            if (_peoples.Count >= _maxCapacity)
                return false;

            bool hasTeacher = HasTeacher();
            if (hasTeacher && p.IsTeacher())
                return false;

            if (_peoples.Count == _maxCapacity - 1 && !hasTeacher && !p.IsTeacher())
                return false;

            _peoples.Add(p);
            return true;
        }

        public bool LeaveRoom(Guid guid)
        {
            if (guid == Guid.Empty)
                return false;

            People? peopleToRemove = FindPeople(guid);
            
            if (peopleToRemove == null)
                return false;

            _peoples.Remove(peopleToRemove);
            return true;
        }

        private bool HasTeacher()
        {
            foreach (People p in _peoples)
            {
                if (p.IsTeacher())
                {
                    return true;
                }
            }
            return false;
        }

        private People? FindPeople(Guid guid)
        {
            foreach (People pe in _peoples)
            {
                if (pe.Id == guid)
                {
                    return pe;
                }
            }

            return null;
        }
    }
}
