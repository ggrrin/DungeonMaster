using DungeonMasterEngine.DungeonContent.Actions;

namespace DungeonMasterEngine.DungeonContent.Projectiles.Impacts
{
    public class HarmNonMaterialExplosionImpact : ExplosionImpact
    {

        public override bool CreatesExplosion => true;

        protected override AttackInfo GetDefaultImpact(Projectile projectile)
        {
            //TODO what about damage ?? original code in Impact base class
            return new AttackInfo
            {
                Type = CreatureAttackType.C5_ATTACK_MAGIC
            };
        }

        public override void FinishImpact(Projectile projectile)
        {
            //centered only poison cloud
            var attack = projectile.KineticEnergy;
            var location = projectile.Location;
            //send event ==>

            F191_aayz_GROUP_GetDamageAllCreaturesOutcome(attack, location);
        }
    }
}