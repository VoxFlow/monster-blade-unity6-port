using UnityEngine;

public class HolyDiamondsPopup : MonoBehaviour
{
	public enum Type
	{
		REWARD = 0,
		WARNING = 1
	}

	[SerializeField]
	private UILabel m_diamondsLabel;

	[SerializeField]
	private UILabel m_currentDiamondsLabel;

	[SerializeField]
	private UILabel m_infoLabel;

	[SerializeField]
	private UIButton m_okButton;

	[SerializeField]
	private UIButton m_backButton;

	[SerializeField]
	private UIButton m_treasureButton;

	[SerializeField]
	private NBUILocalize m_headerLabel;

	public Type m_type;

	private void OnEnable()
	{
		ShowButton(m_okButton, m_type == Type.REWARD);
		ShowButton(m_backButton, m_type == Type.WARNING);
		ShowButton(m_treasureButton, m_type == Type.WARNING);
	}

	private void OnButtonClick(GameObject p_sender)
	{
		if (null != m_okButton && m_okButton.gameObject == p_sender)
		{
			OnYes();
		}
		else if (null != m_backButton && m_backButton.gameObject == p_sender)
		{
			OnClose();
		}
		else if (null != m_treasureButton && m_treasureButton.gameObject == p_sender)
		{
			MainMenu instance = MainMenu.Instance;
			if (null != instance)
			{
				OnYes();
				instance.ShowGachaPage(true);
			}
		}
	}

	private void OnYes()
	{
		if (null != UIPopupController.Instance)
		{
			UIPopupController.Instance.SendMessage("OnYes", SendMessageOptions.DontRequireReceiver);
		}
	}

	private void OnClose()
	{
		if (null != UIPopupController.Instance)
		{
			UIPopupController.Instance.SendMessage("OnClose", SendMessageOptions.DontRequireReceiver);
		}
	}

	public void ShowButton(UIButton p_button, bool p_show)
	{
		if (null != p_button)
		{
			p_button.gameObject.SetActiveRecursively(p_show);
		}
	}

	public void SetHeader(string p_textKey)
	{
		if (null != m_headerLabel)
		{
			m_headerLabel.Key = p_textKey;
		}
	}

	public void SetDiamondsNumber(int p_amount, Color p_color)
	{
		if (null != m_diamondsLabel)
		{
			m_diamondsLabel.text = p_amount + string.Empty;
			m_diamondsLabel.color = p_color;
		}
	}

	public void SetCurrentDiamonds(string p_textKey, int p_amount)
	{
		if (!(null == m_currentDiamondsLabel))
		{
			NBLocalization instance = SingletonComponent<NBLocalization>.Instance;
			string text = p_textKey;
			if (null != instance)
			{
				string format = instance.Get(p_textKey);
				text = string.Format(format, p_amount);
			}
			m_currentDiamondsLabel.text = text;
		}
	}

	public void SetInfoWithReward(string p_textKey, int p_amount)
	{
		if (!(null == m_infoLabel))
		{
			NBLocalization instance = SingletonComponent<NBLocalization>.Instance;
			string text = p_textKey;
			if (null != instance)
			{
				string format = instance.Get(p_textKey);
				text = string.Format(format, p_amount);
			}
			m_infoLabel.text = text;
		}
	}

	public void SetInfo(string p_textKey)
	{
		if (null != m_infoLabel)
		{
			NBUILocalize component = m_infoLabel.GetComponent<NBUILocalize>();
			if (null != component)
			{
				component.Key = p_textKey;
			}
		}
	}
}
