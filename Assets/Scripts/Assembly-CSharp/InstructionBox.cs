using System.Collections;
using UnityEngine;

public class InstructionBox : MonoBehaviour
{
	public enum Anchor
	{
		Top = 0,
		Bottom = 1
	}

	public Transform m_container;

	public UILabel m_label;

	private float m_tweenSpeed = 1.5f;

	private NBUILocalize m_localize;

	private UIAnchor m_anchor;

	public string text
	{
		get
		{
			if (!m_label)
			{
				return string.Empty;
			}
			return m_label.text;
		}
		set
		{
			if ((bool)m_label)
			{
				if ((bool)m_localize)
				{
					m_localize.Key = value;
				}
				else
				{
					m_label.text = value;
				}
			}
		}
	}

	public Anchor anchor
	{
		get
		{
			if ((bool)m_anchor && m_anchor.side == UIAnchor.Side.Bottom)
			{
				return Anchor.Bottom;
			}
			return Anchor.Top;
		}
		set
		{
			if ((bool)m_anchor && (bool)m_container)
			{
				switch (value)
				{
				case Anchor.Top:
					m_anchor.side = UIAnchor.Side.Top;
					m_container.transform.localPosition = Vector3.up * 40f;
					break;
				case Anchor.Bottom:
					m_anchor.side = UIAnchor.Side.Bottom;
					m_container.transform.localPosition = Vector3.up * -40f;
					break;
				}
			}
		}
	}

	private void Awake()
	{
		m_localize = MBHier.GetComponentInChildren<NBUILocalize>(this);
		m_anchor = GetComponent<UIAnchor>();
		Dbg.Assert(m_localize);
		Dbg.Assert(m_anchor);
		Dbg.Assert(m_container);
		Dbg.Assert(m_label);
	}

	public void Show()
	{
		if ((bool)m_container)
		{
			iTween.StopImmediate(base.gameObject);
			int num = ((anchor != Anchor.Top) ? 1 : (-1));
			Hashtable hashtable = new Hashtable();
			hashtable.Add("position", Vector3.up * 40f * num);
			hashtable.Add("time", 1f / m_tweenSpeed);
			hashtable.Add("ignoretimescale", true);
			hashtable.Add("islocal", true);
			iTween.MoveTo(m_container.gameObject, hashtable);
			m_container.gameObject.SetActiveRecursively(true);
		}
	}

	public void Hide()
	{
		if ((bool)m_container)
		{
			iTween.StopImmediate(base.gameObject);
			int num = ((anchor == Anchor.Top) ? 1 : (-1));
			Hashtable hashtable = new Hashtable();
			hashtable.Add("position", Vector3.up * 40f * num);
			hashtable.Add("time", 1f / m_tweenSpeed);
			hashtable.Add("ignoretimescale", true);
			hashtable.Add("islocal", true);
			hashtable.Add("oncomplete", "OnHideCompleted");
			hashtable.Add("oncompletetarget", base.gameObject);
			iTween.MoveTo(m_container.gameObject, hashtable);
		}
	}

	private void OnHideCompleted()
	{
		m_container.gameObject.SetActiveRecursively(false);
	}
}
