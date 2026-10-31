using Roomers.Domain;

namespace Roomers.Server.Tests
{
    public class RoomRegistryTests
    {
        [Fact]
        public void TryGet_ConfiguredRoom_ReturnsRoom()
        {
            RoomRegistry registry = new RoomRegistry(new[] { new RoomConfig("Salle 1", 15) });

            bool found = registry.TryGet("Salle 1", out Room? room);

            Assert.True(found);
            Assert.NotNull(room);
        }

        [Fact]
        public void TryGet_UnknownRoom_ReturnsFalse()
        {
            RoomRegistry registry = new RoomRegistry(new[] { new RoomConfig("Salle 1", 15) });

            bool found = registry.TryGet("Salle 2", out Room? room);

            Assert.False(found);
            Assert.Null(room);
        }

        [Fact]
        public void TryGet_CalledTwice_ReturnsSameRoom()
        {
            RoomRegistry registry = new RoomRegistry(new[] { new RoomConfig("Salle 1", 15) });

            registry.TryGet("Salle 1", out Room? first);
            registry.TryGet("Salle 1", out Room? second);

            Assert.Same(first, second);
        }

        [Fact]
        public void Constructor_DuplicateRoomNames_Throws()
        {
            RoomConfig[] configs =
            {
                new RoomConfig("Salle 1", 15),
                new RoomConfig("Salle 1", 20)
            };

            Assert.Throws<ArgumentException>(() => new RoomRegistry(configs));
        }

        [Fact]
        public void Constructor_InvalidCapacity_Throws()
        {
            RoomConfig[] configs = { new RoomConfig("Salle 1", 5) };

            Assert.Throws<ArgumentOutOfRangeException>(() => new RoomRegistry(configs));
        }
    }
}