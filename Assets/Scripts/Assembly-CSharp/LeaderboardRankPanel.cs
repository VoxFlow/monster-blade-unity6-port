using System;
using System.Collections.Generic;
using UnityEngine;

public class LeaderboardRankPanel : NBUIMenuPanel
{
	private readonly int c_showRange = 3;

	private readonly int c_showTopX = 10;

	[SerializeField]
	private Camera m_mainUICamera;

	[SerializeField]
	private LeaderboardGrid m_grid;

	[SerializeField]
	private UIDraggablePanel m_scrollView;

	[SerializeField]
	private LeaderboardFakeColumn m_fakeColumn;

	[SerializeField]
	private LeaderboardFakeColumn m_fakeColumnTop;

	[SerializeField]
	private Camera m_fakeColumnCamera;

	[SerializeField]
	private UILabel m_top1Name;

	[SerializeField]
	private UILabel m_top1Title;

	[SerializeField]
	private UILabel m_top1RankLabel;

	[SerializeField]
	private GameObject m_headerObj;

	[SerializeField]
	private GameObject m_spShopBtnObj;

	[SerializeField]
	private UILabel m_remainTimeText;

	private long m_remainTime;

	private GameObject m_top1CharaObj;

	private GameObject m_top1PetModel;

	private bool m_showGlobal = true;

	private PlayerData m_tempPlayerData;

	private int m_tempPlayerRank;

	public UILabel m_Top1RewardAmount;

	public UILabel m_Top10RewardAmount;

	public UILabel m_Top100RewardAmount;

	public UILabel m_Top1000RewardAmount;

	public UISprite m_Top1RewardIcon;

	public UISprite m_Top10RewardIcon;

	public UISprite m_Top100RewardIcon;

	public UISprite m_Top1000RewardIcon;

	public Transform m_topBlocker;

	public Transform m_bottomBlocker;

	private bool m_bottomFakeColumnShow;

	private bool m_topFakeColumnShow;

	private float timer;

	public bool m_BottomFakeColumnShow
	{
		get
		{
			return m_bottomFakeColumnShow;
		}
		set
		{
			m_bottomFakeColumnShow = value;
			if (m_bottomFakeColumnShow)
			{
				m_bottomBlocker.transform.localPosition = new Vector3(m_bottomBlocker.transform.localPosition.x, -414f, m_bottomBlocker.transform.localPosition.z);
			}
			else
			{
				m_bottomBlocker.transform.localPosition = new Vector3(m_bottomBlocker.transform.localPosition.x, -365f, m_bottomBlocker.transform.localPosition.z);
			}
		}
	}

	public bool m_TopFakeColumnShow
	{
		get
		{
			return m_topFakeColumnShow;
		}
		set
		{
			m_topFakeColumnShow = value;
			if (m_topFakeColumnShow)
			{
				m_topBlocker.transform.localPosition = new Vector3(m_topBlocker.transform.localPosition.x, 300f, m_topBlocker.transform.localPosition.z);
			}
			else
			{
				m_topBlocker.transform.localPosition = new Vector3(m_topBlocker.transform.localPosition.x, 350f, m_topBlocker.transform.localPosition.z);
			}
		}
	}

