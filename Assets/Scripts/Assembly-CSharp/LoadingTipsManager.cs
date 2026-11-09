using System.Collections.Generic;
using UnityEngine;

public class LoadingTipsManager : SingletonComponent<LoadingTipsManager>
{
	private class TipsData
	{
		public string category = string.Empty;

		public string prefabName = string.Empty;

		public string locKeyHeader = string.Empty;

		public string locKeyDesc = string.Empty;

		public int startingLevel = 1;

		public TipsData(string p_cat, string p_prefab, string p_locKeyHeader, string p_locKeyDesc, int p_startingLevel)
		{
			category = p_cat;
			prefabName = p_prefab;
			locKeyDesc = p_locKeyDesc;
			locKeyHeader = p_locKeyHeader;
			startingLevel = p_startingLevel;
		}
	}

	private const string TIPS_PATH = "UI/Prefabs/POPUPS/LOADING/";

	private const string TIPS_COL_ID = "TIP_ID";

	private const string TIPS_COL_CAT = "TIP_TYPE";

	private const string TIPS_COL_START_LEVEL = "STARTING_LEVEL";

	private const string TIPS_COL_PREFAB_NAME = "PREFAB_NAME";

	private const string TIPS_COL_LOC_KEY_HEADER = "LOCALIZATION_KEY_HEADER";

	private const string TIPS_COL_LOC_KEY_DESC = "LOCALIZATION_KEY_DESC";

	private const string TIPS_DEFAULT = "GENERIC_TIP";

	private string TIPS_KEY = "Tips";

	private string m_nextTipCategory = string.Empty;

	private List<TipsData> m_tipsList;

	public string NextTipCategory
	{
		get
		{
			return m_nextTipCategory;
		}
		set
		{
			m_nextTipCategory = value;
		}
	}

	private TipsData GetRandomTip(string p_tipCategory)
	{
		if (string.IsNullOrEmpty(p_tipCategory))
		{
			p_tipCategory = "GENERIC_TIP";
		}
		PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		int num = 1;
		if (playerData != null)
		{
			num = playerData.Level;
		}
		List<TipsData> list = new List<TipsData>();
		foreach (TipsData tips in m_tipsList)
		{
			if (p_tipCategory == tips.category && num >= tips.startingLevel)
			{
				list.Add(tips);
			}
		}
		if (list.Count < 1)
		{
			if (p_tipCategory != "GENERIC_TIP")
			{
				return GetRandomTip("GENERIC_TIP");
			}
			return null;
		}
		if (list.Count == 1)
		{
			return list[0];
		}
		int index = Random.Range(0, list.Count);
		return list[index];
	}

	public GameObject GetTipGameObject()
	{
		GameObject gameObject = null;
		TipsData randomTip = GetRandomTip(m_nextTipCategory);
		if (randomTip != null)
		{
			GameObject gameObject2 = Resources.Load("UI/Prefabs/POPUPS/LOADING/" + randomTip.prefabName) as GameObject;
			if (gameObject2 != null)
			{
				gameObject = Object.Instantiate(gameObject2) as GameObject;
				if (gameObject != null)
				{
					Transform transform = gameObject.transform.Find("LabelTipsHeader");
					if ((bool)transform)
					{
						NBUILocalize component = transform.GetComponent<NBUILocalize>();
						if ((bool)component)
						{
							component.key = randomTip.locKeyHeader;
						}
					}
					Transform transform2 = gameObject.transform.Find("LabelTipsDescription");
					if ((bool)transform2)
					{
						NBUILocalize component2 = transform2.GetComponent<NBUILocalize>();
						if ((bool)component2)
						{
							component2.key = randomTip.locKeyDesc;
						}
					}
				}
			}
		}
		m_nextTipCategory = string.Empty;
		return gameObject;
	}

	public void Init()
	{
		if (m_tipsList == null)
		{
			m_tipsList = new List<TipsData>();
		}
		m_tipsList.Clear();
		ParameterManager instance = SingletonComponent<ParameterManager>.Instance;
		if (instance == null)
		{
			Dbg.LogError("Parameter Manager missing");
			return;
		}
		foreach (string item in instance.GetRowsNamesFor(TIPS_KEY))
		{
			string parameter = instance.GetParameter(TIPS_KEY, item, "TIP_TYPE");
			string parameter2 = instance.GetParameter(TIPS_KEY, item, "PREFAB_NAME");
			string parameter3 = instance.GetParameter(TIPS_KEY, item, "LOCALIZATION_KEY_HEADER");
			string parameter4 = instance.GetParameter(TIPS_KEY, item, "LOCALIZATION_KEY_DESC");
			int p_startingLevel = int.Parse(instance.GetParameter(TIPS_KEY, item, "STARTING_LEVEL"));
			m_tipsList.Add(new TipsData(parameter, parameter2, parameter3, parameter4, p_startingLevel));
		}
	}
}
