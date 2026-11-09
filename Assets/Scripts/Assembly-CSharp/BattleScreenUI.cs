using Models;
using ServiceCalls;
using UnityEngine;

public class BattleScreenUI : SingletonComponent<BattleScreenUI>
{
	public enum TextEffect
	{
		None = -1,
		Hit = 0,
		Critical = 1,
		Combo = 2,
		ComboBonus = 3,
		Dodge = 4,
		Stun = 5,
		Heal = 6,
		Super = 7,
		LifeSteal = 8,
		MassiveDamage = 9,
		PetSkill = 10,
		PerfectDodge = 11,
		PerfectBlock = 12,
		BlockFailed = 13,
		DodgeFailed = 14,
		DamageBuff = 15,
		CriticalBuff = 16,
		Max = 17
	}

	public enum PostBattleText
	{
		None = -1,
		Perfect = 0,
		Max = 1
	}

	public enum Popup
	{
		None = -1,
		Retry = 0,
		Pause = 1,
		Exit = 2,
		Continue = 3,
		PreNextBattle = 4,
		Error = 5
	}

	public enum Cooldown
	{
		None = -1,
		Dodge = 0,
		Max = 1
	}

	public enum Button
	{
		None = -1,
		DodgeLeft = 0,
		DodgeRight = 1,
		Block = 2,
		AssistLeft = 3,
		AssistRight = 4,
		Pause = 5
	}

	public delegate void Callback();

	private const int m_retryMaxCount = 5;

	private Player m_player;

	private Monster m_enemy;

	private GameObject m_pressingBtn;

	private Button m_pressingBtnEnum = Button.None;

	private Button m_lastPressedBtnEnum = Button.None;

	[SerializeField]
	private EncounterEnemyUi m_EncounterEnemyUi;

	public UIImageButton m_pauseButton;

	private Popup m_popupType = Popup.None;

	public UISpriteAnimation m_fxFeedback;

	public ParticleSystem m_focusRing;

	public UISprite m_screenFlashFx;

	private Color m_flashSrc = Color.white;

	private Color m_flashDst = Color.white;

	private float m_flashDuration;

	private float m_lerp;

	private float m_lerpStep;

	private float m_lerpTime;

	public UIImageButton m_blockButton;

	public UIImageButton[] m_dodgeButtons = new UIImageButton[2];

	public FriendAssistButton m_assistButton;

	public SuperButton m_superButton;

	private bool m_dodgeEnable = true;

	private bool m_blockEnable = true;

	public NameLabel m_friendName;

	public MomentumBar m_playerStaminaBar;

	public float m_textDisplayTime;

	public float m_postBattleTextDisplayTime;

	public float m_textPopupTime = 0.15f;

	public float m_textFadeTime = 0.25f;

	public float m_normalDamageScale = 0.5f;

	public float m_heavyDamageScale = 0.8f;

	public float m_alertDelay = 0.3f;

	public float m_alertTextAlpha = 0.7f;

	private float m_alertDelayCounter;

	public BattleTextEffect m_textEffect;

	public BattleTextEffect[] m_postBattleTexts = new BattleTextEffect[1];

	public BattleTextEffect[] m_comboTexts;

	public Transform m_comboTextRoot;

	public BattleTextEffect m_attackResultText;

	public BattleTextEffect m_atkOrDefAlert;

	private bool m_enableAlert = true;

	private TextEffect m_showingText = TextEffect.None;

	private BattleTextEffect m_showingPostBattleText;

	public Color[] m_comboTextColors;

	public float[] m_comboTextScales;

	public float m_friendAssistTextDisplayTime = 1.5f;

	public GameObject m_battleUiRoot;

	public GameObject m_battleUiButtonRoot;

	public GameObject m_dodoboHudRoot;

	public GameObject m_enemyDamagePanel;

	public GameObject m_dodoboDamagePanel;

	private CinematicBar m_cinematicBar;

	private PlayerDataManager m_playerDataManager;

	private PlayerData m_playerData;

	private Services m_services;

	private PowerMove.MoveEnum m_powerMove = PowerMove.MoveEnum.None;

	private uint m_retryRequestID;

	private Camera battleHudCamera;

	public static bool disableInputsAfterFB;

	private float tempTimeScale;

	public Button PressingButton
	{
		get
		{
			return m_pressingBtnEnum;
		}
	}

	public NameLabel FriendLabel
	{
		get
		{
			return m_friendName;
		}
	}

	public bool EnableAlertText
	{
		get
		{
			return m_enableAlert;
		}
		set
		{
			m_enableAlert = value;
			if (!m_enableAlert && m_atkOrDefAlert.enabled)
			{
				m_atkOrDefAlert.Stop();
			}
		}
	}

	public void EnablePauseButton(bool p_enable)
	{
		if (null != m_pauseButton && null != m_pauseButton.GetComponent<Collider>())
		{
			m_pauseButton.GetComponent<Collider>().enabled = p_enable;
		}
	}

	private void Awake()
	{
		battleHudCamera = base.gameObject.transform.Find("Battle HUD Camera").GetComponent<Camera>();
		m_playerDataManager = SingletonComponent<PlayerDataManager>.Instance;
		if (m_playerDataManager != null)
		{
			m_playerData = m_playerDataManager.GetPlayerData();
		}
		m_services = SingletonComponent<Services>.Instance;
		EnableAlertText = false;
		base.enabled = false;
		Messenger.AddListener("battle_ready", Initialize);
		Messenger.AddListener("battle_start", OnBattleStart);
		Messenger.AddListener("battle_end", OnBattleEnd);
		Messenger<Player.DodgeDirection>.AddListener("perfect_dodge_start", OnPerfectDodgeStart);
		Messenger<PlayerAttack>.AddListener("player_begin_atk", OnPlayerBeginAttack);
		Messenger<MonsterCollisionData>.AddListener("player_get_hit", OnPlayerGetHit);
		Messenger.AddListener("perfect_dodge_end", OnPerfectDodgeEnd);
		Messenger.AddListener("perfect_block", OnPerfectBlock);
		Messenger.AddListener("block_failed", OnBlockFailed);
		Messenger<bool>.AddListener("enable_dodge", OnDodgeEnable);
		Messenger<bool>.AddListener("enable_block", OnBlockEnable);
		Messenger<CutsceneController.CutsceneType>.AddListener(CutsceneMessage.CUTSCENE_START, OnCutsceneStart);
		Messenger<CutsceneController.CutsceneType>.AddListener(CutsceneMessage.CUTSCENE_END, OnCutsceneEnd);
		Messenger.AddListener("monster_stun", OnMonsterStun);
		Messenger<DamageData>.AddListener("monster_receive_dmg", OnMonsterReceiveDamage);
		Messenger<PowerMove.MoveEnum>.AddListener("power_move", OnPowerMove);
		Messenger<SkillEnum.AssistSkill>.AddListener("friend_assist", OnTriggerFriendAssist);
	}

