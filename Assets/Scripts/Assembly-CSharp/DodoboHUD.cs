using UnityEngine;

public class DodoboHUD : MonoBehaviour
{
	public UILabel m_moneyLabel;

	public UILabel m_countdownLabel;

	public UILabel m_timesupLabel;

	private Dodobo m_monster;

	private void Awake()
	{
	}

	private void OnEnable()
	{
		if ((bool)m_timesupLabel)
		{
			m_timesupLabel.gameObject.SetActiveRecursively(false);
		}
		if ((bool)m_countdownLabel)
		{
			m_countdownLabel.gameObject.SetActiveRecursively(true);
		}
	}

	protected virtual void FixedUpdate()
	{
		if (!(m_countdownLabel == null) && !(m_timesupLabel == null) && !(m_monster == null))
		{
			if ((int)m_monster.GetDodoboLifeTime() != 0)
			{
				m_countdownLabel.text = ((int)m_monster.GetDodoboLifeTime()).ToString();
				return;
			}
			m_timesupLabel.gameObject.SetActiveRecursively(true);
			m_countdownLabel.gameObject.SetActiveRecursively(false);
		}
	}

	public void StartCountDown(Dodobo monster)
	{
		m_monster = monster;
		m_countdownLabel.text = ((int)m_monster.GetDodoboLifeTime()).ToString();
	}

	public void UpdateCountdownTime(float p_time)
	{
		if (!(m_countdownLabel == null) && !(m_timesupLabel == null))
		{
			if ((int)p_time != 0)
			{
				m_countdownLabel.text = ((int)p_time).ToString();
				return;
			}
			m_timesupLabel.gameObject.SetActiveRecursively(true);
			m_countdownLabel.gameObject.SetActiveRecursively(false);
		}
	}

	public void UpdateMoney(int p_money)
	{
		if (!(m_moneyLabel == null))
		{
			m_moneyLabel.text = p_money.ToString();
		}
	}
}
