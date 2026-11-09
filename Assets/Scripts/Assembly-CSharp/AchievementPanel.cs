using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementPanel : NBUIMenuPanel
{
	public AchievementGrid grid;

	public UIDraggablePanel scrollView;

	[SerializeField]
	private GameObject mascotGameObj;

	private GameObject mascotGameObjRef;

	[SerializeField]
	private NBUILocalize subtitleText;

	[SerializeField]
	private UISprite gameCenterIcon;

	[SerializeField]
	private NBUILocalize completedText;

	[SerializeField]
	private UILabel completedNum;

	[SerializeField]
	private UISprite m_overlay;

	private Vector3 lastCellPosition = Vector3.zero;

	private void Start()
	{
	}

	private void OnEnable()
	{
		Messenger<AchievementManager.AchievementItem>.AddListener("achievement_update", OnAchievementUpdated);
		Init();
	}

	private void OnDisable()
	{
		Messenger<AchievementManager.AchievementItem>.RemoveListener("achievement_update", OnAchievementUpdated);
		Reset();
	}

	private void OnAchievementUpdated(AchievementManager.AchievementItem p_item)
	{
		RefreshBtnAndProgressInfo(p_item);
		AchievementManager instance = SingletonComponent<AchievementManager>.Instance;
		if (null != instance)
		{
			SetSubtitleInfo(instance.NumOfCompleted(), instance.TotalNumAchievements());
		}
	}

	private void Init()
	{
		UIPopupController.SetEnableLayer("UI", false);
		if (m_overlay != null)
		{
			m_overlay.gameObject.active = true;
			m_overlay.SendMessage("FadeIn", null, SendMessageOptions.DontRequireReceiver);
		}
		else
		{
			Dbg.LogWarn("[Warn] Unable to find \"Overlay\", please add the link to this prefab.");
		}
		StartCoroutine(ShowAchievements());
		ShowMascotSubtitle();
	}

	public override void Reset()
	{
		if (mascotGameObjRef != null)
		{
			Object.DestroyImmediate(mascotGameObjRef);
			mascotGameObjRef = null;
		}
		if (grid != null)
		{
			grid.Clear();
		}
		if (m_overlay != null)
		{
			m_overlay.SendMessage("FadeOut", null, SendMessageOptions.DontRequireReceiver);
		}
		else
		{
			Dbg.LogWarn("[Warn] Unable to find \"Overlay\", please add the link to this prefab.");
		}
		UIPopupController.SetEnableLayer("UI", true);
	}

	private IEnumerator ShowAchievements()
	{
		Dictionary<string, AchievementManager.AchievementItem> tempAchiItems = SingletonComponent<AchievementManager>.Instance.GetAchievements();
		int nCount = 0;
		bool bResetPosition = true;
		int numCellsToCreateFirst = 4;
		bool achieveSys1Added = false;
		bool achieveSys2Added = false;
		List<AchievementManager.AchievementItem> sortedList = CustomizedSortAchievement(tempAchiItems);
		foreach (AchievementManager.AchievementItem tempAchi in sortedList)
		{
			if (tempAchi.id == "achievement_system_1" && !achieveSys1Added)
			{
				if (!tempAchi.Achieved && !tempAchi.Claimed)
				{
					grid.AddData(tempAchi);
					AchievementCell tempAddedCell = grid.AddCell(0f, 0f);
					lastCellPosition = tempAddedCell.transform.localPosition;
					achieveSys1Added = true;
				}
			}
			else if (tempAchi.id == "achievement_system_2" && !achieveSys2Added && !tempAchi.Achieved && !tempAchi.Claimed)
			{
				grid.AddData(tempAchi);
				AchievementCell tempAddedCell2 = grid.AddCell(0f, 0f);
				lastCellPosition = tempAddedCell2.transform.localPosition;
				achieveSys2Added = true;
			}
		}
		foreach (AchievementManager.AchievementItem tempAchi2 in sortedList)
		{
			if (!tempAchi2.Achieved || tempAchi2.Claimed)
			{
				continue;
			}
			grid.AddData(tempAchi2);
			AchievementCell tempAddedCell3 = grid.AddCell(0f, 0f);
			lastCellPosition = tempAddedCell3.transform.localPosition;
			tempAddedCell3.m_ClaimBtnClickCb = ClaimBtnClickedCb;
			if (numCellsToCreateFirst <= nCount++)
			{
				if (bResetPosition)
				{
					scrollView.ResetPosition();
					bResetPosition = false;
				}
				yield return null;
				if (!base.enabled)
				{
					yield break;
				}
			}
		}
		foreach (AchievementManager.AchievementItem tempAchi3 in sortedList)
		{
			if (tempAchi3.Achieved || tempAchi3.Claimed || (tempAchi3.id == "achievement_system_1" && achieveSys1Added) || (tempAchi3.id == "achievement_system_2" && achieveSys2Added))
			{
				continue;
			}
			grid.AddData(tempAchi3);
			AchievementCell tempNotAchievedCell = grid.AddCell(0f, 0f);
			lastCellPosition = tempNotAchievedCell.transform.localPosition;
			if (numCellsToCreateFirst <= nCount++)
			{
				if (bResetPosition)
				{
					scrollView.ResetPosition();
					bResetPosition = false;
				}
				yield return null;
				if (!base.enabled)
				{
					yield break;
				}
			}
		}
		foreach (AchievementManager.AchievementItem tempAchi4 in sortedList)
		{
			if (!tempAchi4.Achieved || !tempAchi4.Claimed)
			{
				continue;
			}
			grid.AddData(tempAchi4);
			AchievementCell tempClaimedCell = grid.AddCell(0f, 0f);
			lastCellPosition = tempClaimedCell.transform.localPosition;
			if (numCellsToCreateFirst <= nCount++)
			{
				if (bResetPosition)
				{
					scrollView.ResetPosition();
					bResetPosition = false;
				}
				yield return null;
				if (!base.enabled)
				{
					break;
				}
			}
		}
	}

	private List<AchievementManager.AchievementItem> CustomizedSortAchievement(Dictionary<string, AchievementManager.AchievementItem> p_achiItems)
	{
		List<AchievementManager.AchievementItem> list = new List<AchievementManager.AchievementItem>();
		if (p_achiItems.ContainsKey("achievement_system_1"))
		{
			list.Add(p_achiItems["achievement_system_1"]);
		}
		if (p_achiItems.ContainsKey("achievement_system_2"))
		{
			list.Add(p_achiItems["achievement_system_2"]);
		}
		foreach (string key in p_achiItems.Keys)
		{
			if (key.Contains("achievement_city"))
			{
				list.Add(p_achiItems[key]);
			}
		}
		foreach (string key2 in p_achiItems.Keys)
		{
			if (!key2.Contains("achievement_city") && !key2.Contains("achievement_system_1") && !key2.Contains("achievement_system_2"))
			{
				list.Add(p_achiItems[key2]);
			}
		}
		return list;
	}

	private void ShowMascotSubtitle()
	{
		mascotGameObjRef = Object.Instantiate(mascotGameObj, Vector3.zero, Quaternion.identity) as GameObject;
		Transform transform = mascotGameObjRef.transform.Find("Mascot");
		if (transform == null)
		{
			Dbg.LogWarn("Cannot find \"Mascot\" tranform under game object: " + mascotGameObjRef.name);
		}
	}

	public void SetSubtitleInfo(int p_completed, int p_total)
	{
		if (completedNum != null && completedText != null)
		{
			completedText.Key = "achievement_subtitle_completed";
			completedNum.text = p_completed + "/" + p_total;
		}
		if (subtitleText != null && gameCenterIcon != null)
		{
			subtitleText.Key = "achievement_subtitle_not_ios";
			subtitleText.enabled = false;
			gameCenterIcon.enabled = false;
		}
	}

	public void RefreshBtnAndProgressInfo(AchievementManager.AchievementItem p_item)
	{
		foreach (AchievementCell cell in grid.cells)
		{
			if (cell.data.id == p_item.id)
			{
				cell.data = p_item;
				cell.SetupClaimBtnStatus();
				cell.SetupProgressStatus();
			}
		}
	}

	public void OnBackBtnClick()
	{
		if (MainMenu.Instance != null && MainMenu.Instance.currScreen != null)
		{
			Dbg.Log("[Info] Try to update achievement num in the option menu main panel");
			MainMenu.Instance.currScreen.UpdateRedIconNum();
		}
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_cancel");
		base.gameObject.SetActiveRecursively(false);
	}

	public void ClaimBtnClickedCb(AchievementCell p_cell)
	{
		grid.RemoveData(p_cell.data);
		grid.Remove(p_cell);
		Dictionary<string, AchievementManager.AchievementItem> achievements = SingletonComponent<AchievementManager>.Instance.GetAchievements();
		if (achievements.ContainsKey(p_cell.data.id))
		{
			grid.AddData(achievements[p_cell.data.id]);
			grid.AddCell(0f, 0f);
		}
		scrollView.ResetPosition();
	}
}
