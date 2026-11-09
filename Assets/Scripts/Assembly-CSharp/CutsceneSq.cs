using UnityEngine;

public class CutsceneSq : MonoBehaviour
{
	public bool m_loop;

	public float[] m_sequenceTime;

	protected int m_index;

	protected float m_time;

	public virtual void Awake()
	{
		Messenger.AddListener(CutsceneMessage.CUTSCENE_PLAYER_BEGIN, OnBeginPlay);
		Messenger<GameObject>.AddListener(CutsceneMessage.CUTSCENE_EVENT_ENABLED, OnCutsceneEnabled);
		Messenger<GameObject>.AddListener(CutsceneMessage.CUTSCENE_EVENT_DISABLED, OnCutsceneDisabled);
	}

	protected virtual void OnDestroy()
	{
		Messenger.RemoveListener(CutsceneMessage.CUTSCENE_PLAYER_BEGIN, OnBeginPlay);
		Messenger<GameObject>.RemoveListener(CutsceneMessage.CUTSCENE_EVENT_ENABLED, OnCutsceneEnabled);
		Messenger<GameObject>.RemoveListener(CutsceneMessage.CUTSCENE_EVENT_DISABLED, OnCutsceneDisabled);
	}

	protected virtual void OnEnable()
	{
		if (!CanPlay())
		{
			base.enabled = false;
		}
	}

	protected virtual void OnDisable()
	{
		m_index = 0;
		m_time = 0f;
	}

	protected virtual void OnBeginPlay()
	{
		Play();
	}

	protected virtual void OnCutsceneEnabled(GameObject p_obj)
	{
	}

	protected virtual void OnCutsceneDisabled(GameObject p_obj)
	{
	}

	public virtual bool CanPlay()
	{
		return m_sequenceTime != null;
	}

	public void Play()
	{
		if (!CanPlay())
		{
			Dbg.LogWarn("There's some invalid parameter.");
			base.enabled = false;
			return;
		}
		m_time = 0f;
		m_index = 0;
		base.enabled = true;
		OnBeginSequence();
	}

	protected virtual void OnBeginSequence()
	{
	}

	protected virtual void PlayNextSequence(int p_index)
	{
	}

	protected virtual void Update()
	{
		if (!CanPlay())
		{
			base.enabled = false;
			return;
		}
		m_time += Time.deltaTime;
		if (m_index >= m_sequenceTime.Length)
		{
			return;
		}
		while (m_time > m_sequenceTime[m_index])
		{
			PlayNextSequence(m_index);
			if (++m_index >= m_sequenceTime.Length)
			{
				if (m_loop)
				{
					m_time = 0f;
					m_index = 0;
				}
				break;
			}
		}
	}

	private void OnWindUp(string p_param)
	{
	}

	private void OnDamageWindow()
	{
	}

	private void OnWindDown()
	{
	}

	private void OnPerfectDodgeWindowStart()
	{
	}

	private void OnDodgeWindowStart()
	{
	}

	private void OnDodgeWindowEnd()
	{
	}

	private void EnableTrailRenderer()
	{
	}

	private void OnAttackEnd()
	{
	}

	private void OnTrailStart()
	{
	}

	private void OnTrailEnd()
	{
	}
}
