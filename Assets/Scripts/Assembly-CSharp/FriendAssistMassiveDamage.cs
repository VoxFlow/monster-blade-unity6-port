using UnityEngine;

public class FriendAssistMassiveDamage : FriendAssistAttack
{
	protected DamageData m_massiveDamageData = new DamageData();

	protected MonsterDamageLogic m_monsterDamageLogic;

	public override SkillEnum.AssistSkill assistSkillType
	{
		get
		{
			return SkillEnum.AssistSkill.MassiveDamage;
		}
	}

	public override void OnSkillTrigger()
	{
		if (m_massiveDamageData != null && base.friendData != null && base.monster != null)
		{
			m_massiveDamageData.attacker = base.friendData;
			m_massiveDamageData.damageType = DamageType.Critical;
			m_massiveDamageData.rawDamage = (m_massiveDamageData.finalDamage = base.friendData.FriendAssistValue);
			m_massiveDamageData.defender = base.monster.Data;
			PlayHitReaction();
			Messenger<DamageData>.Invoke("monster_receive_dmg", m_massiveDamageData);
			m_monsterDamageLogic.DealDamageToMonster(m_massiveDamageData);
		}
	}

	protected override void OnBattleReady()
	{
		base.OnBattleReady();
		GameObject gameObject = GameObject.Find("BattleLogic");
		if (gameObject != null)
		{
			m_monsterDamageLogic = gameObject.GetComponent<MonsterDamageLogic>();
		}
	}
}
