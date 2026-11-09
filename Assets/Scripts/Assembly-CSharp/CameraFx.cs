using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public abstract class CameraFx : MonoBehaviour
{
	protected enum Type
	{
		None = 0,
		Rotation = 1,
		Position = 2,
		FOV = 4
	}

	public delegate void Callback();

	private static List<CameraFx> m_playingEffects = new List<CameraFx>();

	private bool m_realTimeUpdate = true;

	protected int m_type;

	public float m_delay;

	private float m_delayTime;

	public bool m_playOnStart;

	public Callback beginCallback;

	public Callback endCallback;

	public bool interuptable = true;

	public bool RealtimeUpdate
	{
		get
		{
			return m_realTimeUpdate;
		}
		set
		{
			m_realTimeUpdate = value;
		}
	}

	public float Delay
	{
		get
		{
			return m_delay;
		}
		set
		{
			m_delay = Mathf.Max(value, 0f);
		}
	}

	protected virtual void Awake()
	{
		if (!m_playOnStart)
		{
			base.enabled = false;
		}
	}

	private void Start()
	{
		if (m_playOnStart)
		{
			Play();
		}
	}

	public void Play()
	{
		for (int num = m_playingEffects.Count - 1; num >= 0; num--)
		{
			if (m_playingEffects[num] != null && m_playingEffects[num].m_type == m_type)
			{
				m_playingEffects[num]._Stop();
			}
			m_playingEffects.RemoveAt(num);
		}
		base.enabled = true;
		m_delayTime = m_delay;
		Dbg.Log(string.Concat(base.gameObject, " is playing camera Effect '", GetType().ToString(), "'."));
	}

	private void BeginPlay()
	{
		bool flag = true;
		List<CameraFx> list = new List<CameraFx>();
		foreach (CameraFx playingEffect in m_playingEffects)
		{
			if (playingEffect != null && (playingEffect.m_type & m_type) != 0)
			{
				if (!playingEffect.interuptable)
				{
					flag = false;
					break;
				}
				list.Add(playingEffect);
			}
		}
		if (!flag)
		{
			Dbg.Log("'" + GetType().ToString() + "' cannot start.");
			return;
		}
		foreach (CameraFx item in list)
		{
			item._Stop();
			m_playingEffects.Remove(item);
		}
		m_playingEffects.Add(this);
		OnPlay();
		if (beginCallback != null)
		{
			beginCallback();
		}
	}

	public void Stop()
	{
		_Stop();
		RemoveFromPlayingList();
	}

	public void Pause(bool p_pause)
	{
		base.enabled = !p_pause;
	}

	public static void StopPlayingEffects()
	{
		for (int i = 0; i < m_playingEffects.Count; i++)
		{
			if (m_playingEffects[i] != null)
			{
				m_playingEffects[i]._Stop();
			}
		}
		m_playingEffects.Clear();
	}

	public static void PausePlayingEffects(bool p_pause)
	{
		for (int num = m_playingEffects.Count - 1; num >= 0; num--)
		{
			if (m_playingEffects[num] == null)
			{
				m_playingEffects.RemoveAt(num);
			}
			else
			{
				m_playingEffects[num].enabled = !p_pause;
			}
		}
	}

	private void LateUpdate()
	{
		float num = Time.deltaTime;
		if (Time.timeScale.Equals(0f))
		{
			return;
		}
		if (m_realTimeUpdate)
		{
			num /= Time.timeScale;
		}
		if (m_delayTime >= 0f)
		{
			m_delayTime -= num;
			if (!(m_delayTime < 0f))
			{
				return;
			}
			BeginPlay();
		}
		OnUpdate(num);
		if (IsFinished())
		{
			base.enabled = false;
			RemoveFromPlayingList();
			if (endCallback != null)
			{
				endCallback();
			}
		}
	}

	private void _Stop()
	{
		Dbg.Log(string.Concat(GetType(), ":_Stop"));
		if (!IsFinished())
		{
			Dbg.Log(string.Concat(GetType(), ":OnInterrupted"));
			OnInterrupted();
		}
		base.enabled = false;
	}

	private void RemoveFromPlayingList()
	{
		for (int i = 0; i < m_playingEffects.Count; i++)
		{
			if (m_playingEffects[i] == this)
			{
				m_playingEffects.RemoveAt(i);
				break;
			}
		}
	}

	public abstract bool IsFinished();

	protected abstract void OnUpdate(float p_deltaTime);

	protected abstract void OnPlay();

	protected abstract void OnInterrupted();
}
