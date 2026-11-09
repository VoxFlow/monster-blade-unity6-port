public class ItemSkill
{
	public readonly ItemEnum.Skill type = ItemEnum.Skill.NONE;

	public readonly int index = 1;

	public readonly int actionPerAssist = 1;

	public readonly SkillEnum.SelfSkill selfAction = SkillEnum.SelfSkill.None;

	public readonly float directValue;

	public readonly SkillEnum.AssistSkill assistAction = SkillEnum.AssistSkill.None;

	public readonly float assistValue;

	public readonly SkillVisual visual = SkillVisual.NONE_SKILL_VISUAL;

	public ItemSkill()
	{
	}

	public ItemSkill(ItemEnum.Skill _type, int _index, int _actionPerAssist = 1, SkillEnum.SelfSkill _selfAction = SkillEnum.SelfSkill.None, float _directValue = 0f, SkillEnum.AssistSkill _assistAction = SkillEnum.AssistSkill.None, float _assistValue = 0f, SkillVisual _visual = null)
	{
		type = _type;
		index = _index;
		actionPerAssist = _actionPerAssist;
		selfAction = _selfAction;
		directValue = _directValue;
		assistAction = _assistAction;
		assistValue = _assistValue;
		if (_visual == null)
		{
			visual = SkillVisual.NONE_SKILL_VISUAL;
		}
		else
		{
			visual = _visual;
		}
	}

	public override string ToString()
	{
		return string.Format("[Skill]\ntype:{0}  idx:{1}  PerAssist:{2}  selfAct:{3}  directValue:{4}  AssistAct:{5}  AssistValue:{6}\n", type, index, actionPerAssist, selfAction, directValue, assistAction, assistValue);
	}
}
