using System;
using System.Collections;
using System.Collections.Generic;
using Models;
using UnityEngine;

public class DungeonCompletePanel : NBUIMenuPanel
{
	public enum Rank
	{
		AAA = 0,
		AA = 1,
		A = 2,
		B = 3,
		C = 4,
		D = 5,
		E = 6,
		F = 7
	}

	private string animDungeonClearedPath = "UI/Prefabs/DungeonComplete/Anim_Dungeon_Cleared";

	private string animDungeonIncompletePath = "UI/Prefabs/DungeonComplete/Anim_Dungeon_Incomplete";

	private string animDungeonChestPath = "UI/Prefabs/DungeonComplete/DungeonCompChest";

	private string animEventClearedPath = "UI/Prefabs/DungeonComplete/Event_Dungeon_Cleared";

	private string promptCompletePrefabPath = "UI/Prefabs/DungeonComplete/AreaCompletePrompt";

	private string promptIncompletePrefabPath = "UI/Prefabs/DungeonComplete/AreaIncompletePrompt";

	public GameObject m_battleResultsGO;

	private GameObject m_promptCompletePrefab;

	private GameObject m_promptIncompletePrefab;

	private GameObject m_promptCompleteGO;

	private GameObject m_promptIncompleteGO;

	[SerializeField]
	private GameObject m_goGold;

	[SerializeField]
	private GameObject m_goExp;

	[SerializeField]
	private GameObject m_goDragonPoints;

	[SerializeField]
	private GameObject m_goScore;

	[SerializeField]
	private GameObject m_goMedal;

	[SerializeField]
	private GameObject m_completeAnchor;

	public UILabel m_labelTotalXPGained;

	public UILabel m_labelGoldCollected;

	public UILabel m_labelDungeonScore;

	public UILabel m_labelTotalXPGainedValue;

	public UILabel m_labelGoldCollectedValue;

	public UILabel m_labelDragonPointsValue;

	public UILabel m_labelDungeonScoreValue;

	[SerializeField]
	private UISprite m_dungeonCompRank;

	public List<Vector3> m_upperPositions = new List<Vector3>();

	public List<Vector3> m_lowerPositions = new List<Vector3>();

	[SerializeField]
	private RewardItemInfoPanel m_rewardItemInfoPanel;

	[SerializeField]
	private NewRevealItem m_newRevealItemAnim;

	[SerializeField]
	private UISprite m_tapToContinue;

	[SerializeField]
	private GameObject m_FXPremium;

	private int m_targetGold = 1;

	private PlayerData m_playerData;

	private bool m_mayContinue;

	private bool m_isOnPopup;

	[SerializeField]
	private float TREASURE_CELL_WIDTH = 190f;

	private string[] medalNames = new string[4] { "medal_broken", "medal_bronze", "medal_silver", "medal_gold" };

	private string[] chestSprites = new string[5] { "Chest_Lv1", "Chest_Lv2", "Chest_Lv3", "Chest_Lv4", "Chest_Lv5" };

	private bool userClicked;

	private new void Awake()
	{
		Dbg.Assert(m_tapToContinue);
		m_tapToContinue.enabled = false;
		m_dungeonCompRank.enabled = false;
		if ((bool)SingletonComponent<PlayerDataManager>.Instance)
		{
			m_playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		}
		m_mayContinue = false;
		if (MainMenu.Instance != null)
		{
			m_rewardItemInfoPanel = MainMenu.Instance.m_RewardItemInfoPanel;
		}
		Dbg.Assert(m_goGold);
		Dbg.Assert(m_goExp);
		Dbg.Assert(m_goDragonPoints);
		Dbg.Assert(m_goMedal);
		Dbg.Assert(m_goScore);
		m_goGold.SetActiveRecursively(false);
		m_goExp.SetActiveRecursively(false);
		m_goDragonPoints.SetActiveRecursively(false);
		m_goMedal.SetActiveRecursively(false);
		m_goScore.SetActiveRecursively(false);
		m_promptCompletePrefab = Resources.Load(promptCompletePrefabPath) as GameObject;
		m_promptIncompletePrefab = Resources.Load(promptIncompletePrefabPath) as GameObject;
	}

