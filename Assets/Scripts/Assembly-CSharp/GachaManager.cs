using System.Collections.Generic;
using Models;
using ServiceCalls;
using UnityEngine;

public class GachaManager : SingletonComponent<GachaManager>
{
	public enum GachaID
	{
		Unknown = 0,
		Social = 1,
		SocialMulti = 2,
		PremiumSingle = 6,
		PremiumMulti = 10,
		ElementSingle = 5,
		ElementMulti = 9,
		LuckyGacha = 11,
		SetMulti = 13,
		FourStarItem = 4
	}

	public enum GachaStatus
	{
		Unknown = 0,
		Pending = 1,
		Active = 2,
		Expiring = 3,
		Expired = 4,
		Cancelled = 5,
		Stopped = 6
	}

	public delegate void GachaCallback(Services.ServiceHandle p_handle);

	private int m_lastHolyDiamondsRewards;

	private GachaCallback m_successCb;

	private GachaCallback m_failCb;

	private uint m_requestId;

	private GachaEventsInfo m_previousGachaEvent;

	private Dictionary<int, GachaEventsInfo> m_gachaEvents = new Dictionary<int, GachaEventsInfo>();

	private List<ServerInventoryItem> m_lastResult = new List<ServerInventoryItem>();

	private ItemDetails m_lastItemResult;

	public int PreviousGachaPrice
	{
		get
		{
			return GachaPrice(m_previousGachaEvent);
		}
	}

	public int SocialGachaPrice
	{
		get
		{
			return GachaPrice(GetEventById(1));
		}
	}

	public int MultiSocialGachaPrice
	{
		get
		{
			return GachaPrice(GetEventById(2));
		}
	}

	public int SinglePremiumPrice
	{
		get
		{
			return GachaPrice(GetEventById(6));
		}
	}

	public int MultiPremiumPrice
	{
		get
		{
			return GachaPrice(GetEventById(10));
		}
	}

	public int SingleElementPrice
	{
		get
		{
			return GachaPrice(GetEventById(5));
		}
	}

	public int MultiElementPrice
	{
		get
		{
			return GachaPrice(GetEventById(9));
		}
	}

	public int SingleLuckyPrice
	{
		get
		{
			return GachaPrice(GetEventById(11));
		}
	}

	public int MultiSetPrice
	{
		get
		{
			return GachaPrice(GetEventById(13));
		}
	}

	public bool IsFromPromotion { get; set; }

	public int lastHolyDiamondsRewards
	{
		get
		{
			int result = m_lastHolyDiamondsRewards;
			m_lastHolyDiamondsRewards = 0;
			return result;
		}
		private set
		{
			m_lastHolyDiamondsRewards = value;
		}
	}

	public static bool IsValidGachaID(int p_gachaID)
	{
		return p_gachaID == 1 || p_gachaID == 6 || p_gachaID == 13 || p_gachaID == 10 || p_gachaID == 4 || p_gachaID == 11;
	}

	public void PlayElementSingleGacha(GachaCallback p_success, GachaCallback p_fail, bool p_isTutorial, bool p_isRetry)
	{
		PlayGachaEvent(GetEventById(5), p_success, p_fail, p_isTutorial, p_isRetry);
	}

	public void PlayElementMultiGacha(GachaCallback p_success, GachaCallback p_fail, bool p_isTutorial, bool p_isRetry)
	{
		PlayGachaEvent(GetEventById(9), p_success, p_fail, p_isTutorial, p_isRetry);
	}

	public void PlayPremiumSingleGacha(GachaCallback p_success, GachaCallback p_fail, bool p_isTutorial, bool p_isRetry)
	{
		PlayGachaEvent(GetEventById(6), p_success, p_fail, p_isTutorial, p_isRetry);
	}

	public void PlayPremiumMultiGacha(GachaCallback p_success, GachaCallback p_fail, bool p_isTutorial, bool p_isRetry)
	{
		PlayGachaEvent(GetEventById(10), p_success, p_fail, p_isTutorial, p_isRetry);
	}

