// WaitForSeconds with Unity 4.0.1's resume rule (single-precision wake time, first frame at or past it). Register CD-059.
using UnityEngine;

public sealed class MBWaitForSeconds : CustomYieldInstruction
{
	private readonly float m_wake;
	private readonly int m_startFrame;

	public MBWaitForSeconds(float seconds)
	{
		m_wake = (float)(Time.timeAsDouble + seconds);
		m_startFrame = Time.frameCount;
	}

	public override bool keepWaiting
	{
		get { return Time.frameCount == m_startFrame || (float)Time.timeAsDouble < m_wake; }
	}
}
