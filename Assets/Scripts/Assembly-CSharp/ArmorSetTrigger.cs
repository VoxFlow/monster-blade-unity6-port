using System;
using UnityEngine;

public class ArmorSetTrigger : BattleLogic
{
	[SerializeField]
	protected bool m_oneTimeEvent = true;

	private ArmorSetPlayerSkill m_skillLogic;

	private bool m_isConditionMet;

	public Action callbackOnSkillTrigger;

	protected override void Awake()
	{
		base.Awake();
		m_skillLogic = base.gameObject.GetComponent<ArmorSetPlayerSkill>();
	}

	private void OnEnable()
	{
		if (null == m_skillLogic || (m_oneTimeEvent && m_isConditionMet))
		{
			base.enabled = false;
		}
	}

	protected void OnConditionMet()
	{
		if (base.enabled && (!m_oneTimeEvent || !m_isConditionMet))
		{
			m_isConditionMet = true;
			if (m_oneTimeEvent)
			{
				base.enabled = false;
			}
			m_skillLogic.TriggerSkill();
			if (callbackOnSkillTrigger != null)
			{
				callbackOnSkillTrigger();
			}
		}
	}
}
