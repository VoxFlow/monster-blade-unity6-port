using UnityEngine;

public class MainMenuPet : MonoBehaviour
{
	private NBTimer m_randomTimer;

	[SerializeField]
	private string[] m_animationList;

	private string m_playingClip = string.Empty;

	private AnimationState m_playingAnim;

	public float m_randomMin = 2f;

	public float m_randomMax = 3.5f;

	private Animation m_anim;

	private void Start()
	{
		m_anim = MBHier.GetComponentInChildren<Animation>(this);
		if (null != m_anim)
		{
			m_anim.clip = null;
			m_anim.playAutomatically = false;
			m_anim.Play("Idle_Happy_01");
		}
		base.enabled = false;
	}

	private void RandomIdleAnim(bool p_firstTime)
	{
		if (null == m_anim || m_animationList == null)
		{
			return;
		}
		int num = m_animationList.Length;
		if (num > 0)
		{
			int num2 = Random.Range(0, num);
			string playingClip = m_animationList[num2];
			AnimationState animationState = (p_firstTime ? null : m_anim[m_playingClip]);
			m_playingAnim = m_anim.CrossFadeQueued(playingClip, 0.3f, QueueMode.PlayNow);
			if (m_anim[playingClip].wrapMode != WrapMode.Loop)
			{
				m_randomTimer.CountDown(float.MaxValue);
			}
			else
			{
				float p_limit = Random.Range(m_randomMin, m_randomMax);
				m_randomTimer.CountDown(p_limit);
			}
			m_playingClip = playingClip;
		}
	}

	private void Update()
	{
		if (null == m_playingAnim || m_playingAnim.normalizedTime >= 1f || m_randomTimer.Counter == 0f)
		{
			RandomIdleAnim(false);
		}
	}
}