	private new void OnDestroy()
	{
		Messenger.RemoveListener("battle_ready", Initialize);
		Messenger.RemoveListener("battle_start", OnBattleStart);
		Messenger.RemoveListener("battle_end", OnBattleEnd);
		Messenger<Player.DodgeDirection>.RemoveListener("perfect_dodge_start", OnPerfectDodgeStart);
		Messenger<PlayerAttack>.RemoveListener("player_begin_atk", OnPlayerBeginAttack);
		Messenger<MonsterCollisionData>.RemoveListener("player_get_hit", OnPlayerGetHit);
		Messenger.RemoveListener("perfect_dodge_end", OnPerfectDodgeEnd);
		Messenger.RemoveListener("perfect_block", OnPerfectBlock);
		Messenger.RemoveListener("block_failed", OnBlockFailed);
		Messenger<bool>.RemoveListener("enable_dodge", OnDodgeEnable);
		Messenger<bool>.RemoveListener("enable_block", OnBlockEnable);
		Messenger<CutsceneController.CutsceneType>.RemoveListener(CutsceneMessage.CUTSCENE_START, OnCutsceneStart);
		Messenger<CutsceneController.CutsceneType>.RemoveListener(CutsceneMessage.CUTSCENE_END, OnCutsceneEnd);
		Messenger.RemoveListener("monster_stun", OnMonsterStun);
		Messenger<DamageData>.RemoveListener("monster_receive_dmg", OnMonsterReceiveDamage);
		Messenger<PowerMove.MoveEnum>.RemoveListener("power_move", OnPowerMove);
		Messenger<SkillEnum.AssistSkill>.RemoveListener("friend_assist", OnTriggerFriendAssist);
	}

	public void Initialize()
	{
		if (m_player == null)
		{
			RetrievePlayerData();
		}
		if (m_enemy == null)
		{
			RetrieveEnemyData();
		}
		if (m_fxFeedback == null)
		{
			Dbg.LogError("Feedback effect has not been specified");
			m_popupType = Popup.Error;
			return;
		}
		m_fxFeedback.gameObject.SetActiveRecursively(false);
		if (m_screenFlashFx != null)
		{
			m_screenFlashFx.gameObject.SetActiveRecursively(false);
		}
		if (m_textEffect != null)
		{
			m_textEffect.Stop();
		}
		BattleTextEffect[] postBattleTexts = m_postBattleTexts;
		foreach (BattleTextEffect battleTextEffect in postBattleTexts)
		{
			battleTextEffect.Stop();
		}
		BattleTextEffect[] comboTexts = m_comboTexts;
		foreach (BattleTextEffect battleTextEffect2 in comboTexts)
		{
			battleTextEffect2.Stop();
		}
		if (m_attackResultText != null)
		{
			m_attackResultText.Stop();
		}
		if (m_playerStaminaBar != null)
		{
			m_playerStaminaBar.SliderValue = 1f;
		}
		m_pressingBtn = null;
		m_pressingBtnEnum = Button.None;
		if (UIPopupController.Instance != null)
		{
			UIPopupController.Instance.HidePopup();
		}
		ShowFocusRing(false);
		Transform transform = base.gameObject.transform.FindInHierarchy("CinematicBar");
		if (transform != null)
		{
			m_cinematicBar = transform.GetComponent<CinematicBar>();
		}
		if (m_atkOrDefAlert != null)
		{
			m_atkOrDefAlert.Stop();
		}
		m_alertDelayCounter = 0f;
		m_popupType = Popup.None;
		base.enabled = true;
	}

	private void RetrievePlayerData()
	{
		StageManager instance = SingletonComponent<StageManager>.Instance;
		if (instance == null)
		{
			Dbg.LogError("Cannot find StageManager instance");
			m_popupType = Popup.Error;
			return;
		}
		GameObject playerObject = instance.PlayerObject;
		if (playerObject == null)
		{
			Dbg.LogError("Cannot find player game-object");
			m_popupType = Popup.Error;
		}
		else
		{
			m_player = playerObject.GetComponent<Player>();
		}
	}

	private void RetrieveEnemyData()
	{
		StageManager instance = SingletonComponent<StageManager>.Instance;
		if (instance == null)
		{
			Dbg.LogError("Cannot find StageManager instance");
			m_popupType = Popup.Error;
			return;
		}
		GameObject enemyObject = SingletonComponent<StageManager>.Instance.EnemyObject;
		if (enemyObject == null)
		{
			Dbg.LogError("Cannot find enemy game-object");
			m_popupType = Popup.Error;
			return;
		}
		m_enemy = enemyObject.GetComponent<Monster>();
		if (enemyObject.GetComponent<Dodobo>() != null)
		{
			m_dodoboHudRoot.SetActiveRecursively(false);
		}
		else
		{
			m_dodoboHudRoot.SetActiveRecursively(false);
		}
		MonsterDamageDisplay component = m_enemy.GetComponent<MonsterDamageDisplay>();
		if (!Dbg.Assert(component != null, "BattleScreenUI.Initialize(), MonsterDamageDisplay is null"))
		{
			component.ReStart();
		}
	}

	public bool IsDodgeEnable()
	{
		if (m_dodgeButtons != null && m_dodgeButtons[0] != null)
		{
			return m_dodgeButtons[0].gameObject.active;
		}
		return false;
	}

	public bool IsPressing()
	{
		return m_pressingBtnEnum != Button.None;
	}

	public void ShowBattleUi(bool p_show)
	{
		if (m_battleUiRoot != null)
		{
			Dbg.Log("ShowBattleUi: " + p_show);
			m_battleUiRoot.SetActiveRecursively(p_show);
			if (!m_blockEnable)
			{
				ShowBlockButton(false);
			}
			else
			{
				ShowBlockButton(p_show);
			}
			if (!m_dodgeEnable)
			{
				ShowDodgeButton(false);
			}
			else
			{
				ShowDodgeButton(p_show);
			}
			if (m_fxFeedback != null)
			{
				m_fxFeedback.gameObject.SetActiveRecursively(false);
			}
			if (m_focusRing != null)
			{
				m_focusRing.Stop();
				m_focusRing.gameObject.SetActiveRecursively(false);
			}
			if (m_screenFlashFx != null)
			{
				m_screenFlashFx.gameObject.SetActiveRecursively(false);
			}
			if (m_EncounterEnemyUi != null)
			{
				ShowEnterEnemyUi(false);
			}
			if (m_atkOrDefAlert != null)
			{
				m_atkOrDefAlert.Stop();
			}
			if (m_textEffect != null)
			{
				m_textEffect.Stop();
			}
		}
	}

