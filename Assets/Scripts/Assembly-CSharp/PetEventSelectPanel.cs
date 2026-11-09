using System.Collections.Generic;
using UnityEngine;

public class PetEventSelectPanel : NBUIMenuPanel
{
	public DungeonGrid dungeonList;

	public UIButton buttonEvent;

	private DungeonCell _dummyCellLeft;

	private DungeonCell _dummyCellRight;

	private CityDbStruct m_currentCity;

	public Component eventTarget;

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
		}
	}

	public override void Reset()
	{
	}

	private void Init()
	{
		if (SingletonComponent<DungeonManager>.Instance == null)
		{
			return;
		}
		CityDbStruct currentCity = SingletonComponent<DungeonManager>.Instance.CurrentCity;
		if (currentCity != m_currentCity)
		{
			List<DungeonManager.DungeonStruct> allUnlockedDungeonsOfCity = SingletonComponent<DungeonManager>.Instance.GetAllUnlockedDungeonsOfCity(currentCity);
			if (allUnlockedDungeonsOfCity.Count <= 0)
			{
				return;
			}
			dungeonList.Clear();
			DungeonManager.DungeonStruct dungeonStruct = new DungeonManager.DungeonStruct();
			dungeonStruct.dungeon = new DungeonDbStruct();
			dungeonList.AddData(dungeonStruct);
			_dummyCellLeft = dungeonList.AddCell(0f, 0f);
			_dummyCellLeft.hidden = true;
			foreach (DungeonManager.DungeonStruct item in allUnlockedDungeonsOfCity)
			{
				dungeonList.AddData(item);
				DungeonCell dungeonCell = dungeonList.AddCell(0f, 0f);
				dungeonCell.m_eventTarget = parentScreen;
			}
			dungeonList.AddData(dungeonStruct);
			_dummyCellRight = dungeonList.AddCell(0f, 0f);
			_dummyCellRight.hidden = true;
		}
		if (dungeonList.cells.Count <= 0)
		{
			return;
		}
		DungeonDbStruct currentDungeon = SingletonComponent<DungeonManager>.Instance.CurrentDungeon;
		if (currentDungeon == null)
		{
			return;
		}
		DungeonCell dungeonCell2 = dungeonList.cells[0];
		foreach (DungeonCell cell in dungeonList.cells)
		{
			if (cell.data == null || cell.data.dungeon == null || !(cell.data.dungeon.id == currentDungeon.id))
			{
				continue;
			}
			dungeonCell2 = cell;
			break;
		}
		GameObject gameObject = dungeonList.transform.parent.gameObject;
		Vector3 localPosition = gameObject.transform.localPosition;
		localPosition.x = 0f - (float)(dungeonCell2.cellIndex - 1) * (dungeonCell2.width + dungeonList.hSpace);
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
