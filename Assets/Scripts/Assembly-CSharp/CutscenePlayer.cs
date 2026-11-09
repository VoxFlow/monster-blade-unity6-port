using System;
using System.Collections.Generic;
using UnityEngine;

public class CutscenePlayer : MonoBehaviour
{
	private float m_time;

	private int m_nextEvent;

	private List<AnimationState> m_playingAnimations = new List<AnimationState>();

	public float totalPlayTime;

	private bool m_pause;

	private bool m_canFadeOut = true;

	private float m_fadeTime = 1f;

	private GameObject m_cutsceneBg;

	private GameObject[] m_backgrounds;

	[SerializeField]
	public List<CutsceneEvent> m_events = new List<CutsceneEvent>();

	public List<CutsceneEvent> Events
	{
		get
		{
			return m_events;
		}
	}

	public bool Pause
	{
		get
		{
			return m_pause;
		}
		set
		{
			float speed = ((!value) ? 1f : 0f);
			for (int i = 0; i < m_playingAnimations.Count; i++)
			{
				if (m_playingAnimations[i] != null)
				{
					m_playingAnimations[i].speed = speed;
				}
			}
			for (int j = 0; j < base.transform.childCount; j++)
			{
				base.transform.GetChild(j).gameObject.BroadcastMessage("Pause", value, SendMessageOptions.DontRequireReceiver);
			}
			m_pause = value;
		}
	}

	public void AddEvent(CutsceneEvent p_event)
	{
		int num = 0;
		for (num = m_events.Count - 1; num >= 0; num--)
		{
			if (p_event.CompareTo(m_events[num]) < 0)
			{
				m_events.Insert(num, p_event);
				break;
			}
		}
		if (num < 0)
		{
			m_events.Add(p_event);
		}
	}

	public void RemoveEvent(CutsceneEvent p_event)
	{
		m_events.Remove(p_event);
	}

	public void Play()
	{
		if (base.enabled)
		{
			Stop();
		}
		m_nextEvent = 0;
		m_time = 0f;
		m_playingAnimations.Clear();
		FadeIn();
		ShowCinematicBar(true);
		m_canFadeOut = true;
		if (m_events.Count > 0)
		{
			MBSort.Sort(m_events);
			base.enabled = true;
			Time.timeScale = 1f;
			Messenger.Invoke(CutsceneMessage.CUTSCENE_PLAYER_BEGIN);
		}
	}

	public void Stop()
	{
		for (int i = 0; i < base.transform.childCount; i++)
		{
			GameObject gameObject = base.transform.GetChild(i).gameObject;
			gameObject.SetActiveRecursively(false);
		}
		CutsceneTimeScaleSq component = base.gameObject.GetComponent<CutsceneTimeScaleSq>();
		if (component != null)
		{
			UnityEngine.Object.Destroy(component);
		}
		base.enabled = false;
	}

	private void OnEnable()
	{
		Func<Transform, object, bool> p_function = (Transform current, object param) => current.tag == "Background";
		Transform transform = base.gameObject.transform.Iterate(p_function);
		if (!(null != transform))
		{
			return;
		}
		m_cutsceneBg = transform.gameObject;
		m_backgrounds = GameObject.FindGameObjectsWithTag("Background");
		for (int num = 0; num < m_backgrounds.Length; num++)
		{
			if (m_cutsceneBg != m_backgrounds[num])
			{
				m_backgrounds[num].SetActiveRecursively(false);
			}
		}
	}

	private void OnDisable()
	{
		if (!(null != m_cutsceneBg))
		{
			return;
		}
		for (int i = 0; i < m_backgrounds.Length; i++)
		{
			if (m_cutsceneBg != m_backgrounds[i] && null != m_backgrounds[i])
			{
				m_backgrounds[i].SetActiveRecursively(true);
			}
		}
	}

	private void Start()
	{
		Play();
	}

