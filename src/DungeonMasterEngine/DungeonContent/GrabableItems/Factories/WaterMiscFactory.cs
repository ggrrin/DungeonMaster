using System.Collections.Generic;
using DungeonMasterEngine.DungeonContent.Actions.Factories;
using DungeonMasterEngine.DungeonContent.Entity.BodyInventory.Base;
using DungeonMasterEngine.DungeonContent.GrabableItems.Misc;
using DungeonMasterEngine.DungeonContent.Tiles.Renderers;

namespace DungeonMasterEngine.DungeonContent.GrabableItems.Factories
{
    public class WaterMiscFactory : MiscItemFactory
    {
        public int WaterValuePerCharge { get; }
        public WaterMiscFactory(int waterValuePerCharge, string name, int weight, IEnumerable<IActionFactory> attackCombo, IEnumerable<IStorageType> carryLocation, ITextureRenderer renderer) : base(name, weight, attackCombo, carryLocation, renderer)
        {
            WaterValuePerCharge = waterValuePerCharge;
        }

        public override Miscellaneous Create<TItemInitiator>(TItemInitiator initiator)
        {
            return new WaterFlaskMisc(initiator, this); 
        }
    }
}