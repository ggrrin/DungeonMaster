namespace DungeonMasterEngine.DungeonContent.Entity.Skills.Base
{
    public class SkillFactory<TSkill> : ISkillFactory where TSkill : ISkill
    {
        public static SkillFactory<TSkill> Instance { get; } = new SkillFactory<TSkill>();

        private SkillFactory()
        { }
    }
}