using System;
using System.Collections;
using System.Collections.Generic;
using Models;
using UnityEngine;

public class UIPopupController : UIController
{
	public enum DodgeHintType
	{
		Directions = 0,
		StraightAttack = 1
	}

	public enum LoadingScreenMode
	{
		OVERLAY_ONLY = 0,
		FULLSCREEN_STARTUP = 1,
		FULLSCREEN_MONSTER_BLADE = 2,
		FULLSCREEN_TIP = 3
	}

	public enum PopupName
	{
		INFO_POPUP = 0,
		ITEM_WARNING_POPUP = 1,
		ITEM_INFO = 2,
		OK_PICTURE_POPUP = 3,
		OK_POPUP = 4,
		SHOP_BAGSLOTS_POPUP = 5,
		SHOP_LIST_POPUP = 6,
		SHOP_STAMINA_POPUP = 7,
		SMALL = 8,
		FUSION_LEVELUP = 9,
		EVOLVE_LEVELUP = 10,
		ADD_FRIEND_POPUP = 11,
		LEADERBOARD_HELP_POPUP = 12,
		PRE_NEXT_BATTLE_POPUP = 13,
		PROMOTION_POPUP = 14,
		DAILY_BONUS_POPUP = 15,
		SUBTITLE_POPUP = 16,
		DEATHRETRY_POPUP = 17,
		DEATHRETRY_SNS_POPUP = 18,
		ACHIEVEMENT_CLAIM_POPUP = 19,
		DAILY_BONUS_CLAIM_POPUP = 20,
		PET_HUNTING_INFO_POPUP = 21,
		VOLUME_SETTING_POPUP = 22,
		BATTLE_PAUSE_POPUP = 23,
		ANNOUNCEMENT_POPUP = 24,
		DIAMOND_OK_POPUP = 25,
		WEAPON_CHECK_POPUP = 26,
		FRIEND_SELECT_POPUP = 27,
		LEADERBOARD_RESULT_POPUP = 28,
		EVENT_DIFFICULTY_POPUP = 29,
		CREDIT_POPUP = 30,
		DODGE_HINT1_POPUP = 31,
		DODGE_HINT2_POPUP = 32,
		SHOP_BATTLEPOINT_POPUP = 33,
		COLLECTION_VIEWER_POPUP = 34,
		BATTLE_HELP_POPUP = 35,
		ANNOUNCEMENT_ITEM_POPUP = 36,
		PLAYER_PROFILE_POPUP = 37,
		PROMO_CODE_POPUP = 38,
		PROMO_CLAIM_POPUP = 39,
		HOLY_DIAMOND_POPUP = 40,
		WEIBO_FRIEND_SELECT = 41,
		MAX = 42
	}

	public class Popup
	{
		public GameObject m_popupObj;

		public PopupName m_popupName;

		public PopupCallback m_onYes;

		public PopupCallback m_onNo;

		public PopupCallback m_onClose;

		public PopupCallback m_onSNSPopup;

		public PopupCallback m_onBack;

		public PopupCallback m_onOpt1;

		public PopupCallback m_onOpt2;

		public PopupCallback m_onOpt3;

		public bool m_bAutoClose = true;

		public bool m_isClosed;

		public bool m_autoDestroy = true;
	}

	private List<string> m_tipsKey;

	private List<string> m_battleTipsKeys;

	private List<string> m_loreTipsKeys;

	private LoadingScreenMode m_currentLoadingScreenMode;

	[SerializeField]
	private Camera m_popupCamera;

	private List<GameObject> m_popupObjects;

	public List<string> m_popupNames;

	public List<string> m_generalPopupNames;

	public List<string> m_menuPopupNames;

	public List<string> m_battlePopupNames;

	public List<string> m_loadingPopupNames;

	private string m_pathFromResources = "UI/Prefabs/POPUPS/";

	private List<Popup> m_popupStack = new List<Popup>();

	private Popup m_currentPopup;

	private Popup m_popupBattleClear = new Popup();

	public UISprite m_staminaSprite;

	public UISprite m_battlePointSprite;

	public UISprite m_bagSlotsSprite;

	public bool m_SetActiveRecursively = true;

	private float m_shopCellWidth;

	public Transform Panel;

	public GameObject m_loadingPopup;

	public GameObject m_loadingPopupHourglass;

	public GameObject m_loadingWithMonsterBlade;

	public bool m_listenerAdded;

	public UISlider m_progressBar;

	public UISprite m_overlay;

	private bool m_hasFocus;

	private static UIPopupController s_instance;

	private float m_curProgress;

	private string m_curTipKey;

	private bool m_loadingShouldPersist;

	private GameObject m_loadingWithBGInstance;

	[SerializeField]
	private float m_tipsChangeTime = 10f;

	private NBTimer m_tipsTimer;

	[SerializeField]
	private UISprite m_rotatingLizard;

	public static int LoginCounter;

	public Popup currentPopup
	{
		get
		{
			return m_currentPopup;
		}
	}

	public bool isLoading
	{
		get
		{
			return m_loadingPopupHourglass.active || m_loadingPopup.active || m_loadingWithMonsterBlade.active;
		}
	}

	public static UIPopupController Instance
	{
		get
		{
			return s_instance;
		}
	}

	public int GetPopupStackCount()
	{
		if (m_popupStack == null)
		{
			return 0;
		}
		return m_popupStack.Count;
	}

	public override void Awake()
	{
		base.Awake();
		if (s_instance == null)
		{
			s_instance = this;
		}
		InitPopups();
		LoadPopups(ref m_generalPopupNames);
		m_hasFocus = false;
	}

	public override void Start()
	{
		base.Start();
	}

	private void InitPopups()
	{
		m_popupObjects = new List<GameObject>();
		foreach (string popupName in m_popupNames)
		{
			m_popupObjects.Add(null);
		}
	}

	private void LoadPopups(ref List<string> _popupsName)
	{
		for (int i = 0; i < _popupsName.Count; i++)
		{
			string item = _popupsName[i];
			int index = m_popupNames.IndexOf(item);
			LoadPopup(_popupsName[i], index);
		}
	}

	public void LoadMenuPopups()
	{
		UnloadExcept(m_generalPopupNames);
		LoadPopups(ref m_menuPopupNames);
	}

	public void LoadBattlePopups()
	{
		UnloadExcept(m_generalPopupNames);
		LoadPopups(ref m_battlePopupNames);
	}

	private void LoadPopup(PopupName _popupName)
	{
		Dbg.Log("Loading Popup : " + _popupName, Dbg.NBLogType.UI_POPUP);
		if (_popupName < PopupName.INFO_POPUP || (int)_popupName > m_popupObjects.Count)
		{
			Dbg.LogWarn("This popup cannot be loaded : " + _popupName);
			return;
		}
		if (m_popupObjects[(int)_popupName] == null)
		{
			string path = m_pathFromResources + m_popupNames[(int)_popupName];
			m_popupObjects[(int)_popupName] = (GameObject)Resources.Load(path);
		}
		m_SetActiveRecursively = true;
	}

	private void UnloadPopup(PopupName _popupName)
	{
		if (_popupName < PopupName.INFO_POPUP || (int)_popupName > m_popupObjects.Count)
		{
			Dbg.LogWarn("This popup cannot be unloaded : " + _popupName);
			return;
		}
		string item = m_popupNames[(int)_popupName];
		if (!m_generalPopupNames.Contains(item))
		{
			m_popupObjects[(int)_popupName] = null;
		}
	}

	private void LoadPopup(string _popupName, int _index)
	{
		string path = m_pathFromResources + _popupName;
		m_popupObjects[_index] = (GameObject)Resources.Load(path);
	}

	public void MakeFirstPriority()
	{
		if (m_popupStack != null && m_popupStack.Count > 1)
		{
			int count = m_popupStack.Count;
			Popup item = m_popupStack[count - 1];
			m_popupStack.RemoveAt(count - 1);
			m_popupStack.Insert(0, item);
		}
	}

	public void LoadAll()
	{
		for (int i = 0; i < m_popupNames.Count; i++)
		{
			if (i >= m_popupObjects.Count)
			{
				Dbg.LogError("Wrong popup Index.", Dbg.NBLogType.UI_POPUP);
				break;
			}
			if (!(m_popupObjects[i] != null))
			{
				LoadPopup(m_popupNames[i], i);
			}
		}
		Dbg.Log(" -- POPUP LOADED -- ", Dbg.NBLogType.UI_POPUP);
	}

	public void UnloadExcept(List<string> _popupToKeep = null)
	{
		for (int i = 0; i < m_popupNames.Count; i++)
		{
			string item = m_popupNames[i];
			if (_popupToKeep == null || !_popupToKeep.Contains(item))
			{
				m_popupObjects[i] = null;
			}
		}
		Dbg.Log(" -- POPUP UNLOADED -- ", Dbg.NBLogType.UI_POPUP);
	}

	public bool AssertPopup(bool p_condition, string p_message)
	{
		return Dbg.Assert(p_condition, p_message);
	}

	public void Update()
	{
		if (m_currentLoadingScreenMode == LoadingScreenMode.FULLSCREEN_STARTUP)
		{
			UpdateProgressBar();
		}
		if ((m_popupStack != null && m_popupStack.Count == 0) || m_currentPopup != null || m_hasFocus)
		{
			return;
		}
		m_hasFocus = true;
		m_currentPopup = m_popupStack[0];
		SetEnableLayer("PopUpLayer", true);
		TweenPopup tweenPopup = null;
		if (m_currentPopup != null && m_currentPopup.m_popupObj != null)
		{
			if (m_currentPopup.m_popupName != PopupName.SUBTITLE_POPUP)
			{
				Dbg.Log("m_overlay actived");
				m_overlay.gameObject.active = true;
				m_overlay.SendMessage("FadeIn", null, SendMessageOptions.DontRequireReceiver);
			}
			tweenPopup = m_currentPopup.m_popupObj.GetComponent<TweenPopup>();
			if ((bool)tweenPopup)
			{
				tweenPopup.Init();
			}
			if (m_SetActiveRecursively)
			{
				m_currentPopup.m_popupObj.SetActiveRecursively(true);
			}
			else
			{
				m_currentPopup.m_popupObj.active = true;
			}
			Dbg.Log(" SetCurrent Popup : " + m_currentPopup.m_popupName);
			if ((bool)tweenPopup)
			{
				tweenPopup.OnEnter();
			}
		}
	}

	public void HideCurrentPopup()
	{
		if (m_currentPopup != null)
		{
			TweenPopup component = m_currentPopup.m_popupObj.GetComponent<TweenPopup>();
			if ((bool)component)
			{
				component.OnExit(DestroyCurrentPopup);
			}
			else
			{
				DestroyCurrentPopup();
			}
		}
		if (m_popupStack.Count > 0 || isLoading || !(MainMenu.Instance != null))
		{
			return;
		}
		Camera mainMenuCamera = MainMenu.Instance.GetMainMenuCamera();
		if (mainMenuCamera != null)
		{
			UICamera component2 = mainMenuCamera.GetComponent<UICamera>();
			if (component2 != null && !component2.enabled)
			{
				component2.enabled = true;
				Dbg.Log("Enable the main menu UI camera");
			}
		}
		Camera barsCamera = MainMenu.Instance.GetBarsCamera();
		if (null != barsCamera)
		{
			UICamera component3 = barsCamera.GetComponent<UICamera>();
			if (null != component3 && !component3.enabled)
			{
				component3.enabled = true;
				Dbg.Log("Enable the bars UI camera");
			}
		}
	}

	private void DestroyCurrentPopup()
	{
		if (m_currentPopup != null)
		{
			if (m_currentPopup.m_autoDestroy)
			{
				UnityEngine.Object.Destroy(m_currentPopup.m_popupObj);
			}
			else if (null != m_currentPopup.m_popupObj)
			{
				m_currentPopup.m_popupObj.transform.localScale = Vector3.zero;
			}
		}
		m_currentPopup = null;
		m_hasFocus = false;
	}

	public void EnableCurPopupButtons(bool p_enable)
	{
		if (m_currentPopup == null)
		{
			return;
		}
		UIButtonMessage[] componentsInChildren = MBHier.GetComponentsInChildren<UIButtonMessage>(m_currentPopup.m_popupObj);
		if (componentsInChildren != null)
		{
			UIButtonMessage[] array = componentsInChildren;
			foreach (UIButtonMessage uIButtonMessage in array)
			{
				uIButtonMessage.enabled = p_enable;
				Dbg.Log("Disabled the UIButtonMessage of button : " + uIButtonMessage.gameObject.name);
			}
		}
	}

	private void OnYes()
	{
		if (m_popupStack.Count != 0 && m_currentPopup != null && !m_currentPopup.m_isClosed)
		{
			m_currentPopup.m_isClosed = m_currentPopup.m_bAutoClose;
			Messenger.Invoke("yes_click");
			if (m_currentPopup.m_onYes != null)
			{
				m_currentPopup.m_onYes();
			}
			if (m_currentPopup != null && m_currentPopup.m_bAutoClose)
			{
				Dbg.Log("m_overlay SendMessage FadeOut");
				m_overlay.SendMessage("FadeOut", null, SendMessageOptions.DontRequireReceiver);
				m_popupStack.Remove(m_currentPopup);
				Dbg.Log(" Close popup OnYes : " + m_currentPopup.m_popupName);
				UnloadPopup(m_currentPopup.m_popupName);
				HideCurrentPopup();
				SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_ok");
			}
		}
	}

