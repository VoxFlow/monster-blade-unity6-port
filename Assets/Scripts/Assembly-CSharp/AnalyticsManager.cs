using System.Collections;
using ServiceCalls;
using UnityEngine;

public class AnalyticsManager : SingletonComponent<AnalyticsManager>
{
	public int m_allowableRetries = 5;

	private Services m_serv;

	private int m_retries;

	private bool m_allowPing;

	private bool m_doingPing;

	public static void FirstInstallEvents(string p_name)
	{
		if (null != SingletonComponent<SaveData>.Instance && SingletonComponent<SaveData>.Instance.GameData.IsFirstTimeToRun)
		{
			Apsalar.NBSendEvent(p_name, true);
		}
	}

	public void StartPinger()
	{
		m_allowPing = true;
	}

	public void StopPinger()
	{
		m_allowPing = false;
	}

	public void RecalculateNextPing()
	{
	}

	public void applicationWillEnterForeground()
	{
		bool flag = MBPrefs.GetInt("StoreManager.IsMakingPurchase", 0) > 0;
		Dbg.Log("AnalyticsManager.applicationWillEnterForeground, allowPing : " + m_allowPing + " doingPing : " + m_doingPing + " making purchase : " + flag);
		if (flag)
		{
			Dbg.Log("Do not call analytics when making Purchase.");
		}
		else if (m_allowPing && !m_doingPing)
		{
			StartCoroutine(PerformPing());
		}
	}

	private IEnumerator PerformPing()
	{
		m_doingPing = true;
		m_serv.SaveSessionTimestamps(m_serv.NubeeId, m_serv.DeviceId, InternalOnSaveSessionTimestampsSuccess, InternalOnSaveSessionTimestampsFail);
		yield break;
	}

	private void InternalOnSaveSessionTimestampsSuccess(Services.ServiceHandle p_handle)
	{
		Dbg.Log("SaveSessionTimestamps Success");
		m_doingPing = false;
	}

	private void InternalOnSaveSessionTimestampsFail(Services.ServiceHandle p_handle, string p_error)
	{
		Dbg.LogWarn("SaveSessionTimestamps Fail. Error: " + ServiceCallUtils.GetErrorMessage(p_handle));
		m_doingPing = false;
		if (m_retries >= m_allowableRetries)
		{
			m_retries = 0;
			return;
		}
		m_retries++;
		Dbg.LogWarn("Retry attempt " + (m_retries + 1) + "...");
		StartCoroutine(PerformPing());
	}

	private void Start()
	{
		m_serv = SingletonComponent<Services>.Instance;
		if (m_serv == null)
		{
			Dbg.LogError("Analytics Manager requires Services Manager.");
		}
		else
		{
			m_retries = 0;
		}
	}
}
