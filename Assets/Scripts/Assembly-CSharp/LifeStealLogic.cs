using UnityEngine;

public class LifeStealLogic : PlayerSkill
{
	private float m_lifeStealValue;

	private PlayerLifeStealFx m_fx;

	public override SkillEnum.SelfSkill selfSkillType
	{
		get
		{
			return SkillEnum.SelfSkill.LifeSteal;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Messenger<DamageData>.AddListener("monster_receive_dmg", OnMonsterReceiveDamage);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Messenger<DamageData>.RemoveListener("monster_receive_dmg", OnMonsterReceiveDamage);
	}

	protected override void OnBattleReady()
	{
		base.OnBattleReady();
		m_lifeStealValue = 0f;
		if (base.player != null && base.player.Data != null)
		{
			SkillEnum.SelfSkill[] selfSkills = base.player.Data.SelfSkills;
			float[] selfSkillValues = base.player.Data.SelfSkillValues;
			for (int i = 0; i < selfSkills.Length; i++)
			{
				if (selfSkills[i] == SkillEnum.SelfSkill.LifeSteal)
				{
					m_lifeStealValue += selfSkillValues[i];
				}
			}
		}
		if (m_lifeStealValue == 0f)
		{
			base.enabled = false;
		}
		else
		{
			m_fx = InitLifeStealEffect(base.player.gameObject);
		}
	}

	public override bool DoesCharacterHaveSkill(PlayerData p_data)
	{
		return p_data.GetSkillIndex(SkillEnum.SelfSkill.LifeSteal) > -1;
	}

	protected virtual void OnMonsterReceiveDamage(DamageData p_dmg)
	{
		if (!base.enabled || m_lifeStealValue == 0f || p_dmg.damageType == DamageType.None || p_dmg.damageType == DamageType.Special || p_dmg.attacker != base.player.Data)
		{
			return;
		}
		float num = m_lifeStealValue * p_dmg.finalDamage * 0.01f;
		if ((int)num > 0)
		{
			base.player.DrainLife(num);
			if (m_fx != null)
			{
				m_fx.Play();
			}
		}
	}

	private PlayerLifeStealFx InitLifeStealEffect(GameObject p_character)
	{
		if (p_character == null)
		{
			return null;
		}
		PlayerLifeStealFx playerLifeStealFx = p_character.GetComponent<PlayerLifeStealFx>();
		if (playerLifeStealFx == null)
		{
			playerLifeStealFx = p_character.AddComponent<PlayerLifeStealFx>();
		}
		return playerLifeStealFx;
	}
}
