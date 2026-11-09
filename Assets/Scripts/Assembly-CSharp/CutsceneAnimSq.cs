using UnityEngine;

public class CutsceneAnimSq : CutsceneSq
{
	public string[] m_animationList;

	public float[] m_blendTime;

	public int[] m_startFrames;

	public int[] m_holdFrames;

	protected AnimationState m_playingAnim;

	public void CopyPlayList(CutsceneAnimSq p_source)
	{
		if (p_source.m_animationList != null)
		{
			m_animationList = (string[])p_source.m_animationList.Clone();
		}
		if (p_source.m_blendTime != null)
		{
			m_blendTime = (float[])p_source.m_blendTime.Clone();
		}
		if (p_source.m_sequenceTime != null)
		{
			m_sequenceTime = (float[])p_source.m_sequenceTime.Clone();
		}
		if (p_source.m_startFrames != null)
		{
			m_startFrames = (int[])p_source.m_startFrames.Clone();
		}
		if (p_source.m_holdFrames != null)
		{
			m_holdFrames = (int[])p_source.m_holdFrames.Clone();
		}
	}

	public override bool CanPlay()
	{
		return base.CanPlay() && m_animationList != null && m_sequenceTime != null && base.GetComponent<Animation>() != null;
	}

	protected override void OnBeginSequence()
	{
		base.OnBeginSequence();
		if (m_animationList.Length > 0)
		{
			base.GetComponent<Animation>().Rewind();
			base.GetComponent<Animation>().Play(m_animationList[0]);
			base.GetComponent<Animation>().Sample();
			base.GetComponent<Animation>().Stop();
		}
	}

	protected override void PlayNextSequence(int p_index)
	{
		base.PlayNextSequence(p_index);
		float fadeLength = ((m_blendTime != null && m_blendTime.Length > p_index) ? m_blendTime[p_index] : 0.3f);
		if (base.GetComponent<Animation>()[m_animationList[p_index]] != null)
		{
			Dbg.Log("CutsceneAnimSq - Playing animation '" + m_animationList[p_index] + "'");
			m_playingAnim = base.GetComponent<Animation>().CrossFadeQueued(m_animationList[p_index], fadeLength, QueueMode.PlayNow);
			m_playingAnim.speed = 1f;
			if (m_startFrames != null && p_index < m_startFrames.Length)
			{
				m_playingAnim.SetFrame(m_startFrames[p_index]);
			}
		}
		else
		{
			Dbg.LogWarn("Animation '" + m_animationList[p_index] + "'is not avialable");
		}
	}

	protected override void Update()
	{
		base.Update();
		if (!CanPlay())
		{
			return;
		}
		int num = m_index - 1;
		if (m_playingAnim != null && m_holdFrames != null && num >= 0 && num < m_holdFrames.Length && m_holdFrames[num] >= 0)
		{
			int frame = m_playingAnim.GetFrame();
			if (frame >= m_holdFrames[num])
			{
				m_playingAnim.SetFrame(m_holdFrames[num]);
				m_playingAnim.speed = 0f;
				base.GetComponent<Animation>().Sample();
			}
		}
	}
}
