using System;

[Serializable]
public class SkillData : ShareData
{
	private bool m_skillUsable;

	public bool isSkillUsable
	{
		get
		{
			return m_skillUsable;
		}
		private set
		{
			bool skillUsable = m_skillUsable;
			m_skillUsable = value;
			if (skillUsable != value && onUsableCallback != null)
			{
				onUsableCallback(value);
			}
		}
	}

	public Action<bool> onUsableCallback { get; set; }

	public void EnableSkill(object p_key)
	{
		if (!base.mutex.locked || base.mutex.holdingKey == p_key)
		{
			isSkillUsable = true;
		}
		Unlock(p_key);
	}

	public void DisableSkill(object p_key)
	{
		Lock(p_key, OnReceiveLockDisable);
	}

	protected virtual void OnReceiveLockDisable(ShareData p_this)
	{
		isSkillUsable = false;
	}
}