	public void ShowBattleUiButtons(bool p_show)
	{
		Dbg.Log("ShowBattleUiButtons: " + p_show);
		if (!p_show)
		{
			UnpressedButton();
		}
		ShowDodgeButton(p_show);
		ShowBlockButton(p_show);
	}

	public void ShowDodgeButton(bool p_show)
	{
		if (!m_dodgeEnable && p_show)
		{
			return;
		}
		Dbg.Log("ShowDodgeButton: " + p_show);
		UIImageButton[] dodgeButtons = m_dodgeButtons;
		foreach (UIImageButton uIImageButton in dodgeButtons)
		{
			if (uIImageButton != null)
			{
				uIImageButton.gameObject.SetActiveRecursively(p_show);
			}
		}
	}

	public void ShowBlockButton(bool p_show)
	{
		if ((m_blockEnable || !p_show) && m_blockButton != null)
		{
			m_blockButton.gameObject.SetActiveRecursively(p_show);
		}
	}

	public void UnpressedButton()
	{
		if (m_pressingBtn != null)
		{
			if (m_pressingBtn.active)
			{
				m_pressingBtn.SendMessage("ResetButtonScale", SendMessageOptions.DontRequireReceiver);
			}
			m_pressingBtn = null;
			m_pressingBtnEnum = Button.None;
		}
	}

	private void OnBtnPressed(GameObject p_button, Button p_buttonEnum, bool p_oneClick)
	{
		if (base.enabled && battleHudCamera.enabled && (!(null != SingletonComponent<FaderGui>.Instance) || !SingletonComponent<FaderGui>.Instance.enabled) && m_popupType == Popup.None)
		{
			m_pressingBtn = p_button;
			m_pressingBtnEnum = p_buttonEnum;
			m_lastPressedBtnEnum = p_buttonEnum;
			if (p_oneClick)
			{
				p_button.GetComponent<ButtonScale>().OnClicked(OnBtnFinishAnimateClicked);
			}
			else
			{
				p_button.GetComponent<ButtonScale>().OnPressed();
			}
		}
	}

	private void OnBtnReleased()
	{
		if (base.enabled)
		{
			UnpressedButton();
		}
	}

	private void OnBtnFinishAnimateClicked(GameObject p_button)
	{
		if (!base.enabled || m_player.IsDead())
		{
			return;
		}
		if (p_button == m_pauseButton.gameObject)
		{
			bool flag = true;
			if (SingletonComponent<DungeonManager>.Instance != null)
			{
				flag = !SingletonComponent<DungeonManager>.Instance.IsSendingDungeonEnd;
			}
			if (flag)
			{
				Pause(true);
			}
		}
		if (m_pressingBtnEnum != Button.Block)
		{
			UnpressedButton();
		}
	}

	private void OnDodgeLeftBtnPressed(GameObject p_sender)
	{
		if (base.enabled && m_popupType == Popup.None && !m_dodgeButtons[0].m_locked)
		{
			OnBtnPressed(p_sender, Button.DodgeLeft, false);
		}
	}

	private void OnDodgeRightBtnPressed(GameObject p_sender)
	{
		if (base.enabled && m_popupType == Popup.None && !m_dodgeButtons[1].m_locked)
		{
			OnBtnPressed(p_sender, Button.DodgeRight, false);
		}
	}

	private void OnBlockBtnPressed(GameObject p_sender)
	{
		if (base.enabled && m_popupType == Popup.None && !m_blockButton.m_locked)
		{
			OnBtnPressed(p_sender, Button.Block, true);
		}
	}

	private void OnAssistRightBtnPressed(GameObject p_sender)
	{
		if (base.enabled && !(m_assistButton == null) && m_assistButton.gameObject.active && m_assistButton.enabled)
		{
			SkillSystem instance = SingletonComponent<SkillSystem>.Instance;
			if (m_popupType == Popup.None && instance != null)
			{
				instance.TriggerFriendAssistSkill();
			}
		}
	}

	private void OnPausetBtnPressed(GameObject p_sender)
	{
		if (base.enabled && m_popupType == Popup.None)
		{
			OnBtnPressed(p_sender, Button.Pause, true);
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_event_notification_new");
		}
	}

	public uint GenerateNewRetryRequestID()
	{
		m_retryRequestID = (uint)(Random.value * 4.2949673E+09f);
		return m_retryRequestID;
	}

	private void Retry()
	{
		if (SingletonComponent<TutorialManager>.Instance.enabled)
		{
			RetryBattle();
			return;
		}
		GenerateNewRetryRequestID();
		RetryMonetization();
	}

	private void RetryMonetization()
	{
		if (!(m_services == null) && m_playerData != null && !(SingletonComponent<BattleManager>.Instance == null) && !(SingletonComponent<DungeonManager>.Instance == null))
		{
			string dungeonKey = SingletonComponent<DungeonManager>.Instance.DungeonKey;
			int num = SingletonComponent<DungeonManager>.Instance.GetCurrentBattleCounter() - 1;
			if (num < 0)
			{
				num = 0;
			}
			UIPopupController.Instance.ShowLoadingPopupNetWait();
			m_services.RetryBattle(m_services.NubeeId, m_services.DeviceId, dungeonKey, num, 0, m_retryRequestID.ToString(), OnRetrySucceeded, OnRetryFailed);
		}
	}

	public void OnSNSRetryFailed(Services.ServiceHandle _handle, string _error)
	{
		Dbg.LogError("[BattleScreenUI::OnSNSRetryFailed] " + ServiceCallUtils.GetErrorMessage(_handle));
		UIPopupController.Instance.HideLoadingPopup();
		if (_handle == null)
		{
			UIPopupController.Instance.ShowServerGenericError(SNSRetry);
			return;
		}
		if (_handle.returnJson != null)
		{
			JSONObject field = _handle.returnJson.GetField("data");
			if (field != null)
			{
				int field2 = ServiceCallUtils.GetField<int>(field, "next_warcry");
				if (0 < field2 && null != m_playerDataManager && m_playerDataManager.GetPlayerData() != null && m_playerDataManager.GetPlayerData().m_userData != null)
				{
					m_playerDataManager.GetPlayerData().m_userData.timeNextWarcry = field2;
				}
			}
		}
		if (_handle.IsNoInternet())
		{
			UIPopupController.Instance.ShowNoNetRetry(SNSRetry, OnGoBackToReviveMain);
			return;
		}
		int nReturnCode = _handle.nReturnCode;
		if (nReturnCode == 915)
		{
			UIPopupController.Instance.ShowMediumPopupMSG("common_error", "server_generic_error", SNSRetry, OnGoBackToReviveMain, OnGoBackToReviveMain);
		}
		else
		{
			UIPopupController.Instance.ShowMediumPopupMSG("common_error", "server_generic_error", SNSRetry, OnGoBackToReviveMain, OnGoBackToReviveMain);
		}
	}