	private void OnEnable()
	{
		if ((bool)m_goGold)
		{
			m_goGold.SetActiveRecursively(false);
		}
		if ((bool)m_goExp)
		{
			m_goExp.SetActiveRecursively(false);
		}
		if ((bool)m_goDragonPoints)
		{
			m_goDragonPoints.SetActiveRecursively(false);
		}
		if ((bool)m_goMedal)
		{
			m_goMedal.SetActiveRecursively(false);
		}
		if ((bool)m_goScore)
		{
			m_goScore.SetActiveRecursively(false);
		}
		MainMenu.Instance.ShowBottomBar(false);
		MainMenu.Instance.topBar.labelPC.text = m_playerData.m_userData.premiumCurrency.ToString();
		MainMenu.Instance.topBar.labelStamina.text = m_playerData.m_userData.stamina + "/" + m_playerData.m_userData.maxStamina;
	}

	private void OnDisable()
	{
	}

	public void Play()
	{
	}

	private IEnumerator Start()
	{
		Messenger.Invoke("dungeon_cleared_start");
		MainMenu.Instance.EnableHeader(false, string.Empty);
		m_isOnPopup = false;
		UIPopupController.LockUICamera("UI_Bar", true);
		m_rewardItemInfoPanel.Hide();
		yield return null;
		Animation animDungeonCleared = null;
		bool isEvent = false;
		if (SingletonComponent<DungeonManager>.Instance != null)
		{
			isEvent = SingletonComponent<DungeonManager>.Instance.IsPlayingEventDungeon && !SingletonComponent<TutorialManager>.Instance.enabled;
			if (SingletonComponent<DungeonManager>.Instance.HasDungeonComplete())
			{
				string animPath = ((!isEvent) ? animDungeonClearedPath : animEventClearedPath);
				Animation go = Resources.Load(animPath, typeof(Animation)) as Animation;
				if (go != null)
				{
					animDungeonCleared = UnityEngine.Object.Instantiate(go, Vector3.right * 1000f, Quaternion.identity) as Animation;
				}
				else
				{
					Dbg.LogWarn("Dungeon Complete Animation Prefab missing");
				}
			}
			else
			{
				Animation go2 = Resources.Load(animDungeonIncompletePath, typeof(Animation)) as Animation;
				if (go2 != null)
				{
					animDungeonCleared = UnityEngine.Object.Instantiate(go2, Vector3.zero, Quaternion.identity) as Animation;
				}
				else
				{
					Dbg.LogWarn("Dungeon Incomplete Animation Prefab missing");
				}
			}
		}
		else
		{
			Dbg.Assert(false, "Problem with dungeon manager");
		}
		if (animDungeonCleared == null)
		{
			Dbg.LogWarn("Animation Missing");
		}
		else
		{
			MainMenu.Instance.ShowTopBar(true);
			if (m_completeAnchor != null)
			{
				animDungeonCleared.transform.parent = m_completeAnchor.transform;
				animDungeonCleared.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
				animDungeonCleared.transform.localPosition = new Vector3(-80f, -203f, -190f);
			}
			else
			{
				animDungeonCleared.transform.parent = base.transform;
				animDungeonCleared.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
				animDungeonCleared.transform.localPosition = new Vector3(-80f, 59f, -190f);
			}
			yield return new MBWaitForSeconds(0.3f);   // PORT (CD-059)
			yield return null;
			animDungeonCleared.Play();
			yield return null;
			yield return new MBWaitForSeconds(animDungeonCleared.clip.length);   // PORT (CD-059)
		}
		List<DungeonCompChest> upperChests = new List<DungeonCompChest>();
		List<DungeonCompChest> lowerChests = new List<DungeonCompChest>();
		List<ServerInventoryItem> chestItems = SingletonComponent<DungeonManager>.Instance.ChestRewards;
		if (chestItems != null)
		{
			foreach (ServerInventoryItem temp in chestItems)
			{
				Dbg.Log("The reward item to show : " + temp.item_id);
			}
		}
		AdjustChestPos(ref m_upperPositions, ref m_lowerPositions, chestItems.Count);
		if (chestItems.Count <= 0)
		{
			m_upperPositions.Add(new Vector3(310f, 25f, 0f));
		}
		float posy = m_upperPositions[0].y;
		Vector3 initPos = Vector3.right * 700f + Vector3.up * posy;
		GameObject dungeonChest = Resources.Load(animDungeonChestPath) as GameObject;
		if (dungeonChest == null)
		{
			Dbg.LogError("Could not load prefab at " + animDungeonChestPath);
			yield break;
		}
		for (int i = 0; i < 5 && i < chestItems.Count; i++)
		{
			initPos.x += 155f;
			GameObject tempGo = UnityEngine.Object.Instantiate(dungeonChest, initPos, Quaternion.identity) as GameObject;
			if (tempGo == null)
			{
				Dbg.LogError("Dungeon animation gameobject failed to instantiatiate");
				continue;
			}
			tempGo.transform.parent = ((!m_battleResultsGO) ? base.transform : m_battleResultsGO.transform);
			tempGo.transform.localScale = Vector3.one;
			tempGo.transform.localPosition += new Vector3(-2f, 0f, 0f);
			DungeonCompChest tempChest = tempGo.GetComponent<DungeonCompChest>();
			if (tempChest != null)
			{
				tempChest.m_dungeonCompletePanelRef = this;
				tempChest.StopParticleEffects();
				upperChests.Add(tempChest);
			}
			else
			{
				Dbg.LogWarn("DungeonCompChest component not found, beware!");
			}
		}
		if (chestItems.Count > 5)
		{
			posy = m_lowerPositions[0].y;
			initPos = Vector3.right * -700f + Vector3.up * posy;
			for (int j = 5; j < chestItems.Count; j++)
			{
				initPos.x -= 155f;
				GameObject chest = UnityEngine.Object.Instantiate(dungeonChest, initPos, Quaternion.identity) as GameObject;
				if (chest == null)
				{
					Dbg.LogError("Dungeon Animation/Chest failed to instantiate");
					continue;
				}
				chest.transform.parent = ((!m_battleResultsGO) ? base.transform : m_battleResultsGO.transform);
				chest.transform.localScale = Vector3.one;
				chest.transform.localPosition += Vector3.right * -2f;
				DungeonCompChest tempChest2 = chest.GetComponent<DungeonCompChest>();
				if (tempChest2 != null)
				{
					tempChest2.m_dungeonCompletePanelRef = this;
					lowerChests.Add(tempChest2);
				}
				else
				{
					Dbg.LogWarn("Missing DungeonCompChest component");
				}
			}
		}
		foreach (DungeonCompChest chest2 in upperChests)
		{
			chest2.m_itemCell.gameObject.GetComponent<Collider>().enabled = false;
		}
		foreach (DungeonCompChest chest3 in lowerChests)
		{
			chest3.m_itemCell.gameObject.GetComponent<Collider>().enabled = false;
		}
		int itemCounter = 0;
		foreach (DungeonCompChest chest4 in upperChests)
		{
			ParticleSystem[] ps = MBHier.GetComponentsInChildren<ParticleSystem>(chest4);
			ParticleSystem[] array = ps;
			foreach (ParticleSystem p in array)
			{
				p.Play();
			}
			chest4.SetItemData(chestItems[itemCounter].db_id, SingletonComponent<InventoryManager>.Instance.IsNewNameItem(chestItems[itemCounter].item_id));
			int idx = (int)chest4.GetChestLevel();
			Transform chestType = chest4.gameObject.transform.Find("ChestType");
			UISprite sprite = ((!(chestType != null)) ? null : chestType.gameObject.GetComponent<UISprite>());
			if (sprite != null && idx > -1 && idx < chestSprites.Length)
			{
				if (!chestItems[itemCounter].isFinalLoot)
				{
					sprite.spriteName = chestSprites[idx];
				}
				else
				{
					sprite.spriteName = "chest";
				}
				sprite.MakePixelPerfect();
			}
			itemCounter++;
		}
		lowerChests.Reverse();
		foreach (DungeonCompChest chest5 in lowerChests)
		{
			ParticleSystem[] ps2 = MBHier.GetComponentsInChildren<ParticleSystem>(chest5);
			ParticleSystem[] array2 = ps2;
			foreach (ParticleSystem p2 in array2)
			{
				p2.Play();
			}
			chest5.SetItemData(chestItems[itemCounter].db_id, SingletonComponent<InventoryManager>.Instance.IsNewNameItem(chestItems[itemCounter].item_id));
			int idx2 = (int)chest5.GetChestLevel();
			Transform chestType2 = chest5.gameObject.transform.Find("ChestType");
			UISprite sprite2 = ((!(chestType2 != null)) ? null : chestType2.gameObject.GetComponent<UISprite>());
			if (sprite2 != null && idx2 > -1 && idx2 < chestSprites.Length)
			{
				if (!chestItems[itemCounter].isFinalLoot)
				{
					sprite2.spriteName = chestSprites[idx2];
				}
				else
				{
					sprite2.spriteName = "chest";
				}
				sprite2.MakePixelPerfect();
			}
			itemCounter++;
		}
		if (chestItems.Count > 0)
		{
			StartCoroutine(MoveChests(upperChests, m_upperPositions));
			yield return StartCoroutine(MoveChests(lowerChests, m_lowerPositions));
		}
		yield return new MBWaitForSeconds(0.5f);   // PORT (CD-059)
		foreach (DungeonCompChest chest6 in lowerChests)
		{
			upperChests.Add(chest6);
		}
		foreach (DungeonCompChest chest7 in upperChests)
		{
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_score_counting");
			chest7.ShowItem();
			yield return new MBWaitForSeconds(0.9f);   // PORT (CD-059)
		}
		yield return new MBWaitForSeconds(0.3f);   // PORT (CD-059)
		Vector3 pos = m_goGold.transform.localPosition;
		pos.x -= 500f;
		float moveDuration = 0.2f;
		iTween.MoveFrom(args: iTween.Hash("position", pos, "time", moveDuration, "easetype", iTween.EaseType.easeInOutCubic, "islocal", true), target: m_goGold);
		m_goGold.SetActiveRecursively(true);
		yield return new MBWaitForSeconds(moveDuration);   // PORT (CD-059)
		m_labelGoldCollectedValue.enabled = true;
		float goldVal = 0f;
		float countDuration = 2f;
		int ingameGained = SingletonComponent<DungeonManager>.Instance.GetInGameGained();
		int currInGame = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData().m_userData.inGameCurrency - ingameGained;
		if (currInGame < 0)
		{
			currInGame = 0;
		}
		MainMenu.Instance.topBar.labelIGC.text = currInGame.ToString();
		m_targetGold = currInGame + ingameGained;
		float inc = Time.fixedDeltaTime / countDuration * (float)ingameGained;
		userClicked = false;
		while (goldVal < (float)ingameGained)
		{
			goldVal += inc;
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_score_counting");
			if (userClicked)
			{
				goldVal = ingameGained;
				userClicked = false;
			}
			int iGoldVal = Mathf.FloorToInt(goldVal);
			m_labelGoldCollectedValue.text = iGoldVal.ToString();
			MainMenu.Instance.topBar.labelIGC.text = (currInGame + iGoldVal).ToString();
			yield return null;
		}
		m_labelGoldCollectedValue.text = ingameGained.ToString();
		MainMenu.Instance.topBar.labelIGC.text = m_targetGold.ToString();
		yield return new MBWaitForSeconds(0.7f);   // PORT (CD-059)
		PlayerData oldData = SingletonComponent<DungeonManager>.Instance.GetOriginalStats();
		PlayerData newData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		if (newData != null && oldData != null && newData.m_userData.premiumCurrency > oldData.m_userData.premiumCurrency)
		{
			m_isOnPopup = true;
			UIPopupController.Instance.ShowDiamondGot(newData.m_userData.premiumCurrency - oldData.m_userData.premiumCurrency, OnExitPremiumCurrencyReceivedPopup);
			while (m_isOnPopup)
			{
				yield return null;
			}
		}
		pos = m_goExp.transform.localPosition;
		pos.x -= 500f;
		moveDuration = 0.2f;
		iTween.MoveFrom(args: iTween.Hash("position", pos, "time", moveDuration, "easetype", iTween.EaseType.easeInOutCubic, "islocal", true), target: m_goExp.gameObject);
		m_goExp.SetActiveRecursively(true);
		yield return new MBWaitForSeconds(moveDuration);   // PORT (CD-059)
		int totalXpGained = SingletonComponent<DungeonManager>.Instance.GetGainedXP();
		if (oldData != null && oldData.Level == 1)
		{
			int targetXp = oldData.TargetXp;
			if (targetXp > 0)
			{
			}
		}
		if (newData != null && oldData != null && (float)oldData.m_targetXp > 0f)
		{
			int levelGained = newData.Level - oldData.Level;
			int xpGained = 0;
			int currentXp = oldData.Xp;
			PlayerData levelingData = null;
			int levelUpDone = 0;
			int cumulativeXp = 0;
			if (levelGained > 0)
			{
				xpGained = oldData.m_targetXp - oldData.Xp;
				levelingData = new PlayerData(oldData);
			}
			else
			{
				xpGained = totalXpGained;
			}
			countDuration = 2f;
			int xpDelta = Mathf.CeilToInt(Time.fixedDeltaTime / countDuration * (float)xpGained);
			float xpRatio = currentXp / oldData.TargetXp;
			float xpTargetToLevelUp = oldData.TargetXp;
			while (cumulativeXp < totalXpGained)
			{
				SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_score_counting");
				if (userClicked)
				{
					if (levelUpDone < levelGained)
					{
						cumulativeXp += (int)xpTargetToLevelUp - currentXp;
						currentXp = (int)xpTargetToLevelUp;
					}
					else
					{
						currentXp = newData.Xp;
						cumulativeXp = totalXpGained;
					}
					userClicked = false;
				}
				cumulativeXp += xpDelta;
				if (cumulativeXp >= totalXpGained)
				{
					currentXp += xpDelta - (cumulativeXp - totalXpGained);
					cumulativeXp = totalXpGained;
				}
				else
				{
					currentXp += xpDelta;
				}
				xpRatio = (float)currentXp / xpTargetToLevelUp;
				m_labelTotalXPGainedValue.text = cumulativeXp.ToString();
				if (xpRatio >= 1f)
				{
					MainMenu.Instance.topBar.SetXP(0f);
					MainMenu.Instance.topBar.SetName(m_playerData.Name, levelingData.Level + 1);
					MainMenu.Instance.CreateLevelUpWindow();
					SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_level_up_notification");
					if (MainMenu.Instance.m_levelUpWindow != null)
					{
						MainMenu.Instance.MoveLevelUpWindowToPopup();
						if (oldData != null && levelingData != null)
						{
							MainMenu.Instance.m_levelUpWindow.SetLevelUpData(oldData, newData, levelUpDone);
						}
						while (MainMenu.Instance.m_levelUpWindow.Show())
						{
							yield return null;
						}
					}
					MainMenu.Instance.m_levelUpWindow.Hide();
					MainMenu.Instance.DestroyLevelUpWindow();
					levelUpDone++;
					if (levelingData != null)
					{
						levelingData.GainXp(levelingData.m_targetXp - levelingData.Xp);
						PlayerData playerData = levelingData;
						int xp;
						currentXp = (xp = currentXp - Mathf.FloorToInt(xpTargetToLevelUp));
						playerData.Xp = xp;
						xpTargetToLevelUp = levelingData.m_targetXp;
						float remainXp = totalXpGained - cumulativeXp;
						xpDelta = ((!(remainXp > xpTargetToLevelUp)) ? Mathf.CeilToInt(Time.fixedDeltaTime / countDuration * remainXp) : Mathf.CeilToInt(Time.fixedDeltaTime / countDuration * xpTargetToLevelUp));
						Dbg.Log("LEVELUP: currentXp:" + currentXp + " xpTargetToLevelup:" + xpTargetToLevelUp);
					}
					if (SingletonComponent<SaveData>.Instance != null && levelingData != null)
					{
						MainMenu.Instance.topBar.SetName(m_playerData.Name, levelingData.Level);
					}
					MainMenu.Instance.topBar.SetXP(0f);
					MainMenu.Instance.topBar.labelXPPercentage.text = "0%";
				}
				else
				{
					MainMenu.Instance.topBar.SetXP(xpRatio, (levelingData != null) ? levelingData.Level : 0);
				}
				yield return null;
			}
		}
		m_labelTotalXPGainedValue.text = totalXpGained.ToString();
		if (isEvent)
		{
			float oldPoints = 0f;
			if (oldData != null)
			{
				oldPoints = oldData.details.currencies.dragonPoints;
			}
			float dragonPoints = (float)newData.details.currencies.dragonPoints - oldPoints;
			pos = m_goDragonPoints.transform.localPosition;
			pos.x -= 500f;
			moveDuration = 0.2f;
			iTween.MoveFrom(args: iTween.Hash("position", pos, "time", moveDuration, "easetype", iTween.EaseType.easeOutCubic, "islocal", true), target: m_goDragonPoints.gameObject);
			m_goDragonPoints.SetActiveRecursively(true);
			yield return new MBWaitForSeconds(moveDuration);   // PORT (CD-059)
			m_labelDragonPointsValue.enabled = true;
			m_labelDragonPointsValue.text = "0";
			float points = 0f;
			float increment = Time.fixedDeltaTime / 2f * dragonPoints;
			userClicked = false;
			while (points < dragonPoints)
			{
				SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_score_counting");
				points += increment;
				if (userClicked)
				{
					points = dragonPoints;
					userClicked = false;
				}
				m_labelDragonPointsValue.text = (int)points + string.Empty;
				yield return null;
			}
			m_labelDragonPointsValue.text = (int)dragonPoints + string.Empty;
		}
		pos = m_goScore.transform.localPosition;
		pos.x -= 500f;
		moveDuration = 0.2f;
		iTween.MoveFrom(args: iTween.Hash("position", pos, "time", moveDuration, "easetype", iTween.EaseType.easeOutCubic, "islocal", true), target: m_goScore.gameObject);
		m_goScore.SetActiveRecursively(true);
		yield return new MBWaitForSeconds(moveDuration);   // PORT (CD-059)
		m_labelDungeonScoreValue.enabled = true;
		m_labelDungeonScoreValue.text = "0";
		float score = 0f;
		float increasement = Time.fixedDeltaTime / 2f * (float)SingletonComponent<DungeonManager>.Instance.m_dungeonCompScore;
		userClicked = false;
		while (score < (float)SingletonComponent<DungeonManager>.Instance.m_dungeonCompScore)
		{
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_score_counting");
			score += increasement;
			if (userClicked)
			{
				score = SingletonComponent<DungeonManager>.Instance.m_dungeonCompScore;
				userClicked = false;
			}
			m_labelDungeonScoreValue.text = ((int)score).ToString();
			yield return null;
		}
		m_labelDungeonScoreValue.text = SingletonComponent<DungeonManager>.Instance.m_dungeonCompScore.ToString();
		int medalRank = SingletonComponent<LeaderboardManager>.Instance.GetLowestMedalFromDungeon();
		userClicked = false;
		if (medalRank != 0)
		{
			moveDuration = 0.5f;
			m_goMedal.SetActiveRecursively(true);
			TweenFade[] tfs = MBHier.GetComponentsInChildren<TweenFade>(m_goMedal, true);
			TweenFade[] array3 = tfs;
			foreach (TweenFade tf in array3)
			{
				tf.enterDuration = moveDuration;
				tf.Init();
				tf.OnEnter();
			}
			yield return new MBWaitForSeconds(moveDuration);   // PORT (CD-059)
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_notification");
			m_dungeonCompRank.spriteName = medalNames[medalRank - 1];
			m_dungeonCompRank.enabled = true;
			moveDuration = 0.6f;
			iTween.ScaleFrom(m_dungeonCompRank.gameObject, iTween.Hash("x", m_dungeonCompRank.transform.localScale.x * 3f, "y", m_dungeonCompRank.transform.localScale.y * 3f, "time", moveDuration));
			yield return new MBWaitForSeconds(moveDuration);   // PORT (CD-059)
		}
		yield return new MBWaitForSeconds(0.7f);   // PORT (CD-059)
		foreach (DungeonCompChest chest8 in upperChests)
		{
			chest8.m_canClick = true;
		}
		if (SingletonComponent<TutorialManager>.Instance != null)
		{
			if (SingletonComponent<TutorialManager>.Instance.IsCompleted(Tutorial.State.All) && null != SingletonComponent<DungeonManager>.Instance)
			{
				SaveData.MsData data = SingletonComponent<SaveData>.Instance.GameData;
				data.MonsterKillCount = SingletonComponent<DungeonManager>.Instance.NumOfMonsterDefeated;
				data.SaveAchievementData();
			}
			if (SingletonComponent<TutorialManager>.Instance.DelayShowOchaSaviorAchievement && !SingletonComponent<SaveData>.Instance.GameData.SaveOcha)
			{
				SingletonComponent<SaveData>.Instance.GameData.SaveOcha = true;
				SingletonComponent<SaveData>.Instance.GameData.SaveAchievementData();
			}
		}
		Messenger<Action>.Invoke("dungeon_cleared_end", EndDungeonComplete);
		Delegate d = Messenger<Action>.GetListener("dungeon_cleared_end");
		if ((object)d == null)
		{
			EndDungeonComplete();
		}
	}

