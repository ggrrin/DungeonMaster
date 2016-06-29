using DungeonMasterEngine.DungeonContent.Entity.BodyInventory.Base;

namespace DungeonMasterEngine.DungeonContent.Entity.BodyInventory
{
    internal class BackPackStorageType : IStorageType
    {
        public int Size { get; } = 17;

        public static BackPackStorageType Instance { get; } = new BackPackStorageType();

        private BackPackStorageType() { }
    }
}