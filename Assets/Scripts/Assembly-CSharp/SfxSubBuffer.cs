using UnityEngine;

public class SfxSubBuffer
{
	private float m_timeLastUsed;

	private int m_lastIndex;

	private int m_sourceCount;

	private GameObject m_gameObject;

	private AudioSource[] m_audioSource;

	public SfxSubBuffer(string p_identifier, int p_sourceCount, AudioClip p_soundClip)
	{
		m_lastIndex = 0;
		m_timeLastUsed = 0f;
		m_sourceCount = p_sourceCount;
		m_gameObject = new GameObject(p_identifier);
		m_audioSource = new AudioSource[p_sourceCount];
		if ((bool)SingletonComponent<SoundManager>.Instance.GetGameObj())
		{
			m_gameObject.transform.parent = SingletonComponent<SoundManager>.Instance.GetGameObj().transform;
		}
		for (int i = 0; i < p_sourceCount; i++)
		{
			m_audioSource[i] = m_gameObject.AddComponent<AudioSource>();
			m_audioSource[i].clip = p_soundClip;
			MBClip3D.Apply(m_audioSource[i]);   // PORT (CD-047)
			m_audioSource[i].volume = 1f;
			m_audioSource[i].pitch = 1f;
			m_audioSource[i].bypassEffects = false;
			m_audioSource[i].playOnAwake = false;
			m_audioSource[i].dopplerLevel = 0f;
		}
	}

	public SfxSubBuffer(string p_identifier, int p_sourceCount, AudioClip p_soundClip, Transform p_where)
	{
		m_lastIndex = 0;
		m_timeLastUsed = 0f;
		m_sourceCount = p_sourceCount;
		m_gameObject = new GameObject(p_identifier);
		m_audioSource = new AudioSource[p_sourceCount];
		for (int i = 0; i < p_sourceCount; i++)
		{
			m_audioSource[i] = m_gameObject.AddComponent<AudioSource>();
			m_audioSource[i].clip = p_soundClip;
			MBClip3D.Apply(m_audioSource[i]);   // PORT (CD-047)
			m_audioSource[i].volume = 1f;
			m_audioSource[i].pitch = 1f;
			m_audioSource[i].bypassEffects = false;
			m_audioSource[i].playOnAwake = false;
			m_audioSource[i].transform.parent = p_where;
			m_audioSource[i].transform.position = p_where.position;
		}
	}

	public void CollectGarbage()
	{
		if ((bool)m_gameObject)
		{
			Object.Destroy(m_gameObject);
			m_gameObject = null;
		}
	}

	public AudioSource Play(float p_timeNow)
	{
		m_timeLastUsed = p_timeNow;
		AudioSource audioSource = m_audioSource[m_lastIndex];
		if (audioSource.isPlaying)
		{
			return audioSource;
		}
		audioSource.Play();
		m_lastIndex++;
		if (m_lastIndex >= m_sourceCount)
		{
			m_lastIndex = 0;
		}
		return audioSource;
	}

	public bool StopMusicOnStart()
	{
		AudioSource audioSource = m_audioSource[m_lastIndex];
		if (audioSource == null)
		{
			return false;
		}
		if (audioSource.isPlaying)
		{
			audioSource.Stop();
			return true;
		}
		return false;
	}

	public float GetLastTimeUsed()
	{
		return m_timeLastUsed;
	}
}