	private void OnEnable()
	{
		SetHeaderPosBasedOnRes();
		if (null == SingletonComponent<LeaderboardManager>.Instance.m_LeaderboardRankPanelRef)
		{
			SingletonComponent<LeaderboardManager>.Instance.m_LeaderboardRankPanelRef = this;
		}
		LeaderPlayer leaderPlayer = null;
		if (m_showGlobal)
		{
			ShowRewardsForGlobal();
			ShowLeaderboard(SingletonComponent<LeaderboardManager>.Instance.m_Leaderboard, SingletonComponent<LeaderboardManager>.Instance.m_player);
			if (SingletonComponent<LeaderboardManager>.Instance.m_Leaderboard.ContainsKey(1))
			{
				leaderPlayer = SingletonComponent<LeaderboardManager>.Instance.m_Leaderboard[1];
			}
			else
			{
				Dbg.LogWarn("[Warn] There's no rank 1 player! Possible reason, there's no player join the ranking.");
			}
		}
		else
		{
			ShowRewardsForSocial();
			ShowLeaderboard(SingletonComponent<LeaderboardManager>.Instance.m_socialLeaderboard, SingletonComponent<LeaderboardManager>.Instance.m_socialPlayer);
			if (SingletonComponent<LeaderboardManager>.Instance.m_socialLeaderboard.ContainsKey(1))
			{
				leaderPlayer = SingletonComponent<LeaderboardManager>.Instance.m_socialLeaderboard[1];
			}
			else
			{
				Dbg.LogWarn("[Warn] There's no rank 1 player! Possible reason, there's no friend player join the ranking.");
			}
		}
		if (MenuBackground3D.Instance != null)
		{
			MenuBackground3D.Instance.ShowDefaultBackGround();
		}
		UIPopupController.Instance.ShowLoadingPopup(SingletonComponent<NBLocalization>.Instance.Get("character_creation_header"), SingletonComponent<NBLocalization>.Instance.Get("character_creation_content"));
		PlayerData playerData;
		if (leaderPlayer == null)
		{
			leaderPlayer = new LeaderPlayer(0, string.Empty, 0);
			playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		}
		else
		{
			playerData = SingletonComponent<SocialManager>.Instance.ServerCharaToPlayerData(leaderPlayer.CharaAccountData);
			SingletonComponent<StageManager>.Instance.NeedUpdatePlayerTemp = true;
		}
		m_top1Name.text = playerData.Name;
		m_top1Title.text = SingletonComponent<NBLocalization>.Instance.Get("common_level") + " " + playerData.Level;
		m_top1RankLabel.text = SingletonComponent<NBLocalization>.Instance.Get("leaderboard_rank_label") + " #1";
		m_remainTime = SingletonComponent<LeaderboardManager>.Instance.GetRemainingTime();
		SingletonComponent<StageManager>.Instance.CreatePlayerMainMenuTemplate(SetupCharacter, playerData, false);
		m_top1PetModel = SingletonComponent<PetManager>.Instance.CreateMainMenuPet(playerData);
		SetCharacterPosition();
		if (MainMenu.Instance != null)
		{
			m_fakeColumnCamera.transform.localScale = MainMenu.Instance.transform.localScale;
			m_fakeColumnCamera.transform.parent.localScale = new Vector3(0.003125f, 0.003125f, 0.003125f);
		}
	}

	private void OnDisable()
	{
		if (m_top1CharaObj != null)
		{
			UnityEngine.Object.DestroyImmediate(m_top1CharaObj);
		}
		if (m_top1PetModel != null)
		{
			UnityEngine.Object.DestroyImmediate(m_top1PetModel);
		}
		m_grid.Clear();
		if (MenuBackground3D.Instance != null)
		{
			MenuBackground3D.Instance.HideBackground();
			MenuBackground3D.Instance.m_camera.gameObject.SetActiveRecursively(false);
		}
		m_fakeColumnCamera.gameObject.SetActiveRecursively(false);
	}

	public void ShowFakeColumn(bool p_enable, bool show_top = false)
	{
		if (m_fakeColumnCamera.gameObject.active != p_enable)
		{
			m_fakeColumnCamera.gameObject.SetActiveRecursively(p_enable);
		}
		if (p_enable)
		{
			if (show_top)
			{
				m_fakeColumn.gameObject.SetActiveRecursively(false);
				m_TopFakeColumnShow = true;
				m_BottomFakeColumnShow = false;
			}
			else
			{
				m_fakeColumnTop.gameObject.SetActiveRecursively(false);
				m_BottomFakeColumnShow = true;
				m_TopFakeColumnShow = false;
			}
		}
		else
		{
			m_TopFakeColumnShow = false;
			m_BottomFakeColumnShow = false;
		}
	}

	private new void Update()
	{
		if (timer < 1f && m_remainTime > 0)
		{
			timer += Time.deltaTime;
			if (timer > 1f)
			{
				timer -= 1f;
				m_remainTime--;
				SetRemainTimeText(m_remainTime);
			}
		}
		if (MainMenu.Instance.GetFaderAlpha() >= 1f)
		{
			MainMenu.Instance.FadeOutOnly();
		}
	}

