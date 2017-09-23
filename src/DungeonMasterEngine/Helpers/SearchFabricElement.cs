using DungeonMasterEngine.DungeonContent.Tiles.Support;

namespace DungeonMasterEngine.Helpers
{
    struct SearchFabricElement<TTile, TBundle> where TTile : INeighbourable<TTile>
    {
        public int Flag;
        public int Layer;
        public TTile PreviousTile;
        public TBundle Bundle;
    }
}