	public void OnSNSRetrySuccess(Services.ServiceHandle _handle)
	{
		OnRetrySucceeded(_handle);
	}

	public void SNSRetry()
	{
		if (!(m_services == null) && m_playerData != null && !(SingletonComponent<BattleManager>.Instance == null) && !(SingletonComponent<DungeonManager>.Instance == null))
		{
			string dungeonKey = SingletonComponent<DungeonManager>.Instance.DungeonKey;
			int num = SingletonComponent<DungeonManager>.Instance.GetCurrentBattleCounter() - 1;
			if (num < 0)
			{
				num = 0;
			}
			UIPopupController.Instance.ShowLoadingPopupNetWait();
			m_services.RetryBattle(m_services.NubeeId, m_services.DeviceId, dungeonKey, num, 1, m_retryRequestID.ToString(), OnSNSRetrySuccess, OnSNSRetryFailed);
		}
	}

	public void OnRetrySucceeded(Services.ServiceHandle _handle)
	{
		UIPopupController.Instance.HidePopup();
		if (m_playerData == null || m_playerData.m_userData == null)
		{
			Dbg.LogWarn("player data is null, try to get it from PlayerDataManager");
			if (SingletonComponent<PlayerDataManager>.Instance != null)
			{
				m_playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
			}
		}
		if (m_playerData != null && m_playerData.m_userData != null)
		{
			string text = m_playerData.m_userData.ToString() + "\n";
			CharacterAccountDetails playerData = (CharacterAccountDetails)_handle.returnValue;
			m_playerDataManager.SetPlayerData(playerData);
			Messenger.Invoke("retry_battle");
			text += "After : \n";
			text += m_playerData.m_userData.ToString();
			Dbg.Log("BATTLE_SCREEN_UI : --> Currencies Updated <--\n before : \n" + text);
		}
		else
		{
			Dbg.LogWarn("player data is null");
		}
	}

	private void RetryBattle()
	{
		Messenger.Invoke("retry_battle");
	}

	private void RetryPurchaseDiamond()
	{
		SingletonComponent<StoreManager>.Instance.ShowDiamondsShop(RetryBattle, OnGoBackToReviveMain, "death_retry_btn_need_diamond");
	}

	public void OnRetryFailed(Services.ServiceHandle _handle, string _error)
	{
		if (_handle != null)
		{
			if (_handle.result == Services.ConnectionResults.Timeout)
			{
				UIPopupController.Instance.HideLoadingPopup();
				UIPopupController.Instance.ShowTimeoutForceRetry(RetryMonetization);
			}
			else if (_handle.IsNoInternet())
			{
				UIPopupController.Instance.HideLoadingPopup();
				UIPopupController.Instance.ShowNoNetForceRetry(RetryMonetization);
			}
			else if (_handle.nReturnCode == 700)
			{
				UIPopupController.Instance.HideLoadingPopup();
				SingletonComponent<StoreManager>.Instance.ShowDiamondsShop(Retry, OnGoBackToReviveMain, "death_retry_btn_need_diamond");
			}
			else
			{
				UIPopupController.Instance.HideLoadingPopup();
				UIPopupController.Instance.ShowMediumPopupMSG("common_error", ServiceCallUtils.FormatMessage("common_prompt_nonet_retry", _handle), RetryMonetization, AbortBattle);
			}
		}
	}

	public void ResetRetryCounter()
	{
	}

	private void OnYesBtnPressed()
	{
		Popup popup = Popup.None;
		switch (m_popupType)
		{
		case Popup.Retry:
			Retry();
			break;
		case Popup.Exit:
			popup = Popup.Pause;
			ShowPopup(popup);
			break;
		case Popup.Pause:
			Pause(false);
			break;
		case Popup.Continue:
			SingletonComponent<BattleManager>.Instance.StartBattle();
			break;
		case Popup.Error:
			AbortBattle();
			break;
		default:
			Dbg.LogError("Invalid popup type: " + m_popupType);
			break;
		}
		m_popupType = popup;
	}

	private void OnNoBtnPressed()
	{
		if (!base.enabled)
		{
			return;
		}
		Popup popup = Popup.None;
		switch (m_popupType)
		{
		case Popup.Exit:
			AbortBattle();
			break;
		case Popup.Retry:
			if (!ShowOchaTipsBeforeGiveUp())
			{
				AbortBattle();
			}
			break;
		case Popup.Pause:
			popup = Popup.Exit;
			ShowPopup(popup);
			break;
		default:
			Dbg.LogError("Invalid popup type: " + m_popupType);
			break;
		}
		m_popupType = popup;
	}

	private bool ShowOchaTipsBeforeGiveUp()
	{
		if (!SingletonComponent<TutorialManager>.Instance)
		{
			return false;
		}
		TutorialDataRow randomRowFromParameterData = SingletonComponent<TutorialManager>.Instance.GetRandomRowFromParameterData(Tutorial.State.PlayerGiveup);
		if (randomRowFromParameterData == null)
		{
			return false;
		}
		string subtitleKey = randomRowFromParameterData.SubtitleKey;
		string p_text = subtitleKey;
		if ((bool)SingletonComponent<NBLocalization>.Instance)
		{
			p_text = SingletonComponent<NBLocalization>.Instance.Get(subtitleKey);
		}
		SingletonComponent<TutorialManager>.Instance.ShowSubtitle(p_text, true, randomRowFromParameterData.MascotAnim, randomRowFromParameterData.MascotFacial, randomRowFromParameterData.Sound, AbortBattle, true);
		return true;
	}

	private void OnGoToSNSRevive()
	{
		m_popupType = Popup.None;
		UIPopupController.Instance.ShowOnDeathRetryViaSNSPopup(SingletonComponent<NBLocalization>.Instance.Get("common_retry"), OnFBClick, OnGoBackToReviveMain);
	}

	private void OnGoBackToReviveMain()
	{
		ShowPopup(Popup.Retry);
	}

	private void OnFBClick()
	{
		SingletonComponent<SocialManager>.Instance.InviteFriend(null, OnFBLoginCallback, OnFBInviteFriendCallback);
	}

