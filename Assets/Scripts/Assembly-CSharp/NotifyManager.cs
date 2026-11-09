using System;
using System.Collections.Generic;
using UnityEngine;

public class NotifyManager : SingletonComponent<NotifyManager>
{
	public class NotifyItem
	{
		public string message = string.Empty;

		public int priority;

		public float lifetime = 3f;

		public object userData;

		public NotifyItem(string p_message, int p_priority, float p_lifetime, object p_userData)
		{
			message = p_message;
			priority = p_priority;
			lifetime = p_lifetime;
			userData = p_userData;
		}
	}

	private class CompareNotifyItem : IComparer<NotifyItem>
	{
		public int Compare(NotifyItem p1, NotifyItem p2)
		{
			if (p1.priority == p2.priority)
			{
				return 0;
			}
			if (p1.priority > p2.priority)
			{
				return 1;
			}
			return -1;
		}
	}

	private string m_notificationTitle = string.Empty;

	private int delayToShowNotification = 20;

	public int m_numberToDisplay = 5;

	private List<NotifyItem> m_notifications = new List<NotifyItem>();

	private List<NotifyItem> m_onDisplay = new List<NotifyItem>();

	private string notificationTitle
	{
		get
		{
			if (string.IsNullOrEmpty(m_notificationTitle) && null != SingletonComponent<NBLocalization>.Instance)
			{
				m_notificationTitle = SingletonComponent<NBLocalization>.Instance.Get("monster_blade");
			}
			return m_notificationTitle;
		}
		set
		{
			m_notificationTitle = value;
		}
	}

	public void InitManager()
	{
		EnableNotifications(false);
	}

	public void EnableNotifications(bool p_value)
	{
		base.enabled = p_value;
	}

	public void PostNotification(string p_message, int p_priority, float p_lifetime = 3f, object p_userData = null)
	{
		m_notifications.Add(new NotifyItem(p_message, p_priority, p_lifetime, p_userData));
		Sort();
	}

	public List<NotifyItem> GetNotificationToDisplay()
	{
		return m_onDisplay;
	}

	public void CheckServerForNotifications()
	{
	}

	private void OnCheckServerNotificationsSuccess(Services.ServiceHandle p_handle)
	{
	}

	private void OnCheckServerNotificationsFail(Services.ServiceHandle p_handle, string p_error)
	{
	}

	private void Update()
	{
		if (m_notifications.Count > 0)
		{
			while (m_onDisplay.Count < m_numberToDisplay && m_notifications.Count > 0)
			{
				Dbg.Log("Added notification: " + m_notifications[0].message);
				m_onDisplay.Add(m_notifications[0]);
				m_notifications.RemoveAt(0);
			}
		}
		if (m_onDisplay.Count <= 0)
		{
			return;
		}
		for (int num = m_onDisplay.Count - 1; num >= 0; num--)
		{
			m_onDisplay[num].lifetime -= Time.deltaTime;
			if (m_onDisplay[num].lifetime <= 0f)
			{
				Dbg.Log("Removed notification: " + m_onDisplay[num].message);
				m_onDisplay.RemoveAt(num);
			}
		}
	}

	private void RegisterNextDayNotification()
	{
		string p_msg = SingletonComponent<NBLocalization>.Instance.Get("notification_daily");
		TimeSpan timeSpan = new TimeSpan(DateTime.Today.AddDays(1.0).AddHours(12.0).Ticks - DateTime.Now.Ticks);
		double totalSeconds = timeSpan.TotalSeconds;
		RegisterLocalNotification(totalSeconds, notificationTitle, p_msg);
	}

	private void Register36HoursNotification()
	{
		string p_msg = SingletonComponent<NBLocalization>.Instance.Get("not_login_36hours");
		RegisterLocalNotification(129600.0, notificationTitle, p_msg);
	}