	private void FixedUpdate()
	{
		if (m_pause)
		{
			return;
		}
		m_time += Time.deltaTime;
		for (int i = 0; i < m_playingAnimations.Count; i++)
		{
			if (m_playingAnimations[i] == null || m_playingAnimations[i].normalizedTime >= 1f)
			{
				m_playingAnimations.RemoveAt(i);
			}
		}
		for (int j = m_nextEvent; j < m_events.Count; j++)
		{
			CutsceneEvent cutsceneEvent = m_events[m_nextEvent];
			if (cutsceneEvent.time > m_time)
			{
				break;
			}
			Transform transform = base.transform.Find(cutsceneEvent.actor);
			ParticleSystemController[] array = null;
			if (transform != null)
			{
				switch (cutsceneEvent.action)
				{
				case CutsceneEvent.Action.Acitvate:
				{
					transform.gameObject.SetActiveRecursively(true);
					Messenger<GameObject>.Invoke(CutsceneMessage.CUTSCENE_EVENT_ENABLED, transform.gameObject);
					array = MBHier.GetComponentsInChildren<ParticleSystemController>(transform);
					for (int m = 0; m < array.Length; m++)
					{
						array[m].Stop();
					}
					Dbg.Log("Cutscene Enabled: " + transform.gameObject.name);
					break;
				}
				case CutsceneEvent.Action.Deacitvate:
					Messenger<GameObject>.Invoke(CutsceneMessage.CUTSCENE_EVENT_DISABLED, transform.gameObject);
					transform.gameObject.SetActiveRecursively(false);
					Dbg.Log("Cutscene Disabled: " + transform.gameObject.name);
					break;
				case CutsceneEvent.Action.PlayAnim:
					m_playingAnimations.Add(transform.GetComponent<Animation>().CrossFadeQueued(cutsceneEvent.stringParam, 0f, QueueMode.PlayNow));
					break;
				case CutsceneEvent.Action.PlayParticle:
				{
					array = MBHier.GetComponentsInChildren<ParticleSystemController>(transform, true);
					for (int l = 0; l < array.Length; l++)
					{
						if (array[l].name == cutsceneEvent.stringParam)
						{
							array[l].Play();
						}
					}
					break;
				}
				case CutsceneEvent.Action.StopParticle:
				{
					array = MBHier.GetComponentsInChildren<ParticleSystemController>(transform);
					for (int k = 0; k < array.Length; k++)
					{
						if (array[k].name == cutsceneEvent.stringParam)
						{
							array[k].Stop();
						}
					}
					break;
				}
				}
			}
			m_nextEvent++;
		}
		if (m_canFadeOut && m_time > totalPlayTime - m_fadeTime)
		{
			FadeOut();
			ShowCinematicBar(false);
			m_canFadeOut = false;
		}
		if (m_time >= totalPlayTime)
		{
			Messenger.Invoke(CutsceneMessage.CUTSCENE_PLAYER_FINISH);
			Stop();
			FadeIn();
			m_canFadeOut = true;
		}
	}

	protected void FadeIn()
	{
		if ((bool)SingletonComponent<FaderGui>.Instance && m_fadeTime != 0f)
		{
			SingletonComponent<FaderGui>.Instance.FadeIn(new Color(0f, 0f, 0f, 1f), m_fadeTime);
		}
	}

	protected void FadeOut()
	{
		if ((bool)SingletonComponent<FaderGui>.Instance && m_fadeTime != 0f)
		{
			SingletonComponent<FaderGui>.Instance.FadeOut(new Color(0f, 0f, 0f, 1f), m_fadeTime);
		}
	}

	protected void ShowCinematicBar(bool p_show)
	{
		if (SingletonComponent<BattleScreenUI>.Instance != null)
		{
			SingletonComponent<BattleScreenUI>.Instance.EnableCinematicBar(p_show, m_fadeTime);
		}
	}
}
