using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NBAnimation : MonoBehaviour
{
	public string m_animationConfig = string.Empty;

	private string m_currentAnimName = string.Empty;

	private bool m_isPlaying;

	private AnimationState m_currentAnim;

	private float m_timeScale = 1f;

	private bool m_pause;

	private Dictionary<string, Dictionary<float, List<AnimationEvent>>> m_animEvents;

	private string[] m_animList;

	private float m_fixedUpdateTime;

	private bool m_fixedUpdate;

	private float m_animSpeed = 1f;

	private List<string> m_aliasList;

	public AnimationState CurrentAnim
	{
		get
		{
			return m_currentAnim;
		}
	}

	public string CurrentAnimName
	{
		get
		{
			if (m_currentAnim == null)
			{
				return string.Empty;
			}
			return m_currentAnimName;
		}
	}

	public bool IsPaused
	{
		get
		{
			return m_pause;
		}
	}

	public bool IsPlaying
	{
		get
		{
			return m_currentAnim != null && m_isPlaying;
		}
	}

	public float TimeScale
	{
		get
		{
			return m_timeScale;
		}
		set
		{
			if (m_currentAnim != null)
			{
				m_currentAnim.speed = this[m_currentAnimName].speed * value;
				if (m_fixedUpdate)
				{
					m_animSpeed = m_currentAnim.speed;
				}
			}
			if (value < 0f)
			{
				Dbg.LogWarn("The value of NBAnimation.TimeScale cannot be minus or zero.");
				TimeScale = 1f;
			}
			else
			{
				m_timeScale = value;
			}
		}
	}

	public float RemainingPlayTime
	{
		get
		{
			if (m_currentAnim == null || m_currentAnim.speed <= 0f || m_timeScale == 0f)
			{
				return 0f;
			}
			return (m_currentAnim.length - m_currentAnim.time) / (m_currentAnim.speed * m_timeScale);
		}
	}

	public string[] AnimationList
	{
		get
		{
			return m_animList;
		}
	}

	public string ConfigFile
	{
		get
		{
			return m_animationConfig;
		}
		set
		{
			m_animationConfig = value;
		}
	}

	public AnimationState this[string p_alias]
	{
		get
		{
			return base.GetComponent<Animation>()[p_alias];
		}
	}

	private void Awake()
	{
		if (m_animEvents == null)
		{
			m_animEvents = new Dictionary<string, Dictionary<float, List<AnimationEvent>>>();
		}
		if (m_animationConfig != string.Empty)
		{
			ParseConfig();
		}
		List<string> list = new List<string>();
		foreach (AnimationState item in base.GetComponent<Animation>())
		{
			list.Add(item.name);
		}
		m_animList = list.ToArray();
		base.GetComponent<Animation>().Stop();
		base.GetComponent<Animation>().clip = null;
	}

	private void Start()
	{
	}

	public AnimationClip AddAnimationClip(string p_clipPath, string p_clipName)
	{
		AnimationClip animationClip = null;
		if (base.GetComponent<Animation>()[p_clipName] == null)
		{
			animationClip = Resources.Load(p_clipPath, typeof(AnimationClip)) as AnimationClip;
			if (animationClip == null)
			{
				Dbg.LogError("Unable to load animation clip: " + p_clipPath + ". Either the resource doesn't exist or the data is invalid.");
			}
			else
			{
				base.GetComponent<Animation>().AddClip(animationClip, p_clipName);
			}
		}
		else
		{
			animationClip = this[p_clipName].clip;
			Dbg.LogWarn(string.Concat(base.gameObject, " already had animation with name ", p_clipName, " registered."));
		}
		return animationClip;
	}

	public void AddAnimEvent(string p_clipName, string p_eventString, bool isFrameBase = false)
	{
		AnimationEvent animationEvent = new AnimationEvent();
		bool flag = true;
		string text = "Unable to parse animation event. Animation: " + p_clipName + ", Event String: " + p_eventString + ".";
		for (int i = 0; i < 1; flag = false, i++)
		{
			string text2 = p_eventString.Trim('[', ']');
			string[] array = text2.Split(',');
			if (array.Length != 4)
			{
				text += " Invalid event string!";
				break;
			}
			AnimationState animationState = this[p_clipName];
			if (animationState == null)
			{
				text += " Invalid animation!";
				break;
			}
			string functionName = array[0];
			string text3 = array[1];
			string text4 = array[2];
			float result = 0f;
			animationEvent.functionName = functionName;
			if (!float.TryParse(array[3], out result))
			{
				text += " Invalid event time!";
				break;
			}
			if (isFrameBase && animationState.clip.frameRate > 0f)
			{
				animationEvent.time = result / animationState.clip.frameRate;
			}
			else
			{
				animationEvent.time = result;
			}
			switch (text3)
			{
			case "string":
				animationEvent.stringParameter = text4;
				continue;
			case "int":
			{
				int result3 = 0;
				if (!int.TryParse(text4, out result3))
				{
					text += " Invalid parameter!";
					break;
				}
				animationEvent.intParameter = result3;
				continue;
			}
			case "float":
			{
				float result2 = 0f;
				if (!float.TryParse(text4, out result2))
				{
					text += " Invalid parameter!";
					break;
				}
				animationEvent.floatParameter = result2;
				continue;
			}
			default:
				continue;
			}
			break;
		}
		if (!flag)
		{
			AddAnimEvent(p_clipName, animationEvent);
		}
		else
		{
			Dbg.LogWarn(text);
		}
	}

	public void AddAnimEvent(string p_clipName, AnimationEvent p_event)
	{
		AnimationState animationState = this[p_clipName];
		if (animationState == null)
		{
			Dbg.LogError(string.Concat(base.gameObject, " has no registered animation by the name: ", p_clipName));
			return;
		}
		Dictionary<float, List<AnimationEvent>> dictionary = null;
		AnimationClip clip = animationState.clip;
		if (m_animEvents.ContainsKey(p_clipName))
		{
			dictionary = m_animEvents[p_clipName];
		}
		else
		{
			dictionary = new Dictionary<float, List<AnimationEvent>>();
			m_animEvents.Add(p_clipName, dictionary);
		}
		List<AnimationEvent> list = null;
		if (dictionary.ContainsKey(p_event.time))
		{
			list = dictionary[p_event.time];
		}
		else
		{
			list = new List<AnimationEvent>();
			dictionary.Add(p_event.time, list);
		}
		clip.AddEvent(p_event);
		list.Add(p_event);
	}

	public float GetEventTime(string p_clipName, string p_eventName)
	{
		if (!m_animEvents.ContainsKey(p_clipName))
		{
			Dbg.LogError(p_clipName + " does not have event registered.");
			return -1f;
		}
		Dictionary<float, List<AnimationEvent>> dictionary = m_animEvents[p_clipName];
		foreach (KeyValuePair<float, List<AnimationEvent>> item in dictionary)
		{
			foreach (AnimationEvent item2 in item.Value)
			{
				if (item2.functionName == p_eventName)
				{
					return item.Key;
				}
			}
		}
		Dbg.LogError("Cannot find event '" + p_eventName + "' in animation clip '" + p_clipName + "'.");
		return -1f;
	}

	public float GetAnimLength(string p_anim)
	{
		return this[p_anim].length / this[p_anim].speed;
	}

	public float GetCurrentAnimNormalizeTime()
	{
		if (m_currentAnim == null)
		{
			return 0f;
		}
		return m_currentAnim.normalizedTime;
	}

	private bool IsCurrentAnimEnd()
	{
		return m_currentAnim == null || (m_currentAnim.wrapMode != WrapMode.Loop && m_currentAnim.wrapMode != WrapMode.PingPong && m_currentAnim.normalizedTime >= 1f);
	}

	private void OnEnable()
	{
		Pause(false);
	}

	private void OnDisable()
	{
		Pause(true);
	}

	private void OnDestroy()
	{
		if (m_animEvents != null)
		{
			foreach (KeyValuePair<string, Dictionary<float, List<AnimationEvent>>> animEvent in m_animEvents)
			{
				foreach (KeyValuePair<float, List<AnimationEvent>> item in animEvent.Value)
				{
					item.Value.Clear();
				}
				animEvent.Value.Clear();
			}
			m_animEvents.Clear();
			m_animEvents = null;
		}
		RemoveAllClips();
	}

	public void ParseConfig()
	{
		ParseConfig(m_animationConfig);
	}

	public void ParseConfig(string p_config)
	{
		Dbg.Log("Parsing config: " + p_config);
		TextAsset textAsset = Resources.Load(p_config, typeof(TextAsset)) as TextAsset;
		if (textAsset == null)
		{
			Dbg.LogError(p_config + " does not exist.");
			base.enabled = false;
			return;
		}
		string text = textAsset.text.ToString();
		string[] array = text.Split('\r', '\n');
		if (array == null || array.Length == 0)
		{
			Dbg.LogError("Config file \"" + p_config + "\" is empty.");
			base.enabled = false;
			return;
		}
		string text2 = string.Empty;
		string text3 = string.Empty;
		int num = 0;
		string[] array2 = array;
		foreach (string text4 in array2)
		{
			num++;
			if (text4 == string.Empty || text4[0] == '#')
			{
				continue;
			}
			switch (text4)
			{
			case "[resource]":
				text2 = "res";
				continue;
			case "[remove]":
				text2 = "rmv";
				continue;
			case "[clip]":
				text2 = "clp";
				continue;
			case "[alias]":
				text2 = "als";
				continue;
			case "[event]":
				text2 = "evt";
				continue;
			case "[event_frame]":
				text2 = "evt_frame";
				continue;
			case "[wrap_mode]":
				text2 = "wrp";
				continue;
			case "[speed]":
				text2 = "spd";
				continue;
			case "[layer]":
				text2 = "lyr";
				continue;
			}
			string[] array3 = text4.Split('=');
			if (text2 != "rmv" && array3.Length != 2)
			{
				Dbg.LogError("Parsing error at line " + num + ". : Unknown token \"" + text4 + "\"");
				base.enabled = false;
				return;
			}
			string text5 = array3[0];
			switch (text2)
			{
			case "res":
				if (array3[0] == "path")
				{
					text3 = array3[1];
				}
				break;
			case "rmv":
				if (this[array3[0]] != null)
				{
					base.GetComponent<Animation>().RemoveClip(array3[0]);
				}
				break;
			case "clp":
				AddAnimationClip(text3 + array3[1], array3[0]);
				break;
			case "als":
				RenameClip(array3[1], array3[0]);
				break;
			case "evt":
				AddAnimEvent(text5, array3[1]);
				break;
			case "evt_frame":
				AddAnimEvent(text5, array3[1], true);
				break;
			case "wrp":
			{
				AnimationState animationState = this[text5];
				if (!(animationState == null))
				{
					if (array3[1] == "Once")
					{
						animationState.wrapMode = WrapMode.Once;
						break;
					}
					if (array3[1] == "Loop")
					{
						animationState.wrapMode = WrapMode.Loop;
						break;
					}
					if (array3[1] == "Clamp")
					{
						animationState.wrapMode = WrapMode.Once;
						break;
					}
					if (array3[1] == "ClampForever")
					{
						animationState.wrapMode = WrapMode.ClampForever;
						break;
					}
					if (array3[1] == "PingPong")
					{
						animationState.wrapMode = WrapMode.PingPong;
						break;
					}
					Debug.LogError("Parsing error at line " + num + ". : Invalid wrapping mode: " + array3[1]);
				}
				break;
			}
			case "spd":
			{
				AnimationState animationState = this[text5];
				animationState.speed = float.Parse(array3[1]);
				break;
			}
			case "lyr":
			{
				AnimationState animationState = this[text5];
				animationState.layer = int.Parse(array3[1]);
				break;
			}
			}
		}
	}

	public void Pause(bool p_pause)
	{
		if (m_currentAnim != null)
		{
			m_currentAnim.enabled = !p_pause;
		}
		m_pause = p_pause;
	}

	public AnimationState Play(string p_anim)
	{
		return Play(p_anim, 0.3f, false);
	}

	public AnimationState Play(string p_anim, float p_fadeLength)
	{
		return Play(p_anim, p_fadeLength, false);
	}

	public AnimationState Play(string p_anim, float p_fadeLength, bool p_needRewind)
	{
		if (string.IsNullOrEmpty(p_anim))
		{
			Dbg.LogWarn("Animation name is emptied.");
			return null;
		}
		if (base.GetComponent<Animation>()[p_anim] == null)
		{
			Dbg.LogWarn("Cannot play " + p_anim + " , because, it cannot be found.");
			return null;
		}
		Dbg.Log(string.Concat(base.gameObject, " is playing animation: ", p_anim));
		if (p_needRewind)
		{
			base.GetComponent<Animation>().Rewind(p_anim);
		}
		m_isPlaying = true;
		m_currentAnim = base.GetComponent<Animation>().CrossFadeQueued(p_anim, p_fadeLength, QueueMode.PlayNow);
		m_currentAnim.speed = this[p_anim].speed * m_timeScale;
		m_currentAnimName = p_anim;
		SendMessageUpwards("OnAnimBegin", p_anim, SendMessageOptions.DontRequireReceiver);
		return m_currentAnim;
	}

	public AnimationState PlayFixedFrameRate(string p_anim, float p_fadeLength, float p_fps)
	{
		AnimationState animationState = Play(p_anim, p_fadeLength);
		if (null == animationState)
		{
			return null;
		}
		if (p_fps == 0f)
		{
			return null;
		}
		if (animationState.length <= 0f)
		{
			return null;
		}
		m_fixedUpdateTime = 1f / p_fps;
		m_fixedUpdate = true;
		m_animSpeed = animationState.speed;
		return animationState;
	}

	public IEnumerator PlayAndWait(string p_anim)
	{
		return PlayAndWait(p_anim, 0.3f, 0f);
	}

	public IEnumerator PlayAndWait(string p_anim, float p_fadeLength)
	{
		return PlayAndWait(p_anim, p_fadeLength, 0f);
	}

	public IEnumerator PlayAndWait(string p_anim, float p_fadeLength, float p_waitTime)
	{
		Play(p_anim, p_fadeLength, false);
		if (p_waitTime > 0f)
		{
			Waiter waiter = new Waiter();
			yield return StartCoroutine(waiter.WaitUntil(p_waitTime));
		}
		else
		{
			while (IsPlaying && m_currentAnimName == p_anim)
			{
				yield return null;
			}
		}
	}

	public void BlendMixedAnimOnTarget(string p_mixAnim, Transform p_target, WrapMode p_wrapMode, int addLayer)
	{
		string text = p_mixAnim + "_temp_" + p_target.name;
		base.GetComponent<Animation>().AddClip(base.GetComponent<Animation>()[p_mixAnim].clip, text);
		base.GetComponent<Animation>()[text].layer = CurrentAnim.layer + 2 + addLayer;
		base.GetComponent<Animation>()[text].blendMode = AnimationBlendMode.Blend;
		base.GetComponent<Animation>()[text].AddMixingTransform(p_target, true);
		base.GetComponent<Animation>()[text].weight = 0.1f;
		base.GetComponent<Animation>()[text].enabled = true;
		base.GetComponent<Animation>()[text].wrapMode = WrapMode.Loop;
		StartCoroutine(BlendMixedAnimOnTargetByTime(text));
	}

	public void BlendMixedAnimOnTarget(string p_mixAnim, Transform p_target, WrapMode p_wrapMode)
	{
		BlendMixedAnimOnTarget(p_mixAnim, p_target, p_wrapMode, 0);
	}

	private IEnumerator BlendMixedAnimOnTargetByTime(string p_anim)
	{
		float tempWeight = base.GetComponent<Animation>()[p_anim].weight;
		while (tempWeight < 1f)
		{
			tempWeight += Time.deltaTime * 3f;
			if (tempWeight >= 1f)
			{
				base.GetComponent<Animation>()[p_anim].weight = 1f;
			}
			else
			{
				base.GetComponent<Animation>()[p_anim].weight = tempWeight;
			}
			yield return new WaitForFixedUpdate();
		}
		base.GetComponent<Animation>().Blend(p_anim);
	}

	public void PlayMixedAnimOnTarget(string p_mixAnim, Transform p_target, WrapMode p_wrapMode)
	{
		string newName = p_mixAnim + "_temp_" + p_target.name;
		base.GetComponent<Animation>().AddClip(base.GetComponent<Animation>()[p_mixAnim].clip, newName);
		base.GetComponent<Animation>()[newName].layer = CurrentAnim.layer + 1;
		base.GetComponent<Animation>()[newName].blendMode = AnimationBlendMode.Blend;
		base.GetComponent<Animation>()[newName].AddMixingTransform(p_target, true);
		base.GetComponent<Animation>()[newName].weight = 1f;
		base.GetComponent<Animation>()[newName].enabled = true;
		base.GetComponent<Animation>()[newName].wrapMode = WrapMode.Loop;
		base.GetComponent<Animation>().CrossFadeQueued(newName);
	}

	public void StopMixedAnimImmediately(string p_anim, Transform p_transform)
	{
		base.GetComponent<Animation>()[p_anim].weight = 0f;
	}

	public void StopSmoothly(string p_anim, Transform p_transform)
	{
		StartCoroutine(StopMixedAnimByTime(p_anim, p_transform));
	}

	private IEnumerator StopMixedAnimByTime(string p_anim, Transform p_transform)
	{
		float tempWeight = base.GetComponent<Animation>()[p_anim].weight;
		while (tempWeight > 0f)
		{
			tempWeight -= Time.deltaTime * 2f;
			if (tempWeight <= 0f)
			{
				base.GetComponent<Animation>()[p_anim].weight = 0f;
			}
			else
			{
				base.GetComponent<Animation>()[p_anim].weight = tempWeight;
			}
			yield return new WaitForFixedUpdate();
		}
	}

	public IEnumerator PlaySequence(string p_anim, float p_fadeLength = 0.3f, float p_fixedFPS = 0f)
	{
		Dbg.Log("Playing animation: " + p_anim);
		AnimationState waitingAnim = null;
		waitingAnim = ((!(p_fixedFPS > 0f)) ? Play(p_anim, p_fadeLength) : PlayFixedFrameRate(p_anim, p_fadeLength, p_fixedFPS));
		float speed = waitingAnim.speed;
		waitingAnim.speed = 0f;
		while (waitingAnim != null)
		{
			if (waitingAnim.weight >= 1f)
			{
				waitingAnim.speed = speed;
				break;
			}
			yield return null;
		}
	}

	public IEnumerator Wait()
	{
		while (IsPlaying)
		{
			yield return null;
		}
	}

	public void RenameClip(string p_name, string p_alias)
	{
		AnimationState animationState = this[p_name];
		if (animationState == null)
		{
			Dbg.LogWarn("Cannot find animation name " + p_name + ".");
			return;
		}
		AnimationClip clip = animationState.clip;
		base.GetComponent<Animation>().RemoveClip(clip);
		base.GetComponent<Animation>().AddClip(clip, p_alias);
		if (m_aliasList == null)
		{
			m_aliasList = new List<string>();
		}
		m_aliasList.Add(p_alias);
	}

	public bool IsRenamedClip(string p_name)
	{
		if (m_aliasList == null)
		{
			return false;
		}
		return m_aliasList.Contains(p_name);
	}

	public void RemoveAllClips()
	{
		foreach (AnimationState item in base.GetComponent<Animation>())
		{
			base.GetComponent<Animation>().RemoveClip(item.clip);
		}
	}

	private void Update()
	{
		if (m_isPlaying && !(null == m_currentAnim) && m_fixedUpdate && m_currentAnim.speed > 0f)
		{
			if (Time.deltaTime > m_fixedUpdateTime)
			{
				m_currentAnim.speed = m_fixedUpdateTime / Time.deltaTime * m_animSpeed;
			}
			else
			{
				m_currentAnim.speed = m_animSpeed;
			}
		}
	}

	private void LateUpdate()
	{
		if (m_isPlaying && IsCurrentAnimEnd())
		{
			m_isPlaying = false;
			m_fixedUpdate = false;
			SendMessageUpwards("OnAnimEnded", m_currentAnimName, SendMessageOptions.DontRequireReceiver);
		}
	}
}
