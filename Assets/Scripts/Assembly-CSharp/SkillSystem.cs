using System;
using System.Collections.Generic;

public class SkillSystem : SingletonComponent<SkillSystem>
{
	public static bool infiniteFriendAsssist;

	public Skill[] skills { get; private set; }

	public PlayerSkill[] playerSkills { get; private set; }

	public FriendAssistSkill friendSkill { get; private set; }

	public Action<FriendAssistSkill> onFriendAssistSkillChangedCallback { get; set; }

	private void Start()
	{
		StageManager instance = SingletonComponent<StageManager>.Instance;
		Player player = null;
		PlayerData playerData = null;
		if (instance != null)
		{
			if (instance.PlayerObject != null)
			{
				player = instance.PlayerObject.GetComponent<Player>();
			}
			playerData = instance.GetFriendData(0);
		}
		skills = MBHier.GetComponentsInChildren<Skill>(base.gameObject);
		List<PlayerSkill> list = new List<PlayerSkill>();
		Skill[] array = skills;
		foreach (Skill skill in array)
		{
			if (skill == null)
			{
				continue;
			}
			if (player != null && skill is PlayerSkill && skill.DoesCharacterHaveSkill(player.Data))
			{
				skill.enabled = true;
				list.Add((PlayerSkill)skill);
			}
			else if (playerData != null && skill is FriendAssistSkill && skill.DoesCharacterHaveSkill(playerData))
			{
				skill.enabled = true;
				if (friendSkill == null || (skill is ArmorSetSkill && !(friendSkill is ArmorSetSkill)))
				{
					friendSkill = (FriendAssistSkill)skill;
					if (onFriendAssistSkillChangedCallback != null)
					{
						onFriendAssistSkillChangedCallback(friendSkill);
					}
				}
			}
			else
			{
				skill.enabled = false;
			}
		}
		playerSkills = list.ToArray();
		if (friendSkill != null)
		{
			CutsceneController.LoadCutscenePrefab(friendSkill.cutsceneEnum);
		}
	}

	public bool DoesPlayerHaveSkill(Type p_skillType)
	{
		for (int i = 0; i < playerSkills.Length; i++)
		{
			if (playerSkills[i].GetType() == p_skillType)
			{
				return true;
			}
		}
		return false;
	}

	public void TriggerFriendAssistSkill()
	{
		if (friendSkill != null)
		{
			friendSkill.Trigger();
			if (!infiniteFriendAsssist)
			{
				friendSkill.enabled = false;
			}
		}
	}
}
