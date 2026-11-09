using System;
using System.Collections;
using UnityEngine;

public class SubtitlePopup : MonoBehaviour
{
	public NBUILocalize m_subtitle;

	public Transform m_offset;

	public UIAnchor m_anchor;

	public UISprite m_spriteArrow;

	public float m_defaultTextSpeed = 10f;

	public UISprite m_spriteOverlay;

	private PopupCallback m_onClose;

	protected Transform m_mascot;

	protected string m_text;

	protected UILabel m_label;

	protected CutsceneAnimSq m_animSequence;

	private float m_textSpeed;

	private bool m_done;

	private bool m_pressed;

	private void OnEnable()
	{
	}

	private void Awake()
	{
		Dbg.Assert(m_subtitle);
		Dbg.Assert(m_offset);
		Dbg.Assert(m_anchor);
		Dbg.Assert(m_spriteOverlay);
		UIButtonMessage[] components = GetComponents<UIButtonMessage>();
		UIButtonMessage[] array = components;
		foreach (UIButtonMessage uIButtonMessage in array)
		{
			if (!uIButtonMessage.target)
			{
				uIButtonMessage.target = base.gameObject;
			}
		}
		m_label = m_subtitle.GetComponent<UILabel>();
		Dbg.Assert(m_label);
		Reset();
	}

	private void Reset()
	{
		m_textSpeed = m_defaultTextSpeed;
	}

	public void SetSubtitle(string p_text)
	{
		if (m_subtitle != null)
		{
			m_subtitle.Key = p_text;
		}
	}

	private void OnClose()
	{
		if (m_onClose != null && m_done)
		{
			m_onClose();
		}
	}

	public void Show(string p_text, bool p_showMascot = true, string p_mascotAnimName = "Idle01", string p_mascotFaceName = "Default", string p_soundName = "", PopupCallback p_onClose = null, bool p_isTutorial = false)
	{
		if (!base.gameObject.active)
		{
			base.gameObject.SetActiveRecursively(true);
		}
		if ((bool)m_label)
		{
			m_label.text = string.Empty;
		}
		m_text = SingletonComponent<NBLocalization>.Instance.Get(p_text);
		m_onClose = p_onClose;
		if (p_showMascot)
		{
			if (!m_mascot)
			{
				m_mascot = base.transform.FindInHierarchy("Mascot");
				Dbg.Assert(m_mascot);
			}
			if ((bool)m_mascot)
			{
				if (p_mascotFaceName != string.Empty)
				{
					MascotFacial componentInChildren = MBHier.GetComponentInChildren<MascotFacial>(m_mascot);
					Dbg.Assert(componentInChildren);
					if ((bool)componentInChildren)
					{
						componentInChildren.enabled = !p_isTutorial;
						componentInChildren.faceType = (MascotFacial.FaceType)(int)Enum.Parse(typeof(MascotFacial.FaceType), p_mascotFaceName);
					}
				}
				if (p_isTutorial && p_mascotAnimName != string.Empty)
				{
					Animation componentInChildren2 = MBHier.GetComponentInChildren<Animation>(m_mascot);
					Dbg.Assert(componentInChildren2);
					if ((bool)componentInChildren2)
					{
						StartCoroutine(componentInChildren2.Play(p_mascotAnimName, true));
					}
				}
				if (p_soundName != string.Empty && (bool)SingletonComponent<SoundManager>.Instance)
				{
					SingletonComponent<SoundManager>.Instance.PlayPlaylist(p_soundName);
				}
				if (null == m_animSequence)
				{
					m_animSequence = m_mascot.GetComponent<CutsceneAnimSq>();
				}
				if (null != m_animSequence)
				{
					m_animSequence.enabled = !p_isTutorial;
				}
			}
		}
		m_done = false;
		StopCoroutine("DoAnimateText");
		StartCoroutine("DoAnimateText");
	}

	private IEnumerator DoAnimateText()
	{
		if (!m_label)
		{
			yield break;
		}
		int i = 0;
		bool pressed = false;
		bool coloring = false;
		while (m_label.text.Length < m_text.Length)
		{
			if (pressed != m_pressed)
			{
				pressed = m_pressed;
				m_textSpeed = m_defaultTextSpeed * ((!pressed) ? 1f : 5f);
			}
			float nextTime = UnityEngine.Time.realtimeSinceStartup + 1f / m_textSpeed;
			while (UnityEngine.Time.realtimeSinceStartup < nextTime)
			{
				yield return null;
			}
			if (m_label.text.Length >= m_text.Length)
			{
				break;
			}
			char ch = m_text[i];
			if (ch == '[')
			{
				int start = i;
				int end = m_text.IndexOf(']', start);
				int length = end - start + 1;
				string str = string.Empty;
				i = end;
				if (length == 3)
				{
					str = m_text.Substring(start, length);
					if (str[1] == '-')
					{
						if (coloring)
						{
						}
						coloring = false;
					}
				}
				else if (length > 3)
				{
					i++;
					str = m_text.Substring(start, length);
					str += m_text[i];
					coloring = true;
				}
				m_label.text += str;
			}
			else
			{
				m_label.text += ch;
			}
			i++;
		}
		if (!m_pressed)
		{
			m_done = true;
		}
		Reset();
	}

	private void OnPress(bool isPressed)
	{
		m_pressed = isPressed;
		if (!m_pressed)
		{
			OnRelease();
		}
	}

	private void OnRelease()
	{
		if (!m_done)
		{
			if (m_label.text.Length == m_text.Length)
			{
				m_done = true;
			}
			else
			{
				m_label.text = m_text;
			}
		}
		else
		{
			OnClose();
		}
	}

	public void Hide()
	{
		if ((bool)m_mascot)
		{
			Animation componentInChildren = MBHier.GetComponentInChildren<Animation>(m_mascot);
			if ((bool)componentInChildren)
			{
				componentInChildren.Stop();
			}
		}
		Reset();
		base.gameObject.SetActiveRecursively(false);
	}
}