	private void OnNo()
	{
		if (m_popupStack.Count != 0 && m_currentPopup != null && !m_currentPopup.m_isClosed)
		{
			m_currentPopup.m_isClosed = m_currentPopup.m_bAutoClose;
			Messenger.Invoke("no_click");
			if (m_currentPopup.m_onNo != null)
			{
				m_currentPopup.m_onNo();
			}
			if (m_currentPopup != null && m_currentPopup.m_bAutoClose)
			{
				Dbg.Log("m_overlay SendMessage FadeOut");
				m_overlay.SendMessage("FadeOut", null, SendMessageOptions.DontRequireReceiver);
				m_popupStack.Remove(m_currentPopup);
				Dbg.Log(" Close popup OnNo : " + m_currentPopup.m_popupName);
				UnloadPopup(m_currentPopup.m_popupName);
				HideCurrentPopup();
				SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_cancel");
			}
		}
	}

	private void OnClose()
	{
		if (m_popupStack.Count != 0 && m_currentPopup != null && !m_currentPopup.m_isClosed)
		{
			m_currentPopup.m_isClosed = true;
			if (m_currentPopup.m_popupName != PopupName.SUBTITLE_POPUP)
			{
				Dbg.Log("m_overlay SendMessage FadeOut");
				m_overlay.SendMessage("FadeOut", null, SendMessageOptions.DontRequireReceiver);
			}
			m_popupStack.Remove(m_currentPopup);
			PopupCallback onClose = m_currentPopup.m_onClose;
			Dbg.Log(" Close popup OnClose : " + m_currentPopup.m_popupName);
			UnloadPopup(m_currentPopup.m_popupName);
			HideCurrentPopup();
			if (onClose != null)
			{
				onClose();
			}
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_cancel");
		}
	}

