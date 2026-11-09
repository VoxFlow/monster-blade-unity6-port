using Models;
using UnityEngine;

public class DailyBonusClaimPopupNew : MonoBehaviour
{
	[SerializeField]
	private UISprite m_rewardIcon;

	[SerializeField]
	private NBUILocalize m_amount;

	[SerializeField]
	private UISprite m_frame;

	[SerializeField]
	private UISprite m_dragonHead;

	[SerializeField]
	private NBUILocalize m_desc;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void ShowDailyBonusData(DailyBonusData p_data)
	{
		SingletonComponent<DailyManager>.Instance.DisplayReward(p_data, ref m_rewardIcon, ref m_amount);
		string text = SingletonComponent<NBLocalization>.Instance.Get("you_got_daily_bonus");
		if (p_data.TypeId == 4)
		{
			m_frame.spriteName = "popup/gold_frame_shop_no_bg";
			m_dragonHead.spriteName = "popup/dragon_head_gold";
			if (text.Contains("{0"))
			{
				m_desc.Key = string.Format(text, "[D]" + p_data.Item);
			}
			else
			{
				m_desc.key = text;
			}
			return;
		}
		m_frame.spriteName = "common/frame_silver";
		m_frame.transform.localScale = new Vector3(m_frame.transform.localScale.x, 362f, m_frame.transform.localScale.z);
		m_frame.transform.localPosition = new Vector3(m_frame.transform.localPosition.x, -23f, m_frame.transform.localPosition.z);
		m_dragonHead.spriteName = "popup/message_box_dragonhead";
		if (p_data.TypeId == 1)
		{
			if (text.Contains("{0"))
			{
				m_desc.Key = string.Format(text, p_data.Item);
			}
			else
			{
				m_desc.key = text;
			}
		}
		else if (p_data.TypeId == 2)
		{
			m_desc.Key = string.Format(text, "[G]" + p_data.Item);
		}
	}

	public void SetRewardIconPixelPerfect()
	{
		if (m_rewardIcon != null)
		{
			m_rewardIcon.MakePixelPerfect();
		}
	}
}
