using UnityEngine;

public class CutsceneTimeScaleSq : MonoBehaviour
{
	protected float m_time;

	public float[] m_sequenceTime;

	public float[] m_timeScale;

	protected int m_index;

	public virtual void Awake()
	{
		m_time = 0f;
		m_index = 0;
		base.enabled = true;
	}

	public virtual void Update()
	{
		if (m_timeScale == null)
		{
			base.enabled = false;
			Dbg.LogWarn("m_timeScale is null.");
			return;
		}
		if (m_index >= m_sequenceTime.Length)
		{
			base.enabled = false;
			return;
		}
		m_time += Time.deltaTime;
		if (!(m_time > m_sequenceTime[m_index]))
		{
			return;
		}
		float num = m_timeScale[m_index];
		CutsceneController instance = SingletonComponent<CutsceneController>.Instance;
		if (instance != null)
		{
			if (m_index < m_sequenceTime.Length)
			{
				instance.globalTimeScale = num;
			}
		}
		else if (m_index < m_sequenceTime.Length)
		{
			Time.timeScale = num;
		}
		m_index++;
		if (m_index >= m_sequenceTime.Length)
		{
			base.enabled = false;
			m_index = 0;
			m_time = 0f;
		}
	}
}
