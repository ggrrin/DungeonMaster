using System.Collections.Generic;
using DungeonMasterEngine.Builders.ItemInitializers;
using DungeonMasterEngine.DungeonContent.Entity.Actions.Factories;
using DungeonMasterEngine.DungeonContent.Entity.BodyInventory.@base;
using DungeonMasterEngine.DungeonContent.GrabableItems.Initializers;
using Microsoft.Xna.Framework.Graphics;

namespace DungeonMasterEngine.DungeonContent.GrabableItems.Factories
{
    public class PotionItemFactory : GrabableItemFactoryBase
    {
        public Potion Create<TItemInitializator>(TItemInitializator initializator) where TItemInitializator : IPotionInitializer
        {
            return new Potion(initializator, this);
        }

        public PotionItemFactory(string name, int weight, IEnumerable<IActionFactory> attackCombo, IEnumerable<IStorageType> carryLocation, Texture2D texture) : base(name, weight, attackCombo, carryLocation, texture)
        { }

        public override IGrabableItem Create()
        {
            return Create(new PotionInitializer { PotionPower = 255 });
        }
    }
}