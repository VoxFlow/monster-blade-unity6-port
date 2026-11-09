using System;
using System.Collections.Generic;
using UnityEngine;

public class BattleCompleteScreen : MonoBehaviour
{
	private enum BattleClearState
	{
		SHOW_CLEAR_TEXT = 0,
		SHOW_TREASURE_GOT = 1,
		SHOW_MAIN_SCREEN = 2,
		SHOW_ALL = 3,
		SHOW_CONFIRM = 4,
		SHOW_FINAL_LOOT = 5
	}

	private enum BattleClearAnimState
	{
		IDLE = -1,
		GOLD = 0,
		EXP = 1,
		TREASURE = 2,
		DATA = 3,
		MEDAL = 4,
		MYSTERY_PROGRESS = 5
	}

	public float treasureShowTimeFrameInAnimation = 2.8f;

	public float waitTreasureTime = 1f;

	[SerializeField]
	private GameObject m_clearScreen;

	[SerializeField]
	private GameObject m_treasureScreen;

	[SerializeField]
	private GameObject m_mainScreen;

	[SerializeField]
	private Animation m_titleAnim;

	[SerializeField]
	private UISprite m_rank;

	[SerializeField]
	private UILabel m_scoreValue;

	private int m_targetScoreValue;

	[SerializeField]
	private UILabel m_goldValue;

	private int m_targetGoldValue;

	[SerializeField]
	private UILabel m_expValue;

	private int m_targetExpValue;

	private int m_targetTreasureValue;

	private int m_targetDamageDealt;

	private int m_targetDamageRecieved;

	private int m_targetTimeTaken;

	private int m_targetSpecialMoveScore;

	private float m_targetProgress;

	private int m_totalBattles;

	[SerializeField]
	private UILabel m_treasureValue;

	[SerializeField]
	private UILabel m_damageOnMonsterValue;

	[SerializeField]
	private UILabel m_damageOnPlayerValue;

	[SerializeField]
	private UILabel m_timeTakenValue;

	[SerializeField]
	private UILabel m_specialMoveValue;

	[SerializeField]
	private UILabel m_finalScoreValue;

	[SerializeField]
	private UISlider m_finalScoreProgres;

	[SerializeField]
	public GameObject m_continueBtn;

	[SerializeField]
	public GameObject m_leaveBtn;

	[SerializeField]
	private UISlider m_itemProgressBar;

	[SerializeField]
	private UISprite m_battleNum;

	[SerializeField]
	private UISprite[] m_battleNum1st;

	[SerializeField]
	private UISprite m_medalIcon;

	[SerializeField]
	private GameObject m_medalGroup;

	[SerializeField]
	private BattleRankChestIcons m_battleRankChestIconTemp;

	[SerializeField]
	private GameObject m_treasure;

	[SerializeField]
	private GameObject m_clear_header;

	[SerializeField]
	private GameObject m_titleBattle;

	[SerializeField]
	private GameObject m_titleClear;

	[SerializeField]
	private GameObject m_group;

	[SerializeField]
	private ParticleSystem[] m_particles;

	private bool m_gotMysteryTreasure;

	private ItemEnum.Chest m_treasureGot;

	[SerializeField]
	private UILabel m_labelExtra;

	[SerializeField]
	private UILabel m_labelChestWarning;

	[SerializeField]
	private UISprite m_mysteryTreasureChest;

	[SerializeField]
	private GameObject m_tapToSkip;

	[SerializeField]
	private GameObject m_confirmExitPopup;

	public GameObject m_confirmExitPopupGO;

	private bool canUpdate;

	private BattleClearState m_curState;

	private bool m_showMedal;

	private int[] m_medalList;

	private string[] m_chestSprites = new string[5] { "Chest_Lv1", "Chest_Lv2", "Chest_Lv3", "Chest_Lv4", "Chest_Lv5" };

	private float tempGoldValue;

	private float tempScoreValue;

	private float tempExpValue;

	private float treasureCount;

	private float tempDamageDealt;

	private float tempDamageReceived;

	private float tempTimeTaken;

	private float tempSpecialMove;

	private float tempProgress;

	private float magicNumber = 1.5f;

	public float m_clearBattleDelay;

	public float m_treasureBattleDelay;

	private BattleClearAnimState m_animSequence = BattleClearAnimState.IDLE;

	private bool m_touchDown;

	private bool isAnimateBattleClearIntroAnimating;

	private List<GameObject> m_treasureList;

	private float timerForWaitTreasureAnim;

	public string[] m_historyRanks;

	private float progressPrior;

	private bool clickedToTreasureShow;

	private bool treasureShown;