	public void PlaySocialGacha(GachaCallback p_success, GachaCallback p_fail, bool p_isTutorial, bool p_isRetry)
	{
		PlayGachaEvent(GetEventById(1), p_success, p_fail, p_isTutorial, p_isRetry);
	}

	public void PlaySocialMultiGacha(GachaCallback p_success, GachaCallback p_fail, bool p_isTutorial, bool p_isRetry)
	{
		PlayGachaEvent(GetEventById(2), p_success, p_fail, p_isTutorial, p_isRetry);
	}

	public void PlayLuckyGacha(GachaCallback p_success, GachaCallback p_fail, bool p_isTutorial, bool p_isRetry)
	{
		PlayGachaEvent(GetEventById(11), p_success, p_fail, p_isTutorial, p_isRetry);
	}

	public void PlaySetMultiGacha(GachaCallback p_success, GachaCallback p_fail, bool p_isTutorial, bool p_isRetry)
	{
		PlayGachaEvent(GetEventById(13), p_success, p_fail, p_isTutorial, p_isRetry);
	}

	private int GachaPrice(GachaEventsInfo p_gachaEvent)
	{
		if (p_gachaEvent != null)
		{
			int num = (int)p_gachaEvent.price;
			if (num < 0)
			{
				Dbg.LogError("The Gacha price is negative!!");
			}
			return num;
		}
		Dbg.LogError("No price was found for p_gachaEvent. Return -1 price");
		return -1;
	}

	private void PlayGachaEvent(GachaEventsInfo p_gachaEvent, GachaCallback p_success, GachaCallback p_fail, bool p_isTutorial, bool p_isRetry)
	{
		if (p_gachaEvent == null)
		{
			Dbg.LogError("Gacha Event is not recognize / available. Call the fail callback if any.");
			if (p_fail != null)
			{
				p_fail(null);
			}
			return;
		}
		m_successCb = p_success;
		m_failCb = p_fail;
		if (!p_isRetry)
		{
			m_requestId = (uint)(Random.value * 4.2949673E+09f);
		}
		switch (int.Parse(p_gachaEvent.id))
		{
		case 1:
		case 5:
		case 6:
		case 11:
			SingletonComponent<Services>.Instance.PlayGacha(SingletonComponent<Services>.Instance.NubeeId, SingletonComponent<Services>.Instance.DeviceId, p_gachaEvent.id, m_requestId.ToString(), false, OnGachaSuccess, OnGachaFail, p_isTutorial);
			break;
		case 2:
		case 9:
		case 10:
		case 13:
			SingletonComponent<Services>.Instance.PlayMultiGacha(SingletonComponent<Services>.Instance.NubeeId, SingletonComponent<Services>.Instance.DeviceId, p_gachaEvent.id, m_requestId.ToString(), OnGachaSuccess, OnGachaFail);
			break;
		}
		m_previousGachaEvent = p_gachaEvent;
	}

	public List<ServerInventoryItem> GetLastGachaResult()
	{
		return m_lastResult;
	}

	public ItemDetails GetLastItemResult()
	{
		return m_lastItemResult;
	}

	public void RefreshGachaEvents(GachaCallback p_success, GachaCallback p_fail)
	{
		m_successCb = p_success;
		m_failCb = p_fail;
		SingletonComponent<Services>.Instance.GetAllGachaEvents(OnRefreshGachaEventSuccess, OnRefreshGachaEventFail);
	}

