using System.Collections.Generic;
using Models;
using UnityEngine;

public class DailyBonusPopup : MonoBehaviour
{
	[SerializeField]
	private float gridLeft = -332f;

	[SerializeField]
	private float gridRight = 332f;

	[SerializeField]
	private float gridTop = 128f;

	[SerializeField]
	private float gridBottom = -145f;

	[SerializeField]
	private int numberInColumn = 7;

	[SerializeField]
	private int numberInRow = 4;

	public UIAtlas m_TreasureAtlas;

	public DailyBonusCell BonusCellTemplate;

	public Dictionary<int, DailyBonusCell> BonusCells = new Dictionary<int, DailyBonusCell>();

	[SerializeField]
	private UISprite m_overlay;

	private DailyBonusCell todayCell;

	private void Awake()
	{
	}

	private void Start()
	{
		if (SingletonComponent<DailyManager>.Instance.DailyBonuses == null)
		{
			return;
		}
		int num = 0;
		foreach (DailyBonusData dailyBonuse in SingletonComponent<DailyManager>.Instance.DailyBonuses)
		{
			GameObject gameObject = Object.Instantiate(BonusCellTemplate.gameObject, Vector3.zero, Quaternion.identity) as GameObject;
			gameObject.transform.parent = base.transform;
			DailyBonusCell component = gameObject.GetComponent<DailyBonusCell>();
			if (component != null)
			{
				component.m_DailyBonusWinRef = this;
				component.transform.localPosition = CalculatePosByIndex(num);
				component.transform.localScale = Vector3.one;
				component.SetDailyBonusData(dailyBonuse);
				BonusCells.Add(dailyBonuse.Sequence, component);
			}
			num++;
		}
		foreach (int key in BonusCells.Keys)
		{
			if (key < SingletonComponent<DailyManager>.Instance.TodayIndex && !BonusCells[key].m_data.Marked)
			{
				BonusCells[key].SetBrightness(0.3f);
			}
		}
		UIPopupController.SetEnableLayer("UI", false);
		if (m_overlay == null)
		{
			Transform transform = base.gameObject.transform.parent.Find("Inventory Overlay");
			if (transform != null)
			{
				m_overlay = transform.gameObject.GetComponent<UISprite>();
			}
		}
		if (m_overlay != null)
		{
			m_overlay.gameObject.active = true;
			m_overlay.SendMessage("FadeIn", null, SendMessageOptions.DontRequireReceiver);
		}
		else
		{
			Dbg.LogWarn("[Warn] Unable to find \"Overlay\", please add the link to this prefab.");
		}
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_notification");
	}

	private Vector3 CalculatePosByIndex(int p_index)
	{
		int num = p_index % numberInColumn;
		int num2 = p_index / numberInColumn;
		return new Vector3(gridLeft + (gridRight - gridLeft) / (float)(numberInColumn - 1) * (float)num, gridTop + (gridBottom - gridTop) / (float)(numberInRow - 1) * (float)num2, -1f);
	}

	public void OnDailyBonusPopupClose()
	{
		int num = SingletonComponent<DailyManager>.Instance.TodayIndex;
		if (num <= 0 || !BonusCells.ContainsKey(num))
		{
			num = 1;
		}
		todayCell = BonusCells[num];
		if (!todayCell.IsAnimatingStamp)
		{
			StartCoroutine(todayCell.AnimStamp(FinishStampAnim));
		}
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_ok");
	}

	private void FinishStampAnim()
	{
		UIPopupController.Instance.ShowDailyBonusClaim(todayCell.m_data, todayCell.m_DailyBonusAtlas, m_TreasureAtlas, ClosePopup);
	}

	private void ClosePopup()
	{
		if (m_overlay != null)
		{
			m_overlay.SendMessage("FadeOut", null, SendMessageOptions.DontRequireReceiver);
			m_overlay = null;
		}
		else
		{
			Dbg.LogWarn("[Warn] Unable to find \"Overlay\", please add the link to this prefab.");
		}
		UIPopupController.SetEnableLayer("UI", true);
		MainMenu.Instance.ShowPromotionPopup();
		Object.DestroyImmediate(base.gameObject);
	}
}