	private void EndDungeonComplete()
	{
		StartCoroutine(DoEndDungeonComplete());
	}

	private IEnumerator DoEndDungeonComplete()
	{
		if (MainMenu.Instance.CheckExcessItem())
		{
			yield return null;
		}
		else if (SingletonComponent<DungeonManager>.Instance != null && SingletonComponent<DungeonManager>.Instance.IsPlayingEventDungeon && !SingletonComponent<TutorialManager>.Instance.enabled)
		{
			Dbg.Log("Player lose, show promotion banner for players to play gacha");
			bool hasGachaPromo = false;
			foreach (PromotionData tempData in SingletonComponent<DailyManager>.Instance.PromoDatas)
			{
				if (tempData.PromoType == "gacha")
				{
					hasGachaPromo = true;
					break;
				}
			}
			if (hasGachaPromo)
			{
				UIPopupController.Instance.ShowPromotion(true);
			}
		}
		UIPopupController.LockUICamera("UI_Bar", false);
		yield return new MBWaitForSeconds(1f);   // PORT (CD-059)
		Dbg.Assert(m_tapToContinue);
		m_tapToContinue.enabled = true;
		m_mayContinue = true;
		SingletonComponent<DungeonManager>.Instance.SetShowDungeonResultScreen(false);
	}

