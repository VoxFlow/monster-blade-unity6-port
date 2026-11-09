using System.Collections;
using UnityEngine;

public abstract class FriendAssistSkill : Skill
{
	[SerializeField]
	protected CutsceneController.CutsceneType m_cutsceneEnum = CutsceneController.CutsceneType.NONE;

	[SerializeField]
	protected string m_spriteName = string.Empty;

	[SerializeField]
	protected string m_darkSpriteName = string.Empty;

	[SerializeField]
	private FriendAssistData m_skillData;

	private GameObject m_friend;

	private PlayerData m_friendData;

	public abstract SkillEnum.AssistSkill assistSkillType { get; }

	public virtual string spriteName
	{
		get
		{
			return m_spriteName;
		}
	}

	public virtual string darkSpriteName
	{
		get
		{
			return m_darkSpriteName;
		}
	}

	protected GameObject friend
	{
		get
		{
			if (m_friend == null)
			{
				StageManager instance = SingletonComponent<StageManager>.Instance;
				if (instance != null)
				{
					m_friend = instance.FriendObjects[0];
				}
			}
			return m_friend;
		}
		private set
		{
			m_friend = value;
		}
	}

	protected PlayerData friendData
	{
		get
		{
			if (m_friendData == null)
			{
				StageManager instance = SingletonComponent<StageManager>.Instance;
				if (instance != null)
				{
					m_friendData = instance.GetFriendData(0);
				}
			}
			return m_friendData;
		}
		private set
		{
			m_friendData = value;
		}
	}

	public CutsceneController.CutsceneType cutsceneEnum
	{
		get
		{
			return m_cutsceneEnum;
		}
	}

	public SkillData skillData
	{
		get
		{
			return m_skillData;
		}
	}

	public abstract void OnSkillTrigger();

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public override bool DoesCharacterHaveSkill(PlayerData p_data)
	{
		if (p_data == null)
		{
			return false;
		}
		return p_data.FriendAssistSkill == assistSkillType;
	}

	public virtual void Trigger()
	{
		if (CanTriggerSkill())
		{
			StartCoroutine(PlayCutscene());
		}
	}

	protected virtual bool CanTriggerSkill()
	{
		return base.enabled && friendData != null && base.player != null && base.monster != null && base.player.Data.Hp > 0f && base.monster.Data.Hp > 0f && m_skillData.isSkillUsable;
	}

	protected IEnumerator PlayCutscene()
	{
		CutsceneController cutsceneController = SingletonComponent<CutsceneController>.Instance;
		if (!(cutsceneController == null))
		{
			yield return StartCoroutine(cutsceneController.PlayCutscene(cutsceneEnum));
			Time.timeScale = 1f;
			friend.SetActiveRecursively(false);
			if (base.player.Data.Hp != 0f)
			{
				OnSkillTrigger();
				Messenger<SkillEnum.AssistSkill>.Invoke("friend_assist", assistSkillType);
			}
		}
	}
}