	private void OnEnable()
	{
		Dbg.Log("BattleCompleteScreen, OnEnable, time : " + DateTime.Now.ToString("HH:mm:ss.ffffff"));
		Dbg.Assert(m_clearScreen);
		Dbg.Assert(m_treasureScreen);
		Dbg.Assert(m_mainScreen);
		Dbg.Assert(m_titleAnim);
		Dbg.Assert(m_rank);
		Dbg.Assert(m_scoreValue);
		Dbg.Assert(m_goldValue);
		Dbg.Assert(m_expValue);
		Dbg.Assert(m_treasureValue);
		Dbg.Assert(m_damageOnMonsterValue);
		Dbg.Assert(m_damageOnPlayerValue);
		Dbg.Assert(m_timeTakenValue);
		Dbg.Assert(m_specialMoveValue);
		Dbg.Assert(m_finalScoreValue);
		Dbg.Assert(m_finalScoreProgres);
		Dbg.Assert(m_continueBtn);
		Dbg.Assert(m_leaveBtn);
		Dbg.Assert(m_itemProgressBar);
		Dbg.Assert(m_battleNum);
		UISprite[] battleNum1st = m_battleNum1st;
		foreach (UISprite uISprite in battleNum1st)
		{
			Dbg.Assert(uISprite);
		}
		Dbg.Assert(m_medalIcon);
		Dbg.Assert(m_medalGroup);
		Dbg.Assert(m_battleRankChestIconTemp);
		Dbg.Assert(m_clear_header);
		Dbg.Assert(m_treasure);
		Dbg.Assert(m_titleClear);
		Dbg.Assert(m_group);
		ParticleSystem[] particles = m_particles;
		foreach (ParticleSystem particleSystem in particles)
		{
			Dbg.Assert(particleSystem);
		}
		Dbg.Assert(m_labelExtra);
		if (SingletonComponent<TutorialManager>.Instance != null && !SingletonComponent<TutorialManager>.Instance.IsCompleted(Tutorial.State.All))
		{
			if (m_mysteryTreasureChest != null)
			{
				m_mysteryTreasureChest.enabled = false;
				m_mysteryTreasureChest.gameObject.SetActiveRecursively(false);
			}
			m_gotMysteryTreasure = false;
		}
	}

	private void OnDestroy()
	{
		UIPopupController.Instance.m_SetActiveRecursively = true;
	}

	public void EnableBackground(bool p_enable)
	{
	}

	public void ShowBackground()
	{
	}

	private void Start()
	{
	}

	private void AnimateBattleClearIntro()
	{
		Dbg.Log("AnimateBattleClearIntro, time : " + DateTime.Now.ToString("HH:mm:ss.ffffff"));
		m_clearScreen.gameObject.SetActiveRecursively(true);
		isAnimateBattleClearIntroAnimating = true;
	}

	private void Play1stParticle()
	{
		if (m_curState == BattleClearState.SHOW_MAIN_SCREEN && m_particles.Length > 0)
		{
			m_particles[0].Play();
		}
	}

	private void Play2ndParticle()
	{
		if (m_curState == BattleClearState.SHOW_MAIN_SCREEN && m_particles.Length > 1)
		{
			m_particles[1].Play();
		}
	}

	private void Play3rdParticle()
	{
		if (m_curState == BattleClearState.SHOW_MAIN_SCREEN && m_particles.Length > 2)
		{
			m_particles[2].Play();
		}
	}

	private void DisableFirstScreen()
	{
		if (m_curState == BattleClearState.SHOW_CLEAR_TEXT)
		{
			m_clearScreen.gameObject.SetActiveRecursively(false);
			isAnimateBattleClearIntroAnimating = false;
			ResetAnimation();
			if (m_targetTreasureValue > 0)
			{
				m_curState = BattleClearState.SHOW_TREASURE_GOT;
				SetChestSprite();
				m_treasureScreen.gameObject.SetActiveRecursively(true);
				m_labelExtra.gameObject.SetActiveRecursively(false);
			}
			else
			{
				m_curState = BattleClearState.SHOW_MAIN_SCREEN;
				ShowMainSequence();
			}
		}
	}

	private void DisableSecondScreen()
	{
		if (m_curState == BattleClearState.SHOW_TREASURE_GOT)
		{
			m_treasureScreen.gameObject.SetActiveRecursively(false);
			ResetAnimation();
			m_curState = BattleClearState.SHOW_MAIN_SCREEN;
			ShowMainSequence();
		}
		else if (m_curState == BattleClearState.SHOW_FINAL_LOOT)
		{
			DoContinueLogic();
		}
	}

	private void FadeOutMainScreen(bool p_showMysteryChest = false)
	{
		ChangeNGUIAlpha[] componentsInChildren = MBHier.GetComponentsInChildren<ChangeNGUIAlpha>(m_mainScreen);
		ChangeNGUIAlpha[] array = componentsInChildren;
		foreach (ChangeNGUIAlpha changeNGUIAlpha in array)
		{
			changeNGUIAlpha.enabled = false;
		}
		TweenFade[] componentsInChildren2 = MBHier.GetComponentsInChildren<TweenFade>(m_mainScreen);
		TweenFade[] array2 = componentsInChildren2;
		foreach (TweenFade tweenFade in array2)
		{
			if (p_showMysteryChest)
			{
				tweenFade.OnExit(StartShowMysteryChest);
			}
			else
			{
				tweenFade.OnExit(OnFadeoutMainScreen);
			}
		}
		if (m_continueBtn != null && m_continueBtn.GetComponent<Collider>() != null)
		{
			m_continueBtn.GetComponent<Collider>().enabled = false;
		}
		if (m_leaveBtn != null && m_leaveBtn.GetComponent<Collider>() != null)
		{
			m_leaveBtn.GetComponent<Collider>().enabled = false;
		}
	}

