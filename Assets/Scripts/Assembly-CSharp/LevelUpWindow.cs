using UnityEngine;

public class LevelUpWindow : MonoBehaviour
{
	private Animation m_cachedAnim;

	private int m_tempLevel = -1;

	[SerializeField]
	private UISprite[] m_digits;

	[SerializeField]
	private UILabel m_staminaValue;

	[SerializeField]
	private UILabel m_attackValue;

	[SerializeField]
	private UILabel m_hpValue;

	[SerializeField]
	private UILabel m_armorValue;

	public bool Show()
	{
		if (!base.gameObject.active)
		{
			UpdateGameObjects();
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_level_up_notification");
		}
		if (m_cachedAnim.isPlaying)
		{
			return true;
		}
		return false;
	}

	public void Hide()
	{
		base.gameObject.SetActiveRecursively(false);
	}

	public void SetLevelDigits(int p_level)
	{
		Dbg.Log("LEVELUP:" + p_level);
		m_tempLevel = p_level;
		int num = p_level;
		int num2 = 0;
		while (num > 9)
		{
			int num3 = num % 10;
			num = Mathf.FloorToInt(num / 10);
			m_digits[num2].spriteName = "level_up_" + num3;
			num2++;
		}
		if (num > 0)
		{
			m_digits[num2].spriteName = "level_up_" + num;
		}
	}

	public void SetLevelUpData(PlayerData p_oldData, PlayerData p_newData, int p_levelOffset = 0)
	{
		if (p_oldData == null)
		{
			Dbg.LogError("Old Stats/Data Missing");
			return;
		}
		if (p_newData == null)
		{
			Dbg.LogError("New Stats/Data Missing");
			return;
		}
		float num = p_newData.Level - p_oldData.Level;
		float num2 = (float)(p_levelOffset + 1) * (1f / num);
		float num3 = (float)p_levelOffset * (1f / num);
		float num4 = p_newData.m_userData.maxStamina - p_oldData.m_userData.maxStamina;
		float num5 = p_newData.MaxAttack - p_oldData.MaxAttack;
		float num6 = p_newData.MaxHp - p_oldData.MaxHp;
		float num7 = p_newData.Def - p_oldData.Def;
		int num8 = Mathf.FloorToInt((float)p_oldData.m_userData.maxStamina + num4 * num3);
		int num9 = Mathf.FloorToInt(p_oldData.MaxAttack + num5 * num3);
		int num10 = Mathf.FloorToInt(p_oldData.MaxHp + num6 * num3);
		int num11 = Mathf.FloorToInt(p_oldData.Def + num7 * num3);
		int num12 = Mathf.FloorToInt((float)p_oldData.m_userData.maxStamina + num4 * num2);
		int num13 = Mathf.FloorToInt(p_oldData.MaxAttack + num5 * num2);
		int num14 = Mathf.FloorToInt(p_oldData.MaxHp + num6 * num2);
		int num15 = Mathf.FloorToInt(p_oldData.Def + num7 * num2);
		SetLevelDigits(p_oldData.Level + (p_levelOffset + 1));
		if (m_staminaValue != null)
		{
			m_staminaValue.text = num8 + NBColorTags.TAG_GREEN + "->" + num12 + NBColorTags.TAG_END;
		}
		if (m_attackValue != null)
		{
			m_attackValue.text = num9 + NBColorTags.TAG_GREEN + "->" + num13 + NBColorTags.TAG_END;
		}
		if (m_hpValue != null)
		{
			m_hpValue.text = num10 + NBColorTags.TAG_GREEN + "->" + num14 + NBColorTags.TAG_END;
		}
		if (m_armorValue != null)
		{
			m_armorValue.text = num11 + NBColorTags.TAG_GREEN + "->" + num15 + NBColorTags.TAG_END;
		}
		MainMenu.Instance.topBar.SetXP(0f);
		int lvl = p_oldData.Level + (p_levelOffset + 1);
		MainMenu.Instance.topBar.SetName(p_oldData.Name, lvl);
		UpdateGameObjects();
	}

	private void Awake()
	{
		m_cachedAnim = base.GetComponent<Animation>();
		if (m_digits.Length < 3)
		{
			Dbg.LogError("Sprites were less than expected, ERRAWR!!!");
		}
		UISprite[] digits = m_digits;
		foreach (UISprite uISprite in digits)
		{
			Dbg.Assert(uISprite, "Sprite was not assigned, ERRAWR!!!");
		}
	}

	public void PauseAnimation()
	{
		foreach (AnimationState item in base.GetComponent<Animation>())
		{
			item.speed = 0f;
		}
	}

	public void ResumeAnimation()
	{
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_ok");
		foreach (AnimationState item in base.GetComponent<Animation>())
		{
			item.speed = 1f;
		}
	}

	public void UpdateGameObjects()
	{
		base.gameObject.SetActiveRecursively(true);
		if (m_tempLevel < 10)
		{
			m_digits[1].gameObject.SetActiveRecursively(false);
			m_digits[2].gameObject.SetActiveRecursively(false);
			Vector3 localPosition = m_digits[0].gameObject.transform.localPosition;
			localPosition.x = 0f;
			m_digits[0].gameObject.transform.localPosition = localPosition;
		}
		else if (m_tempLevel < 100)
		{
			m_digits[2].gameObject.SetActiveRecursively(false);
			Vector3 localPosition2 = m_digits[1].gameObject.transform.localPosition;
			localPosition2.x = m_digits[1].gameObject.transform.localScale.x * -0.4f;
			m_digits[1].gameObject.transform.localPosition = localPosition2;
			Vector3 localPosition3 = m_digits[0].gameObject.transform.localPosition;
			localPosition3.x = m_digits[0].gameObject.transform.localScale.x * 0.4f;
			m_digits[0].gameObject.transform.localPosition = localPosition3;
		}
	}
}
