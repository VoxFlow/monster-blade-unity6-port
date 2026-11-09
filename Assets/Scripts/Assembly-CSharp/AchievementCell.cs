using System;
using UnityEngine;

public class AchievementCell : NBUIGridCell<AchievementManager.AchievementItem>
{
	public delegate void AchievementClaimBtnClickCb(AchievementCell p_cell);

	[SerializeField]
	private InventoryCell[] m_invCells;

	[SerializeField]
	private UISprite m_igcIcon;

	[SerializeField]
	private UISprite m_pgcIcon;

	[SerializeField]
	private UISprite m_xpIcon;

	[SerializeField]
	private UILabel[] m_rewardItemAmountTexts;

	[SerializeField]
	private UISprite m_medalIcon;

	[SerializeField]
	private UILabel m_medalNum;

	[SerializeField]
	private UISlider m_progressBar;

	[SerializeField]
	private UILabel m_progressText;

	[SerializeField]
	private UILabel m_achiName;

	[SerializeField]
	private UILabel m_achiDesc;

	[SerializeField]
	private Color m_enableToClaimTextColor = new Color(1f, 0.94509804f, 49f / 85f, 1f);

	private AchievementClaimBtn m_ClaimBtn;

	public AchievementClaimBtnClickCb m_ClaimBtnClickCb;

	public void OnEnable()
	{
	}

	public void OnDisable()
	{
	}

	protected override void Awake()
	{
		base.Awake();
		UnityEngine.Object.DestroyImmediate(GetComponent<UIPanel>());
		GameObject gameObject = base.transform.Find("ClaimBtn").gameObject;
		if (gameObject != null)
		{
			m_ClaimBtn = gameObject.GetComponent<AchievementClaimBtn>();
		}
	}

	protected override void Start()
	{
		base.Start();
		ShowAchievementInfo();
		DefineClaimBtn();
	}

	public void Update()
	{
	}

	public void OnClaimButtonClick()
	{
		SingletonComponent<AchievementManager>.Instance.m_OldPlayerData = SingletonComponent<PlayerDataManager>.Instance.GetCopyOfPlayerData();
		if (SingletonComponent<AchievementManager>.Instance.m_OldPlayerData != null)
		{
			SingletonComponent<AchievementManager>.Instance.m_OldPlayerData.ComputeFinalValues();
		}
		SingletonComponent<AchievementManager>.Instance.ClaimAchievement(data, OnClaimSuccess);
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_ok");
		Dbg.Log("[Info] OnClaimButtonClick");
	}

	private void OnClaimSuccess()
	{
		m_ClaimBtn.SetEnable(false);
		m_ClaimBtn.SetBtnText(SingletonComponent<NBLocalization>.Instance.Get("achievement_claimed"));
		if (m_ClaimBtnClickCb != null)
		{
			m_ClaimBtnClickCb(this);
		}
	}

	private void OnFbLikeUsClick()
	{
		SingletonComponent<SaveData>.Instance.GameData.FbLikedUs = true;
		Application.OpenURL("https://www.facebook.com/pages/Monster-Blade/119059501574462");
	}

	private void OnRateUsClick()
	{
		SingletonComponent<SaveData>.Instance.GameData.AppStoreRateUs = true;
		Application.OpenURL(GLOBAL.APP_STORE_LINK);
	}

	private void DefineClaimBtn()
	{
		if (SingletonComponent<AchievementManager>.Instance != null && !data.Achieved && SingletonComponent<AchievementManager>.Instance.m_ActionMap.ContainsKey(data.id))
		{
			m_ClaimBtn.SetEnable(true);
			m_ClaimBtn.SetBtnMessageFunc(SingletonComponent<AchievementManager>.Instance.m_ActionMap[data.id].m_Action);
			m_ClaimBtn.SetBtnText(SingletonComponent<NBLocalization>.Instance.Get(SingletonComponent<AchievementManager>.Instance.m_ActionMap[data.id].m_Text));
			m_achiDesc.color = Color.white;
		}
	}