	private new void Update()
	{
	}

	private void TapToContinue()
	{
		userClicked = true;
		if (userClicked && m_mayContinue)
		{
			Messenger.Invoke("dungeon_cleared_end");
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_mm_button_tap");
			m_mayContinue = false;
			SingletonComponent<InventoryManager>.Instance.ClearNewNameItems();
			int num = MainMenu.Instance.inventory.HowManyExcessItems();
			TutorialManager instance = SingletonComponent<TutorialManager>.Instance;
			if ((null != instance && instance.enabled) || num > 0)
			{
				MainMenu.Instance.StartHomePage();
			}
			else
			{
				ShowPrompt();
			}
			SingletonComponent<DungeonManager>.Instance.ResetDungeonCompleteFlag();
		}
	}

	private void ShowPrompt()
	{
		if (null != m_battleResultsGO)
		{
			m_battleResultsGO.SetActiveRecursively(false);
		}
		DungeonManager instance = SingletonComponent<DungeonManager>.Instance;
		if (null != instance)
		{
			if (instance.HasDungeonComplete())
			{
				m_promptCompleteGO = UnityEngine.Object.Instantiate(m_promptCompletePrefab) as GameObject;
				m_promptCompleteGO.transform.parent = base.transform;
				m_promptCompleteGO.transform.localScale = Vector3.one;
				m_promptCompleteGO.transform.SetLayerRecursively(base.gameObject.layer);
			}
			else
			{
				m_promptIncompleteGO = UnityEngine.Object.Instantiate(m_promptIncompletePrefab) as GameObject;
				m_promptIncompleteGO.transform.parent = base.transform;
				m_promptIncompleteGO.transform.localScale = Vector3.one;
				m_promptIncompleteGO.transform.SetLayerRecursively(base.gameObject.layer);
			}
		}
		else
		{
			MainMenu.Instance.StartHomePage();
		}
	}

