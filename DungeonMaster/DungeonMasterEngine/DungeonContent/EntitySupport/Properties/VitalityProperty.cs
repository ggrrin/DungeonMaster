using DungeonMasterEngine.DungeonContent.EntitySupport.Properties.@base;

namespace DungeonMasterEngine.DungeonContent.EntitySupport.Properties
{
    internal class VitalityProperty : Property {
        public override int BaseValue { get; set; }
        public override IPropertyFactory Type { get; }
    }
}