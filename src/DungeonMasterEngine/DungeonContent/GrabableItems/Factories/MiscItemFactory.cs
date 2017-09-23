using System.Collections.Generic;
using DungeonMasterEngine.Builders.ItemCreators;
using DungeonMasterEngine.DungeonContent.Actions.Factories;
using DungeonMasterEngine.DungeonContent.Entity.BodyInventory.Base;
using DungeonMasterEngine.DungeonContent.GrabableItems.Initializers;
using DungeonMasterEngine.DungeonContent.GrabableItems.Misc;
using DungeonMasterEngine.DungeonContent.Tiles.Renderers;

namespace DungeonMasterEngine.DungeonContent.GrabableItems.Factories
{
    public class MiscItemFactory : GrabableItemFactoryBase
    {
        public MiscItemFactory(string name, int weight, IEnumerable<IActionFactory> attackCombo, IEnumerable<IStorageType> carryLocation, ITextureRenderer renderer) : base(name, weight, attackCombo, carryLocation, renderer)
        {
            
        }

        public virtual Miscellaneous Create<TItemInitiator>(TItemInitiator initiator) where TItemInitiator : IMiscInitializer
        {
            return new Miscellaneous(initiator, this);
        }

        public override IGrabableItem CreateItem()
        {
            return Create(new MiscInitializer());
        }
    }
}