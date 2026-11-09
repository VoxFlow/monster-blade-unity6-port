using UnityEngine;

public class DeathRetryPopup : MonoBehaviour
{
	public GameObject m_giveupButton;

	public GameObject m_snsButton;

	public UILabel m_timeToWarCry;

	public NBUILocalize m_reviveDesc;

	[SerializeField]
	private UILabel m_currentPgc;

	private UISprite m_bgSprite;

	private UIButtonScale m_scaleScript;

	private UIButtonMessage m_messageScript;

	private void Start()
	{
		if (!(m_snsButton == null))
		{
			Transform transform = m_snsButton.transform.Find("BG");
			if (transform != null)
			{
				m_bgSprite = transform.gameObject.GetComponent<UISprite>();
			}
			m_scaleScript = m_snsButton.GetComponent<UIButtonScale>();
			m_messageScript = m_snsButton.GetComponent<UIButtonMessage>();
			m_timeToWarCry.enabled = false;
		}
	}

	private void Update()
	{
		bool flag = SingletonComponent<SocialManager>.Instance.UpdateWarCryAbility();
		if ((bool)m_scaleScript)
		{
			m_scaleScript.enabled = flag;
		}
		if ((bool)m_messageScript)
		{
			m_messageScript.enabled = flag;
		}
		if ((bool)m_timeToWarCry)
		{
			m_timeToWarCry.enabled = !flag;
		}
		if (!flag)
		{
			if ((bool)m_bgSprite)
			{
				m_bgSprite.spriteName = "buttons/grey_btn";
			}
			m_timeToWarCry.text = SingletonComponent<SocialManager>.Instance.GetTimeToWarCry().ToString();
		}
		else if ((bool)m_bgSprite)
		{
			m_bgSprite.spriteName = "buttons/blue_btn2";
		}
	}

	public void SetReviveDescFree()
	{
		if (m_reviveDesc != null)
		{
			m_reviveDesc.Key = "death_retry_main_desc_free";
		}
	}

	public void SetCurrentPgcAmount(int p_pgc)
	{
		m_currentPgc.text = SingletonComponent<NBLocalization>.Instance.Get("death_retry_current_pgc") + ":" + p_pgc + "[D]";
	}
}
