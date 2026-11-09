public class FriendAssistDmgBuff : FriendAssistBuff
{
	private float m_defaultMinAtk;

	private float m_defaultMaxAtk;

	public override SkillEnum.AssistSkill assistSkillType
	{
		get
		{
			return SkillEnum.AssistSkill.BuffDamage;
		}
	}

	protected override void ApplyBuff(PlayerData p_playerData)
	{
		if (base.friendData != null)
		{
			float friendAssistValue = base.friendData.FriendAssistValue;
			Dbg.Log("Friend Assist Damage Buf: " + friendAssistValue);
			m_defaultMinAtk = p_playerData.MinAttack;
			m_defaultMaxAtk = p_playerData.MaxAttack;
			p_playerData.MaxAttack += friendAssistValue;
			p_playerData.MinAttack += friendAssistValue;
		}
	}

	protected override void ResetBuff(PlayerData p_playerData)
	{
		if (base.friendData != null)
		{
			p_playerData.MaxAttack = m_defaultMinAtk;
			p_playerData.MinAttack = m_defaultMaxAtk;
		}
	}
}