	private void ShowLeaderboard(Dictionary<int, LeaderPlayer> p_players, LeaderPlayer p_selfPlayer)
	{
		int num = 0;
		int num2 = p_selfPlayer.Rank - c_showRange;
		int num3 = p_selfPlayer.Rank + c_showRange;
		if (num2 < 0)
		{
			num2 = 0;
		}
		bool flag = false;
		foreach (int key in p_players.Keys)
		{
			if ((key > 0 && key <= c_showTopX) || (key >= num2 && key <= num3))
			{
				LeaderPlayer leaderPlayer = p_players[key];
				m_grid.AddData(leaderPlayer);
				LeaderboardCell leaderboardCell = m_grid.AddCell(0f, 0f);
				if (leaderPlayer.Score <= 0)
				{
					leaderboardCell.SetScoreText("0");
				}
				else
				{
					leaderboardCell.SetScoreText(leaderPlayer.Score.ToString());
				}
				leaderboardCell.SetRankText(leaderPlayer.Rank.ToString());
				leaderboardCell.SetNameText(leaderPlayer.CharaName);
				if (key == p_selfPlayer.Rank)
				{
					flag = true;
					leaderboardCell.SetPlayerBackground();
				}
				else
				{
					leaderboardCell.SetBackground(num % 2 == 1);
					UIButtonMessage component = leaderboardCell.GetComponent<UIButtonMessage>();
					component.target = base.gameObject;
					component.functionName = "OnRankPlayerCellClicked";
				}
				num++;
				if (num == c_showTopX && p_selfPlayer.Rank > c_showTopX + c_showRange + 1)
				{
					LeaderPlayer data = new LeaderPlayer(0, string.Empty, 0);
					m_grid.AddData(data);
					LeaderboardCell leaderboardCell2 = m_grid.AddCell(0f, 0f);
					leaderboardCell2.SetBackground(num % 2 == 1);
					leaderboardCell2.SetEmpty();
					num++;
				}
			}
		}
		if (!flag)
		{
			PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
			LeaderPlayer leaderPlayer2 = new LeaderPlayer(0, playerData.Name, 0);
			m_grid.AddData(leaderPlayer2);
			LeaderboardCell leaderboardCell3 = m_grid.AddCell(0f, 0f);
			leaderboardCell3.SetRankText("-");
			leaderboardCell3.SetScoreText("0");
			leaderboardCell3.SetNameText(leaderPlayer2.CharaName);
			leaderboardCell3.SetPlayerBackground();
			flag = true;
		}
		m_scrollView.ResetPosition();
		m_grid.Reposition();
	}

	private void OnButtonClick()
	{
	}

	private void OnSpShopBtnClick()
	{
		Dbg.LogWarn("[Info] OnSpShopBtnClick()");
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_secondary_ui_tap");
	}

	private void SetupCharacter()
	{
		if (!(SingletonComponent<StageManager>.Instance.PlayerTemplate == null))
		{
			GameObject playerTemplate = SingletonComponent<StageManager>.Instance.PlayerTemplate;
			if (null == playerTemplate)
			{
				UIPopupController.Instance.HideLoadingPopup();
				UIPopupController.Instance.ShowOKPopupMSG("common_info", "character_not_found");
				return;
			}
			m_top1CharaObj = playerTemplate;
			m_top1CharaObj.SetActiveRecursively(true);
			string s_idleAnimation = CharacterInfoPanel.s_idleAnimation;
			m_top1CharaObj.GetComponent<Animation>().CrossFade(s_idleAnimation);
			m_top1CharaObj.GetComponent<Animation>()[s_idleAnimation].wrapMode = WrapMode.Loop;
			UIPopupController.Instance.HideLoadingPopup();
			SetCharacterPosition();
			SetupFakeColumn();
		}
	}

	private void SetCharacterPosition()
	{
		CharacterSetting3D characterSetting3D = UnityEngine.Object.FindObjectOfType(typeof(CharacterSetting3D)) as CharacterSetting3D;
		float num = 1.5f;
		float num2 = (float)Screen.width / (float)Screen.height;
		bool flag = num2 > num;
		string p_nodeName = ((!flag) ? "LeaderboardPlayerRoot" : "LeaderboardPlayerRoot_Wide");
		string p_nodeName2 = ((!flag) ? "LeaderboardPetRoot" : "LeaderboardPetRoot_Wide");
		if (null != characterSetting3D)
		{
			characterSetting3D.AttachObject(m_top1CharaObj, p_nodeName);
			characterSetting3D.AttachObject(m_top1PetModel, p_nodeName2);
			characterSetting3D.AttachCamera("MainMenuCameraRoot");
		}
	}