	private void OnFBLoginCallback(ServiceResponseCodes p_code)
	{
		Dbg.Log("OnFBLoginCallback. ServiceResponseCodes: " + p_code);
		switch (p_code)
		{
		case ServiceResponseCodes.SUCCESS:
			disableInputsAfterFB = true;
			UIPopupController.SetAllInputsEnabled(false);
			break;
		case ServiceResponseCodes.FAILED:
			UIPopupController.SetAllInputsEnabled(true);
			UIPopupController.Instance.EnableCurPopupButtons(true);
			break;
		}
	}

	private void OnFBInviteFriendCallback(ServiceResponseCodes p_code)
	{
		Dbg.Log("OnFBInviteFriendCallback. ServiceResponseCodes: " + p_code);
		UIPopupController.Instance.CloseCurrentPopup();
		UIPopupController.SetAllInputsEnabled(true);
		if (p_code == ServiceResponseCodes.SUCCESS)
		{
			SNSRetry();
			return;
		}
		UIPopupController.Instance.HideLoadingPopup();
		UIPopupController.Instance.ShowOKPopupMSG("common_error", "warcry_facebook_error", OnGoBackToReviveMain);
	}

	public void AbortBattle()
	{
		BattleManager instance = SingletonComponent<BattleManager>.Instance;
		if (null != instance && instance.state != BattleManager.State.Exit && instance.state != BattleManager.State.PostExit)
		{
			Time.timeScale = 1f;
			SingletonComponent<SoundManager>.Instance.StopMusic(true);
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_jingle_death_1");
			instance.ChangeState(BattleManager.State.Exit);
		}
	}

	private void OnEndDungeonSuccess(Services.ServiceHandle p_handle)
	{
		UIPopupController.Instance.HideLoadingPopup();
		SingletonComponent<LoadingTipsManager>.Instance.NextTipCategory = "GACHA_TIP";
		SingletonComponent<GameManager>.Instance.LoadScene("MainMenuInit", true, false);
	}

	private void OnEndDungeonFail(Services.ServiceHandle p_handle, string p_error)
	{
		UIPopupController.Instance.HideLoadingPopup();
		Dbg.LogError("OnEndDungeonFail code : " + ServiceCallUtils.GetErrorMessage(p_handle));
		if (p_handle == null)
		{
			UIPopupController.Instance.ShowServerGenericError();
		}
		else if (p_handle.IsNoInternet())
		{
			UIPopupController.Instance.ShowNoNetRetry(AbortBattle, GoBackToMainMenu);
		}
		else
		{
			UIPopupController.Instance.ShowMediumPopupMSG("common_error", ServiceCallUtils.FormatMessage("common_prompt_unableprogress", p_handle), AbortBattle, GoBackToMainMenu);
		}
	}

	private void Pause(bool p_flag)
	{
		CameraFx.PausePlayingEffects(p_flag);
		if (m_fxFeedback.gameObject.active)
		{
			m_fxFeedback.enabled = !p_flag;
		}
		if (m_textEffect != null && m_textEffect.gameObject.active)
		{
			m_textEffect.enabled = !p_flag;
		}
		if (m_playerStaminaBar != null)
		{
			m_playerStaminaBar.enabled = !p_flag;
		}
		if (p_flag)
		{
			if (m_focusRing.gameObject.active)
			{
				m_focusRing.Pause();
			}
			ShowPopup(Popup.Pause);
		}
		else if (m_focusRing.gameObject.active)
		{
			m_focusRing.Play();
		}
		Waiter.PauseAll(p_flag);
		Time.timeScale = ((!p_flag) ? 1f : 0f);
	}

	public void ShowPopup(Popup p_type)
	{
		if (!base.enabled)
		{
			return;
		}
		m_popupType = p_type;
		UIPopupController instance = UIPopupController.Instance;
		if (instance == null)
		{
			Dbg.LogError("Missing instance of UIPopupController");
			return;
		}
		switch (p_type)
		{
		case Popup.Retry:
			instance.ShowOnDeathRetryPopup(SingletonComponent<NBLocalization>.Instance.Get("common_retry"), OnYesBtnPressed, OnNoBtnPressed, OnGoToSNSRevive);
			break;
		case Popup.Pause:
		{
			bool p_isFirstTimeTutorial = false;
			TutorialManager instance2 = SingletonComponent<TutorialManager>.Instance;
			if (instance2 != null && instance2.enabled)
			{
				p_isFirstTimeTutorial = true;
			}
			instance.ShowBattlePausePopup("pause_setting_title", OnYesBtnPressed, OnNoBtnPressed, null, p_isFirstTimeTutorial);
			break;
		}
		case Popup.Exit:
			instance.ShowGenericPopupMSG("dungeon_give_up_popup_body", "dungeon_give_up_popup_continue", "dungeon_give_up_popup_give_up", OnYesBtnPressed, OnNoBtnPressed);
			break;
		case Popup.Continue:
			instance.ShowOKPopupMSG(string.Empty, "Continue Lah!", OnYesBtnPressed);
			break;
		case Popup.PreNextBattle:
			break;
		case Popup.Error:
			ShowErrorPopup("ERROR", "Error #37");
			break;
		}
	}

	public void ShowErrorPopup(string p_header, string p_message)
	{
		m_popupType = Popup.Error;
		UIPopupController.Instance.ShowOKPopupMSG(p_header, p_message, OnYesBtnPressed);
		Dbg.LogError(p_header + ": " + p_message);
	}

	public void ShowFocusRing(bool p_show)
	{
		ShowFocusRing(p_show, SingletonComponent<BattleManager>.Instance.BattleReferencePoint);
	}

	public void ShowFocusRing(bool p_show, Vector3 position)
	{
		ButtonScale[] componentsInChildren = MBHier.GetComponentsInChildren<ButtonScale>(base.gameObject);
		ButtonScale[] array = componentsInChildren;
		foreach (ButtonScale buttonScale in array)
		{
			buttonScale.Stop();
		}
		m_focusRing.gameObject.SetActiveRecursively(p_show);
		m_focusRing.transform.position = position;
		if (p_show)
		{
			m_focusRing.Play();
		}
	}

	public void ShowFeedbackFx(bool p_show, CameraFx.Callback p_cameraFinishCallback)
	{
		if (m_fxFeedback.gameObject.active != p_show)
		{
			Dbg.Log("ShowFeedbackFx: " + p_show);
			UnpressedButton();
			m_fxFeedback.gameObject.SetActiveRecursively(p_show);
		}
	}

