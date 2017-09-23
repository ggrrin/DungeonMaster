using DungeonMasterEngine.DungeonContent.Entity.Skills.Base;

namespace DungeonMasterEngine.DungeonContent.Entity.Skills
{
    public class EmptySkill : ISkill {
        public ISkillFactory Type { get; }
        public SkillBase BaseSkill { get; }
        public long Experience { get; }
        public int TemporaryExperience { get; set; }
        public int SkillLevel { get; }
        public int BaseSkillLevel { get; }

        public void AddExperience(int experience)
        {
        }
    }
}