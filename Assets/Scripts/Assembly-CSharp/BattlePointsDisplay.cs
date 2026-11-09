using System;
using UnityEngine;

public class BattlePointsDisplay : MonoBehaviour
{
	[SerializeField]
	private UISprite[] m_bpIcons;

	[SerializeField]
	private UILabel m_timerText;

	private int m_maxBp = 3;

	private int m_lastPoints = -1;

	private float timer;

	private void SetPoints(int p_points)
	{
		if (p_points != m_lastPoints)
		{
			for (int i = 0; i < p_points; i++)
			{
				m_bpIcons[i].spriteName = "Icon_battlepoint";
			}
			for (int j = p_points; j < m_maxBp; j++)
			{
				m_bpIcons[j].spriteName = "Icon_battlepoint_grey";
			}
			m_lastPoints = p_points;
		}
	}

	private string SecondsToTimeString(long p_time)
	{
		TimeSpan timeSpan = new TimeSpan(p_time * 10000000);
		string text = ((timeSpan.Hours >= 10) ? timeSpan.Hours.ToString() : ("0" + timeSpan.Hours));
		string text2 = ((timeSpan.Minutes >= 10) ? timeSpan.Minutes.ToString() : ("0" + timeSpan.Minutes));
		string text3 = ((timeSpan.Seconds >= 10) ? timeSpan.Seconds.ToString() : ("0" + timeSpan.Seconds));
		if (timeSpan.Hours <= 0 && timeSpan.Minutes <= 0 && timeSpan.Seconds <= 0)
		{
			return string.Empty;
		}
		return text + ":" + text2 + ":" + text3;
	}

	private void RefreshDisplay()
	{
		PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		if (playerData != null)
		{
			m_maxBp = playerData.m_userData.maxShield;
			SetTimer(playerData.m_userData.timeNextShield);
			SetPoints(playerData.m_userData.shield);
		}
	}

	private void Update()
	{
		timer += Time.deltaTime;
		if (timer >= 1f)
		{
			timer -= 1f;
			RefreshDisplay();
		}
	}

	private void SetTimer(long p_time)
	{
		if (null != m_timerText)
		{
			m_timerText.text = SecondsToTimeString(p_time);
		}
	}

	private void OnEnable()
	{
		Messenger.AddListener("battlepoints_update_points", RefreshDisplay);
		RefreshDisplay();
	}

	private void OnDisable()
	{
		Messenger.RemoveListener("battlepoints_update_points", RefreshDisplay);
	}

	public void OnAddBpBtnClick()
	{
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_secondary_ui_tap");
		if (SingletonComponent<StoreManager>.Instance != null)
		{
			SingletonComponent<StoreManager>.Instance.ShowBattlePointShop();
		}
	}
}
