using System;
using UnityEngine;

[RequireComponent(typeof(UIImageButton))]
public class FriendAssistButton : MonoBehaviour
{
	protected UIImageButton m_button;

	protected FriendAssistSkill m_skill;

	public FriendAssistSkill Skill
	{
		get
		{
			return m_skill;
		}
		set
		{
			if (null != m_skill)
			{
				SkillData skillData = m_skill.skillData;
				skillData.onUsableCallback = (Action<bool>)Delegate.Remove(skillData.onUsableCallback, new Action<bool>(OnSkillEnableOrDisable));
			}
			m_skill = value;
			if (null != m_skill)
			{
				SkillData skillData2 = m_skill.skillData;
				skillData2.onUsableCallback = (Action<bool>)Delegate.Combine(skillData2.onUsableCallback, new Action<bool>(OnSkillEnableOrDisable));
			}
			Refresh();
		}
	}

	protected void Awake()
	{
		m_button = base.gameObject.GetComponent<UIImageButton>();
		if (null != SingletonComponent<SkillSystem>.Instance)
		{
			SkillSystem instance = SingletonComponent<SkillSystem>.Instance;
			instance.onFriendAssistSkillChangedCallback = (Action<FriendAssistSkill>)Delegate.Combine(instance.onFriendAssistSkillChangedCallback, new Action<FriendAssistSkill>(OnSkillChanged));
		}
	}

	protected void OnDestroy()
	{
		if (null != SingletonComponent<SkillSystem>.Instance)
		{
			SkillSystem instance = SingletonComponent<SkillSystem>.Instance;
			instance.onFriendAssistSkillChangedCallback = (Action<FriendAssistSkill>)Delegate.Remove(instance.onFriendAssistSkillChangedCallback, new Action<FriendAssistSkill>(OnSkillChanged));
		}
	}

	protected void OnEnable()
	{
		Refresh();
	}

	protected void OnDisable()
	{
		if (base.gameObject.active)
		{
			Refresh();
		}
	}

	public void Refresh()
	{
		if (null != m_skill && m_skill.assistSkillType != SkillEnum.AssistSkill.None && m_skill.skillData.isSkillUsable)
		{
			if (m_skill.enabled && !string.IsNullOrEmpty(m_skill.spriteName))
			{
				m_button.hoverSprite = m_skill.spriteName;
				m_button.normalSprite = m_skill.spriteName;
				m_button.pressedSprite = m_skill.spriteName;
				m_button.target.spriteName = m_skill.spriteName;
			}
			else if (!m_skill.enabled && !string.IsNullOrEmpty(m_skill.darkSpriteName))
			{
				m_button.hoverSprite = m_skill.darkSpriteName;
				m_button.normalSprite = m_skill.darkSpriteName;
				m_button.pressedSprite = m_skill.darkSpriteName;
				m_button.target.spriteName = m_skill.darkSpriteName;
			}
			else
			{
				base.gameObject.SetActiveRecursively(false);
			}
		}
		else
		{
			base.gameObject.SetActiveRecursively(false);
		}
	}

	private void OnSkillChanged(FriendAssistSkill p_skill)
	{
		Skill = p_skill;
	}

	private void OnSkillEnableOrDisable(bool p_enable)
	{
		base.gameObject.SetActiveRecursively(p_enable);
	}
}