	private void SetupFakeColumn()
	{
		if (SingletonComponent<LeaderboardManager>.Instance != null && SingletonComponent<LeaderboardManager>.Instance.m_player != null)
		{
			if (SingletonComponent<LeaderboardManager>.Instance.m_player.Rank <= 0)
			{
				m_fakeColumn.SetRankText("-");
			}
			else
			{
				m_fakeColumn.SetRankText(SingletonComponent<LeaderboardManager>.Instance.m_player.Rank.ToString());
			}
			m_fakeColumn.SetScoreText(SingletonComponent<LeaderboardManager>.Instance.m_player.Score.ToString());
			m_fakeColumn.SetNameText(SingletonComponent<LeaderboardManager>.Instance.m_player.CharaName);
			if (SingletonComponent<LeaderboardManager>.Instance.m_player.Rank <= 0)
			{
				m_fakeColumnTop.SetRankText("-");
			}
			else
			{
				m_fakeColumnTop.SetRankText(SingletonComponent<LeaderboardManager>.Instance.m_player.Rank.ToString());
			}
			m_fakeColumnTop.SetScoreText(SingletonComponent<LeaderboardManager>.Instance.m_player.Score.ToString());
			m_fakeColumnTop.SetNameText(SingletonComponent<LeaderboardManager>.Instance.m_player.CharaName);
			if (SingletonComponent<LeaderboardManager>.Instance.m_player.Rank > 8)
			{
				m_fakeColumnCamera.gameObject.SetActiveRecursively(true);
			}
			else
			{
				m_fakeColumnCamera.gameObject.SetActiveRecursively(false);
			}
		}
	}

	private void OnHelpBtnClick()
	{
		UIPopupController.Instance.ShowLbHelpPopup();
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_secondary_ui_tap");
	}

	private void OnRankPlayerCellClicked(GameObject sender)
	{
		LeaderboardCell component = sender.GetComponent<LeaderboardCell>();
		Dbg.Log("[Info] Click on : " + component.data.CharaName);
		m_tempPlayerData = SingletonComponent<SocialManager>.Instance.ServerCharaToPlayerData(component.data.CharaAccountData);
		m_tempPlayerRank = component.data.Rank;
		string text = SingletonComponent<NBLocalization>.Instance.Get("see_rank_player_details");
		if (text.Contains("{0") && text.Contains("{1"))
		{
			text = string.Format(text, component.data.CharaName, component.data.Rank);
		}
		UIPopupController.Instance.ShowSmallPopupMSG(text, OnCharacterSelected);
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_ok");
	}

	private void OnCharacterSelected()
	{
		if (m_tempPlayerData != null)
		{
			SingletonComponent<LeaderboardManager>.Instance.m_LeaderboardMenuScreenRef.ShowRankPlayerDetails(m_tempPlayerData, m_tempPlayerRank);
			m_tempPlayerData = null;
		}
	}

	public void SetShowGlobal(bool p_showGlobal)
	{
		m_showGlobal = p_showGlobal;
	}

	public void SetRemainTimeText(long p_sec)
	{
		m_remainTime = p_sec;
		if (m_remainTimeText != null)
		{
			TimeSpan timeSpan = new TimeSpan(0, 0, (int)m_remainTime);
			string text = SingletonComponent<NBLocalization>.Instance.Get("leaderboard_timer_day");
			string text2 = SingletonComponent<NBLocalization>.Instance.Get("leaderboard_timer_hour");
			string text3 = SingletonComponent<NBLocalization>.Instance.Get("leaderboard_timer_minute");
			m_remainTimeText.text = string.Format("{0}{1} {2}{3} {4}{5}", timeSpan.Days, text, timeSpan.Hours, text2, timeSpan.Minutes, text3);
		}
		else
		{
			Dbg.LogWarn("[Warn] The \"m_remainTimeText\" is null, please fix the link in the prefab.");
		}
	}

	private void ShowRewardsForGlobal()
	{
		m_Top1RewardAmount.text = GLOBAL.GAME_SETTINGS.global_lb_top1_reward_pgc.ToString();
		m_Top10RewardAmount.text = GLOBAL.GAME_SETTINGS.global_lb_top10_reward_pgc.ToString();
		m_Top100RewardAmount.text = GLOBAL.GAME_SETTINGS.global_lb_top100_reward_pgc.ToString();
		m_Top1000RewardAmount.text = GLOBAL.GAME_SETTINGS.global_lb_top1000_reward_social.ToString();
		m_Top1RewardIcon.spriteName = "icon_reward_diamond";
		m_Top10RewardIcon.spriteName = "icon_reward_diamond";
		m_Top100RewardIcon.spriteName = "icon_reward_diamond";
		m_Top1000RewardIcon.spriteName = "icon_reward_diamond";
	}

	private void ShowRewardsForSocial()
	{
	}

	private void SetHeaderPosBasedOnRes()
	{
		if (Screen.width % 1024 == 0)
		{
			m_headerObj.transform.localPosition = new Vector3(m_headerObj.transform.localPosition.x, 325.5f, m_headerObj.transform.localPosition.z);
		}
		else
		{
			m_headerObj.transform.localPosition = new Vector3(m_headerObj.transform.localPosition.x, 286f, m_headerObj.transform.localPosition.z);
		}
	}
}