	public BattleTextEffect ShowTextEffect(TextEffect p_text, float p_time = 0f)
	{
		if (p_text.CompareTo(TextEffect.None) <= 0 || p_text.CompareTo(TextEffect.Max) >= 0 || m_showingText.CompareTo(p_text) > 0)
		{
			return null;
		}
		string[] array = new string[17]
		{
			"text_hit", "text_critical", "text_combo", "text_combo_bonus", "text_dodge", "text_stun", "text_heal", "text_super2", "text_life_steal", "text_massive_damage",
			"text_pet_skill", "text_superdodge", "text_perfect_block", "text_block_failed", "text_dodge_failed", "text_damagebuff", "text_criticalbuff"
		};
		if (m_textEffect != null)
		{
			float p_displayTime = ((p_time != 0f) ? p_time : m_textDisplayTime);
			m_textEffect.Sprite.spriteName = array[(int)p_text];
			m_textEffect.Sprite.MakePixelPerfect();
			m_textEffect.Show(p_displayTime, m_textPopupTime, m_textFadeTime);
			m_showingText = p_text;
		}
		return m_textEffect;
	}

	public void ShowComboCounter(BattleTextEffect p_hitText, int p_combo, DamageType p_damageType)
	{
		if (m_player == null)
		{
			return;
		}
		string[] array = new string[10] { "Text_combo_00", "Text_combo_01", "Text_combo_02", "Text_combo_03", "Text_combo_04", "Text_combo_05", "Text_combo_06", "Text_combo_07", "Text_combo_08", "Text_combo_09" };
		bool flag = m_comboTexts == null || m_comboTexts.Length != 4 || m_comboTextColors == null || m_comboTextColors.Length != 3 || m_comboTextScales == null || m_comboTextScales.Length != 3;
		if (!flag)
		{
			for (int i = 0; i < m_comboTexts.Length; i++)
			{
				BattleTextEffect battleTextEffect = m_comboTexts[i];
				if (battleTextEffect == null)
				{
					flag = true;
					break;
				}
				battleTextEffect.Stop();
			}
		}
		if (flag)
		{
			return;
		}
		string text = null;
		Color white = Color.white;
		switch (p_damageType)
		{
		case DamageType.Normal:
			text = "ComboText_Normal";
			white = m_comboTextColors[0];
			break;
		case DamageType.Critical:
			text = "ComboText_Heavy";
			white = m_comboTextColors[2];
			break;
		default:
			text = "ComboText_Weak";
			white = m_comboTextColors[1];
			break;
		}
		if (p_combo > 1 && p_combo <= 99)
		{
			SetupComboSprite(m_comboTexts[0], string.Empty, white);
			int num = p_combo % 10;
			SetupComboSprite(m_comboTexts[1], array[num], white);
			if (p_combo >= 10)
			{
				num = p_combo / 10;
				SetupComboSprite(m_comboTexts[2], array[num], white);
			}
			else
			{
				m_comboTexts[2].gameObject.SetActiveRecursively(false);
			}
			if (m_player.FigureOfEight)
			{
				m_comboTexts[3].gameObject.SetActiveRecursively(true);
			}
			if (m_comboTextRoot.GetComponent<Animation>() != null)
			{
				m_comboTextRoot.GetComponent<Animation>().Rewind();
				m_comboTextRoot.GetComponent<Animation>().Play(text, AnimationPlayMode.Stop);
			}
		}
		else
		{
			HideComboCounter();
		}
	}

	private void SetupComboSprite(BattleTextEffect p_text, string p_spriteName, Color p_color)
	{
		p_text.Sprite.spriteName = ((!(p_spriteName != string.Empty)) ? p_text.Sprite.spriteName : p_spriteName);
		p_text.Sprite.color = p_color;
		p_text.gameObject.SetActiveRecursively(true);
	}

	public void HideComboCounter()
	{
		BattleTextEffect[] comboTexts = m_comboTexts;
		foreach (BattleTextEffect battleTextEffect in comboTexts)
		{
			if (battleTextEffect != null)
			{
				battleTextEffect.Stop();
			}
		}
	}

	public void ShowAttackResult(DamageType p_damageType)
	{
		if (!(m_attackResultText == null))
		{
			float comboRecognitionTime = m_player.ComboRecognitionTime;
			switch (p_damageType)
			{
			case DamageType.Critical:
				m_attackResultText.Sprite.spriteName = "text_great_hit";
				m_attackResultText.Show(comboRecognitionTime, m_textPopupTime, m_textFadeTime);
				break;
			default:
				m_attackResultText.Sprite.spriteName = "text_good_hit";
				m_attackResultText.Show(comboRecognitionTime, m_textPopupTime, m_textFadeTime);
				break;
			case DamageType.Normal:
				break;
			}
		}
	}

	public void ShowPowerMoveText(PowerMove.MoveEnum p_move)
	{
		string[] array = new string[3] { "eagle_claw", "phoenix_talon", "viper_strike" };
		if (m_textEffect != null && p_move != PowerMove.MoveEnum.None)
		{
			m_textEffect.Sprite.spriteName = array[(int)p_move];
			m_textEffect.Show(m_textDisplayTime, m_textPopupTime, m_textFadeTime);
		}
	}

	public void ShowAlertText(string p_sprite)
	{
		m_atkOrDefAlert.Sprite.spriteName = p_sprite;
		m_atkOrDefAlert.Show(m_textDisplayTime, m_textPopupTime, m_textFadeTime, 1f, m_alertTextAlpha);
		m_alertDelayCounter = 0f;
	}

	public void ShowPostBattleText(PostBattleText p_text)
	{
		if (p_text != PostBattleText.None)
		{
			if (m_showingPostBattleText != null)
			{
				m_showingPostBattleText.Stop();
				m_showingPostBattleText = null;
			}
			if (m_postBattleTexts[(int)p_text] != null)
			{
				m_postBattleTexts[(int)p_text].Show(m_postBattleTextDisplayTime, 0f, 0.5f);
				m_showingPostBattleText = m_postBattleTexts[(int)p_text];
			}
		}
	}

	public bool IsPostBattleTextShowing()
	{
		BattleTextEffect[] postBattleTexts = m_postBattleTexts;
		foreach (BattleTextEffect battleTextEffect in postBattleTexts)
		{
			if (battleTextEffect.gameObject.active)
			{
				return true;
			}
		}
		return false;
	}

	private void OnBlockEnable(bool p_enable)
	{
		m_blockEnable = p_enable;
		ShowBlockButton(p_enable);
	}

	private void OnDodgeEnable(bool p_enable)
	{
		m_dodgeEnable = p_enable;
		ShowDodgeButton(p_enable);
	}

	private void OnBattleStart()
	{
		m_blockEnable = true;
		m_dodgeEnable = true;
		ShowBattleUiButtons(true);
		HideComboCounter();
		if (m_attackResultText != null)
		{
			m_attackResultText.Stop();
		}
	}