	private void RegisterStaminaRefillNotification(PlayerData p_playerData)
	{
		int num = p_playerData.m_userData.maxStamina - p_playerData.m_userData.stamina;
		if (num > 0)
		{
			double num2 = 0.0;
			num2 = ((num <= 1) ? ((double)p_playerData.m_userData.timeNextStamina) : ((double)((float)num * GLOBAL.GAME_SETTINGS.stamina_reset_time + (float)p_playerData.m_userData.timeNextStamina)));
			if (num2 > 0.0)
			{
				num2 += (double)delayToShowNotification;
				string p_msg = SingletonComponent<NBLocalization>.Instance.Get("stamina_refilled");
				RegisterLocalNotification(num2, notificationTitle, p_msg);
				Dbg.Log("Register stamina refill notification after " + num2 + " secs, remain stamina : " + num);
			}
		}
	}

	private void RegisterBattlePointRefillNotification(PlayerData p_playerData)
	{
		int num = p_playerData.m_userData.maxShield - p_playerData.m_userData.shield;
		if (num > 0)
		{
			double num2 = 0.0;
			num2 = ((num <= 1) ? ((double)p_playerData.m_userData.timeNextShield) : ((double)((num - 1) * GLOBAL.GAME_SETTINGS.shields_reset_time + p_playerData.m_userData.timeNextShield)));
			if (num2 > 0.0)
			{
				num2 += (double)delayToShowNotification;
				string p_msg = SingletonComponent<NBLocalization>.Instance.Get("battle_point_refilled");
				RegisterLocalNotification(num2, notificationTitle, p_msg);
				Dbg.Log("Register battle point refill notification after " + num2 + " secs, consumedBp : " + num);
			}
		}
	}

	public void RegisterAllLocalNotifications()
	{
		Dbg.Log("[Enter] RegisterAllLocalNotifications");
		Dbg.Log("enable_local_push_notification flag: " + GLOBAL.GAME_SETTINGS.enable_local_push_notification);
		if (GLOBAL.GAME_SETTINGS.enable_local_push_notification)
		{
			Register36HoursNotification();
			RegisterNextDayNotification();
			if (SingletonComponent<PlayerDataManager>.Instance != null)
			{
				PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
				RegisterStaminaRefillNotification(playerData);
				RegisterBattlePointRefillNotification(playerData);
			}
			else
			{
				Dbg.LogWarn("The PlayerData is null!");
			}
		}
		else
		{
			ClearLocalNotification();
		}
		Dbg.Log("[Leave] RegisterAllLocalNotifications");
	}

	public void ClearLocalNotification()
	{
		Dbg.Log("[Enter] ClearLocalNotification");
		string text = MBPrefs.GetString("Ms:AndroidNotify:" + SingletonComponent<Services>.Instance.NubeeId, string.Empty);
		Dbg.Log("Registered Android notification ID string: " + text);
		string[] array = text.Split('|');
		string[] array2 = array;
		foreach (string text2 in array2)
		{
			if (!string.IsNullOrEmpty(text2))
			{
				int result = 0;
				if (int.TryParse(text2, out result))
				{
					EtceteraAndroid.cancelNotification(result);
					Dbg.Log("Cancel notification id : " + text2);
				}
			}
		}
		MBPrefs.SetString("Ms:AndroidNotify:" + SingletonComponent<Services>.Instance.NubeeId, string.Empty);
		Dbg.Log("[Leave] ClearLocalNotification");
	}

	public void RegisterLocalNotification(double p_seconds, string p_title, string p_msg)
	{
		int num = EtceteraAndroid.scheduleNotification((long)p_seconds, p_title, p_msg, p_msg, string.Empty);
		string text = MBPrefs.GetString("Ms:AndroidNotify:" + SingletonComponent<Services>.Instance.NubeeId, string.Empty);
		text = text + num + "|";
		MBPrefs.SetString("Ms:AndroidNotify:" + SingletonComponent<Services>.Instance.NubeeId, text);
	}

	private void Sort()
	{
		MBSort.Sort(m_notifications, new CompareNotifyItem().Compare);
	}
}
