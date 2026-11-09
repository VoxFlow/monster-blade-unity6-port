public class SkillVisual
{
	public static SkillVisual NONE_SKILL_VISUAL = new SkillVisual();

	public readonly ItemEnum.Skill skill = ItemEnum.Skill.NONE;

	public readonly string label_self = string.Empty;

	public readonly string description_self = string.Empty;

	public readonly string sprite_self = string.Empty;

	public readonly string label_assist = string.Empty;

	public readonly string description_assist = string.Empty;

	public readonly string sprite_assist = string.Empty;

	private SkillVisual()
	{
	}

	public SkillVisual(ItemEnum.Skill _skill, string _label_self, string _description_self, string _sprite_self, string _label_assist, string _description_assist, string _sprite_assist)
	{
		skill = _skill;
		label_self = _label_self;
		description_self = _description_self;
		sprite_self = _sprite_self;
		label_assist = _label_assist;
		description_assist = _description_assist;
		sprite_assist = _sprite_assist;
	}

	public override string ToString()
	{
		return string.Format("[SkillVisual]\n{0}  label_s:{1}  desc_s:{2}  sprite_s:{3}\nlabel_a:{4}  desc_a:{5}  sprite_a:{6}\n", skill, label_self, description_self, sprite_self, label_self, description_self, sprite_self);
	}
}
