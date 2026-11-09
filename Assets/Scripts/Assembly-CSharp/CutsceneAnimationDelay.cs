using UnityEngine;

public class CutsceneAnimationDelay : MonoBehaviour
{
	private NBTimer m_delayTimer;

	public float m_delay;

	private void OnDelayEnd()
	{
		base.GetComponent<Animation>().Play(base.GetComponent<Animation>().clip.name);
	}

	private void Start()
	{
		m_delayTimer = base.gameObject.AddComponent<NBTimer>();
		m_delayTimer.CountDown(m_delay, "OnDelayEnd");
		base.GetComponent<Animation>().Stop();
	}
}