	private void OnBattleEnd()
	{
		if (m_enemy.Data.Hp == 0f)
		{
			m_enemy = null;
		}
		m_blockEnable = false;
		m_dodgeEnable = false;
		ShowFeedbackFx(false, null);
		ShowBattleUi(false);
		if (m_player.Data.HpPercent == 1f)
		{
			ShowPostBattleText(PostBattleText.Perfect);
		}
		if (m_atkOrDefAlert != null)
		{
			m_atkOrDefAlert.Stop();
		}
	}

	private void OnCutsceneStart(CutsceneController.CutsceneType p_type)
	{
		ShowBattleUi(false);
		HideComboCounter();
	}

	private void OnCutsceneEnd(CutsceneController.CutsceneType p_type)
	{
		if (!CutsceneController.IsMobIntroCutscene(p_type) && !CutsceneController.IsMobDeathCutscene(p_type))
		{
			ShowBattleUi(true);
		}
	}

	private void OnTriggerFriendAssist(SkillEnum.AssistSkill p_skill)
	{
		TextEffect p_text = TextEffect.None;
		switch (p_skill)
		{
		case SkillEnum.AssistSkill.Heal:
			p_text = TextEffect.Heal;
			break;
		case SkillEnum.AssistSkill.Default:
			p_text = TextEffect.Hit;
			break;
		case SkillEnum.AssistSkill.MassiveDamage:
			p_text = TextEffect.MassiveDamage;
			break;
		case SkillEnum.AssistSkill.BuffDamage:
			p_text = TextEffect.DamageBuff;
			break;
		case SkillEnum.AssistSkill.BuffCritical:
			p_text = TextEffect.CriticalBuff;
			break;
		}
		ShowTextEffect(p_text, m_friendAssistTextDisplayTime);
	}

	private void OnPerfectDodgeStart(Player.DodgeDirection p_dir)
	{
		ShowTextEffect(TextEffect.PerfectDodge, 1f);
		ShowBattleUiButtons(false);
	}

	private void OnPerfectDodgeEnd()
	{
		ShowBattleUiButtons(true);
	}

	private void OnPlayerBeginAttack(PlayerAttack p_atk)
	{
		if (p_atk.type != PlayerAttack.Type.PowerMove)
		{
			m_powerMove = PowerMove.MoveEnum.None;
		}
	}

	private void OnPlayerGetHit(MonsterCollisionData p_collisionData)
	{
		HideComboCounter();
		if (m_attackResultText != null)
		{
			m_attackResultText.Stop();
		}
		TutorialManager instance = SingletonComponent<TutorialManager>.Instance;
		UIPopupController instance2 = UIPopupController.Instance;
		if (null != instance && instance.enabled && instance.showDodgeInstructions && null != p_collisionData && p_collisionData.attack != null && null != instance2)
		{
			MonsterAttack.Zone zone = p_collisionData.attack.zone;
			bool flag = zone == MonsterAttack.Zone.Left && m_lastPressedBtnEnum == Button.DodgeRight;
			bool flag2 = zone == MonsterAttack.Zone.Right && m_lastPressedBtnEnum == Button.DodgeLeft;
			if ((flag || flag2) && m_player.CurrentState == Player.State.Strafe)
			{
				Time.timeScale = 0f;
				instance2.ShowDodgeHintPopup(UIPopupController.DodgeHintType.Directions, OnDodgeHintClosed);
			}
			else if (zone == MonsterAttack.Zone.Center && m_player.CurrentState == Player.State.Block)
			{
				Time.timeScale = 0f;
				instance2.ShowDodgeHintPopup(UIPopupController.DodgeHintType.StraightAttack, OnDodgeHintClosed);
			}
		}
	}

	private void OnDodgeHintClosed()
	{
		Time.timeScale = 1f;
	}

	private void OnPerfectBlock()
	{
		ShowTextEffect(TextEffect.PerfectBlock, 1f);
	}

	private void OnBlockFailed()
	{
		ShowTextEffect(TextEffect.BlockFailed);
	}

	private void OnMonsterStun()
	{
		ShowTextEffect(TextEffect.Stun, 1f);
	}

	private void OnMonsterReceiveDamage(DamageData p_damage)
	{
		if (p_damage.damageType == DamageType.Special)
		{
			return;
		}
		Color white = Color.white;
		Color white2 = Color.white;
		white.a = 0f;
		white2.a = 0.3f;
		FlashScreen(white, white2, 0.25f);
		if (m_powerMove != PowerMove.MoveEnum.None)
		{
			ShowPowerMoveText(m_powerMove);
		}
		else
		{
			DamageType damageType = p_damage.damageType;
			if (damageType == DamageType.Critical)
			{
				ShowTextEffect(TextEffect.Critical);
			}
		}
		ShowComboCounter(null, m_player.ComboCount, p_damage.damageType);
		ShowAttackResult(p_damage.damageType);
	}

	private void OnPowerMove(PowerMove.MoveEnum p_move)
	{
		m_powerMove = p_move;
	}

	public void FlashScreen(Color p_fromColor, Color p_toColor, float p_duration)
	{
		if (m_screenFlashFx == null)
		{
			Dbg.LogError("Screen flashing sprite was not provided.");
			return;
		}
		m_flashSrc = p_fromColor;
		m_flashDst = p_toColor;
		m_flashDuration = p_duration;
		m_lerpStep = 2f / p_duration;
		m_lerp = 0f;
		m_lerpTime = 0f;
		m_screenFlashFx.color = m_flashSrc;
		m_screenFlashFx.gameObject.SetActiveRecursively(true);
	}

	private bool IsPauseButtonEnabled()
	{
		return m_pauseButton.GetComponent<Collider>() != null && m_pauseButton.GetComponent<Collider>().enabled;
	}

	private void LateUpdate()
	{
		if (m_popupType == Popup.None)
		{
			bool flag = MBInput.touchCount > 0 && MBInput.GetTouch(0).phase != TouchPhase.Ended;
			flag |= MBInput.GetMouseButton(0);
			if (m_pressingBtn != null && !flag)
			{
				UnpressedButton();
			}
			if (m_screenFlashFx.gameObject.active)
			{
				m_lerp += m_lerpStep * Time.deltaTime;
				m_lerpTime += Time.deltaTime;
				m_screenFlashFx.color = Color.Lerp(m_flashSrc, m_flashDst, m_lerp);
				if (m_lerp >= 1f)
				{
					if (m_lerpTime < m_flashDuration)
					{
						NBGeneric.Swap(ref m_flashSrc, ref m_flashDst);
						m_lerp = 0f;
					}
					else
					{
						m_screenFlashFx.gameObject.SetActiveRecursively(false);
					}
				}
			}
			if (m_player != null && m_playerStaminaBar != null)
			{
				float num = m_player.Stamina;
				if (num > 0f && num < 0.05f)
				{
					num = 0.05f;
				}
				m_playerStaminaBar.SliderValue = num;
			}
			if (m_enemy != null)
			{
				if (EnableAlertText)
				{
					UpdateAlertText();
				}
				else if (m_atkOrDefAlert.enabled)
				{
					m_atkOrDefAlert.Stop();
				}
			}
			if (m_textEffect != null && !m_textEffect.enabled && m_showingText != TextEffect.None)
			{
				m_showingText = TextEffect.None;
			}
		}
		else if (m_popupType != Popup.Pause && m_popupType != Popup.Exit)
		{
		}
	}

