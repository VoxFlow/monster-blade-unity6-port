using System.Collections.Generic;
using UnityEngine;

public class PetEventAreaSelectPanel : NBUIMenuPanel
{
	public DungeonAreaGrid areaList;

	public UIButton buttonEvent;

	public Component eventTarget;

	private DungeonAreaCell _dummyCellLeft;

	private DungeonAreaCell _dummyCellRight;

	private bool _started;

	private void OnEnable()
	{
		Init();
	}

	private void OnDisable()
	{
		Reset();
	}

	private void Start()
	{
		if (!SingletonComponent<DungeonManager>.Instance)
		{
			return;
		}
		List<DungeonManager.CityStruct> allUnlockedCities = SingletonComponent<DungeonManager>.Instance.GetAllUnlockedCities();
		if (allUnlockedCities.Count <= 0)
		{
			return;
		}
		DungeonManager.CityStruct cityStruct = new DungeonManager.CityStruct();
		cityStruct.city = new CityDbStruct();
		areaList.AddData(cityStruct);
		_dummyCellLeft = areaList.AddCell(0f, 0f);
		_dummyCellLeft.hidden = true;
		foreach (DungeonManager.CityStruct item in allUnlockedCities)
		{
			areaList.AddData(item);
			DungeonAreaCell dungeonAreaCell = areaList.AddCell(0f, 0f);
			dungeonAreaCell.eventTarget = parentScreen;
		}
		areaList.AddData(cityStruct);
		_dummyCellRight = areaList.AddCell(0f, 0f);
		_dummyCellRight.hidden = true;
		_started = true;
		Init();
	}

	public override void Reset()
	{
	}

	private void Init()
	{
		if (!_started || SingletonComponent<DungeonManager>.Instance == null || areaList.cells.Count <= 0)
		{
			return;
		}
		CityDbStruct currentCity = SingletonComponent<DungeonManager>.Instance.CurrentCity;
		if (currentCity == null)
		{
			return;
		}
		DungeonAreaCell dungeonAreaCell = areaList.cells[0];
		foreach (DungeonAreaCell cell in areaList.cells)
		{
			if (cell.data == null || cell.data.city == null || !(cell.data.city.id == currentCity.id))
			{
				continue;
			}
			dungeonAreaCell = cell;
			break;
		}
		GameObject gameObject = areaList.transform.parent.gameObject;
		Vector3 localPosition = gameObject.transform.localPosition;
		localPosition.x = 0f - (float)(dungeonAreaCell.cellIndex - 1) * (dungeonAreaCell.width + areaList.hSpace);
		SpringPanel.Begin(gameObject, localPosition, 13f);
	}

	private new void Update()
	{
	}

	private void OnButtonClick(GameObject source)
	{
		if (source == buttonEvent.gameObject && (bool)eventTarget)
		{
			eventTarget.SendMessage("OnEventButtonClick", null, SendMessageOptions.DontRequireReceiver);
		}
	}
}
