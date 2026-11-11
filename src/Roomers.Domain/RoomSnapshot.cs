namespace Roomers.Domain
{
    public record PersonInfo(Guid Id, string Name);

    public record RoomSnapshot(
        string Name,
        int MaxCapacity,
        bool IsLocked,
        PersonInfo? Teacher,
        IReadOnlyList<PersonInfo> Students);
}