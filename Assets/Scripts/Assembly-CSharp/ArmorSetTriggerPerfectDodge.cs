using UnityEngine;

public class ArmorSetTriggerPerfectDodge : ArmorSetTrigger
{
	[SerializeField]
	protected int m_triggerCondition = 5;

	private int m_count;

	public int triggerCondition
	{
		get
		{
			return m_triggerCondition;
		}
	}

	public int counter
	{
		get
		{
			return m_count;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Messenger<Player.DodgeDirection>.AddListener("perfect_dodge_start", OnPerfectDodge);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Messenger<Player.DodgeDirection>.RemoveListener("perfect_dodge_start", OnPerfectDodge);
	}

	private void OnPerfectDodge(Player.DodgeDirection p_dir)
	{
		if (base.enabled)
		{
			m_count++;
			if (m_triggerCondition == m_count)
			{
				OnConditionMet();
				m_count = 0;
			}
		}
	}
}
