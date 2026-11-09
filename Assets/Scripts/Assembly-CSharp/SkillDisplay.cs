using UnityEngine;

public class SkillDisplay : MonoBehaviour
{
	public NBUILocalize m_headerKey;

	public NBUILocalize m_descriptionKey;

	public UISprite m_sprite;

	public UIAtlas m_skillAtlas;

	private bool m_ownSkill = true;

	private bool m_simpleDisplay;

	private ItemDetails m_item;

	private bool CheckPrefabConnections()
	{
		if (m_headerKey == null)
		{
			Dbg.LogError("m_headerKey is null.");
			return false;
		}
		if (m_descriptionKey == null)
		{
			Dbg.LogError("m_descriptionKey is null.");
			return false;
		}
		if (m_sprite == null)
		{
			Dbg.LogError("m_sprite is null.");
			return false;
		}
		if (SingletonComponent<ItemManager>.Instance.m_skillAtlas == null)
		{
			Dbg.LogError("Could not find the skill atlas in the ItemManager.");
			return false;
		}
		return true;
	}

	private void UpdateDisplay()
	{
		SkillVisual visual = m_item.skill.visual;
		m_sprite.atlas = m_skillAtlas;
		if (m_ownSkill)
		{
			m_sprite.spriteName = visual.sprite_self;
			m_headerKey.Key = visual.label_self;
			m_descriptionKey.Key = visual.description_self;
		}
		else
		{
			m_sprite.spriteName = visual.sprite_assist;
			m_headerKey.Key = visual.label_assist;
			m_descriptionKey.Key = visual.label_assist;
		}
		if (m_simpleDisplay)
		{
			m_headerKey.Key = string.Empty;
			m_descriptionKey.Key = string.Empty;
		}
	}

	private void SetItem(ItemDetails _item, bool _showSimple = false, bool _ownSkill = true)
	{
		if (CheckPrefabConnections())
		{
			if (_item == null)
			{
				Dbg.LogError("Cannot set null items.");
				return;
			}
			m_item = _item;
			m_ownSkill = _ownSkill;
			m_simpleDisplay = _showSimple;
			UpdateDisplay();
			Dbg.Log("Skill display set with item : " + _item);
		}
	}

	public void ShowFriendSkill(ItemDetails _item)
	{
	}

	public void ShowSeflSkill(ItemDetails _item)
	{
	}

	public void ShowFriendAssistSkill(ItemDetails _item)
	{
	}
}
