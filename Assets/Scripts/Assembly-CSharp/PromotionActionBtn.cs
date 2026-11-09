using UnityEngine;

public class PromotionActionBtn : MonoBehaviour
{
	[SerializeField]
	private UISprite m_btnBackground;

	[SerializeField]
	private UISprite m_btnIcon;

	[SerializeField]
	private NBUILocalize m_btnTextLabel;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void SetButtonIconGacha()
	{
		if (m_btnIcon != null)
		{
			m_btnIcon.spriteName = "Wolf_nemesis_treasure";
			m_btnIcon.MakePixelPerfect();
		}
	}

	public void SetButtonIconShop()
	{
		if (m_btnIcon != null)
		{
			m_btnIcon.spriteName = "icon/icon_reward_diamond";
			m_btnIcon.MakePixelPerfect();
		}
	}

	public void EnableButtonIcon(bool p_enable)
	{
		if (m_btnIcon != null)
		{
			m_btnIcon.enabled = p_enable;
		}
	}

	public void SetBtnTextL10nKey(string p_key)
	{
		if (!(m_btnTextLabel != null))
		{
			return;
		}
		m_btnTextLabel.Key = p_key;
		UILabel component = m_btnTextLabel.GetComponent<UILabel>();
		if (!(component != null))
		{
			return;
		}
		float num = 180f;
		if (SingletonComponent<SaveData>.Instance != null)
		{
			switch (SingletonComponent<SaveData>.Instance.GameData.Language)
			{
			case "English":
				num = 252f;
				break;
			case "Chinese_s":
				num = 120f;
				break;
			case "Chinese_t":
				num = 140f;
				break;
			case "Japanese":
				num = 180f;
				break;
			case "Korean":
				num = 196f;
				break;
			}
		}
		if (component.transform.localScale.x * (float)component.text.Length > num)
		{
			float num2 = num / (float)component.text.Length;
			component.transform.localScale = new Vector3(num2, num2, 1f);
		}
	}

	public void OffsetBtnText(bool p_needOffset)
	{
		if (m_btnTextLabel != null)
		{
			m_btnTextLabel.transform.localPosition = new Vector3((!p_needOffset) ? 0f : 24f, m_btnTextLabel.transform.localPosition.y, m_btnTextLabel.transform.localPosition.z);
		}
	}

	public void EnableBtnVisual(bool p_enable)
	{
		if (m_btnIcon != null)
		{
			m_btnIcon.enabled = p_enable;
		}
		if (m_btnTextLabel != null)
		{
			UILabel component = m_btnTextLabel.GetComponent<UILabel>();
			if (component != null)
			{
				component.enabled = p_enable;
			}
		}
		if (m_btnBackground != null)
		{
			m_btnBackground.enabled = p_enable;
		}
	}
}
