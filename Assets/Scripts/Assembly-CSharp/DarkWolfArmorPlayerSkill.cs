using UnityEngine;

public class DarkWolfArmorPlayerSkill : WolfArmorPlayerSkill
{
	[SerializeField]
	private float m_monsterSloMoPercent = 0.25f;

	protected override void OnExtremeModeEnd()
	{
		base.skillEffectOn = false;
		SetPlayerStatModifier(m_exhaustMod, m_exhaustMod, m_exhaustMod);
		if (null != base.monster)
		{
			base.monster.AnimPlayer.TimeScale = 1f;
		}
	}

	protected override void OnTriggerExtremeMode()
	{
		base.OnTriggerExtremeMode();
		if (null != base.monster)
		{
			base.monster.AnimPlayer.TimeScale = m_monsterSloMoPercent;
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (null != SingletonComponent<SuperAttackSystem>.Instance)
		{
			SingletonComponent<SuperAttackSystem>.Instance.ChangeSuperAttack(typeof(OmniSlash));
		}
	}

	protected override void OnBattleEnd()
	{
		base.OnBattleEnd();
		OnExtremeModeEnd();
	}
}
