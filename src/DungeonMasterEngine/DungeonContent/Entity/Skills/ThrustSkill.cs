using DungeonMasterEngine.DungeonContent.Entity.Skills.Base;

namespace DungeonMasterEngine.DungeonContent.Entity.Skills
{
    public class ThrustSkill : HiddenSkill {

        public override ISkillFactory Type => SkillFactory<ThrustSkill>.Instance;

        public ThrustSkill(ILiveEntity liveEntity, SkillBase baseSkill, int skillLevel) : base(liveEntity, baseSkill, skillLevel) {}
    }
}