	private void FadeInMainScreen()
	{
		TweenFade[] componentsInChildren = MBHier.GetComponentsInChildren<TweenFade>(m_mainScreen, true);
		TweenFade[] array = componentsInChildren;
		foreach (TweenFade tweenFade in array)
		{
			tweenFade.OnEnter(OnFadeInMainScreen);
		}
	}

	public void OnFadeInMainScreen()
	{
		if (m_continueBtn != null && m_continueBtn.GetComponent<Collider>() != null)
		{
			m_continueBtn.GetComponent<Collider>().enabled = true;
		}
		if (m_leaveBtn != null && m_leaveBtn.GetComponent<Collider>() != null)
		{
			m_leaveBtn.GetComponent<Collider>().enabled = true;
		}
	}

	public void OnFadeoutMainScreen()
	{
		m_mainScreen.gameObject.SetActiveRecursively(false);
		m_medalGroup.gameObject.SetActiveRecursively(false);
		m_tapToSkip.gameObject.SetActiveRecursively(false);
		GameObject gameObject = UnityEngine.Object.Instantiate(m_confirmExitPopup, Vector3.zero, Quaternion.identity) as GameObject;
		if (gameObject == null)
		{
			Dbg.LogError("ERRAWR: failed to instantiate the Confirm on Exit Popup, check if this was set on inspector");
			return;
		}
		gameObject.transform.parent = base.gameObject.transform;
		gameObject.transform.localPosition = Vector3.zero;
		Transform transform = gameObject.transform.Find("DESC_TXT");
		if (transform != null)
		{
			NBUILocalize component = transform.GetComponent<NBUILocalize>();
			if (component != null)
			{
				component.Key = "dungeon_give_up_popup_body";
			}
		}
		Transform transform2 = gameObject.transform.Find("BTN_YES");
		if (transform2 != null)
		{
			NBUILocalize componentInChildren = MBHier.GetComponentInChildren<NBUILocalize>(transform2);
			if (componentInChildren != null)
			{
				componentInChildren.Key = "dungeon_give_up_popup_continue";
			}
			UIButtonMessage component2 = transform2.GetComponent<UIButtonMessage>();
			if (component2 != null)
			{
				component2.target = base.gameObject;
				component2.functionName = "OnYesBtnPressed";
			}
		}
		Transform transform3 = gameObject.transform.Find("BTN_NO");
		if (transform3 != null)
		{
			NBUILocalize componentInChildren2 = MBHier.GetComponentInChildren<NBUILocalize>(transform3);
			if (componentInChildren2 != null)
			{
				componentInChildren2.Key = "dungeon_give_up_popup_give_up";
			}
			UIButtonMessage component3 = transform3.GetComponent<UIButtonMessage>();
			if (component3 != null)
			{
				component3.target = base.gameObject;
				component3.functionName = "OnNoBtnPressed";
			}
		}
		TweenFade tweenFade = gameObject.AddComponent<TweenFade>();
		if (tweenFade != null)
		{
			tweenFade.OnEnter();
		}
		m_confirmExitPopupGO = gameObject;
		m_curState = BattleClearState.SHOW_CONFIRM;
	}

	public void StartShowMysteryChest()
	{
		Dbg.Log("StartShowMysteryChest, time : " + DateTime.Now.ToString("HH:mm:ss.ffffff"));
		m_treasure.GetComponent<UISprite>().spriteName = "chest";
		m_curState = BattleClearState.SHOW_FINAL_LOOT;
		m_mainScreen.gameObject.SetActiveRecursively(false);
		m_medalGroup.gameObject.SetActiveRecursively(false);
		ResetAnimation();
		m_treasureScreen.SetActiveRecursively(true);
	}

