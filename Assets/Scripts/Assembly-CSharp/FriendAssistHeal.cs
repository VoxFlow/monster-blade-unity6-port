public class FriendAssistHeal : FriendAssistSkill
{
	public override SkillEnum.AssistSkill assistSkillType
	{
		get
		{
			return SkillEnum.AssistSkill.Heal;
		}
	}

	public override void OnSkillTrigger()
	{
		if (base.player != null)
		{
			base.player.RefillHp(base.friendData.FriendAssistValue);
		}
	}
}
