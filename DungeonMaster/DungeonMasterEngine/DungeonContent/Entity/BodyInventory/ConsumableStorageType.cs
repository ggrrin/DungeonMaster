using DungeonMasterEngine.DungeonContent.Entity.BodyInventory.Base;

namespace DungeonMasterEngine.DungeonContent.Entity.BodyInventory
{
    internal class ConsumableStorageType : IStorageType
    {
        public int Size { get; } = 1;

        public static ConsumableStorageType  Instance { get; } = new ConsumableStorageType();

        private ConsumableStorageType() { }
    }
}