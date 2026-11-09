using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
	public class Action
	{
		public string id;

		public string text;

		public string function;

		public string triggerKey;

		public string prevId;

		public string nextId;

		public string mascotFace;

		public string mascotAnim;

		public string musicName;

		public string sfxName;

		public string arg;
	}

	public static class TriggerEvent
	{
		public const string Click = "click";

		public const string ButtonLeft = "button_Left";

		public const string ButtonRight = "button_Right";

		public const string ButtonBlock = "button_Block";

		public const string SuperMove = "super_move";

		public const string PowerMove = "power_move";

		public const string BattleReady = "battle_ready";

		public const string BattleEnd = "battle_end";

		public const string BattleExit = "battle_exit";

		public const string SlashBottomLeftToTopRight = "slash_BottomLeftToTopRight";

		public const string SlashBottomRightToTopLeft = "slash_BottomRightToTopLeft";

		public const string ClickFusionMenu = "click_fusion_menu";

		public const string ClickOk = "click_ok";

		public const string ClickCancel = "click_cancel";

		public const string ClickItem = "click_item";

		public const string ClickEquip = "click_equip";

		public const string ClickMaterialSlot = "click_material_slot";

		public const string FusionStart = "fusion_start";

		public const string FusionDone = "fusion_done";

		public const string PetFeed = "click_feed";

		public const string PetPlay = "click_train";

		public const string PetHunt = "click_hunt";

		public const string PetHuntTime = "click_hunt_time";

		public const string DelayTimeEnd = "delay_time_end";
	}

	public enum State
	{
		None = 0,
		Combat1 = 1,
		Gacha = 2,
		Gacha2 = 4,
		Inventory = 8,
		Combat2 = 16,
		Fusion = 32,
		Combat3 = 64,
		Combat4 = 128,
		AfterEnterName = 256,
		Pet = 512,
		Rewards = 1024,
		PreCombat2 = 2048,
		WingDragon = 4096,
		PlayerGiveup = 8192,
		LastCombat = 64,
		AllCombats = 81,
		All = 379
	}

	private State m_state;

	private Dictionary<string, Action> m_actionMap = new Dictionary<string, Action>();

	private Action m_currentAction;

	private Dictionary<string, List<string>> m_triggerMap = new Dictionary<string, List<string>>();

	private bool m_init;

	public State state
	{
		get
		{
			return m_state;
		}
		set
		{
			m_state = value;
		}
	}

	protected virtual void OnEnable()
	{
	}

	protected virtual void OnDisable()
	{
	}

	protected virtual void Awake()
	{
	}

	public virtual void Start()
	{
		if (m_actionMap.Count != 0)
		{
			IEnumerator<string> enumerator = m_actionMap.Keys.GetEnumerator();
			enumerator.MoveNext();
			string current = enumerator.Current;
			Action action = m_actionMap[current];
			DoAction(action);
		}
	}

	public void Init(TutorialData data)
	{
		if (data == null || m_init)
		{
			return;
		}
		m_actionMap.Clear();
		m_triggerMap.Clear();
		foreach (string key in data.Keys)
		{
			TutorialDataRow tutorialDataRow = data[key];
			string triggerKey = tutorialDataRow.TriggerKey;
			List<string> list = null;
			if (m_triggerMap.ContainsKey(triggerKey))
			{
				list = m_triggerMap[triggerKey];
			}
			else
			{
				list = new List<string>();
				m_triggerMap.Add(triggerKey, list);
			}
			list.Add(tutorialDataRow.Id);
			Action action = new Action();
			action.id = tutorialDataRow.Id;
			action.text = tutorialDataRow.SubtitleKey;
			action.function = tutorialDataRow.CallbackStr;
			action.triggerKey = tutorialDataRow.TriggerKey;
			action.nextId = tutorialDataRow.NextId;
			action.mascotAnim = tutorialDataRow.MascotAnim;
			action.mascotFace = tutorialDataRow.MascotFacial;
			action.musicName = tutorialDataRow.Music;
			action.sfxName = tutorialDataRow.Sound;
			action.arg = tutorialDataRow.Arg;
			m_actionMap.Add(action.id, action);
		}
		foreach (Action value in m_actionMap.Values)
		{
			if (m_actionMap.ContainsKey(value.nextId))
			{
				Action action2 = m_actionMap[value.nextId];
				action2.prevId = value.id;
			}
		}
		m_init = true;
	}

	public Action GetActionWithId(string id)
	{
		if (m_actionMap.ContainsKey(id))
		{
			return m_actionMap[id];
		}
		return null;
	}

	public virtual void DoAction(Action action)
	{
		string function = action.function;
		SendMessage(function, action, SendMessageOptions.DontRequireReceiver);
		m_currentAction = action;
	}

	public void Rollback(Action action)
	{
		if (m_actionMap.ContainsValue(action))
		{
			m_currentAction = action;
		}
	}

	public void RollbackAndDoAction(Action action)
	{
		if (action != null && m_actionMap.ContainsValue(action))
		{
			DoAction(action);
		}
	}

	public bool TryAdvance(string triggerKey)
	{
		if (m_currentAction == null)
		{
			return false;
		}
		string nextId = m_currentAction.nextId;
		if (CanAdvance(triggerKey, nextId))
		{
			Action action = m_actionMap[nextId];
			Messenger<string>.Invoke("tutorial_next_action", nextId);
			Dbg.Log("TUTORIAL ACTION ======> " + nextId + " ==> " + action.function, Dbg.NBLogType.TUTORIAL);
			DoAction(action);
			return true;
		}
		return false;
	}

	public bool CanAdvance(string triggerKey, string nextStateId)
	{
		if (!m_triggerMap.ContainsKey(triggerKey))
		{
			return false;
		}
		List<string> list = m_triggerMap[triggerKey];
		if (list == null)
		{
			return false;
		}
		return list.Contains(nextStateId);
	}

	public virtual void ShowInstruction(Action action)
	{
		SingletonComponent<TutorialManager>.Instance.ShowInstruction(action.text);
	}

	public virtual void HideInstruction()
	{
		SingletonComponent<TutorialManager>.Instance.HideInstruction();
	}

	public virtual void ShowSubtitle(Action action)
	{
		bool p_behindUIBar = action.arg == "TRUE";
		SingletonComponent<TutorialManager>.Instance.ShowSubtitle(action.text, true, action.mascotAnim, action.mascotFace, action.sfxName, OnPopupClose, true, p_behindUIBar);
	}

	public IEnumerator DelayShowSubtitle(Action action)
	{
		yield return new MBWaitForSeconds(1.5f);   // PORT (CD-059)
		ShowSubtitle(action);
	}

	public virtual void HideSubtitle()
	{
		SingletonComponent<TutorialManager>.Instance.EndSubtitle();
	}

	protected void DelayTime(Action action)
	{
		float result = 0f;
		if (float.TryParse(action.arg, out result))
		{
			StartCoroutine(DoDelayTime(result));
		}
	}

	private IEnumerator DoDelayTime(float duration)
	{
		yield return new MBWaitForSeconds(duration);   // PORT (CD-059)
		TryAdvance("delay_time_end");
	}

	protected virtual void OnPopupClose()
	{
		TryAdvance("click");
	}
}