	public void SetupClaimBtnStatus()
	{
		if (!(m_ClaimBtn != null))
		{
			return;
		}
		if (data.Achieved)
		{
			if (data.Claimed)
			{
				m_ClaimBtn.SetBtnText(SingletonComponent<NBLocalization>.Instance.Get("achievement_claimed"));
				m_ClaimBtn.SetEnable(false);
				m_ClaimBtn.SetBtnBackgroundTexEnable(false);
			}
			else
			{
				m_ClaimBtn.SetEnable(true);
				m_ClaimBtn.SetBtnText(SingletonComponent<NBLocalization>.Instance.Get("achievement_claim"));
				m_ClaimBtn.SetFunctionToClaimBtn();
				if (m_ClaimBtnClickCb == null)
				{
					m_ClaimBtnClickCb = MainMenu.Instance.m_AchievementPanel.ClaimBtnClickedCb;
				}
			}
			m_achiName.color = m_enableToClaimTextColor;
			m_achiDesc.color = Color.white;
			m_medalIcon.spriteName = "icon/icon_shield";
		}
		else
		{
			m_ClaimBtn.SetEnable(false);
			m_ClaimBtn.SetBtnText(SingletonComponent<NBLocalization>.Instance.Get("achievement_claim"));
			m_achiName.color = new Color(0.75f, 0.75f, 0.75f, 1f);
			m_achiDesc.color = Color.gray;
			m_medalIcon.spriteName = "icon/icon_shield_locked";
		}
	}

	public void SetupProgressStatus()
	{
		if ("achievement_event_2" == data.id)
		{
			HandleWeekendEventDisplayLogic();
		}
		if ("achievement_event_3" == data.id)
		{
			HandleWeekdayWeeklyMonthlyInOneWeekDisplayLogic();
		}
		if (data.targetProgress > 0)
		{
			m_progressText.text = data.progress + "/" + data.targetProgress;
			m_progressBar.sliderValue = (float)data.progress / (float)data.targetProgress;
		}
		else
		{
			Dbg.LogWarn("[Warn] The target progress is zero");
			m_progressBar.sliderValue = 0f;
			m_progressText.text = string.Empty;
		}
	}

	public void ShowAchievementInfo()
	{
		m_igcIcon.gameObject.SetActiveRecursively(false);
		m_pgcIcon.gameObject.SetActiveRecursively(false);
		m_xpIcon.gameObject.SetActiveRecursively(false);
		UILabel[] rewardItemAmountTexts = m_rewardItemAmountTexts;
		foreach (UILabel uILabel in rewardItemAmountTexts)
		{
			uILabel.text = string.Empty;
		}
		SetupClaimBtnStatus();
		m_achiName.text = SingletonComponent<NBLocalization>.Instance.Get(data.title);
		m_achiDesc.text = SingletonComponent<NBLocalization>.Instance.Get(data.description);
		if (m_achiDesc.relativeSize.y > 1.8f)
		{
			m_achiDesc.transform.localScale = new Vector3(20f, 20f, 1f);
			m_achiDesc.text = SingletonComponent<NBLocalization>.Instance.Get(data.description);
		}
		SetupProgressStatus();
		int num = 0;
		InventoryCell[] invCells = m_invCells;
		foreach (InventoryCell inventoryCell in invCells)
		{
			if (inventoryCell.transform.parent == base.transform)
			{
				inventoryCell.gameObject.SetActiveRecursively(false);
			}
		}
		foreach (string rewardItemId in data.rewards.rewardItemIds)
		{
			if (!string.IsNullOrEmpty(rewardItemId))
			{
				ItemDetails itemDetails = SingletonComponent<InventoryManager>.Instance.CreateItem(rewardItemId);
				InventoryCell inventoryCell2 = m_invCells[num];
				if (inventoryCell2.transform.parent != base.transform)
				{
					GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(m_invCells[num].gameObject);
					gameObject.transform.parent = base.transform;
					gameObject.transform.localRotation = Quaternion.identity;
					gameObject.transform.localScale = Vector3.one;
					inventoryCell2 = gameObject.GetComponent<InventoryCell>();
					m_invCells[num] = inventoryCell2;
				}
				inventoryCell2.data = itemDetails;
				m_rewardItemAmountTexts[num].gameObject.SetActiveRecursively(true);
				m_rewardItemAmountTexts[num].text = "1";
				inventoryCell2.transform.localPosition = DefineIconPosition(num);
				num++;
			}
			if (num >= 2)
			{
				return;
			}
		}
		if (data.rewards.rewardPrem > 0)
		{
			m_pgcIcon.gameObject.SetActiveRecursively(true);
			m_rewardItemAmountTexts[num].gameObject.SetActiveRecursively(true);
			if (data.rewards.rewardPrem > 0)
			{
				m_rewardItemAmountTexts[num].text = data.rewards.rewardPrem.ToString();
			}
			else
			{
				m_rewardItemAmountTexts[num].text = string.Empty;
			}
			m_rewardItemAmountTexts[num].transform.localPosition = new Vector3(-85f, m_rewardItemAmountTexts[num].transform.localPosition.y, m_rewardItemAmountTexts[num].transform.localPosition.z);
			m_pgcIcon.transform.localPosition = DefineIconPosition(num);
			num++;
			if (num >= 2)
			{
				return;
			}
		}
		if (data.rewards.rewardIgc > 0)
		{
			m_igcIcon.gameObject.SetActiveRecursively(true);
			m_rewardItemAmountTexts[num].gameObject.SetActiveRecursively(true);
			if (data.rewards.rewardIgc > 0)
			{
				m_rewardItemAmountTexts[num].text = data.rewards.rewardIgc.ToString();
			}
			else
			{
				m_rewardItemAmountTexts[num].text = string.Empty;
			}
			m_rewardItemAmountTexts[num].transform.localPosition = new Vector3(-85f, m_rewardItemAmountTexts[num].transform.localPosition.y, m_rewardItemAmountTexts[num].transform.localPosition.z);
			m_igcIcon.transform.localPosition = DefineIconPosition(num);
			num++;
			if (num >= 2)
			{
				return;
			}
		}
		if (data.rewards.rewardXP > 0)
		{
			m_xpIcon.gameObject.SetActiveRecursively(true);
			m_rewardItemAmountTexts[num].gameObject.SetActiveRecursively(true);
			if (data.rewards.rewardXP > 0)
			{
				m_rewardItemAmountTexts[num].text = data.rewards.rewardXP.ToString();
			}
			else
			{
				m_rewardItemAmountTexts[num].text = string.Empty;
			}
			m_xpIcon.transform.localPosition = DefineIconPosition(num);
			num++;
			if (num < 2)
			{
			}
		}
	}

