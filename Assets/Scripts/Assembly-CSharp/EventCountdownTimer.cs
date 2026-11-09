using UnityEngine;

public class EventCountdownTimer : MonoBehaviour
{
	[SerializeField]
	private UISprite m_background;

	[SerializeField]
	private UILabel m_label;

	[SerializeField]
	private UILabel m_timerLabel;

	[SerializeField]
	private float m_countdownDuration = 60f;

	public bool m_disableWhenEmpty = true;

	private long m_endTime;

	private NBTimer m_timer;

	public void SetEndTime(long p_time)
	{
		m_endTime = p_time;
	}

	public void Refresh()
	{
		long unixEpochTime = TimeUtil.GetUnixEpochTime();
		long time = m_endTime - unixEpochTime;
		SetTime(time);
	}

	private void OnRefresh()
	{
		Refresh();
		if (null == m_timer)
		{
			m_timer = base.gameObject.AddComponent<NBTimer>();
		}
		if (null != m_timer)
		{
			m_timer.CountDown(m_countdownDuration, "OnRefresh");
		}
	}

	public void StartTimer()
	{
		OnRefresh();
	}

	public void SetTime(long p_time)
	{
		if (null != m_timerLabel)
		{
			string text = NBGeneric.ConvertTimeToDDHHMM(p_time);
			m_timerLabel.text = text;
			if (string.IsNullOrEmpty(text) && m_disableWhenEmpty)
			{
				base.gameObject.SetActiveRecursively(false);
			}
		}
	}

	public void ShowBackground(bool p_show)
	{
		if (null != m_background)
		{
			m_background.gameObject.SetActiveRecursively(p_show);
		}
	}
}