	public override void PostShowPanel()
	{
		base.PostShowPanel();
	}

	public override void PreHidePanel()
	{
		base.PreHidePanel();
	}

	private IEnumerator MoveChests(List<DungeonCompChest> chests, List<Vector3> targets)
	{
		int k = 0;
		foreach (DungeonCompChest chest in chests)
		{
			float moveDuration = 0.5f;
			int index = k % 5;
			chest.StopParticleEffects();
			if (index < targets.Count)
			{
				iTween.MoveTo(chest.gameObject, iTween.Hash("position", targets[index], "time", moveDuration, "easetype", iTween.EaseType.easeOutCubic, "islocal", true));
			}
			k++;
		}
		yield return new MBWaitForSeconds(0.2f);   // PORT (CD-059)
	}

	private void ShowItemStats(GachaResult p_resultAnim, ItemDetails item)
	{
		p_resultAnim.elementType = item.Element;
		p_resultAnim.m_spriteFrame.spriteName = item.ElementSprite;
		p_resultAnim.m_spriteItem.atlas = item.BGAtlas;
		p_resultAnim.m_spriteItem.spriteName = item.BGSpriteName;
		p_resultAnim.Show3DModel(item);
		p_resultAnim.m_itemInfo.ShowItem(item, false);
	}

	public void ShowItemInfo(ItemDetails p_itemData)
	{
		m_rewardItemInfoPanel.ShowItemInfo(p_itemData);
	}

