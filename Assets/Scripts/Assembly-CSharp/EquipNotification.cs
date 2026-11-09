using System.Collections;
using UnityEngine;

public class EquipNotification : MonoBehaviour
{
	public UISprite m_background;

	public UILabel m_label;

	public float m_showDuration = 3f;

	public float m_fadeDuration = 0.5f;

	private void Awake()
	{
		Dbg.Assert(m_background);
		Dbg.Assert(m_label);
	}

	private IEnumerator Start()
	{
		yield return new MBWaitForSeconds(m_showDuration);   // PORT (CD-059)
		FadeOut();
	}

	private void FadeOut()
	{
		if ((bool)m_background && (bool)m_label)
		{
			TweenColor.Begin(m_background.gameObject, m_fadeDuration, Color.clear);
			TweenColor tweenColor = TweenColor.Begin(m_label.gameObject, m_fadeDuration, Color.clear);
			if ((bool)tweenColor)
			{
				tweenColor.eventReceiver = base.gameObject;
				tweenColor.callWhenFinished = "OnFadeOutDone";
			}
		}
	}

	private void OnFadeOutDone()
	{
		if (this != null)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