	private void OnDeviceBackButtonPressed()
	{
		Popup popup = m_currentPopup;
		Dbg.Log("OnDeviceBackButtonPressed()");
		if (popup == null)
		{
			return;
		}
		bool flag = popup.m_popupName == PopupName.ACHIEVEMENT_CLAIM_POPUP;
		UITweener[] componentsInChildren = MBHier.GetComponentsInChildren<UITweener>(base.gameObject);
		bool flag2 = false;
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			flag2 |= componentsInChildren[i].enabled;
		}
		if (!flag2 || flag)
		{
			Dbg.Log("Popup is not tweening");
			if (popup.m_popupName == PopupName.WEAPON_CHECK_POPUP)
			{
				popup.m_onClose = null;
				OnClose();
			}
			else if (popup.m_onBack != null)
			{
				OnGoBack();
			}
			else if (popup.m_onNo != null)
			{
				OnNo();
			}
			else
			{
				OnClose();
			}
		}
	}

	public void OnCloseWithoutCallback()
	{
		if (m_popupStack.Count != 0 && m_currentPopup != null)
		{
			if (m_currentPopup.m_popupName != PopupName.SUBTITLE_POPUP)
			{
				Dbg.Log("m_overlay SendMessage FadeOut");
				m_overlay.SendMessage("FadeOut", null, SendMessageOptions.DontRequireReceiver);
			}
			m_popupStack.Remove(m_currentPopup);
			Dbg.Log(" Close popup OnClose : " + m_currentPopup.m_popupName);
			UnloadPopup(m_currentPopup.m_popupName);
			HideCurrentPopup();
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_cancel");
		}
	}

	private void OnSNSRevive()
	{
		Dbg.Log("Do SNS Revive");
		if (m_popupStack.Count != 0 && m_currentPopup != null)
		{
			Messenger.Invoke("yes_click");
			Dbg.Log("m_overlay SendMessage FadeOut");
			m_overlay.SendMessage("FadeOut", null, SendMessageOptions.DontRequireReceiver);
			m_popupStack.Remove(m_currentPopup);
			if (m_currentPopup.m_onSNSPopup != null)
			{
				m_currentPopup.m_onSNSPopup();
			}
			Dbg.Log(" Close popup OnYes : " + m_currentPopup.m_popupName);
			UnloadPopup(m_currentPopup.m_popupName);
			HideCurrentPopup();
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_ok");
		}
	}

	private void OnGoBack()
	{
		Dbg.Log("Going back to previous popup");
		if (m_popupStack.Count != 0 && m_currentPopup != null)
		{
			Messenger.Invoke("yes_click");
			Dbg.Log("m_overlay SendMessage FadeOut");
			m_overlay.SendMessage("FadeOut", null, SendMessageOptions.DontRequireReceiver);
			m_popupStack.Remove(m_currentPopup);
			if (m_currentPopup.m_onBack != null)
			{
				m_currentPopup.m_onBack();
			}
			Dbg.Log(" Close popup OnYes : " + m_currentPopup.m_popupName);
			UnloadPopup(m_currentPopup.m_popupName);
			HideCurrentPopup();
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_ok");
		}
	}

	public void CloseCurrentPopup()
	{
		OnClose();
	}

	public bool HasFocus()
	{
		return m_hasFocus;
	}

	private void SetParentToPopupController(GameObject _go)
	{
		Vector3 localPosition = _go.transform.localPosition;
		_go.transform.parent = Panel;
		_go.transform.localScale = Vector3.one;
		_go.transform.localPosition = localPosition;
		_go.SetActiveRecursively(false);
	}

	private void SetMessageTarget(GameObject _gao)
	{
		UIButtonMessage[] componentsInChildren = MBHier.GetComponentsInChildren<UIButtonMessage>(_gao, true);
		UIButtonMessage[] array = componentsInChildren;
		foreach (UIButtonMessage uIButtonMessage in array)
		{
			uIButtonMessage.target = base.gameObject;
		}
		SetParentToPopupController(_gao);
	}

	public void SetMessageTargetToBtnParent(GameObject p_parent)
	{
		UIButtonMessage[] componentsInChildren = MBHier.GetComponentsInChildren<UIButtonMessage>(p_parent);
		UIButtonMessage[] array = componentsInChildren;
		foreach (UIButtonMessage uIButtonMessage in array)
		{
			uIButtonMessage.target = p_parent;
		}
	}

	public void SetPopupParentToPanel(GameObject p_parent)
	{
		p_parent.transform.parent = Panel;
		p_parent.transform.localScale = Vector3.one;
	}

	private void ShowOkPicture(UISprite _uiSprite, string _localizationKey, PopupCallback _onClose = null)
	{
		LoadPopup(PopupName.OK_PICTURE_POPUP);
		Popup popup = new Popup();
		GameObject gameObject = UnityEngine.Object.Instantiate(m_popupObjects[3]) as GameObject;
		UISprite component = gameObject.transform.Find("ITEM_SPRITE").GetComponent<UISprite>();
		NBUILocalize component2 = gameObject.transform.Find("DESC_TXT").GetComponent<NBUILocalize>();
		if (component == null)
		{
			popup = null;
			UnityEngine.Object.Destroy(gameObject);
			gameObject = null;
			Dbg.LogError("UIPopupController : --> Could not find UISprite object with name ITEM_SPRITE in this prefab <--", Dbg.NBLogType.UI_POPUP);
			return;
		}
		popup.m_popupObj = gameObject;
		popup.m_popupName = PopupName.OK_PICTURE_POPUP;
		popup.m_onYes = null;
		popup.m_onNo = null;
		popup.m_onClose = _onClose;
		SetMessageTarget(gameObject);
		m_popupStack.Add(popup);
		component.atlas = _uiSprite.atlas;
		component.spriteName = _uiSprite.spriteName;
		component2.Key = _localizationKey;
		component.MakePixelPerfect();
	}

	public void ShowOKPopupMSG(string title, string txt, PopupCallback onClose = null, string p_customButtonText = null)
	{
		LoadPopup(PopupName.OK_POPUP);
		Popup popup = new Popup();
		GameObject gameObject = UnityEngine.Object.Instantiate(m_popupObjects[4]) as GameObject;
		NBUILocalize component = gameObject.transform.Find("HEADER_TXT").GetComponent<NBUILocalize>();
		NBUILocalize component2 = gameObject.transform.Find("DESC_TXT").GetComponent<NBUILocalize>();
		popup.m_popupObj = gameObject;
		popup.m_popupName = PopupName.OK_POPUP;
		popup.m_onYes = null;
		popup.m_onNo = null;
		popup.m_onClose = onClose;
		if (p_customButtonText != null && p_customButtonText.Length > 0)
		{
			Dbg.Assert(gameObject);
			Transform transform = gameObject.transform.Find("BTN_OK");
			if ((bool)transform)
			{
				NBUILocalize componentInChildren = MBHier.GetComponentInChildren<NBUILocalize>(transform);
				if ((bool)componentInChildren)
				{
					componentInChildren.Key = p_customButtonText;
				}
			}
		}
		SetMessageTarget(gameObject);
		component.Key = title;
		component2.Key = txt;
		m_popupStack.Add(popup);
	}

	public void ShowInvalidDevice()
	{
		ShowOKPopupMSG("common_info", "common_invalid_device");
	}

	public void ShowBattlePausePopup(string txt, PopupCallback onYes, PopupCallback onNo, PopupCallback onClose = null, bool p_isFirstTimeTutorial = false, BattlePausePopup.PauseTab p_defaultTab = BattlePausePopup.PauseTab.COMBOS, BattlePausePopup.Mode p_mode = BattlePausePopup.Mode.BATTLE_PAUSE)
	{
		LoadPopup(PopupName.BATTLE_PAUSE_POPUP);
		Popup popup = new Popup();
		GameObject gameObject = (popup.m_popupObj = UnityEngine.Object.Instantiate(m_popupObjects[23]) as GameObject);
		popup.m_popupName = PopupName.BATTLE_PAUSE_POPUP;
		popup.m_onYes = onYes;
		popup.m_onNo = onNo;
		popup.m_onClose = onClose;
		SetMessageTarget(gameObject);
		BattlePausePopup component = gameObject.GetComponent<BattlePausePopup>();
		if (component != null)
		{
			component.SetTitle(SingletonComponent<NBLocalization>.Instance.Get(txt));
			component.m_Mode = p_mode;
			component.m_defaultShowingTab = p_defaultTab;
			component.SetButtonCallbackTarget();
			if (p_isFirstTimeTutorial)
			{
				UIButton exitBtn = component.m_exitBtn;
				if ((bool)exitBtn)
				{
					exitBtn.isEnabled = false;
				}
			}
		}
		m_popupStack.Add(popup);
	}

	public void ShowSmallPopupMSG(string txt, PopupCallback onYes = null, PopupCallback onNo = null, PopupCallback onClose = null)
	{
		LoadPopup(PopupName.SMALL);
		Popup popup = new Popup();
		GameObject gameObject = UnityEngine.Object.Instantiate(m_popupObjects[8]) as GameObject;
		NBUILocalize component = gameObject.transform.Find("DESC_TXT").GetComponent<NBUILocalize>();
		popup.m_popupObj = gameObject;
		popup.m_popupName = PopupName.SMALL;
		popup.m_onYes = onYes;
		popup.m_onNo = onNo;
		popup.m_onClose = onClose;
		SetMessageTarget(gameObject);
		component.Key = txt;
		m_popupStack.Add(popup);
	}

	public void ShowGenericPopupMSG(string p_descKey, string p_yesKey, string p_noKey, PopupCallback onYes = null, PopupCallback onNo = null, PopupCallback onClose = null)
	{
		LoadPopup(PopupName.SMALL);
		Popup popup = new Popup();
		GameObject gameObject = UnityEngine.Object.Instantiate(m_popupObjects[8]) as GameObject;
		if (gameObject == null)
		{
			Dbg.LogError("Failed to instantiate popup");
		}
		Transform transform = gameObject.transform.Find("DESC_TXT");
		if (transform != null)
		{
			NBUILocalize component = transform.GetComponent<NBUILocalize>();
			if (component != null)
			{
				component.Key = p_descKey;
			}
		}
		Transform transform2 = gameObject.transform.Find("BTN_YES");
		if (transform2 != null)
		{
			NBUILocalize componentInChildren = MBHier.GetComponentInChildren<NBUILocalize>(transform2);
			if (componentInChildren != null)
			{
				componentInChildren.Key = p_yesKey;
			}
		}
		Transform transform3 = gameObject.transform.Find("BTN_NO");
		if (transform3 != null)
		{
			NBUILocalize componentInChildren2 = MBHier.GetComponentInChildren<NBUILocalize>(transform3);
			if (componentInChildren2 != null)
			{
				componentInChildren2.Key = p_noKey;
			}
		}
		popup.m_popupObj = gameObject;
		popup.m_popupName = PopupName.SMALL;
		popup.m_onYes = onYes;
		popup.m_onNo = onNo;
		popup.m_onClose = onClose;
		SetMessageTarget(gameObject);
		m_popupStack.Insert(0, popup);
		m_SetActiveRecursively = true;
	}

	private void SetWarCryButton(Transform snsButtonTran)
	{
		if (false)
		{
			return;
		}
		Transform transform = snsButtonTran.Find("BG");
		if (!(transform == null))
		{
			UISprite component = transform.gameObject.GetComponent<UISprite>();
			if (!(component == null))
			{
				component.spriteName = "buttons/grey_btn";
			}
		}
	}

	public void ShowDodgeHintPopup(DodgeHintType p_type, PopupCallback p_onClose = null)
	{
		PopupName popupName = PopupName.DODGE_HINT1_POPUP;
		switch (p_type)
		{
		case DodgeHintType.StraightAttack:
			popupName = PopupName.DODGE_HINT1_POPUP;
			break;
		case DodgeHintType.Directions:
			popupName = PopupName.DODGE_HINT2_POPUP;
			break;
		}
		LoadPopup(popupName);
		Popup popup = new Popup();
		GameObject gameObject = UnityEngine.Object.Instantiate(m_popupObjects[(int)popupName]) as GameObject;
		if (gameObject == null)
		{
			Dbg.Assert(false, "Failed to instantiate popup");
			return;
		}
		popup.m_popupObj = gameObject;
		popup.m_popupName = popupName;
		popup.m_onYes = null;
		popup.m_onNo = null;
		popup.m_onClose = p_onClose;
		SetMessageTarget(gameObject);
		m_popupStack.Add(popup);
	}

	public void ShowOnDeathRetryPopup(string txt, PopupCallback onYes, PopupCallback onNo, PopupCallback onSNS, PopupCallback onClose = null)
	{
		LoadPopup(PopupName.DEATHRETRY_POPUP);
		Popup popup = new Popup();
		GameObject gameObject = UnityEngine.Object.Instantiate(m_popupObjects[17]) as GameObject;
		if (gameObject == null)
		{
			Dbg.Assert(false, "Failed to instantiate popup");
			return;
		}
		Transform transform = gameObject.transform.Find("RETRY_TIPS");
		Dbg.Assert(transform, "Transform not found");
		if (transform != null)
		{
			NBUILocalize component = transform.GetComponent<NBUILocalize>();
			if (component != null)
			{
				component.Key = GetRandomBattleTipKey();
			}
		}
		PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		TutorialManager instance = SingletonComponent<TutorialManager>.Instance;
		bool flag = instance != null && !instance.IsCompleted(Tutorial.State.All);
		Transform transform2 = gameObject.transform.Find("DESC_TXT");
		if (transform2 != null)
		{
			UILabel component2 = transform2.GetComponent<UILabel>();
			if (component2 != null && (bool)SingletonComponent<PlayerDataManager>.Instance)
			{
				int premiumCurrency = playerData.m_userData.premiumCurrency;
				int death_retry_cost = GLOBAL.GAME_SETTINGS.death_retry_cost;
				string p_key = "death_retry_main_desc";
				if (flag)
				{
					p_key = "death_retry_main_desc_free";
				}
				string text = ((!(SingletonComponent<NBLocalization>.Instance != null)) ? string.Empty : SingletonComponent<NBLocalization>.Instance.Get(p_key));
				if (text != string.Empty)
				{
					if (text.Contains("{0") && text.Contains("{1"))
					{
						component2.text = string.Format(text, death_retry_cost, premiumCurrency);
					}
					else
					{
						component2.text = text;
					}
				}
			}
		}
		DeathRetryPopup component3 = gameObject.GetComponent<DeathRetryPopup>();
		if (component3 != null)
		{
			if (flag)
			{
				component3.SetReviveDescFree();
				if (null != component3.m_snsButton)
				{
					UIButton component4 = component3.m_snsButton.GetComponent<UIButton>();
					if (null != component4)
					{
						component4.isLocked = true;
						onSNS = null;
					}
				}
				if (null != component3.m_giveupButton)
				{
					UIButton component5 = component3.m_giveupButton.GetComponent<UIButton>();
					if (null != component5)
					{
						component5.isLocked = true;
						onNo = null;
					}
				}
			}
			component3.SetCurrentPgcAmount(playerData.m_userData.premiumCurrency);
		}
		popup.m_popupObj = gameObject;
		popup.m_popupName = PopupName.DEATHRETRY_POPUP;
		popup.m_onYes = onYes;
		popup.m_onNo = onNo;
		popup.m_onClose = onClose;
		popup.m_onSNSPopup = onSNS;
		SetMessageTarget(gameObject);
		m_popupStack.Add(popup);
	}

	public void ShowOnDeathRetryViaSNSPopup(string txt, PopupCallback oninvite, PopupCallback onBack)
	{
		LoadPopup(PopupName.DEATHRETRY_SNS_POPUP);
		Popup popup = new Popup();
		GameObject messageTarget = (popup.m_popupObj = UnityEngine.Object.Instantiate(m_popupObjects[18]) as GameObject);
		popup.m_popupName = PopupName.DEATHRETRY_SNS_POPUP;
		popup.m_onYes = oninvite;
		popup.m_onNo = null;
		popup.m_onClose = null;
		popup.m_onBack = onBack;
		popup.m_bAutoClose = false;
		SetMessageTarget(messageTarget);
		m_popupStack.Add(popup);
	}

	public void ShowNoNetRetryInfo(string _infoKey, PopupCallback _retry = null, PopupCallback _abort = null)
	{
		string text = SingletonComponent<NBLocalization>.Instance.Get(_infoKey);
		string txt = SingletonComponent<NBLocalization>.Instance.Get("common_prompt_nonet_retry") + "\n" + text;
		ShowMediumPopupMSG("common_error", txt, _retry, _abort);
	}

	public void ShowNoNetRetry(PopupCallback _retry = null, PopupCallback _abort = null)
	{
		ShowMediumPopupMSG("common_error", "common_prompt_nonet_retry", _retry, _abort);
	}

	public void ShowNoNetMessage(PopupCallback p_close = null)
	{
		ShowOKPopupMSG("common_error", "common_prompt_nonet_retry", p_close);
	}

	public void ShowServerGenericError(PopupCallback p_close = null, int p_error = 0)
	{
		if (p_close == null)
		{
			p_close = OnClose;
		}
		switch (p_error)
		{
		case 0:
			ShowOKPopupMSG("common_error", "server_generic_error", p_close);
			break;
		case -11:
			ShowOKPopupMSG("common_error", "[Error:" + p_error + "] " + SingletonComponent<NBLocalization>.Instance.Get("server_generic_timeout"), p_close);
			break;
		default:
			ShowOKPopupMSG("common_error", "[Error:" + p_error + "] " + SingletonComponent<NBLocalization>.Instance.Get("server_generic_error"), p_close);
			break;
		}
	}

	public void ShowServerError(string p_key, PopupCallback p_close = null, int p_error = 0)
	{
		switch (p_error)
		{
		case 0:
			ShowMediumPopupMSG("common_error", "server_generic_error", null, null, p_close);
			break;
		case -11:
			ShowOKPopupMSG("common_error", "[Error:" + p_error + "] " + SingletonComponent<NBLocalization>.Instance.Get("server_generic_timeout"), p_close);
			break;
		default:
			ShowOKPopupMSG("common_error", "[Error:" + p_error + "] " + SingletonComponent<NBLocalization>.Instance.Get(p_key), p_close);
			break;
		}
	}

	public void ShowServerGenericErrorRetry(PopupCallback p_retry = null, PopupCallback p_close = null, int p_error = 0)
	{
		switch (p_error)
		{
		case 0:
			ShowMediumPopupMSG("common_error", "server_generic_error", p_retry, p_close);
			break;
		case -11:
			ShowOKPopupMSG("common_error", "[Error:" + p_error + "] " + SingletonComponent<NBLocalization>.Instance.Get("server_generic_timeout"), p_close);
			break;
		default:
			ShowMediumPopupMSG("common_error", "[Error:" + p_error + "] " + SingletonComponent<NBLocalization>.Instance.Get("server_generic_error"), p_retry, p_close);
			break;
		}
	}

	public void ShowServerErrorRetry(string p_key, PopupCallback p_retry = null, PopupCallback p_close = null, int p_error = 0)
	{
		switch (p_error)
		{
		case 0:
			ShowMediumPopupMSG("common_error", "server_generic_error", p_retry, p_close);
			break;
		case -11:
			ShowMediumPopupMSG("common_error", "[Error:" + p_error + "] " + SingletonComponent<NBLocalization>.Instance.Get("server_generic_timeout"), p_retry, p_close);
			break;
		default:
			ShowMediumPopupMSG("common_error", "[Error:" + p_error + "] " + SingletonComponent<NBLocalization>.Instance.Get(p_key), p_retry, p_close);
			break;
		}
	}

	public void ShowTimeoutForceRetry(PopupCallback _retry)
	{
		ShowOKPopupMSG("common_error", "server_generic_timeout", _retry);
	}

	public void ShowNoNetForceRetry(PopupCallback _retry)
	{
		ShowOKPopupMSG("common_error", "common_prompt_nonet_retry", _retry);
	}

	public void ShowTimeoutRetryOrRestart(PopupCallback _retry)
	{
		ShowMediumPopupMSG("common_error", "server_generic_timeout_restart", "common_retry", "common_restart", SingletonComponent<GameManager>.Instance.RestartApplication, _retry);
	}

	public void ShowNoNetRetryOrRestart(PopupCallback _retry)
	{
		ShowMediumPopupMSG("common_error", "common_prompt_nonet_retry_restart", "common_retry", "common_restart", SingletonComponent<GameManager>.Instance.RestartApplication, _retry);
	}

	public void ShowInsufficientShield(PopupCallback p_close = null)
	{
		ShowOKPopupMSG("common_error", "dungeon_prompt_not_enough_shield", p_close);
	}

	public void ShowExitConfirmationPopup(PopupCallback p_onConfirm, PopupCallback p_onCancel, PopupCallback p_onClose = null)
	{
		ShowMediumPopupMSG("gamehud_prompt_exit_header", "common_prompt_quit", "common_resume_button", "common_quit_button", p_onCancel, p_onConfirm, p_onClose);
	}

	public void ShowMediumPopupMSG(string p_title, string p_txt, string p_confirmTxt, string p_cancelTxt, PopupCallback p_onCancel, PopupCallback p_onConfirm, PopupCallback p_onClose = null)
	{
		LoadPopup(PopupName.INFO_POPUP);
		Popup popup = new Popup();
		GameObject gameObject = UnityEngine.Object.Instantiate(m_popupObjects[0]) as GameObject;
		if (gameObject == null)
		{
			Dbg.Assert(false, "Failed to instantiate popup");
			return;
		}
		Transform transform = gameObject.transform.Find("HEADER_TXT");
		Transform transform2 = gameObject.transform.Find("DESC_TXT");
		Transform transform3 = gameObject.transform.Find("BTN_YES");
		Transform transform4 = gameObject.transform.Find("BTN_NO");
		Dbg.Assert(transform);
		Dbg.Assert(transform2);
		Dbg.Assert(transform3);
		Dbg.Assert(transform4);
		if (transform != null)
		{
			NBUILocalize component = transform.GetComponent<NBUILocalize>();
			if (component != null)
			{
				component.Key = p_title;
			}
		}
		if (transform2 != null)
		{
			NBUILocalize component2 = transform2.GetComponent<NBUILocalize>();
			if (component2 != null)
			{
				component2.Key = p_txt;
			}
		}
		if (transform3 != null)
		{
			NBUILocalize componentInChildren = MBHier.GetComponentInChildren<NBUILocalize>(transform3);
			if (componentInChildren != null)
			{
				componentInChildren.Key = p_confirmTxt;
			}
		}
		if (transform4 != null)
		{
			NBUILocalize componentInChildren2 = MBHier.GetComponentInChildren<NBUILocalize>(transform4);
			if (componentInChildren2 != null)
			{
				componentInChildren2.Key = p_cancelTxt;
			}
		}
		if (p_onClose != null)
		{
			InfoPopup component3 = gameObject.GetComponent<InfoPopup>();
			if (null != component3)
			{
				component3.m_showCloseButton = true;
			}
			popup.m_onBack = p_onClose;
		}
		popup.m_popupObj = gameObject;
		popup.m_popupName = PopupName.INFO_POPUP;
		popup.m_onYes = p_onConfirm;
		popup.m_onNo = p_onCancel;
		popup.m_onClose = p_onClose;
		SetMessageTarget(gameObject);
		m_popupStack.Add(popup);
	}

	public void ShowMediumPopupMSG(string title, string txt, PopupCallback onYes, PopupCallback onNo, PopupCallback onClose = null)
	{
		ShowMediumPopupMSG(title, txt, "common_yes", "common_no", onNo, onYes, onClose);
	}

	public void ShowAddFriendPopup()
	{
		LoadPopup(PopupName.ADD_FRIEND_POPUP);
		Popup popup = new Popup();
		GameObject parentToPopupController = (popup.m_popupObj = UnityEngine.Object.Instantiate(m_popupObjects[11]) as GameObject);
		popup.m_popupName = PopupName.ADD_FRIEND_POPUP;
		popup.m_onYes = null;
		popup.m_onNo = null;
		popup.m_onClose = null;
		SetParentToPopupController(parentToPopupController);
		m_popupStack.Add(popup);
	}

	public void ShowItemWarningPopup(string _headerKey, string _footerKey, List<ItemDetails> items, bool isSingleItem, PopupCallback onYes, PopupCallback onNo, PopupCallback onClose = null)
	{
		LoadPopup(PopupName.ITEM_WARNING_POPUP);
		Popup popup = new Popup();
		GameObject gameObject = UnityEngine.Object.Instantiate(m_popupObjects[1]) as GameObject;
		ItemWarningPopup component = gameObject.GetComponent<ItemWarningPopup>();
		if (component == null)
		{
			Dbg.LogError("UIPopupController : --> No ItemWarningPopup found on this prefab <-- ", Dbg.NBLogType.UI_POPUP);
			popup = null;
			UnityEngine.Object.Destroy(gameObject);
			gameObject = null;
			return;
		}
		popup.m_popupObj = gameObject;
		popup.m_popupName = PopupName.ITEM_WARNING_POPUP;
		popup.m_onYes = onYes;
		popup.m_onNo = onNo;
		popup.m_onClose = onClose;
		SetMessageTarget(gameObject);
		bool flag = false;
		bool flag2 = false;
		component.m_items = items;
		foreach (ItemDetails item in items)
		{
			if (item.IsEquip)
			{
				flag = true;
			}
			if (item.Tier >= 3)
			{
				flag2 = true;
			}
		}
		if (flag2 && flag)
		{
			component.m_type = ItemWarningPopup.MessageType.Both;
		}
		else if (flag)
		{
			component.m_type = ItemWarningPopup.MessageType.Equipped;
		}
		else if (flag2)
		{
			component.m_type = ItemWarningPopup.MessageType.Tier3Up;
		}
		component.m_labelHeader.Key = _headerKey;
		component.m_labelFooter.Key = _footerKey;
		component.m_isSingleItem = isSingleItem;
		m_popupStack.Add(popup);
	}

	public void ShowFusionLevelup(FusionEvent evt, PopupCallback _onClose)
	{
		LoadPopup(PopupName.FUSION_LEVELUP);
		Popup popup = new Popup();
		GameObject gameObject = UnityEngine.Object.Instantiate(m_popupObjects[9]) as GameObject;
		FusionLevelup component = gameObject.GetComponent<FusionLevelup>();
		if (component == null)
		{
			Dbg.LogError("UIPopupController : --> No fusionLevelup found on this prefab <-- ", Dbg.NBLogType.UI_POPUP);
			component = null;
			UnityEngine.Object.Destroy(gameObject);
			gameObject = null;
			return;
		}
		popup.m_popupObj = gameObject;
		popup.m_popupName = PopupName.FUSION_LEVELUP;
		popup.m_onYes = null;
		popup.m_onNo = null;
		popup.m_onClose = _onClose;
		component.evt = evt;
		SetMessageTarget(gameObject);
		m_popupStack.Add(popup);
	}

	public void ShowEvolveLevelup(EvolveEvent p_evt, PopupCallback p_onClose)
	{
		LoadPopup(PopupName.EVOLVE_LEVELUP);
		Popup popup = new Popup();
		GameObject gameObject = UnityEngine.Object.Instantiate(m_popupObjects[10]) as GameObject;
		EvolveLevelup component = gameObject.GetComponent<EvolveLevelup>();
		if (component == null)
		{
			Dbg.LogError("UIPopupController : --> No fusionLevelup found on this prefab <-- ", Dbg.NBLogType.UI_POPUP);
			component = null;
			UnityEngine.Object.Destroy(gameObject);
			gameObject = null;
			return;
		}
		popup.m_popupObj = gameObject;
		popup.m_popupName = PopupName.EVOLVE_LEVELUP;
		popup.m_onYes = null;
		popup.m_onNo = null;
		popup.m_onClose = p_onClose;
		component.evt = p_evt;
		SetMessageTarget(gameObject);
		m_popupStack.Add(popup);
	}

	public void LoadBattleClearScreen()
	{
		LoadPopup(PopupName.PRE_NEXT_BATTLE_POPUP);
		m_SetActiveRecursively = false;
		Dbg.Assert(m_popupObjects[13]);
		GameObject gameObject = UnityEngine.Object.Instantiate(m_popupObjects[13], new Vector3(0f, 0f, -15f), Quaternion.identity) as GameObject;
		Dbg.Assert(gameObject);
		gameObject.SetActiveRecursively(false);
		m_popupBattleClear.m_popupObj = gameObject;
		m_popupBattleClear.m_popupName = PopupName.PRE_NEXT_BATTLE_POPUP;
		m_popupBattleClear.m_onYes = null;
		m_popupBattleClear.m_onNo = null;
		m_popupBattleClear.m_onClose = null;
		SetPopupParentToPanel(gameObject);
		SetMessageTargetToBtnParent(gameObject);
		gameObject.transform.localPosition = new Vector3(0f, 0f, -15f);
	}

	public void ShowBattleClearScreen(EndBattleData p_inData, int[] p_medals, int damageDealt, int damageReceived, int timeTaken, int specialScore, int totalScore, int prevScore = 0, bool _hideLeaveAndRetry = false)
	{
		Dbg.Log("ShowBattleClearScreen, time : " + DateTime.Now.ToString("HH:mm:ss.ffffff"));
		if (m_popupBattleClear.m_popupObj == null)
		{
			Dbg.LogError("Missing Battle Clear Popup");
			return;
		}
		if (_hideLeaveAndRetry)
		{
			Transform[] componentsInChildren = MBHier.GetComponentsInChildren<Transform>(m_popupBattleClear.m_popupObj, true);
			Transform[] array = componentsInChildren;
			foreach (Transform transform in array)
			{
				string text = transform.name.ToLower();
				if (text.Contains("leave") || text.Contains("retry"))
				{
					UISprite[] componentsInChildren2 = MBHier.GetComponentsInChildren<UISprite>(transform.gameObject, true);
					UISprite[] array2 = componentsInChildren2;
					foreach (UISprite uISprite in array2)
					{
						uISprite.enabled = false;
					}
					UIButtonMessage[] componentsInChildren3 = MBHier.GetComponentsInChildren<UIButtonMessage>(transform.gameObject, true);
					UIButtonMessage[] array3 = componentsInChildren3;
					foreach (UIButtonMessage uIButtonMessage in array3)
					{
						uIButtonMessage.enabled = false;
					}
					UILabel[] componentsInChildren4 = MBHier.GetComponentsInChildren<UILabel>(transform.gameObject, true);
					UILabel[] array4 = componentsInChildren4;
					foreach (UILabel uILabel in array4)
					{
						uILabel.enabled = false;
					}
				}
			}
		}
		BattleCompleteScreen component = m_popupBattleClear.m_popupObj.GetComponent<BattleCompleteScreen>();
		if (component != null)
		{
			component.PrepareContent();
			Dbg.Log("[Info] battle score: " + p_inData.score);
			component.SetEndBattleData(p_inData);
			component.SetRank(p_medals);
			component.SetPerformanceData(damageDealt, damageReceived, timeTaken, specialScore);
			component.SetScore(totalScore, prevScore);
			component.SetGold(SingletonComponent<DungeonManager>.Instance.GetIGCReward());
			component.SetExp(SingletonComponent<DungeonManager>.Instance.GetXPReward());
			component.SetTreasure(SingletonComponent<DungeonManager>.Instance.GetTreasureReward());
			component.EnableContent(true);
			m_popupBattleClear.m_popupObj.active = true;
		}
		if ((bool)m_overlay)
		{
			m_overlay.gameObject.active = true;
			m_overlay.SendMessage("FadeIn", null, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void HideBattleClearScreen()
	{
		if (m_popupBattleClear.m_popupObj != null)
		{
			m_popupBattleClear.m_popupObj.SetActiveRecursively(false);
		}
		if ((bool)m_overlay)
		{
			m_overlay.SendMessage("FadeOut", null, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void UnloadBattleScreen()
	{
		if (m_popupBattleClear.m_popupObj != null)
		{
			UnityEngine.Object.Destroy(m_popupBattleClear.m_popupObj);
			m_popupBattleClear.m_popupObj = null;
		}
		UnloadPopup(PopupName.PRE_NEXT_BATTLE_POPUP);
	}

	public void ShowPreNextBattle(EndBattleData p_inData, int[] p_medals, int damageDealt, int damageReceived, int timeTaken, int specialScore, int totalScore, int prevScore = 0, PopupCallback onClose = null, bool _hideLeaveAndRetry = false)
	{
		LoadPopup(PopupName.PRE_NEXT_BATTLE_POPUP);
		Popup popup = new Popup();
		m_SetActiveRecursively = false;
		Dbg.Assert(m_popupObjects[13]);
		GameObject gameObject = UnityEngine.Object.Instantiate(m_popupObjects[13], new Vector3(0f, 0f, -15f), Quaternion.identity) as GameObject;
		Dbg.Assert(gameObject);
		gameObject.SetActiveRecursively(false);
		if (_hideLeaveAndRetry)
		{
			Transform[] componentsInChildren = MBHier.GetComponentsInChildren<Transform>(gameObject, true);
			Transform[] array = componentsInChildren;
			foreach (Transform transform in array)
			{
				string text = transform.name.ToLower();
				if (text.Contains("leave") || text.Contains("retry"))
				{
					UISprite[] componentsInChildren2 = MBHier.GetComponentsInChildren<UISprite>(transform.gameObject, true);
					UISprite[] array2 = componentsInChildren2;
					foreach (UISprite uISprite in array2)
					{
						uISprite.enabled = false;
					}
					UIButtonMessage[] componentsInChildren3 = MBHier.GetComponentsInChildren<UIButtonMessage>(transform.gameObject, true);
					UIButtonMessage[] array3 = componentsInChildren3;
					foreach (UIButtonMessage uIButtonMessage in array3)
					{
						uIButtonMessage.enabled = false;
					}
					UILabel[] componentsInChildren4 = MBHier.GetComponentsInChildren<UILabel>(transform.gameObject, true);
					UILabel[] array4 = componentsInChildren4;
					foreach (UILabel uILabel in array4)
					{
						uILabel.enabled = false;
					}
				}
			}
		}
		popup.m_popupObj = gameObject;
		popup.m_popupName = PopupName.PRE_NEXT_BATTLE_POPUP;
		popup.m_onYes = null;
		popup.m_onNo = null;
		popup.m_onClose = onClose;
		BattleCompleteScreen component = gameObject.GetComponent<BattleCompleteScreen>();
		Dbg.Assert(component);
		component.PrepareContent();
		Dbg.Log("[Info] battle score: " + p_inData.score);
		component.SetEndBattleData(p_inData);
		component.SetRank(p_medals);
		component.SetPerformanceData(damageDealt, damageReceived, timeTaken, specialScore);
		component.SetScore(totalScore, prevScore);
		component.SetGold(SingletonComponent<DungeonManager>.Instance.GetIGCReward());
		component.SetExp(SingletonComponent<DungeonManager>.Instance.GetXPReward());
		component.SetTreasure(SingletonComponent<DungeonManager>.Instance.GetTreasureReward());
		SetPopupParentToPanel(gameObject);
		SetMessageTargetToBtnParent(gameObject);
		Dbg.Log("m_overlay enable = false");
		m_overlay.enabled = false;
		m_popupStack.Add(popup);
		component.EnableContent(true);
	}

	public void ShowPreNextBattle(EndBattleData p_inData, string[] p_ranks, PopupCallback onClose = null, bool _hideLeaveAndRetry = false)
	{
		LoadPopup(PopupName.PRE_NEXT_BATTLE_POPUP);
		Popup popup = new Popup();
		m_SetActiveRecursively = false;
		GameObject gameObject = UnityEngine.Object.Instantiate(m_popupObjects[13]) as GameObject;
		if (_hideLeaveAndRetry)
		{
			Transform[] componentsInChildren = MBHier.GetComponentsInChildren<Transform>(gameObject, true);
			Transform[] array = componentsInChildren;
			foreach (Transform transform in array)
			{
				string text = transform.name.ToLower();
				if (text.Contains("leave") || text.Contains("retry"))
				{
					UISprite[] componentsInChildren2 = MBHier.GetComponentsInChildren<UISprite>(transform.gameObject, true);
					UISprite[] array2 = componentsInChildren2;
					foreach (UISprite uISprite in array2)
					{
						uISprite.enabled = false;
					}
					UIButtonMessage[] componentsInChildren3 = MBHier.GetComponentsInChildren<UIButtonMessage>(transform.gameObject, true);
					UIButtonMessage[] array3 = componentsInChildren3;
					foreach (UIButtonMessage uIButtonMessage in array3)
					{
						uIButtonMessage.enabled = false;
					}
					UILabel[] componentsInChildren4 = MBHier.GetComponentsInChildren<UILabel>(transform.gameObject, true);
					UILabel[] array4 = componentsInChildren4;
					foreach (UILabel uILabel in array4)
					{
						uILabel.enabled = false;
					}
				}
			}
		}
		popup.m_popupObj = gameObject;
		popup.m_popupName = PopupName.PRE_NEXT_BATTLE_POPUP;
		popup.m_onYes = null;
		popup.m_onNo = null;
		popup.m_onClose = onClose;
		BattleCompleteScreen component = gameObject.GetComponent<BattleCompleteScreen>();
		component.PrepareContent();
		Dbg.Log("[Info] battle score: " + p_inData.score);
		component.SetEndBattleData(p_inData);
		SetPopupParentToPanel(gameObject);
		SetMessageTargetToBtnParent(gameObject);
		Dbg.Log("m_overlay enable = false");
		m_overlay.enabled = false;
		m_popupStack.Add(popup);
		component.EnableContent(false);
		component.EnableBackground(true);
		component.ShowBackground();
	}

	public void ShowDailyBonus(PopupCallback onClose = null)
	{
		LoadPopup(PopupName.DAILY_BONUS_POPUP);
		Popup popup = new Popup();
		GameObject gameObject = (popup.m_popupObj = UnityEngine.Object.Instantiate(m_popupObjects[15]) as GameObject);
		popup.m_popupName = PopupName.DAILY_BONUS_POPUP;
		popup.m_onYes = null;
		popup.m_onNo = null;
		popup.m_onClose = onClose;
		DailyBonusPopupNew component = gameObject.GetComponent<DailyBonusPopupNew>();
		if (component != null)
		{
			component.SetupDisplay();
		}
		SetParentToPopupController(gameObject);
		m_popupStack.Add(popup);
	}

	public void ShowDailyBonusClaim(DailyBonusData p_bonusData, UIAtlas p_cellAtlas, UIAtlas p_treasureAtlas, PopupCallback onClose = null)
	{
		Dbg.Log("[Info] ShowDailyBonusClaim()");
		LoadPopup(PopupName.DAILY_BONUS_CLAIM_POPUP);
		Popup popup = new Popup();
		GameObject gameObject = (popup.m_popupObj = UnityEngine.Object.Instantiate(m_popupObjects[20]) as GameObject);
		popup.m_popupName = PopupName.DAILY_BONUS_CLAIM_POPUP;
		popup.m_onYes = null;
		popup.m_onNo = null;
		popup.m_onClose = onClose;
		DailyBonusClaimPopup component = gameObject.GetComponent<DailyBonusClaimPopup>();
		if (component != null)
		{
			component.ShowDailyBonusData(p_bonusData, p_cellAtlas, p_treasureAtlas);
		}
		SetMessageTarget(gameObject);
		m_popupStack.Add(popup);
	}

	public void ShowDailyBonusClaimNew(DailyBonusData p_bonusData, PopupCallback onClose = null)
	{
		Dbg.Log("[Info] ShowDailyBonusClaimNew()");
		LoadPopup(PopupName.DAILY_BONUS_CLAIM_POPUP);
		Popup popup = new Popup();
		GameObject gameObject = (popup.m_popupObj = UnityEngine.Object.Instantiate(m_popupObjects[20]) as GameObject);
		popup.m_popupName = PopupName.DAILY_BONUS_CLAIM_POPUP;
		popup.m_onYes = null;
		popup.m_onNo = null;
		popup.m_onClose = onClose;
		SetMessageTarget(gameObject);
		m_popupStack.Add(popup);
		DailyBonusClaimPopupNew component = gameObject.GetComponent<DailyBonusClaimPopupNew>();
		if (component != null)
		{
			component.ShowDailyBonusData(p_bonusData);
			component.SetRewardIconPixelPerfect();
		}
	}

	public void ShowAnnouncementClaimedPopup(string p_rewardType, int p_amount, ItemDetails p_itemData, PopupCallback onClose = null)
	{
		Dbg.Log("[Info] ShowAnnouncementClaimedPopup()");
		LoadPopup(PopupName.DAILY_BONUS_CLAIM_POPUP);
		Popup popup = new Popup();
		GameObject gameObject = (popup.m_popupObj = UnityEngine.Object.Instantiate(m_popupObjects[20]) as GameObject);
		popup.m_popupName = PopupName.DAILY_BONUS_CLAIM_POPUP;
		popup.m_onYes = null;
		popup.m_onNo = null;
		popup.m_onClose = onClose;
		DailyBonusClaimPopup component = gameObject.GetComponent<DailyBonusClaimPopup>();
		if (component != null)
		{
			component.ShowReward(p_rewardType, p_amount, p_itemData);
		}
		SetMessageTarget(gameObject);
		m_popupStack.Add(popup);
	}

	public void ShowPromotion(bool p_onlyShowGachaType = false, PopupCallback onClose = null)
	{
		LoadPopup(PopupName.PROMOTION_POPUP);
		Popup popup = new Popup();
		GameObject gameObject = (popup.m_popupObj = UnityEngine.Object.Instantiate(m_popupObjects[14]) as GameObject);
		popup.m_popupName = PopupName.PROMOTION_POPUP;
		popup.m_onYes = null;
		popup.m_onNo = null;
		popup.m_onClose = onClose;
		PromotionPopup component = gameObject.GetComponent<PromotionPopup>();
		if (component != null)
		{
			component.ShowOnlyGacha = p_onlyShowGachaType;
		}
		SetMessageTarget(gameObject);
		m_popupStack.Add(popup);
	}

	public void ShowCollectionViewerPopup(GachaManager.GachaID p_gachaId, PopupCallback onClose = null)
	{
		LoadPopup(PopupName.COLLECTION_VIEWER_POPUP);
		Popup popup = new Popup();
		GameObject gameObject = (popup.m_popupObj = UnityEngine.Object.Instantiate(m_popupObjects[34]) as GameObject);
		popup.m_popupName = PopupName.COLLECTION_VIEWER_POPUP;
		popup.m_onYes = null;
		popup.m_onNo = null;
		popup.m_onClose = onClose;
		CollectionViewerPopup component = gameObject.GetComponent<CollectionViewerPopup>();
		if (component != null)
		{
			component.SetData(p_gachaId);
		}
		SetMessageTarget(gameObject);
		m_popupStack.Add(popup);
	}

	public void ShowBattleHelpPopup(PopupCallback onClose = null)
	{
		LoadPopup(PopupName.BATTLE_HELP_POPUP);
		Popup popup = new Popup();
		GameObject gameObject = (popup.m_popupObj = UnityEngine.Object.Instantiate(m_popupObjects[35]) as GameObject);
		popup.m_popupName = PopupName.BATTLE_HELP_POPUP;
		popup.m_onYes = null;
		popup.m_onNo = null;
		popup.m_onClose = onClose;
		BattleHelpPopup component = gameObject.GetComponent<BattleHelpPopup>();
		if (component != null)
		{
			component.SetupDisplay();
		}
		SetMessageTarget(gameObject);
		m_popupStack.Add(popup);
	}

	public void ShowSubtitlePopup(string p_subtitle, PopupCallback onClose = null)
	{
		Dbg.Log("Showing subTitle Popup : " + p_subtitle, Dbg.NBLogType.UI_POPUP);
		if (string.IsNullOrEmpty(p_subtitle))
		{
			OnClose();
			return;
		}
		if (m_currentPopup != null && m_currentPopup.m_popupName == PopupName.SUBTITLE_POPUP)
		{
			SubtitlePopup component = m_currentPopup.m_popupObj.GetComponent<SubtitlePopup>();
			if (component != null)
			{
				component.SetSubtitle(p_subtitle);
			}
			return;
		}
		LoadPopup(PopupName.SUBTITLE_POPUP);
		Popup popup = new Popup();
		GameObject gameObject = (popup.m_popupObj = UnityEngine.Object.Instantiate(m_popupObjects[16]) as GameObject);
		popup.m_popupName = PopupName.SUBTITLE_POPUP;
		popup.m_onYes = null;
		popup.m_onNo = null;
		popup.m_onClose = onClose;
		SubtitlePopup component2 = gameObject.GetComponent<SubtitlePopup>();
		if (component2 != null)
		{
			component2.SetSubtitle(p_subtitle);
		}
		SetMessageTarget(gameObject);
		m_popupStack.Add(popup);
	}

	public void ShowAchievementClaimPopup(AchievementManager.AchievementRewards p_achiReward, PopupCallback onClose = null)
	{
		LoadPopup(PopupName.ACHIEVEMENT_CLAIM_POPUP);
		Popup popup = new Popup();
		GameObject gameObject = (popup.m_popupObj = UnityEngine.Object.Instantiate(m_popupObjects[19]) as GameObject);
		popup.m_popupName = PopupName.ACHIEVEMENT_CLAIM_POPUP;
		popup.m_onYes = null;
		popup.m_onNo = null;
		popup.m_onClose = onClose;
		SetMessageTarget(gameObject);
		m_popupStack.Add(popup);
		AchievementClaimPopup component = gameObject.GetComponent<AchievementClaimPopup>();
		if (component != null)
		{
			if (GameManager.qualitySetting == GameManager.QualitySetting.Low)
			{
				component.DeleteEffects();
			}
			component.SetItemInfo(p_achiReward);
		}
		else
		{
			Dbg.LogWarn("[Warn] Unable to find AchievementClaimPopup");
		}
	}

	public void ShowAnnouncementPopup(AnnouncementData p_data, PopupCallback onClose = null)
	{
		LoadPopup(PopupName.ANNOUNCEMENT_POPUP);
		Popup popup = new Popup();
		GameObject gameObject = (popup.m_popupObj = UnityEngine.Object.Instantiate(m_popupObjects[24]) as GameObject);
		popup.m_popupName = PopupName.ANNOUNCEMENT_POPUP;
		popup.m_onYes = null;
		popup.m_onNo = null;
		popup.m_onClose = onClose;
		SetMessageTarget(gameObject);
		m_popupStack.Add(popup);
		AnnouncementPopup component = gameObject.GetComponent<AnnouncementPopup>();
		if (component != null)
		{
			component.SetInfo(p_data);
		}
		else
		{
			Dbg.LogWarn("[Warn] Unable to find AnnouncementPopup");
		}
	}

	public void ShowAnnouncementItemPopup(AnnouncementData p_data, PopupCallback onClose = null)
	{
		LoadPopup(PopupName.ANNOUNCEMENT_ITEM_POPUP);
		Popup popup = new Popup();
		GameObject gameObject = (popup.m_popupObj = UnityEngine.Object.Instantiate(m_popupObjects[36]) as GameObject);
		popup.m_popupName = PopupName.ANNOUNCEMENT_ITEM_POPUP;
		popup.m_onYes = null;
		popup.m_onNo = null;
		popup.m_onClose = onClose;
		SetMessageTarget(gameObject);
		m_popupStack.Add(popup);
		AnnouncementItemPopup component = gameObject.GetComponent<AnnouncementItemPopup>();
		if (component != null)
		{
			component.SetInfo(p_data);
		}
		else
		{
			Dbg.LogWarn("[Warn] Unable to find AnnouncementPopup");
		}
	}

	public void EndSubtitle()
	{
		Dbg.Log("m_overlay SendMessage FadeOut");
		m_overlay.SendMessage("FadeOut", null, SendMessageOptions.DontRequireReceiver);
	}

	public void ShowVolumeSettingPopup(PopupCallback onClose = null)
	{
		Dbg.Log("[Info] ShowVolumeSettingPopup()");
		LoadPopup(PopupName.VOLUME_SETTING_POPUP);
		Popup popup = new Popup();
		GameObject gameObject = (popup.m_popupObj = UnityEngine.Object.Instantiate(m_popupObjects[22]) as GameObject);
		popup.m_popupName = PopupName.VOLUME_SETTING_POPUP;
		popup.m_onYes = null;
		popup.m_onNo = null;
		popup.m_onClose = onClose;
		SetParentToPopupController(gameObject);
		m_popupStack.Add(popup);
		SettingsMenuPanel component = gameObject.GetComponent<SettingsMenuPanel>();
		if (component != null)
		{
			component.LoadLocalVolumeSetting();
			component.SetVibrateBtnState();
		}
	}

	private IEnumerator DoOverlayFadeInAndShowPopup()
	{
		float fadeDuration = 0.5f;
		m_overlay.color = Color.clear;
		m_overlay.gameObject.active = true;
		Dbg.Log("m_overlay active = true");
		TweenColor.Begin(m_overlay.gameObject, fadeDuration, Color.black);
		yield return new MBWaitForSeconds(fadeDuration);   // PORT (CD-059)
		Dbg.Log("DoOverlayFadeInAndShowPopup finished wait");
		m_currentPopup.m_popupObj.SetActiveRecursively(true);
	}

	private ItemInfo ShowItemStatsBase(PopupCallback _onClose)
	{
		LoadPopup(PopupName.ITEM_INFO);
		Popup popup = new Popup();
		GameObject gameObject = UnityEngine.Object.Instantiate(m_popupObjects[2]) as GameObject;
		ItemInfo component = gameObject.GetComponent<ItemInfo>();
		if (component == null)
		{
			Dbg.LogError("UIPopupController : --> No ItemInfo found on this prefab <-- ", Dbg.NBLogType.UI_POPUP);
			popup = null;
			UnityEngine.Object.Destroy(gameObject);
			gameObject = null;
			return null;
		}
		component.InventoryCell.m_size = InventoryCell.Size.Large;
		popup.m_popupObj = gameObject;
		popup.m_popupName = PopupName.ITEM_INFO;
		popup.m_onYes = null;
		popup.m_onNo = null;
		popup.m_onClose = _onClose;
		SetMessageTarget(gameObject);
		m_popupStack.Add(popup);
		return component;
	}

	public void ShowItemStats(ItemDetails _item, PopupCallback _onClose = null, bool showCell = true, bool showSkillDescriptions = true)
	{
		ItemInfo itemInfo = ShowItemStatsBase(_onClose);
		if (itemInfo != null)
		{
			itemInfo.ShowItem(_item, showCell, showSkillDescriptions);
		}
	}

	public void ShowItemStats(string _db_id, PopupCallback _onClose = null, bool showCell = true, bool showSkillDescriptions = true)
	{
		ItemInfo itemInfo = ShowItemStatsBase(_onClose);
		if (itemInfo != null)
		{
			itemInfo.ShowItem(_db_id, showCell, showSkillDescriptions);
		}
	}

	public void ShowHuntingIgc(int p_gold, PopupCallback p_onClose = null)
	{
		ItemInfo itemInfo = ShowItemStatsBase(p_onClose);
		if (itemInfo != null)
		{
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_pet_huntsuccess");
			itemInfo.ShowIgc(p_gold);
		}
	}

	public void ShowLbHelpPopup(PopupCallback onClose = null)
	{
		LoadPopup(PopupName.LEADERBOARD_HELP_POPUP);
		Popup popup = new Popup();
		GameObject messageTarget = (popup.m_popupObj = UnityEngine.Object.Instantiate(m_popupObjects[12]) as GameObject);
		popup.m_popupName = PopupName.LEADERBOARD_HELP_POPUP;
		popup.m_onYes = null;
		popup.m_onNo = null;
		popup.m_onClose = onClose;
		SetMessageTarget(messageTarget);
		m_popupStack.Add(popup);
	}

	public void ShowLbResultPopup(PopupCallback onClose = null)
	{
		LoadPopup(PopupName.LEADERBOARD_RESULT_POPUP);
		Popup popup = new Popup();
		GameObject gameObject = (popup.m_popupObj = UnityEngine.Object.Instantiate(m_popupObjects[28]) as GameObject);
		popup.m_popupName = PopupName.LEADERBOARD_RESULT_POPUP;
		popup.m_onYes = null;
		popup.m_onNo = null;
		popup.m_onClose = onClose;
		LeaderboardResultPopup component = gameObject.GetComponent<LeaderboardResultPopup>();
		if (component != null)
		{
			PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
			SingletonComponent<LeaderboardManager>.Instance.m_lastPlayer.CharaName = playerData.Name;
			component.SetLeaderboardResultInfo(SingletonComponent<LeaderboardManager>.Instance.m_lastTop3Players, SingletonComponent<LeaderboardManager>.Instance.m_lastPlayer);
		}
		SetMessageTarget(gameObject);
		m_popupStack.Add(popup);
	}

	public void ShowEventDifficultySelection(PopupCallback onClose = null)
	{
		LoadPopup(PopupName.EVENT_DIFFICULTY_POPUP);
		Popup popup = new Popup();
		GameObject gameObject = (popup.m_popupObj = UnityEngine.Object.Instantiate(m_popupObjects[29]) as GameObject);
		popup.m_popupName = PopupName.EVENT_DIFFICULTY_POPUP;
		popup.m_onYes = null;
		popup.m_onNo = null;
		popup.m_onClose = onClose;
		SetMessageTarget(gameObject);
		m_popupStack.Add(popup);
		EventDifficultySelectionPopup component = gameObject.GetComponent<EventDifficultySelectionPopup>();
		if (component != null)
		{
			component.SetButtonMessageTarget();
			component.SetButtonBpCost();
		}
	}

	public void ShowCreditPopup()
	{
		LoadPopup(PopupName.CREDIT_POPUP);
		Popup popup = new Popup();
		GameObject messageTarget = (popup.m_popupObj = UnityEngine.Object.Instantiate(m_popupObjects[30]) as GameObject);
		popup.m_popupName = PopupName.EVENT_DIFFICULTY_POPUP;
		popup.m_onYes = null;
		popup.m_onNo = null;
		popup.m_onClose = null;
		SetMessageTarget(messageTarget);
		m_popupStack.Add(popup);
	}

	public void ShowDoubleXpOrLoot(bool p_isDoubleXp, UIAtlas p_atlas, PopupCallback _onClose = null)
	{
		ItemInfo itemInfo = ShowItemStatsBase(_onClose);
		if (itemInfo != null)
		{
			itemInfo.ShowDoubleXpOrLoot(p_isDoubleXp, p_atlas);
		}
	}

	public void ShowSocialOrPremGacha(bool p_isSocialGacha, UIAtlas p_atlas, PopupCallback _onClose = null)
	{
		ItemInfo itemInfo = ShowItemStatsBase(_onClose);
		if (itemInfo != null)
		{
			itemInfo.ShowSocialOrPremGacha(p_isSocialGacha, p_atlas);
		}
	}

	public void SetLoadingPersists(bool shouldPersist)
	{
		m_loadingShouldPersist = shouldPersist;
	}

	private void SetNewTip()
	{
		m_curTipKey = GetRandomTipKey();
		if ((bool)m_loadingPopup)
		{
			NBUILocalize componentInChildren = MBHier.GetComponentInChildren<NBUILocalize>(m_loadingPopup);
			if (componentInChildren != null)
			{
				componentInChildren.Key = m_curTipKey;
			}
		}
		if (m_tipsTimer != null)
		{
			m_tipsTimer.Reset();
			m_tipsTimer.CountDown(m_tipsChangeTime, "SetNewTip");
		}
	}

	public void UpdateProgressBar()
	{
		if (!(m_progressBar == null))
		{
			float fixedDeltaTime = Time.fixedDeltaTime;
			if (m_progressBar.sliderValue + fixedDeltaTime >= m_curProgress)
			{
				m_progressBar.sliderValue = m_curProgress;
			}
			else
			{
				m_progressBar.sliderValue += fixedDeltaTime;
			}
		}
	}

	public float GetProgressBarValue()
	{
		return m_progressBar.sliderValue;
	}

	public void IncrementLoadStep(LoadingProgressManager.LoadProgress progress)
	{
		if (SingletonComponent<LoadingProgressManager>.Instance != null)
		{
			m_curProgress = SingletonComponent<LoadingProgressManager>.Instance.UpdateProgress(progress);
			Dbg.Log("LOADSTEP:" + m_curProgress);
		}
	}

	private void InitTipsKeys()
	{
		if (m_tipsKey == null)
		{
			m_tipsKey = SingletonComponent<NBLocalization>.Instance.GetKeysContaining("loading_tips");
			m_loreTipsKeys = SingletonComponent<NBLocalization>.Instance.GetKeysContaining("loading_tips_lore");
		}
	}

	private string GetRandomLoreTipKey()
	{
		InitTipsKeys();
		int count = m_loreTipsKeys.Count;
		if (count < 1)
		{
			return string.Empty;
		}
		return m_loreTipsKeys[UnityEngine.Random.Range(0, count)];
	}

	private string GetRandomTipKey()
	{
		InitTipsKeys();
		int count = m_tipsKey.Count;
		if (count < 1)
		{
			return string.Empty;
		}
		return m_tipsKey[UnityEngine.Random.Range(0, count)];
	}

	private void InitBattleTipsKeys()
	{
		if (m_battleTipsKeys == null)
		{
			m_battleTipsKeys = SingletonComponent<NBLocalization>.Instance.GetKeysContaining("battle_tips");
		}
	}

	private string GetRandomBattleTipKey()
	{
		InitBattleTipsKeys();
		int count = m_battleTipsKeys.Count;
		if (count < 1)
		{
			return string.Empty;
		}
		return m_battleTipsKeys[UnityEngine.Random.Range(0, count)];
	}

	public void SetLoadingScreenMode(LoadingScreenMode p_mode)
	{
		m_currentLoadingScreenMode = p_mode;
	}

	public void ShowFullScreenLoading()
	{
		ShowFullScreenLoading(m_currentLoadingScreenMode);
	}

	public void ShowFullScreenLoading(LoadingScreenMode p_mode)
	{
		if (m_loadingWithBGInstance != null)
		{
			return;
		}
		m_currentLoadingScreenMode = p_mode;
		HideLoadingPopup(false);
		if (p_mode == LoadingScreenMode.OVERLAY_ONLY)
		{
			m_currentLoadingScreenMode = LoadingScreenMode.FULLSCREEN_TIP;
		}
		SetLoadingPersists(true);
		if (m_currentLoadingScreenMode == LoadingScreenMode.FULLSCREEN_STARTUP && m_loadingWithBGInstance == null)
		{
			m_curTipKey = GetRandomLoreTipKey();
			if (!m_listenerAdded)
			{
				m_listenerAdded = true;
				Messenger<LoadingProgressManager.LoadProgress>.AddListener("progress_bar", IncrementLoadStep);
			}
			if (m_progressBar != null)
			{
				m_progressBar.sliderValue = 0f;
				if ((bool)m_progressBar.transform.parent)
				{
					m_progressBar.transform.parent.gameObject.SetActiveRecursively(true);
				}
			}
			int index = UnityEngine.Random.Range(0, m_loadingPopupNames.Count);
			string text = m_pathFromResources + m_loadingPopupNames[index];
			GameObject gameObject = (GameObject)Resources.Load(text);
			if (gameObject != null)
			{
				m_loadingWithBGInstance = UnityEngine.Object.Instantiate(gameObject) as GameObject;
				if (m_loadingWithBGInstance != null)
				{
					MBLoadingHold.Shown();   // PORT (CD-063)
					Transform parent = m_loadingPopup.transform;
					m_loadingWithBGInstance.transform.parent = parent;
					m_loadingWithBGInstance.transform.localScale = Vector3.one;
					m_loadingWithBGInstance.transform.localPosition = Vector3.zero;
				}
			}
			else
			{
				Dbg.LogWarn("Failed to load " + text + ". continuing anyway");
			}
			if ((bool)m_rotatingLizard)
			{
				m_rotatingLizard.enabled = false;
			}
		}
		else if (m_currentLoadingScreenMode == LoadingScreenMode.FULLSCREEN_TIP && m_loadingWithBGInstance == null)
		{
			if (m_loadingWithBGInstance == null)
			{
				m_loadingWithBGInstance = SingletonComponent<LoadingTipsManager>.Instance.GetTipGameObject();
				if (m_loadingWithBGInstance != null)
				{
					MBLoadingHold.Shown();   // PORT (CD-063)
					Transform parent2 = m_loadingPopup.transform;
					m_loadingWithBGInstance.transform.parent = parent2;
					m_loadingWithBGInstance.transform.localScale = Vector3.one;
					m_loadingWithBGInstance.transform.localPosition = Vector3.zero;
				}
			}
			if ((bool)m_rotatingLizard)
			{
				m_rotatingLizard.enabled = true;
			}
		}
		else
		{
			m_curTipKey = GetRandomTipKey();
			if (m_loadingWithMonsterBlade != null)
			{
				NBUILocalize[] componentsInChildren = MBHier.GetComponentsInChildren<NBUILocalize>(m_loadingWithMonsterBlade, true);
				NBUILocalize[] array = componentsInChildren;
				foreach (NBUILocalize nBUILocalize in array)
				{
					nBUILocalize.Key = m_curTipKey;
				}
			}
			if ((bool)m_rotatingLizard)
			{
				m_rotatingLizard.enabled = false;
			}
		}
		if (m_currentLoadingScreenMode == LoadingScreenMode.FULLSCREEN_STARTUP)
		{
			if (m_tipsTimer == null)
			{
				m_tipsTimer = NBComponentHelper.GetOrAddComponent<NBTimer>(base.gameObject);
			}
			if (m_tipsTimer != null && m_tipsTimer.Counter == 0f)
			{
				m_tipsTimer.CountDown(m_tipsChangeTime, "SetNewTip");
			}
		}
		m_loadingPopup.SetActiveRecursively(false);
	}

	public void ShowLoadingPopup(string p_topLabel = "", string p_bottomLabel = "", bool p_useCircle = true)
	{
		SetAllInputsEnabled(false);
		GameObject gameObject = m_loadingPopupHourglass;
		if (m_currentLoadingScreenMode == LoadingScreenMode.FULLSCREEN_STARTUP)
		{
			SetEnableLayer("PopUpLayer", true);
			gameObject = m_loadingPopup;
			if (gameObject != null)
			{
				NBUILocalize componentInChildren = MBHier.GetComponentInChildren<NBUILocalize>(gameObject);
				if (componentInChildren != null)
				{
					componentInChildren.Key = m_curTipKey;
				}
			}
		}
		else if (m_currentLoadingScreenMode == LoadingScreenMode.FULLSCREEN_MONSTER_BLADE)
		{
			gameObject = m_loadingWithMonsterBlade;
			if (gameObject != null)
			{
				NBUILocalize componentInChildren2 = MBHier.GetComponentInChildren<NBUILocalize>(gameObject);
				if (componentInChildren2 != null)
				{
					componentInChildren2.Key = m_curTipKey;
				}
			}
			if ((bool)m_loadingPopup)
			{
				m_loadingPopup.SetActiveRecursively(false);
			}
		}
		else if (m_currentLoadingScreenMode == LoadingScreenMode.FULLSCREEN_TIP)
		{
			gameObject = m_loadingPopup;
		}
		else
		{
			Transform transform = gameObject.transform.Find("LabelTop");
			if (transform != null)
			{
				NBUILocalize component = transform.GetComponent<NBUILocalize>();
				if (component != null)
				{
					component.Key = p_topLabel;
				}
			}
			Transform transform2 = gameObject.transform.Find("LabelBottom");
			if (transform2 != null)
			{
				NBUILocalize component2 = transform2.GetComponent<NBUILocalize>();
				if (component2 != null)
				{
					component2.Key = p_bottomLabel;
				}
			}
		}
		if (gameObject != null)
		{
			gameObject.SetActiveRecursively(true);
			if (m_currentLoadingScreenMode == LoadingScreenMode.FULLSCREEN_TIP && m_progressBar != null && (bool)m_progressBar.transform.parent)
			{
				m_progressBar.transform.parent.gameObject.SetActiveRecursively(false);
			}
		}
	}

	public void ShowLoadingPopupNetWait(bool p_useCircle = true)
	{
		if (!isLoading)
		{
			SetEnableLayer("UI", false);
			HideFullScreenLoadingInternal();
			ShowLoadingPopup(SingletonComponent<NBLocalization>.Instance.Get("common_loading_connectingserver"), SingletonComponent<NBLocalization>.Instance.Get("common_loading_connectionnotice"), p_useCircle);
		}
	}

	private bool IsThereAPopup()
	{
		return m_popupStack.Count > 0 || m_loadingPopup.gameObject.active;
	}

	public void HideLoadingPopup(bool p_enableInput = true)
	{
		if (p_enableInput)
		{
			EnableUI();
		}
		if (!m_loadingShouldPersist && !(null == m_loadingPopup))
		{
			m_loadingPopup.SetActiveRecursively(false);
			NBUILocalize[] componentsInChildren = MBHier.GetComponentsInChildren<NBUILocalize>(m_loadingPopup);
			NBUILocalize[] array = componentsInChildren;
			foreach (NBUILocalize nBUILocalize in array)
			{
				nBUILocalize.Key = string.Empty;
			}
			m_loadingPopupHourglass.SetActiveRecursively(false);
			NBUILocalize[] componentsInChildren2 = MBHier.GetComponentsInChildren<NBUILocalize>(m_loadingPopupHourglass);
			NBUILocalize[] array2 = componentsInChildren2;
			foreach (NBUILocalize nBUILocalize2 in array2)
			{
				nBUILocalize2.Key = string.Empty;
			}
		}
	}

	public void HideFullScreenLoading()
	{
		if (m_currentLoadingScreenMode == LoadingScreenMode.FULLSCREEN_STARTUP)
		{
			StartCoroutine(HideFullScreenLoadingDelayed());
			return;
		}
		if (m_currentLoadingScreenMode == LoadingScreenMode.FULLSCREEN_MONSTER_BLADE && (bool)m_loadingWithMonsterBlade)
		{
			m_loadingWithMonsterBlade.SetActiveRecursively(false);
		}
		HideFullScreenLoadingInternal();
	}

	private IEnumerator HideFullScreenLoadingDelayed()
	{
		m_curProgress = 1f;
		UpdateProgressBar();
		yield return null;
		Dbg.Log("LOADSTEP: hiding loading screen");
		HideFullScreenLoadingInternal();
	}

	private void HideFullScreenLoadingInternal()
	{
		MBLoadingHold.Hidden();   // PORT (CD-063)
		m_currentLoadingScreenMode = LoadingScreenMode.OVERLAY_ONLY;
		SetLoadingPersists(false);
		HideLoadingPopup(false);
		UnityEngine.Object.Destroy(m_loadingWithBGInstance);
		m_loadingWithBGInstance = null;
		if (m_progressBar != null && (bool)m_progressBar.transform.parent)
		{
			m_progressBar.transform.parent.gameObject.SetActiveRecursively(false);
		}
		if (m_listenerAdded)
		{
			m_listenerAdded = false;
		}
		if (m_tipsTimer != null)
		{
			m_tipsTimer.Reset();
		}
	}

	public void HidePopup()
	{
		HideLoadingPopup();
		m_hasFocus = false;
	}

	public void ShowShopStamina(string hintMsg, PopupCallback onYes, PopupCallback onNo)
	{
		LoadPopup(PopupName.SHOP_STAMINA_POPUP);
		Popup popup = new Popup();
		GameObject gameObject = UnityEngine.Object.Instantiate(m_popupObjects[7]) as GameObject;
		Transform transform = gameObject.transform.Find("DESC_TXT");
		if (transform != null)
		{
			NBUILocalize component = transform.GetComponent<NBUILocalize>();
			if (component != null)
			{
				string text = string.Empty;
				if (hintMsg != null)
				{
					text = SingletonComponent<NBLocalization>.Instance.Get(hintMsg) + "\n";
				}
				component.Key = text + string.Format(SingletonComponent<NBLocalization>.Instance.Get(component.Key), SingletonComponent<StoreManager>.Instance.StaminaPrice);
			}
		}
		popup.m_popupObj = gameObject;
		popup.m_popupName = PopupName.SHOP_STAMINA_POPUP;
		popup.m_onYes = onYes;
		popup.m_onNo = onNo;
		popup.m_onClose = null;
		SetMessageTarget(gameObject);
		m_popupStack.Add(popup);
	}

	public void ShowShopBagSlots(PopupCallback onYes, PopupCallback onNo)
	{
		LoadPopup(PopupName.SHOP_BAGSLOTS_POPUP);
		Popup popup = new Popup();
		GameObject gameObject = UnityEngine.Object.Instantiate(m_popupObjects[5]) as GameObject;
		UILabel component = gameObject.transform.Find("DESC_TXT").GetComponent<UILabel>();
		int bagSlotsPrice = SingletonComponent<StoreManager>.Instance.BagSlotsPrice;
		int store_bag_slot_increase_amount = GLOBAL.GAME_SETTINGS.store_bag_slot_increase_amount;
		string format = SingletonComponent<NBLocalization>.Instance.Get("shop_bagslots_description");
		string text = string.Format(format, store_bag_slot_increase_amount, bagSlotsPrice);
		component.text = text;
		popup.m_popupObj = gameObject;
		popup.m_popupName = PopupName.SHOP_BAGSLOTS_POPUP;
		popup.m_onYes = onYes;
		popup.m_onNo = onNo;
		popup.m_onClose = null;
		SetMessageTarget(gameObject);
		m_popupStack.Add(popup);
	}

	public void ShowDiamondGot(int p_diamondsGot, PopupCallback p_onBack)
	{
		if (p_diamondsGot <= 0)
		{
			Dbg.LogError("You cannot show a negative number of Diamonds : " + p_diamondsGot);
			if (p_onBack != null)
			{
				p_onBack();
			}
			return;
		}
		LoadPopup(PopupName.DIAMOND_OK_POPUP);
		Popup popup = new Popup();
		GameObject gameObject = UnityEngine.Object.Instantiate(m_popupObjects[25]) as GameObject;
		Transform transform = gameObject.transform.Find("NumOfDiamond");
		if (transform != null)
		{
			UILabel component = transform.GetComponent<UILabel>();
			component.text = p_diamondsGot + " " + SingletonComponent<NBLocalization>.Instance.Get("earn_diamond_diamond_quantifier");
		}
		popup.m_popupObj = gameObject;
		popup.m_popupName = PopupName.DIAMOND_OK_POPUP;
		popup.m_onYes = null;
		popup.m_onNo = null;
		popup.m_onClose = p_onBack;
		SetMessageTarget(gameObject);
		m_popupStack.Add(popup);
	}

	public void ShowDiamondShop(PopupCallback _onBack, string hintMsg)
	{
		LoadPopup(PopupName.SHOP_LIST_POPUP);
		Popup popup = new Popup();
		GameObject gameObject = UnityEngine.Object.Instantiate(m_popupObjects[6]) as GameObject;
		if (gameObject == null)
		{
			Dbg.LogError("UIPopupController : --> Shop popup wasn't instatiated <-- ", Dbg.NBLogType.UI_POPUP);
			return;
		}
		ShopGrid componentInChildren = MBHier.GetComponentInChildren<ShopGrid>(gameObject);
		if (componentInChildren == null)
		{
			Dbg.LogError("UIPopupController : --> No shopGrid found on this prefab <-- ", Dbg.NBLogType.UI_POPUP);
			popup = null;
			UnityEngine.Object.Destroy(gameObject);
			gameObject = null;
			return;
		}
		popup.m_popupObj = gameObject;
		popup.m_popupName = PopupName.SHOP_LIST_POPUP;
		popup.m_onYes = null;
		popup.m_onNo = _onBack;
		popup.m_onClose = null;
		SetMessageTarget(gameObject);
		m_popupStack.Add(popup);
		SetDiamondShopList(SingletonComponent<StoreManager>.Instance.Diamonds, componentInChildren);
		UILabel component = gameObject.transform.Find("DESC_TXT").GetComponent<UILabel>();
		component.text = string.Empty;
		if (hintMsg == null)
		{
			hintMsg = "shop_buy_diamonds";
		}
		string text = SingletonComponent<NBLocalization>.Instance.Get(hintMsg);
		component.text = text;
	}

	public void ShowWeaponCheck(Equipment.Data p_weaponData, DungeonDbStruct p_dungeonData, PopupCallback p_ehanceCB = null, PopupCallback p_gachaCB = null, PopupCallback p_continueCB = null)
	{
		LoadPopup(PopupName.WEAPON_CHECK_POPUP);
		Popup popup = new Popup();
		GameObject gameObject = UnityEngine.Object.Instantiate(m_popupObjects[26]) as GameObject;
		if (gameObject == null)
		{
			Dbg.LogError("Weapon Check Popup failed to instantiate");
			return;
		}
		WeaponCheckPopup component = gameObject.GetComponent<WeaponCheckPopup>();
		if (component != null)
		{
			component.SetInfo(p_weaponData, p_dungeonData);
		}
		popup.m_popupObj = gameObject;
		popup.m_popupName = PopupName.WEAPON_CHECK_POPUP;
		popup.m_onYes = p_ehanceCB;
		popup.m_onNo = p_gachaCB;
		popup.m_onClose = p_continueCB;
		SetMessageTarget(gameObject);
		m_popupStack.Add(popup);
	}

	public void ShowFriendSelectConfirm(PlayerData p_playerData, PopupCallback onYes = null, PopupCallback onNo = null, PopupCallback onClose = null)
	{
		Dbg.LogEx(" -- ShowFriendSelectConfirm Popup : Name : {0} ", Dbg.NBLogType.UI_POPUP, p_playerData.Name);
		LoadPopup(PopupName.FRIEND_SELECT_POPUP);
		Popup popup = new Popup();
		GameObject gameObject = UnityEngine.Object.Instantiate(m_popupObjects[27]) as GameObject;
		if (gameObject == null)
		{
			Dbg.LogError("Weapon Check Popup failed to instantiate");
			return;
		}
		FriendSelectPopup component = gameObject.GetComponent<FriendSelectPopup>();
		if (component != null)
		{
			component.SetData(p_playerData);
		}
		popup.m_popupObj = gameObject;
		popup.m_popupName = PopupName.SMALL;
		popup.m_onYes = onYes;
		popup.m_onNo = onNo;
		popup.m_onClose = onClose;
		SetMessageTarget(gameObject);
		m_popupStack.Add(popup);
	}

	public void ShowPlayerProfilePopup(PopupCallback p_onYes = null, PopupCallback p_onClose = null)
	{
		LoadPopup(PopupName.PLAYER_PROFILE_POPUP);
		Popup popup = new Popup();
		GameObject gameObject = UnityEngine.Object.Instantiate(m_popupObjects[37]) as GameObject;
		if (null == gameObject)
		{
			Dbg.LogError("Failed to instantiate PLAYER_PROFILE_POPUP.");
			return;
		}
		popup.m_popupObj = gameObject;
		popup.m_popupName = PopupName.PLAYER_PROFILE_POPUP;
		popup.m_onYes = p_onYes;
		popup.m_onNo = null;
		popup.m_onClose = p_onClose;
		SetParentToPopupController(gameObject);
		m_popupStack.Add(popup);
	}

	public void ShowPromoCodePopup(PopupCallback p_onYes = null, PopupCallback p_onClose = null)
	{
		LoadPopup(PopupName.PROMO_CODE_POPUP);
		Popup popup = new Popup();
		GameObject gameObject = UnityEngine.Object.Instantiate(m_popupObjects[38]) as GameObject;
		if (null == gameObject)
		{
			Dbg.LogError("Failed to instantiate PROMO_CODE_POPUP.");
			return;
		}
		popup.m_popupObj = gameObject;
		popup.m_popupName = PopupName.PROMO_CODE_POPUP;
		popup.m_onYes = p_onYes;
		popup.m_onNo = null;
		popup.m_onClose = p_onClose;
		SetParentToPopupController(gameObject);
		m_popupStack.Add(popup);
	}

	public void ShowPromoClaimPopup(ItemDetails p_data, CommonRewards p_reward = null, UISprite p_sprite = null, PopupCallback p_onYes = null)
	{
		LoadPopup(PopupName.PROMO_CLAIM_POPUP);
		Popup popup = new Popup();
		GameObject gameObject = UnityEngine.Object.Instantiate(m_popupObjects[39]) as GameObject;
		if (null == gameObject)
		{
			Dbg.LogError("Failed to instantiate PROMO_CLAIM_POPUP.");
			return;
		}
		PromoClaimPopup component = gameObject.GetComponent<PromoClaimPopup>();
		if (null != component)
		{
			if (p_reward != null && null != p_sprite)
			{
				component.SetCommonReward(p_reward, p_sprite);
			}
			else
			{
				component.SetItemDetails(p_data);
			}
		}
		popup.m_popupObj = gameObject;
		popup.m_popupName = PopupName.PROMO_CLAIM_POPUP;
		popup.m_onYes = p_onYes;
		popup.m_onNo = null;
		popup.m_onClose = null;
		SetMessageTarget(gameObject);
		m_popupStack.Add(popup);
	}

	public void ShowHolyDiamondsPopup(HolyDiamondsPopup.Type p_type, int p_amount, int p_currentAmount, PopupCallback p_onYes = null, PopupCallback p_onNo = null)
	{
		LoadPopup(PopupName.HOLY_DIAMOND_POPUP);
		Popup popup = new Popup();
		GameObject gameObject = UnityEngine.Object.Instantiate(m_popupObjects[40]) as GameObject;
		if (null == gameObject)
		{
			Dbg.LogError("Failed to instantiate HOLY_DIAMOND_POPUP.");
			return;
		}
		HolyDiamondsPopup component = gameObject.GetComponent<HolyDiamondsPopup>();
		if (null != component)
		{
			component.m_type = p_type;
			component.SetDiamondsNumber(p_amount, (p_type != HolyDiamondsPopup.Type.WARNING) ? Color.white : Color.red);
			component.SetCurrentDiamonds("holydiamonds_popup_current_diamonds", p_currentAmount);
			switch (p_type)
			{
			case HolyDiamondsPopup.Type.REWARD:
				component.SetHeader("holydiamonds_popup_reward_header");
				component.SetInfoWithReward("holydiamonds_popup_reward_info", p_amount);
				break;
			case HolyDiamondsPopup.Type.WARNING:
				component.SetHeader("holydiamonds_popup_warning_header");
				component.SetInfo("holydiamonds_popup_warning_info");
				break;
			}
		}
		popup.m_popupObj = gameObject;
		popup.m_popupName = PopupName.HOLY_DIAMOND_POPUP;
		popup.m_onYes = p_onYes;
		popup.m_onNo = p_onNo;
		popup.m_onClose = null;
		SetParentToPopupController(gameObject);
		m_popupStack.Add(popup);
	}

	public void ShowWeiboFriendSelect(PopupCallback p_onYes = null, PopupCallback p_onClose = null)
	{
		LoadPopup(PopupName.WEIBO_FRIEND_SELECT);
		Popup popup = new Popup();
		GameObject gameObject = UnityEngine.Object.Instantiate(m_popupObjects[41]) as GameObject;
		if (null == gameObject)
		{
			Dbg.LogError("Failed to instantiate WEIBO_FRIEND_SELECT.");
			return;
		}
		popup.m_popupObj = gameObject;
		popup.m_popupName = PopupName.WEIBO_FRIEND_SELECT;
		popup.m_onYes = null;
		popup.m_onNo = null;
		popup.m_onClose = null;
		WeiboUIInviteDlg component = gameObject.GetComponent<WeiboUIInviteDlg>();
		component.m_invitesSentCallback = p_onYes;
		component.m_invitesCanceledCallback = p_onClose;
		SetParentToPopupController(gameObject);
		m_popupStack.Add(popup);
	}

	private bool ContainSprite(UIAtlas _atlas, string _spriteName)
	{
		if (_atlas == null)
		{
			return false;
		}
		string text = _spriteName.ToLower();
		List<string> listOfSprites = _atlas.GetListOfSprites();
		foreach (string item in listOfSprites)
		{
			if (text == item.ToLower())
			{
				return true;
			}
		}
		return false;
	}

	private void SetDiamondShopList(List<ServerStoreItem> _diamondsShop, ShopGrid _shopGrid)
	{
		if (_diamondsShop == null || _diamondsShop.Count == 0 || _shopGrid == null)
		{
			return;
		}
		_shopGrid.Clear();
		UIAtlas uIAtlas = null;
		for (int i = 0; i < _diamondsShop.Count; i++)
		{
			ServerStoreItem data = _diamondsShop[i];
			_shopGrid.AddData(data);
			ShopCell shopCell = _shopGrid.AddCell(0f, 0f);
			m_shopCellWidth = shopCell.GetComponent<Collider>().bounds.size.x;
			UIPanel component = shopCell.GetComponent<UIPanel>();
			if (component != null)
			{
				UnityEngine.Object.Destroy(component);
			}
			if (shopCell != null)
			{
				shopCell.data = data;
				shopCell.SetShopCellTarget(base.gameObject);
			}
		}
	}

	private void OnShopCellClick(GameObject _gao)
	{
		ShopCell component = _gao.GetComponent<ShopCell>();
		OnClose();
		if (SingletonComponent<StoreManager>.Instance != null && component != null)
		{
			SingletonComponent<StoreManager>.Instance.SelectedDiamond(component.cellIndex);
			Dbg.Log(" --> OnDiamondClick <-- " + component.cellIndex + " " + component.data.db_id);
		}
	}

	public void ShowShopStaminaSuccess()
	{
		ShowOkPicture(m_staminaSprite, "shop_buy_stamina_successful");
	}

	public void ShowShopBattlePointSuccess()
	{
		ShowOkPicture(m_battlePointSprite, "shop_buy_battlepoint_successful");
	}

	public void ShowShopBagSlotsSuccess()
	{
		ShowOkPicture(m_bagSlotsSprite, "shop_buy_bagslots_successful");
	}

	public void ShowShopStaminaFailed(string _localizeKey = null, bool needShopDiamond = false)
	{
		if (needShopDiamond)
		{
			if (SingletonComponent<PlayerDataManager>.Instance != null && SingletonComponent<StoreManager>.Instance != null)
			{
				int p_minimumDiamond = SingletonComponent<StoreManager>.Instance.StaminaPrice - SingletonComponent<PlayerDataManager>.Instance.GetPlayerData().m_userData.premiumCurrency;
				SingletonComponent<StoreManager>.Instance.ShowDiamondsShop(SingletonComponent<StoreManager>.Instance.AutoBuyStamina, null, _localizeKey, p_minimumDiamond);
			}
		}
		else
		{
			ShowOkPicture(m_staminaSprite, _localizeKey);
		}
	}

	public void ShowShopBattlePointFailed(string _localizeKey = null, bool needShopDiamond = false)
	{
		if (needShopDiamond)
		{
			if (SingletonComponent<PlayerDataManager>.Instance != null && SingletonComponent<StoreManager>.Instance != null)
			{
				int p_minimumDiamond = SingletonComponent<StoreManager>.Instance.BattlePointPrice - SingletonComponent<PlayerDataManager>.Instance.GetPlayerData().m_userData.premiumCurrency;
				SingletonComponent<StoreManager>.Instance.ShowDiamondsShop(SingletonComponent<StoreManager>.Instance.AutoBuyBattlePoint, null, _localizeKey, p_minimumDiamond);
			}
		}
		else
		{
			ShowOkPicture(m_battlePointSprite, _localizeKey);
		}
	}

	public void ShowShopBagSlotsFailed(string _localizeKey = "shop_buy_bagslots_failed", PopupCallback p_buyCb = null, PopupCallback p_backCB = null)
	{
		SingletonComponent<StoreManager>.Instance.ShowDiamondsShop(p_buyCb, p_backCB, _localizeKey);
	}

	public void ShowShopBattlePoint(string hintMsg, PopupCallback onYes, PopupCallback onNo)
	{
		LoadPopup(PopupName.SHOP_BATTLEPOINT_POPUP);
		Popup popup = new Popup();
		GameObject gameObject = UnityEngine.Object.Instantiate(m_popupObjects[33]) as GameObject;
		Transform transform = gameObject.transform.Find("DESC_TXT");
		if (transform != null)
		{
			NBUILocalize component = transform.GetComponent<NBUILocalize>();
			if (component != null)
			{
				string text = string.Empty;
				if (hintMsg != null)
				{
					text = SingletonComponent<NBLocalization>.Instance.Get(hintMsg) + "\n";
				}
				component.Key = text + string.Format(SingletonComponent<NBLocalization>.Instance.Get(component.Key), SingletonComponent<StoreManager>.Instance.BattlePointPrice);
			}
		}
		popup.m_popupObj = gameObject;
		popup.m_popupName = PopupName.SHOP_BATTLEPOINT_POPUP;
		popup.m_onYes = onYes;
		popup.m_onNo = onNo;
		popup.m_onClose = null;
		SetMessageTarget(gameObject);
		m_popupStack.Add(popup);
	}

	public static void SetEnableLayer(string p_layerName, bool p_enable)
	{
		List<UICamera> allUICameras = UICamera.allUICameras;
		int num = LayerMask.NameToLayer(p_layerName);
		int num2 = 1 << num;
		foreach (UICamera item in allUICameras)
		{
			if ((item.eventReceiverMask.value & num2) != 0)
			{
				item.enabled = !item.locked && p_enable;
			}
		}
		Dbg.Log("SetEnableLayer : " + p_layerName + "\t" + p_enable);
	}

	public Camera GetPopupCamera()
	{
		return m_popupCamera;
	}

	public static void SetAllInputsEnabled(bool val)
	{
		Dbg.Log(" --> SetAllInputsEnabled : " + val + " <-- ", Dbg.NBLogType.UI_POPUP);
		List<UICamera> allUICameras = UICamera.allUICameras;
		foreach (UICamera item in allUICameras)
		{
			item.enabled = val;
		}
	}

	public void DisableUiExceptLayerMask(string p_layerName)
	{
		int num = 1 << LayerMask.NameToLayer(p_layerName);
		List<UICamera> allUICameras = UICamera.allUICameras;
		foreach (UICamera item in allUICameras)
		{
			if (item.eventReceiverMask.value != num)
			{
				item.enabled = false;
			}
		}
	}

	public void EnableKeyboard(bool p_enable)
	{
		if (!(null == m_popupCamera))
		{
			UICamera component = m_popupCamera.GetComponent<UICamera>();
			if (null != component)
			{
				component.useKeyboard = p_enable;
			}
		}
	}

	public void EnableUI()
	{
		Dbg.Log("Enable UI", Dbg.NBLogType.UI_POPUP);
		SetAllInputsEnabled(true);
	}

	public static void LockUICamera(string p_layerName, bool p_locked)
	{
		List<UICamera> allUICameras = UICamera.allUICameras;
		int num = LayerMask.NameToLayer(p_layerName);
		int num2 = 1 << num;
		foreach (UICamera item in allUICameras)
		{
			if ((item.eventReceiverMask.value & num2) != 0)
			{
				item.locked = p_locked;
			}
		}
	}

	public void DisableOtherUILayers()
	{
		Dbg.Log(" --> DisableOtherUILayers <-- ", Dbg.NBLogType.UI_POPUP);
		List<UICamera> allUICameras = UICamera.allUICameras;
		LayerMask layerMask = 1 << LayerMask.NameToLayer("PopUpLayer");
		foreach (UICamera item in allUICameras)
		{
			if ((int)item.eventReceiverMask != (int)layerMask)
			{
				item.enabled = false;
			}
		}
	}

	private void EnableOtherUILayers()
	{
		Dbg.Log(" --> EnableOtherUILayers <-- ", Dbg.NBLogType.UI_POPUP);
		List<UICamera> allUICameras = UICamera.allUICameras;
		LayerMask layerMask = 1 << LayerMask.NameToLayer("PopUpLayer");
		foreach (UICamera item in allUICameras)
		{
			if ((int)item.eventReceiverMask != (int)layerMask)
			{
				item.enabled = true;
			}
		}
	}

	public void RemoveCurrentPopupFromStack()
	{
		if (m_popupStack.Contains(m_currentPopup))
		{
			m_popupStack.Remove(m_currentPopup);
		}
	}

	public void FadeOutBlackOverlay()
	{
		Dbg.Log("m_overlay SendMessage FadeOut");
		m_overlay.SendMessage("FadeOut", null, SendMessageOptions.DontRequireReceiver);
	}

	public static void PrintCameraAndLayers()
	{
	}
}