	private Vector3 DefineIconPosition(int p_index)
	{
		if (p_index == 0)
		{
			return new Vector3(-105f, -30f, -1f);
		}
		return new Vector3(-5f, -30f, -1f);
	}

	private void HandleWeekendEventDisplayLogic()
	{
		if (data.Achieved)
		{
			return;
		}
		bool flag = false;
		long unixEpochTime = TimeUtil.GetUnixEpochTime();
		switch (TimeUtil.GetDayOfTheWeek(unixEpochTime))
		{
		case DayOfWeek.Monday:
		case DayOfWeek.Tuesday:
		case DayOfWeek.Wednesday:
		case DayOfWeek.Thursday:
			flag = true;
			break;
		case DayOfWeek.Sunday:
		case DayOfWeek.Friday:
		case DayOfWeek.Saturday:
			if (SingletonComponent<SaveData>.Instance.GameData.LatestWeekFriday > 0 && unixEpochTime - SingletonComponent<SaveData>.Instance.GameData.LatestWeekFriday > 518400)
			{
				flag = true;
			}
			if (SingletonComponent<SaveData>.Instance.GameData.LatestWeekSaturday > 0 && unixEpochTime - SingletonComponent<SaveData>.Instance.GameData.LatestWeekSaturday > 432000)
			{
				flag = true;
			}
			if (SingletonComponent<SaveData>.Instance.GameData.LatestWeekSunday > 0 && unixEpochTime - SingletonComponent<SaveData>.Instance.GameData.LatestWeekSunday > 345600)
			{
				flag = true;
			}
			break;
		}
		if (flag)
		{
			if (SingletonComponent<AchievementManager>.Instance != null && data.progress > 0)
			{
				SingletonComponent<AchievementManager>.Instance.ClearWeekendEventProgress();
			}
			data.progress = 0;
		}
	}

	private void HandleWeekdayWeeklyMonthlyInOneWeekDisplayLogic()
	{
		if (!data.Achieved)
		{
			long unixEpochTime = TimeUtil.GetUnixEpochTime();
			if (SingletonComponent<SaveData>.Instance.GameData.LatestWeekdayEvent > 0 && unixEpochTime - SingletonComponent<SaveData>.Instance.GameData.LatestWeekdayEvent > 604800)
			{
				data.progress = 0;
			}
			if (SingletonComponent<SaveData>.Instance.GameData.LatestWeeklyEvent > 0 && unixEpochTime - SingletonComponent<SaveData>.Instance.GameData.LatestWeeklyEvent > 604800)
			{
				data.progress = 0;
			}
			if (SingletonComponent<SaveData>.Instance.GameData.LatestMonthlyEvent > 0 && unixEpochTime - SingletonComponent<SaveData>.Instance.GameData.LatestMonthlyEvent > 604800)
			{
				data.progress = 0;
			}
		}
	}
}