	public void InitGachaEvents(List<GachaEventsInfo> _gachaEvents)
	{
		if (_gachaEvents == null)
		{
			Dbg.LogError("Gacha events could not be identified. The gacha Data might be outdated.");
			return;
		}
		m_gachaEvents.Clear();
		foreach (GachaEventsInfo _gachaEvent in _gachaEvents)
		{
			int p_outResult = 0;
			int p_outResult2 = 0;
			NBGeneric.TryParse<int>(_gachaEvent.gacha_status_id, out p_outResult);
			NBGeneric.TryParse<int>(_gachaEvent.id, out p_outResult2);
			if (p_outResult == 2)
			{
				m_gachaEvents.Add(p_outResult2, _gachaEvent);
			}
		}
	}

	private void OnRefreshGachaEventSuccess(Services.ServiceHandle p_handle)
	{
		InitGachaEvents((List<GachaEventsInfo>)p_handle.returnValue);
		foreach (GachaEventsInfo value in m_gachaEvents.Values)
		{
			if (value == null || value.price < 0f)
			{
				Dbg.LogError("One of the gacha event has incorrect information!!");
				if (m_failCb != null)
				{
					m_failCb(p_handle);
					m_failCb = null;
				}
				return;
			}
		}
		if (m_successCb != null)
		{
			m_successCb(p_handle);
			m_successCb = null;
		}
	}

	private void OnRefreshGachaEventFail(Services.ServiceHandle p_handle, string p_error)
	{
		Dbg.LogError("OnRefreshGachaEventFail: " + ServiceCallUtils.GetErrorMessage(p_handle));
		if (m_failCb != null)
		{
			m_failCb(p_handle);
			m_failCb = null;
		}
	}

	private void OnGachaSuccess(Services.ServiceHandle p_handle)
	{
		Models.GachaResult gachaResult = (Models.GachaResult)p_handle.returnValue;
		PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		if (gachaResult.characterDetails != null)
		{
			m_lastHolyDiamondsRewards += gachaResult.characterDetails.currencies.holyDiamonds - playerData.details.currencies.holyDiamonds;
			SingletonComponent<PlayerDataManager>.Instance.SetCurrency(gachaResult.characterDetails.currencies);
			SingletonComponent<PlayerDataManager>.Instance.SetPlayerData(gachaResult.characterDetails);
		}
		m_lastResult = gachaResult.items;
		bool flag = true;
		foreach (ServerInventoryItem item in m_lastResult)
		{
			ItemDetails itemDetails = SingletonComponent<InventoryManager>.Instance.AddItemFromServer(item);
			flag &= itemDetails != ItemManager.s_NULL_DATA;
			m_lastItemResult = itemDetails;
		}
		if (flag)
		{
			if (m_successCb != null)
			{
				m_successCb(p_handle);
			}
		}
		else if (m_failCb != null)
		{
			m_failCb(p_handle);
		}
	}

	private void OnGachaFail(Services.ServiceHandle p_handle, string p_error)
	{
		Dbg.LogWarn("Multiple gacha failed: " + ServiceCallUtils.GetErrorMessage(p_handle));
		if (p_handle != null && p_handle.returnJson != null)
		{
			JSONObject field = p_handle.returnJson.GetField("payload");
			if (field != null)
			{
				JSONObject field2 = field.GetField("character");
				if (field2 != null)
				{
					CharacterAccountDetails p_character = new CharacterAccountDetails();
					ServiceCallUtils.SetCharacterFromPayload(field2, ref p_character);
					SingletonComponent<PlayerDataManager>.Instance.SetPlayerData(p_character);
				}
			}
		}
		if (m_failCb != null)
		{
			m_failCb(p_handle);
		}
	}

	public GachaEventsInfo GetEventById(int p_eventId)
	{
		if (m_gachaEvents == null || !m_gachaEvents.ContainsKey(p_eventId))
		{
			return null;
		}
		return m_gachaEvents[p_eventId];
	}

	public string GetCurrentElementName()
	{
		GachaEventsInfo eventById = GetEventById(5);
		if (eventById != null)
		{
			return eventById.element;
		}
		return "none";
	}

	public GachaEventsInfo GetLastGachaEvent()
	{
		return m_previousGachaEvent;
	}
}