	public void ShowEnterEnemyUi(bool p_show)
	{
		if (m_EncounterEnemyUi != null)
		{
			m_EncounterEnemyUi.gameObject.SetActiveRecursively(p_show);
		}
	}

	public bool OnBackButtonPressed()
	{
		BattleCompleteScreen battleCompleteScreen = Object.FindObjectOfType(typeof(BattleCompleteScreen)) as BattleCompleteScreen;
		if (m_battleUiRoot == null || !m_battleUiRoot.active || UICamera.currentFilter != null || (null != battleCompleteScreen && battleCompleteScreen.gameObject.active))
		{
			return false;
		}
		if (m_popupType == Popup.None)
		{
			bool result = false;
			if (m_EncounterEnemyUi != null && m_EncounterEnemyUi.gameObject.active)
			{
				Pause(true);
				result = true;
			}
			else if (IsPauseButtonEnabled())
			{
				ButtonScale component = m_pauseButton.GetComponent<ButtonScale>();
				if (component == null || !component.enabled)
				{
					OnPausetBtnPressed(m_pauseButton.gameObject);
				}
				result = true;
			}
			return result;
		}
		if (m_popupType == Popup.Pause || m_popupType == Popup.Exit)
		{
			UIPopupController instance = UIPopupController.Instance;
			if (instance != null)
			{
				instance.SendMessage("OnYes", SendMessageOptions.DontRequireReceiver);
				return true;
			}
		}
		return false;
	}

	public bool UserPressedOnMonster()
	{
		if (m_EncounterEnemyUi != null)
		{
			return m_EncounterEnemyUi.UserPressedOnMonster;
		}
		return false;
	}

	public void ResetUserPressedOnMonsterFlag()
	{
		if (m_EncounterEnemyUi != null)
		{
			m_EncounterEnemyUi.ResetUserPressedOnMonsterFlag();
		}
	}

	public void SetMonsterName(string p_monsterName)
	{
		if (m_EncounterEnemyUi != null)
		{
			m_EncounterEnemyUi.SetMonsterName(p_monsterName);
		}
	}

	public void SetMonsterLevel(int p_level, int p_playerLevel, bool p_isBoss)
	{
		if (m_EncounterEnemyUi != null)
		{
			m_EncounterEnemyUi.SetMonsterLevel(p_level, p_playerLevel, p_isBoss);
		}
	}

	public void SetMonsterElement(Element.Type p_elementType)
	{
		if (m_EncounterEnemyUi != null)
		{
			m_EncounterEnemyUi.SetMonsterElement(p_elementType);
		}
	}

	public void DisableBattleHudCamera()
	{
		if (battleHudCamera != null)
		{
			battleHudCamera.enabled = false;
		}
	}

	public void EnableBattleHudCamera()
	{
		if (battleHudCamera != null)
		{
			battleHudCamera.enabled = true;
		}
	}

	public Transform GetBattleHudCameraTransform()
	{
		return battleHudCamera.transform;
	}

	public void EnableDodgeButton(int index, bool p_enable)
	{
		if (index < m_dodgeButtons.Length)
		{
			m_dodgeButtons[index].gameObject.SetActiveRecursively(p_enable);
		}
	}

	public void EnableBlockButton(bool p_enable)
	{
		m_blockButton.gameObject.SetActiveRecursively(p_enable);
	}

	public void EnableCinematicBar(bool p_enable, float fadeTime = 0.5f)
	{
		if (!(m_cinematicBar == null))
		{
			if (p_enable)
			{
				m_cinematicBar.FadeIn(fadeTime);
			}
			else
			{
				m_cinematicBar.FadeOut(fadeTime);
			}
		}
	}

	private void Update()
	{
		UIImageButton[] dodgeButtons = m_dodgeButtons;
		foreach (UIImageButton uIImageButton in dodgeButtons)
		{
			if (uIImageButton.transform.localScale != Vector3.one && (m_pressingBtn == null || m_pressingBtn != uIImageButton.gameObject))
			{
				ButtonScale component = uIImageButton.gameObject.GetComponent<ButtonScale>();
				if (component != null)
				{
					component.Stop();
				}
			}
		}
	}

	private void OnExitIfStuck()
	{
		if (UIPopupController.Instance != null)
		{
			tempTimeScale = Time.timeScale;
			Time.timeScale = 0f;
			UIPopupController.Instance.HidePopup();
			UIPopupController.Instance.ShowMediumPopupMSG("Exit", "Do you want to exit to main Menu?", GoBackToMainMenu, Unpause);
		}
	}

	private void UpdateAlertText()
	{
		if (!(m_player != null) || !(m_enemy != null) || !(m_player.Data.Hp > 0f) || !(m_enemy.Data.Hp > 0f))
		{
			return;
		}
		bool flag = m_enemy.CurrentState != Monster.State.Attack || (m_enemy.CurrentAttack != null && m_enemy.CurrentAttack.phase == MonsterAttack.Phase.WindDown && m_enemy.CurrentAttack.comboIndex == m_enemy.CurrentAttack.comboLength && m_player.CurrentState != Player.State.Hit);
		bool flag2 = m_enemy.CurrentState == Monster.State.Attack;
		if (!(m_atkOrDefAlert != null))
		{
			return;
		}
		m_alertDelayCounter += Time.deltaTime;
		if (!(m_alertDelayCounter >= m_alertDelay))
		{
			return;
		}
		if (flag)
		{
			if (!m_atkOrDefAlert.enabled || m_atkOrDefAlert.Sprite.spriteName != "text_attack_now")
			{
				ShowAlertText("text_attack_now");
			}
		}
		else if (flag2 && (!m_atkOrDefAlert.enabled || m_atkOrDefAlert.Sprite.spriteName != "text_defend_now"))
		{
			ShowAlertText("text_defend_now");
		}
	}

	private void GoBackToMainMenu()
	{
		Time.timeScale = 1f;
		SingletonComponent<GameManager>.Instance.LoadScene("MainMenuInit", true, false);
	}

	private void Unpause()
	{
		Time.timeScale = tempTimeScale;
	}
}
