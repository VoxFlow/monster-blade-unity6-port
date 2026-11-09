public class FriendAssistCriticalBuff : FriendAssistBuff
{
	private float m_defaultCriticalChance;

	public override SkillEnum.AssistSkill assistSkillType
	{
		get
		{
			return SkillEnum.AssistSkill.BuffCritical;
		}
	}

	protected override void ApplyBuff(PlayerData p_playerData)
	{
		if (base.friendData != null)
		{
			float friendAssistValue = base.friendData.FriendAssistValue;
			Dbg.Log("Friend Assist Critical Buf: " + friendAssistValue);
			m_defaultCriticalChance = p_playerData.CriticalChance;
			p_playerData.CriticalChance += friendAssistValue;
		}
	}

	protected override void ResetBuff(PlayerData p_playerData)
	{
		if (base.friendData != null)
		{
			p_playerData.CriticalChance = m_defaultCriticalChance;
		}
	}
}