	private void Phase2Intro()
	{
		float num = 10f;
		float x = m_treasure.transform.localScale.x;
		float num2 = (x * (float)m_targetTreasureValue + (float)(m_targetTreasureValue - 1) * num) / -2f + x / 2f;
		m_treasure.transform.localPosition = new Vector3(num2, m_treasure.transform.localPosition.y, m_treasure.transform.localPosition.z);
		if (m_treasureList != null)
		{
			m_treasureList.Clear();
		}
		m_treasureList = new List<GameObject>();
		string spriteName = m_chestSprites[0];
		if (m_treasureGot >= ItemEnum.Chest.LOWEST && (int)m_treasureGot < m_chestSprites.Length)
		{
			spriteName = m_chestSprites[(int)m_treasureGot];
		}
		else
		{
			Dbg.LogWarn("Invalid treasure enum " + m_treasureGot);
		}
		m_treasure.GetComponent<UISprite>().spriteName = spriteName;
		m_treasureList.Add(m_treasure);
		float num3 = 0.4f;
		float num4 = num3;
		if (m_targetTreasureValue > 1)
		{
			for (int i = 1; i < m_targetTreasureValue; i++)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(m_treasure) as GameObject;
				gameObject.transform.parent = m_treasureScreen.transform;
				gameObject.transform.localPosition = new Vector3(num2 + num * (float)i + x * (float)i, gameObject.transform.localPosition.y, gameObject.transform.localPosition.z);
				m_treasureList.Add(gameObject);
			}
		}
		foreach (GameObject treasure in m_treasureList)
		{
			iTween.ScaleFrom(treasure.gameObject, iTween.Hash("x", 1f, "y", 1f, "time", num3, "delay", num4));
			num4 += num3 + 0.1f;
		}
	}

	private void SetChestSprite()
	{
		string empty = string.Empty;
		empty = "Chest_Lv" + (int)(m_treasureGot + 1);
		m_treasure.GetComponent<UISprite>().spriteName = empty;
	}

	private void ResetAnimation()
	{
		if ((bool)base.GetComponent<Animation>())
		{
			base.GetComponent<Animation>().Rewind();
			base.GetComponent<Animation>().Play();
			base.GetComponent<Animation>().Sample();
			base.GetComponent<Animation>().Stop();
			base.GetComponent<Animation>().Play();
		}
	}

	private void OnTreasureShow()
	{
		if (m_curState == BattleClearState.SHOW_TREASURE_GOT || m_curState == BattleClearState.SHOW_FINAL_LOOT)
		{
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_loot_received");
		}
	}

	private void AnimateGoldFound()
	{
		if (m_curState == BattleClearState.SHOW_MAIN_SCREEN)
		{
			m_animSequence = BattleClearAnimState.GOLD;
			PauseAnimation();
		}
	}

	private void AnimateXPGained()
	{
		if (m_curState == BattleClearState.SHOW_MAIN_SCREEN)
		{
			m_animSequence = BattleClearAnimState.EXP;
			PauseAnimation();
		}
	}

	private void AnimateTreasureFound()
	{
		if (m_curState == BattleClearState.SHOW_MAIN_SCREEN)
		{
			m_animSequence = BattleClearAnimState.TREASURE;
			PauseAnimation();
		}
	}

	private void AnimateFightData()
	{
		if (m_curState == BattleClearState.SHOW_MAIN_SCREEN)
		{
			m_animSequence = BattleClearAnimState.DATA;
			PauseAnimation();
		}
	}

	private void AnimateMysteryTreasure()
	{
		if (m_curState == BattleClearState.SHOW_MAIN_SCREEN)
		{
			m_animSequence = BattleClearAnimState.MYSTERY_PROGRESS;
			PauseAnimation();
		}
	}

	private void AnimateMedal()
	{
		if (m_curState == BattleClearState.SHOW_MAIN_SCREEN && m_showMedal)
		{
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_notification");
		}
	}

	private void PauseAnimation()
	{
		foreach (AnimationState item in base.GetComponent<Animation>())
		{
			item.speed = 0f;
		}
		if (m_curState == BattleClearState.SHOW_MAIN_SCREEN)
		{
			ParticleSystem[] componentsInChildren = MBHier.GetComponentsInChildren<ParticleSystem>(m_medalGroup);
			ParticleSystem[] array = componentsInChildren;
			foreach (ParticleSystem particleSystem in array)
			{
				particleSystem.Pause();
			}
		}
	}

	private void ResumeAnimation()
	{
		foreach (AnimationState item in base.GetComponent<Animation>())
		{
			item.speed = 1f;
		}
		if (m_curState == BattleClearState.SHOW_MAIN_SCREEN)
		{
			ParticleSystem[] componentsInChildren = MBHier.GetComponentsInChildren<ParticleSystem>(m_medalGroup);
			ParticleSystem[] array = componentsInChildren;
			foreach (ParticleSystem particleSystem in array)
			{
				particleSystem.Play();
			}
		}
	}

	private void ShowMainSequence()
	{
		if (m_curState == BattleClearState.SHOW_ALL || m_curState == BattleClearState.SHOW_MAIN_SCREEN)
		{
			if (m_showMedal)
			{
				m_medalGroup.gameObject.SetActiveRecursively(true);
			}
			m_mainScreen.gameObject.SetActiveRecursively(true);
		}
	}

	private void Update()
	{
		if (!canUpdate)
		{
			return;
		}
		if (MBInput.anyKey || MBInput.anyKeyDown)
		{
			if (m_touchDown)
			{
				return;
			}
			m_touchDown = true;
			if (SingletonComponent<TutorialManager>.Instance != null && !SingletonComponent<TutorialManager>.Instance.IsCompleted(Tutorial.State.All))
			{
				if (m_mysteryTreasureChest != null)
				{
					m_mysteryTreasureChest.enabled = false;
					m_mysteryTreasureChest.gameObject.SetActiveRecursively(false);
				}
				m_gotMysteryTreasure = false;
			}
		}
		else
		{
			m_touchDown = false;
		}
		if (m_touchDown)
		{
		}
		if (m_curState == BattleClearState.SHOW_CLEAR_TEXT)
		{
			if (m_touchDown)
			{
				m_touchDown = false;
				DisableFirstScreen();
			}
			else if (!isAnimateBattleClearIntroAnimating)
			{
				AnimateBattleClearIntro();
			}
			return;
		}
		if (m_curState == BattleClearState.SHOW_TREASURE_GOT)
		{
			m_labelExtra.gameObject.SetActiveRecursively(false);
			timerForWaitTreasureAnim += Time.deltaTime;
			if (timerForWaitTreasureAnim > waitTreasureTime)
			{
				ResumeAnimation();
			}
		}
		switch (m_animSequence)
		{
		case BattleClearAnimState.GOLD:
			if (tempGoldValue < (float)m_targetGoldValue && !m_touchDown)
			{
				tempGoldValue += (float)m_targetGoldValue / magicNumber * Time.deltaTime;
				m_goldValue.text = ((int)tempGoldValue).ToString();
				SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_score_counting");
			}
			else
			{
				m_goldValue.text = m_targetGoldValue.ToString();
				m_animSequence = BattleClearAnimState.IDLE;
				ResumeAnimation();
			}
			break;
		case BattleClearAnimState.EXP:
			if (tempExpValue < (float)m_targetExpValue && !m_touchDown)
			{
				tempExpValue += (float)m_targetExpValue / magicNumber * Time.deltaTime;
				m_expValue.text = ((int)tempExpValue).ToString();
				SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_score_counting");
			}
			else
			{
				m_expValue.text = m_targetExpValue.ToString();
				m_animSequence = BattleClearAnimState.IDLE;
				ResumeAnimation();
			}
			break;
		case BattleClearAnimState.TREASURE:
			if (treasureCount < (float)m_targetTreasureValue && !m_touchDown)
			{
				treasureCount += 1f;
				m_treasureValue.text = ((int)treasureCount).ToString();
				SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_score_counting");
			}
			else
			{
				m_treasureValue.text = m_targetTreasureValue.ToString();
				m_animSequence = BattleClearAnimState.IDLE;
				ResumeAnimation();
			}
			break;
		case BattleClearAnimState.DATA:
			if ((tempDamageDealt < (float)m_targetDamageDealt || tempDamageReceived < (float)m_targetDamageRecieved || tempTimeTaken < (float)m_targetTimeTaken || tempSpecialMove < (float)m_targetSpecialMoveScore) && !m_touchDown)
			{
				tempDamageDealt += (float)m_targetDamageDealt / magicNumber * Time.deltaTime;
				if (tempDamageDealt >= (float)m_targetDamageDealt)
				{
					tempDamageDealt = m_targetDamageDealt;
				}
				m_damageOnMonsterValue.text = ((int)tempDamageDealt).ToString();
				tempDamageReceived += (float)m_targetDamageRecieved / magicNumber * Time.deltaTime;
				if (tempDamageReceived >= (float)m_targetDamageRecieved)
				{
					tempDamageReceived = m_targetDamageRecieved;
				}
				m_damageOnPlayerValue.text = ((int)tempDamageReceived).ToString();
				tempTimeTaken += (float)m_targetTimeTaken / magicNumber * Time.deltaTime;
				if (tempTimeTaken >= (float)m_targetTimeTaken)
				{
					tempTimeTaken = m_targetTimeTaken;
				}
				m_timeTakenValue.text = FormatTime((int)tempTimeTaken);
				tempSpecialMove += (float)m_targetSpecialMoveScore / magicNumber * Time.deltaTime;
				if (tempSpecialMove >= (float)m_targetSpecialMoveScore)
				{
					tempSpecialMove = m_targetSpecialMoveScore;
				}
				m_specialMoveValue.text = ((int)tempSpecialMove).ToString();
				SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_score_counting");
			}
			else
			{
				m_damageOnMonsterValue.text = m_targetDamageDealt.ToString();
				m_damageOnPlayerValue.text = m_targetDamageRecieved.ToString();
				m_timeTakenValue.text = FormatTime(m_targetTimeTaken);
				m_specialMoveValue.text = m_targetSpecialMoveScore.ToString();
				m_animSequence = BattleClearAnimState.IDLE;
				ResumeAnimation();
			}
			break;
		case BattleClearAnimState.MEDAL:
			break;
		case BattleClearAnimState.MYSTERY_PROGRESS:
			if ((tempScoreValue < (float)m_targetScoreValue || tempProgress < m_targetProgress) && !m_touchDown)
			{
				tempScoreValue += (float)m_targetScoreValue / magicNumber * Time.deltaTime;
				if (tempScoreValue > (float)m_targetScoreValue)
				{
					tempScoreValue = m_targetScoreValue;
				}
				m_scoreValue.text = ((int)tempScoreValue).ToString();
				tempProgress += (m_targetProgress - progressPrior) / magicNumber * Time.deltaTime;
				if (tempProgress > m_targetProgress)
				{
					tempProgress = m_targetProgress;
				}
				m_itemProgressBar.sliderValue = tempProgress;
				SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_score_counting");
			}
			else
			{
				m_animSequence = BattleClearAnimState.IDLE;
				m_scoreValue.text = m_targetScoreValue.ToString();
				m_itemProgressBar.sliderValue = m_targetProgress;
				m_particles[3].Play();
				ResumeAnimation();
			}
			break;
		}
	}

	public void PrepareContent()
	{
		clickedToTreasureShow = false;
		treasureShown = false;
		m_touchDown = false;
		isAnimateBattleClearIntroAnimating = false;
		m_goldValue.text = "0";
		m_expValue.text = "0";
		m_scoreValue.text = "0";
		m_itemProgressBar.sliderValue = SingletonComponent<BattleManager>.Instance.progressBarProgressTargetValue;
		m_curState = BattleClearState.SHOW_CLEAR_TEXT;
		isAnimateBattleClearIntroAnimating = false;
	}

	public void EnableContent(bool p_enable)
	{
		canUpdate = p_enable;
		if (m_curState == BattleClearState.SHOW_CLEAR_TEXT)
		{
			p_enable = false;
		}
		if (m_curState == BattleClearState.SHOW_TREASURE_GOT)
		{
		}
		if (m_curState == BattleClearState.SHOW_MAIN_SCREEN)
		{
		}
		m_treasureScreen.gameObject.SetActiveRecursively(p_enable);
		m_mainScreen.gameObject.SetActiveRecursively(p_enable);
		m_medalGroup.gameObject.SetActiveRecursively(false);
		m_itemProgressBar.gameObject.SetActiveRecursively(p_enable);
		m_battleNum.gameObject.SetActiveRecursively(p_enable);
		m_continueBtn.SetActiveRecursively(false);
		m_leaveBtn.SetActiveRecursively(false);
		if (m_tapToSkip != null)
		{
			m_tapToSkip.SetActiveRecursively(true);
		}
	}

	private void OnReplayBtnClick()
	{
		Services instance = SingletonComponent<Services>.Instance;
		if (!(instance == null))
		{
			string dungeonKey = SingletonComponent<DungeonManager>.Instance.DungeonKey;
			int num = SingletonComponent<DungeonManager>.Instance.GetCurrentBattleCounter() - 1;
			if (num < 0)
			{
				num = 0;
			}
			string text = MBPrefs.GetString("Ms:Rankings", string.Empty);
			string[] array = text.Split('|');
			text = string.Empty;
			for (int i = 0; i < array.Length - 2; i++)
			{
				text = text + array[i] + "|";
			}
			MBPrefs.SetString("Ms:Rankings", text);
			UIPopupController.Instance.ShowLoadingPopupNetWait();
			uint num2 = SingletonComponent<BattleScreenUI>.Instance.GenerateNewRetryRequestID();
			instance.RetryBattle(instance.NubeeId, instance.DeviceId, dungeonKey, num, 0, num2.ToString(), SingletonComponent<BattleScreenUI>.Instance.OnRetrySucceeded, SingletonComponent<BattleScreenUI>.Instance.OnRetryFailed);
			SingletonComponent<BattleScreenUI>.Instance.ResetRetryCounter();
			UIPopupController.Instance.RemoveCurrentPopupFromStack();
			UIPopupController.Instance.HideCurrentPopup();
			UIPopupController.Instance.FadeOutBlackOverlay();
		}
	}

	private void OnLeaveDungeonBtnClick()
	{
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_cancel");
		FadeOutMainScreen();
	}

	private void OnYesBtnPressed()
	{
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_ok");
		if (m_confirmExitPopupGO != null)
		{
			TweenFade component = m_confirmExitPopupGO.GetComponent<TweenFade>();
			if (component != null)
			{
				component.OnExit(OnConfirmExitFadeOut);
				return;
			}
			UnityEngine.Object.Destroy(m_confirmExitPopupGO);
			m_confirmExitPopupGO = null;
			FadeInMainScreen();
		}
		else
		{
			SkipAllAnimationShowAll();
			FadeInMainScreen();
		}
	}

	private void OnConfirmExitFadeOut()
	{
		if (m_confirmExitPopupGO != null)
		{
			UnityEngine.Object.Destroy(m_confirmExitPopupGO);
			m_confirmExitPopupGO = null;
		}
		SkipAllAnimationShowAll();
		FadeInMainScreen();
	}

	private void OnNoBtnPressed()
	{
		MBPrefs.SetFloat("Ms:PlayerRetryHp", 0f);
		UIPopupController.Instance.HideBattleClearScreen();
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_cancel");
		SingletonComponent<BattleScreenUI>.Instance.AbortBattle();
	}

	private void OnContinueBtnClick()
	{
		if (m_gotMysteryTreasure)
		{
			FadeOutMainScreen(true);
		}
		else
		{
			DoContinueLogic();
		}
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_ok");
	}

	private void DoContinueLogic()
	{
		Messenger<int>.Invoke("battle_continue", SingletonComponent<DungeonManager>.Instance.GetCurrentBattleCounter());
		MBPrefs.SetFloat("Ms:PlayerRetryHp", 0f);
		if (SingletonComponent<DungeonManager>.Instance.IsLastBattle())
		{
			SingletonComponent<BattleManager>.Instance.EndLastBattle();
		}
		else
		{
			SingletonComponent<DungeonManager>.Instance.IncreaseBattleCounter();
			SingletonComponent<BattleManager>.Instance.StartBattle();
		}
		UIPopupController.Instance.HideBattleClearScreen();
	}

	public void SetEndBattleData(EndBattleData p_inData)
	{
		SetRank(p_inData.rank);
		EnableRankPic(false);
		SetGold(p_inData.igc);
		SetExp(p_inData.exp);
		SetBattleNum(SingletonComponent<DungeonManager>.Instance.GetCurrentBattleCounter() + 1);
		m_totalBattles = p_inData.totalNumBattle;
		if (p_inData.scorePerc >= GLOBAL.GAME_SETTINGS.medal_criteria_gold)
		{
			AddProgress(1f / (float)p_inData.totalNumBattle);
		}
		else
		{
			AddProgress(p_inData.scorePerc / (float)p_inData.totalNumBattle);
		}
		Dbg.Log(string.Format("PRENEXTBATTLE: Battles:{0} Player Level:{1} Monster Level:{2}", p_inData.totalNumBattle, p_inData.playerLevel, p_inData.monsterLevel));
		string text = string.Empty;
		NBLocalization instance = SingletonComponent<NBLocalization>.Instance;
		if (instance == null)
		{
			Dbg.LogError("NBLocalization instance is missing");
			return;
		}
		string text2 = instance.Get("prenextbattle_unlock_treasure");
		if (text2.Contains("{0"))
		{
			text = string.Format(text2, p_inData.totalNumBattle);
		}
		string text3 = instance.Get("prenextbattle_level_range_label");
		if (text3.Contains("{0") && text3.Contains("{1"))
		{
			text = text + "\n" + string.Format(text3, (p_inData.monsterLevel - 5 < 1) ? 1 : (p_inData.monsterLevel - 5), p_inData.monsterLevel + 5);
		}
		string text4 = instance.Get("prenextbattle_current_level_label");
		PlayerDataManager instance2 = SingletonComponent<PlayerDataManager>.Instance;
		PlayerData playerData = null;
		if (null != instance2)
		{
			playerData = instance2.GetPlayerData();
		}
		if (text4.Contains("{0}"))
		{
			if (p_inData.monsterLevel - 5 <= p_inData.playerLevel && p_inData.playerLevel <= p_inData.monsterLevel + 5)
			{
				text = text + "\n" + string.Format(text4, "[00FF00]" + p_inData.playerLevel + "[-]");
				if (playerData != null)
				{
					playerData.m_isInLevelRangeForCurrentDungeon = true;
				}
			}
			else
			{
				text = text + "\n" + string.Format(text4, "[FF0000]" + p_inData.playerLevel + "[-]");
				if (playerData != null)
				{
					playerData.m_isInLevelRangeForCurrentDungeon = false;
				}
			}
		}
		if (m_labelChestWarning != null)
		{
			if (SingletonComponent<TutorialManager>.Instance != null && !SingletonComponent<TutorialManager>.Instance.IsCompleted(Tutorial.State.All))
			{
				m_labelChestWarning.text = string.Empty;
			}
			else
			{
				m_labelChestWarning.text = text;
			}
		}
	}

	public void SetPerformanceData(int damageDealt, int damageReceived, int timeTaken, int specialMoveScore)
	{
		m_targetDamageDealt = damageDealt;
		m_targetDamageRecieved = damageReceived;
		m_targetTimeTaken = timeTaken;
		m_targetSpecialMoveScore = specialMoveScore;
	}

	public void SetBattleNum(int p_num)
	{
		m_battleNum.spriteName = "text_nobattle_" + p_num;
		m_battleNum1st[0].spriteName = "text_nobattle_" + p_num;
		m_battleNum1st[1].spriteName = "text_nobattle_" + p_num;
	}

	public void SetRank(string p_rank)
	{
	}

	public void SetRank(int[] medalList)
	{
		m_medalList = medalList;
		m_showMedal = true;
		switch ((LeaderboardManager.MEDAL_RANK)m_medalList[m_medalList.Length - 1])
		{
		case LeaderboardManager.MEDAL_RANK.WOOD:
			m_medalIcon.spriteName = "medal_broken";
			break;
		case LeaderboardManager.MEDAL_RANK.BRONZE:
			m_medalIcon.spriteName = "medal_bronze";
			break;
		case LeaderboardManager.MEDAL_RANK.SILVER:
			m_medalIcon.spriteName = "medal_silver";
			break;
		case LeaderboardManager.MEDAL_RANK.GOLD:
			m_medalIcon.spriteName = "medal_gold";
			break;
		case LeaderboardManager.MEDAL_RANK.NONE:
			m_showMedal = false;
			break;
		}
		float num = 0.25f / (float)m_totalBattles;
		progressPrior = 0f;
		for (int i = 0; i < m_medalList.Length - 1; i++)
		{
			progressPrior += (float)m_medalList[i] * num;
		}
		m_targetProgress = progressPrior + (float)m_medalList[m_medalList.Length - 1] * num;
		m_itemProgressBar.sliderValue = progressPrior;
		tempProgress = progressPrior;
		if (m_medalList.Length != m_totalBattles)
		{
			return;
		}
		int num2 = 0;
		m_gotMysteryTreasure = false;
		int[] medalList2 = m_medalList;
		foreach (int num3 in medalList2)
		{
			if (num3 == 4)
			{
				num2++;
			}
		}
		if (num2 == m_totalBattles)
		{
			m_gotMysteryTreasure = true;
		}
		if (SingletonComponent<TutorialManager>.Instance != null && !SingletonComponent<TutorialManager>.Instance.IsCompleted(Tutorial.State.All))
		{
			if (m_mysteryTreasureChest != null)
			{
				m_mysteryTreasureChest.enabled = false;
				m_mysteryTreasureChest.gameObject.SetActiveRecursively(false);
			}
			m_gotMysteryTreasure = false;
		}
	}

	private void EnableRankPic(bool p_enable)
	{
	}

	public void SetScore(int p_score, int p_prevScore = 0)
	{
		tempScoreValue = p_prevScore;
		if (p_score < 0)
		{
			m_targetScoreValue = 0;
		}
		else
		{
			m_targetScoreValue = p_score;
		}
		m_scoreValue.text = ((int)tempScoreValue).ToString();
	}

	public void SetGold(int p_gold)
	{
		m_targetGoldValue = p_gold;
	}

	public void SetExp(int p_exp)
	{
		m_targetExpValue = p_exp;
	}

	public void SetTreasure(ItemEnum.Chest chest)
	{
		m_treasureGot = chest;
		if (chest != ItemEnum.Chest.NONE)
		{
			m_targetTreasureValue = 1;
		}
		else
		{
			m_targetTreasureValue = 0;
		}
	}

	public void AddProgress(float p_add)
	{
		Dbg.Log("[Info] Add progress bar value : " + p_add);
		MBPrefs.SetFloat("Ms:ItemProgress", SingletonComponent<BattleManager>.Instance.progressBarProgressTargetValue);
		Dbg.Log("[Info] Save target value : " + SingletonComponent<BattleManager>.Instance.progressBarProgressTargetValue + " in local");
		SingletonComponent<BattleManager>.Instance.progressBarProgressTargetValue += p_add;
		Dbg.Log("[Info] Target value : " + SingletonComponent<BattleManager>.Instance.progressBarProgressTargetValue);
	}

	private string FormatTime(int timeValue)
	{
		int num = timeValue % 3600 / 60;
		int num2 = timeValue % 60;
		string text = string.Empty;
		if (num > 0)
		{
			text = text + num + "m";
		}
		if (num2 > 9)
		{
			return text + num2 + "s";
		}
		return text + "0" + num2 + "s";
	}

	public void SkipAnimationToTreasure()
	{
		foreach (AnimationState item in base.GetComponent<Animation>())
		{
			item.speed = 0f;
			item.time = treasureShowTimeFrameInAnimation;
		}
		m_treasureScreen.gameObject.SetActiveRecursively(true);
		m_treasure.gameObject.SetActiveRecursively(true);
		treasureShown = true;
	}

	public void SkipAllAnimationShowAll()
	{
		if (!clickedToTreasureShow && m_targetTreasureValue > 0 && (m_curState == BattleClearState.SHOW_CLEAR_TEXT || m_curState == BattleClearState.SHOW_TREASURE_GOT))
		{
			clickedToTreasureShow = true;
			SkipAnimationToTreasure();
		}
		else if (clickedToTreasureShow && treasureShown)
		{
			ResumeAnimation();
			treasureShown = false;
		}
		else
		{
			if (m_curState == BattleClearState.SHOW_ALL || m_curState == BattleClearState.SHOW_FINAL_LOOT)
			{
				return;
			}
			m_curState = BattleClearState.SHOW_ALL;
			foreach (AnimationState item in base.GetComponent<Animation>())
			{
				item.normalizedTime = 1f;
			}
			m_scoreValue.text = m_targetScoreValue.ToString();
			m_itemProgressBar.sliderValue = m_targetProgress;
			m_damageOnMonsterValue.text = m_targetDamageDealt.ToString();
			m_damageOnPlayerValue.text = m_targetDamageRecieved.ToString();
			m_timeTakenValue.text = FormatTime(m_targetTimeTaken);
			m_specialMoveValue.text = m_targetSpecialMoveScore.ToString();
			m_treasureValue.text = m_targetTreasureValue.ToString();
			m_goldValue.text = m_targetGoldValue.ToString();
			m_expValue.text = m_targetExpValue.ToString();
			m_treasureScreen.gameObject.SetActiveRecursively(false);
			m_clearScreen.gameObject.SetActiveRecursively(false);
			if (m_showMedal)
			{
				m_medalGroup.gameObject.SetActiveRecursively(true);
			}
			m_mainScreen.gameObject.SetActiveRecursively(true);
		}
	}
}
