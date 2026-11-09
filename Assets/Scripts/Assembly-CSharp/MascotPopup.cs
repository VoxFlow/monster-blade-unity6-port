using UnityEngine;

public class MascotPopup : MonoBehaviour
{
	[SerializeField]
	private UILabel m_title;

	[SerializeField]
	private UILabel m_description;

	[SerializeField]
	private int m_descMaxLines = 3;

	[SerializeField]
	private UIButtonMessage m_okOrNextBtn;

	[SerializeField]
	private UILabel m_btnText;

	private Vector3 m_mascotDefaultOffset = new Vector3(-6f, -4f, 15f);

	private MascotController m_mascot;

	[SerializeField]
	private Transform m_mascotCameraPos;

	private string[] m_bufferStrings;

	private int m_totalPages;

	private int m_currentPage;

	private void Start()
	{
	}

	public void SetTitle(string p_title)
	{
		m_title.text = p_title;
	}

	public bool SetDescription(string p_desc)
	{
		m_description.lineWidth = (int)((float)Screen.width * 0.5f);
		m_description.multiLine = true;
		m_description.text = p_desc;
		m_bufferStrings = m_description.processedText.Split('\n');
		m_totalPages = m_bufferStrings.Length / m_descMaxLines + 1;
		Dbg.Log(string.Format("[Info] There're {0} lines, {1} page(s) for the subtitle", m_bufferStrings.Length, m_totalPages));
		if (m_totalPages > 1)
		{
			ShowPage();
		}
		if (m_currentPage >= m_totalPages)
		{
			return false;
		}
		return true;
	}

	public bool ShowPage()
	{
		string text = string.Empty;
		int num = (m_currentPage + 1) * m_descMaxLines;
		if (num > m_bufferStrings.Length)
		{
			num = m_bufferStrings.Length;
		}
		Dbg.Log("[Info] Current page index: " + m_currentPage);
		Dbg.Log(string.Format("[Info] Init index: {0},last index: {1}", m_currentPage * m_descMaxLines, num));
		for (int i = m_currentPage * m_descMaxLines; i < num; i++)
		{
			text = text + m_bufferStrings[i] + " ";
		}
		Dbg.Log("[Info] String to assign: " + text);
		m_description.text = text;
		m_currentPage++;
		if (num == m_bufferStrings.Length)
		{
			return false;
		}
		if (m_currentPage >= m_totalPages)
		{
			return false;
		}
		return true;
	}

	public void Show()
	{
		base.gameObject.SetActiveRecursively(true);
	}

	public void Hide()
	{
		m_currentPage = 0;
		m_totalPages = 0;
		m_bufferStrings = null;
		base.gameObject.SetActiveRecursively(false);
	}

	public void LoadMascot(string p_mascotPath)
	{
		Object obj = Resources.Load(p_mascotPath);
		if (obj != null)
		{
			GameObject gameObject = Object.Instantiate(obj) as GameObject;
			if (gameObject != null)
			{
				m_mascot = gameObject.GetComponent<MascotController>();
				if (m_mascot == null)
				{
					Dbg.LogError("[Error] Unable to find mascot model in path: " + p_mascotPath);
				}
				else
				{
					m_mascot.transform.position = m_mascotCameraPos.transform.position + m_mascotDefaultOffset;
				}
			}
			else
			{
				Dbg.LogError("[Error] Unable to find mascot model in path: " + p_mascotPath);
			}
		}
		else
		{
			Dbg.LogError("[Error] Unable to find mascot model in path: " + p_mascotPath);
		}
	}

	public void ShowMascot()
	{
		m_mascot.Show();
	}

	public void HideMascot()
	{
		m_mascot.Hide();
	}

	public void PlayMascotAnim(string p_anim)
	{
		m_mascot.PlayAnim(p_anim);
	}

	public void SetMascotOffset(Vector3 p_offset)
	{
		m_mascot.transform.Translate(p_offset);
	}

	public void SetBtnCallback(GameObject p_target, string p_callbackName)
	{
		m_okOrNextBtn.target = p_target;
		m_okOrNextBtn.functionName = p_callbackName;
	}

	private void Update()
	{
	}
}
