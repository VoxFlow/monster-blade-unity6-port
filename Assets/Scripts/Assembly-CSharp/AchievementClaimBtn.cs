using UnityEngine;

public class AchievementClaimBtn : MonoBehaviour
{
	private UIButton m_uiBtn;

	private UIButtonMessage m_uiBtnMsg;

	[SerializeField]
	private UILabel m_btnText;

	[SerializeField]
	private UISprite m_btnBackground;

	private void Awake()
	{
		m_uiBtn = GetComponent<UIButton>();
		m_uiBtnMsg = GetComponent<UIButtonMessage>();
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void SetEnable(bool p_enable)
	{
		if (m_uiBtn != null)
		{
			m_uiBtn.SetButtonGrey(!p_enable);
		}
		else
		{
			Dbg.LogWarn("[Warn] The UIButton is null in GameObject: " + base.gameObject.name);
		}
	}

	public void SetBtnBackgroundTexEnable(bool p_enable)
	{
		if (m_btnBackground != null)
		{
			m_btnBackground.gameObject.SetActiveRecursively(p_enable);
		}
		else
		{
			Dbg.LogWarn("[Warn] The m_btnBackground is null in GameObject: " + base.gameObject.name);
		}
	}

	public void SetBtnText(string p_str)
	{
		if (m_btnText != null)
		{
			m_btnText.text = p_str;
		}
		else
		{
			Dbg.LogWarn("[Warn] Unable to find \"Label\" in game object: " + base.gameObject.name);
		}
	}

	public void SetBtnMessageFunc(string p_function)
	{
		if (m_uiBtnMsg != null)
		{
			m_uiBtnMsg.functionName = p_function;
		}
	}

	public void SetFunctionToClaimBtn()
	{
		SetBtnMessageFunc("OnClaimButtonClick");
	}
}