	private void OnClose()
	{
	}

	private void OnGoToCharacterScreen()
	{
		MainMenu.Instance.LockdownPlayerToCharacterMenuOnly();
	}

	private void OnPurchaseSlotsYes()
	{
		m_isOnPopup = false;
		if (SingletonComponent<InventoryManager>.Instance.MaxSpace >= GLOBAL.GAME_SETTINGS.slot_limitation)
		{
			MainMenu.Instance.LockdownPlayerToCharacterMenuOnly();
		}
	}

	private void OnPurchaseSlotsNo()
	{
		m_isOnPopup = false;
	}

	private void OnExitPremiumCurrencyReceivedPopup()
	{
		m_isOnPopup = false;
	}

	private void AdjustChestPos(ref List<Vector3> p_upperPos, ref List<Vector3> p_lowerPos, int p_num)
	{
		p_upperPos.Clear();
		p_lowerPos.Clear();
		if (p_num < 6)
		{
			AssignPos(ref p_upperPos, p_num, 100f);
			return;
		}
		AssignPos(ref p_upperPos, 5, 124f);
		AssignPos(ref p_lowerPos, p_num - 5, 0f);
	}

	private void AssignPos(ref List<Vector3> p_pos, int p_num, float p_posY, float p_posZ = 0f)
	{
		for (int i = 0; i < p_num; i++)
		{
			p_pos.Add(new Vector3(TREASURE_CELL_WIDTH / 2f + (float)i * TREASURE_CELL_WIDTH - TREASURE_CELL_WIDTH * (float)p_num / 2f, p_posY, p_posZ));
		}
	}

	private Rank TranslateToRank(string p_rankText)
	{
		switch (p_rankText)
		{
		case "AAA":
			return Rank.AAA;
		case "AA":
			return Rank.AA;
		case "A":
			return Rank.A;
		case "B":
			return Rank.B;
		case "C":
			return Rank.C;
		case "D":
			return Rank.D;
		case "E":
			return Rank.E;
		default:
			return Rank.F;
		}
	}
}
