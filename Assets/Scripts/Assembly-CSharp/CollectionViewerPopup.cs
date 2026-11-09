using System;
using System.Collections.Generic;
using Models;
using UnityEngine;

public class CollectionViewerPopup : MonoBehaviour
{
	private UIAtlas m_setAtlas;

	public CollectionViewerGrid m_grid;

	public UIDraggablePanel m_scrollView;

	[SerializeField]
	private UIAnchor m_closeBtnUiAnchor;

	[SerializeField]
	private UISprite m_pageDotTemplate;

	[SerializeField]
	private float dotGap = 50f;

	[SerializeField]
	private float dotTop = -280f;

	private List<UISprite> pageDots = new List<UISprite>();

	private void Start()
	{
		m_closeBtnUiAnchor.uiCamera = UIPopupController.Instance.GetPopupCamera();
	}

	private void OnDestroy()
	{
		m_setAtlas = null;
		GC.Collect();
	}

	public void SetData(GachaManager.GachaID p_gachaId)
	{
		if (!(SingletonComponent<GachaManager>.Instance != null))
		{
			return;
		}
		if (p_gachaId == GachaManager.GachaID.PremiumSingle)
		{
			m_setAtlas = Resources.Load("UI/Atlas/UI_ATLAS_MM_TREASURE_WEAPON", typeof(UIAtlas)) as UIAtlas;
		}
		else
		{
			m_setAtlas = Resources.Load("UI/Atlas/UI_ATLAS_MM_TREASURE_SETS", typeof(UIAtlas)) as UIAtlas;
		}
		GachaEventsInfo gachaEventsInfo = MakeFakeGachaInfo();
		CollectionData collectionData = new CollectionData();
		collectionData.Add(new List<ChestDetails>());
		m_grid.AddData(collectionData);
		CollectionViewerCell collectionViewerCell = m_grid.AddCell(0f, 0f);
		collectionViewerCell.ShowDummy();
		foreach (List<ChestDetails> set in gachaEventsInfo.sets)
		{
			CollectionData collectionData2 = new CollectionData();
			collectionData2.Add(set);
			m_grid.AddData(collectionData2);
			CollectionViewerCell collectionViewerCell2 = m_grid.AddCell(0f, 0f);
			collectionViewerCell2.ShowItemsInfo(m_setAtlas, p_gachaId == GachaManager.GachaID.PremiumSingle);
		}
		CollectionData collectionData3 = new CollectionData();
		collectionData3.Add(new List<ChestDetails>());
		m_grid.AddData(collectionData3);
		CollectionViewerCell collectionViewerCell3 = m_grid.AddCell(0f, 0f);
		collectionViewerCell3.ShowDummy();
		UIPanel component = m_scrollView.GetComponent<UIPanel>();
		Vector4 clipRange = component.clipRange;
		component.clipRange = clipRange;
		m_scrollView.DisableSpring();
		Vector3 localPosition = m_scrollView.transform.localPosition;
		localPosition.x = -297f;
		SpringPanel.Begin(m_scrollView.gameObject, localPosition, 20f);
	}

	private void Update()
	{
	}

	private GachaEventsInfo MakeFakeGachaInfo()
	{
		int item_level = 81;
		GachaEventsInfo gachaEventsInfo = new GachaEventsInfo();
		List<ChestDetails> list = new List<ChestDetails>();
		List<ChestDetails> list2 = new List<ChestDetails>();
		List<ChestDetails> list3 = new List<ChestDetails>();
		List<ChestDetails> list4 = new List<ChestDetails>();
		List<ChestDetails> list5 = new List<ChestDetails>();
		ChestDetails chestDetails = new ChestDetails();
		chestDetails.item_id = "WEAPON_GUARDIAN_T5_ROGUE";
		chestDetails.item_level = item_level;
		chestDetails.item_tier = 5;
		ChestDetails chestDetails2 = new ChestDetails();
		chestDetails2.item_id = "ARMOUR_HELM_GUARDIAN_T5_SOLDIER";
		chestDetails2.item_level = item_level;
		chestDetails2.item_tier = 5;
		ChestDetails chestDetails3 = new ChestDetails();
		chestDetails3.item_id = "ARMOUR_SHOULDERS_GUARDIAN_T5_ROGUE";
		chestDetails3.item_level = item_level;
		chestDetails3.item_tier = 5;
		ChestDetails chestDetails4 = new ChestDetails();
		chestDetails4.item_id = "ARMOUR_CHEST_GUARDIAN_T5_SOLDIER";
		chestDetails4.item_level = item_level;
		chestDetails4.item_tier = 5;
		ChestDetails chestDetails5 = new ChestDetails();
		chestDetails5.item_id = "ARMOUR_LEGGINGS_GUARDIAN_T5_HILL";
		chestDetails5.item_level = item_level;
		chestDetails5.item_tier = 5;
		list.Add(chestDetails);
		list.Add(chestDetails2);
		list.Add(chestDetails3);
		list.Add(chestDetails4);
		list.Add(chestDetails5);
		gachaEventsInfo.sets.Add(list);
		ChestDetails chestDetails6 = new ChestDetails();
		chestDetails6.item_id = "WEAPON_BERSERKER_T5_HILL";
		chestDetails6.item_level = item_level;
		chestDetails6.item_tier = 5;
		ChestDetails chestDetails7 = new ChestDetails();
		chestDetails7.item_id = "ARMOUR_HELM_BERSERKER_T5_ROGUE";
		chestDetails7.item_level = item_level;
		chestDetails7.item_tier = 5;
		ChestDetails chestDetails8 = new ChestDetails();
		chestDetails8.item_id = "ARMOUR_SHOULDERS_BERSERKER_T5_SOLDIER";
		chestDetails8.item_level = item_level;
		chestDetails8.item_tier = 5;
		ChestDetails chestDetails9 = new ChestDetails();
		chestDetails9.item_id = "ARMOUR_CHEST_BERSERKER_T5_BRUTE";
		chestDetails9.item_level = item_level;
		chestDetails9.item_tier = 5;
		ChestDetails chestDetails10 = new ChestDetails();
		chestDetails10.item_id = "ARMOUR_LEGGINGS_BERSERKER_T5_SOLDIER";
		chestDetails10.item_level = item_level;
		chestDetails10.item_tier = 5;
		list2.Add(chestDetails6);
		list2.Add(chestDetails7);
		list2.Add(chestDetails8);
		list2.Add(chestDetails9);
		list2.Add(chestDetails10);
		gachaEventsInfo.sets.Add(list2);
		ChestDetails chestDetails11 = new ChestDetails();
		chestDetails11.item_id = "WEAPON_DRAGON_T5_BRUTE";
		chestDetails11.item_level = item_level;
		chestDetails11.item_tier = 5;
		ChestDetails chestDetails12 = new ChestDetails();
		chestDetails12.item_id = "ARMOUR_HELM_DRAGON_T5_HILL";
		chestDetails12.item_level = item_level;
		chestDetails12.item_tier = 5;
		ChestDetails chestDetails13 = new ChestDetails();
		chestDetails13.item_id = "ARMOUR_SHOULDERS_DRAGON_T5_SOLDIER";
		chestDetails13.item_level = item_level;
		chestDetails13.item_tier = 5;
		ChestDetails chestDetails14 = new ChestDetails();
		chestDetails14.item_id = "ARMOUR_CHEST_DRAGON_T5_SOLDIER";
		chestDetails14.item_level = item_level;
		chestDetails14.item_tier = 5;
		ChestDetails chestDetails15 = new ChestDetails();
		chestDetails15.item_id = "ARMOUR_LEGGINGS_DRAGON_T5_ROGUE";
		chestDetails15.item_level = item_level;
		chestDetails15.item_tier = 5;
		list3.Add(chestDetails11);
		list3.Add(chestDetails12);
		list3.Add(chestDetails13);
		list3.Add(chestDetails14);
		list3.Add(chestDetails15);
		gachaEventsInfo.sets.Add(list3);
		ChestDetails chestDetails16 = new ChestDetails();
		chestDetails16.item_id = "WEAPON_DEMON_T5_BRUTE";
		chestDetails16.item_level = item_level;
		chestDetails16.item_tier = 5;
		ChestDetails chestDetails17 = new ChestDetails();
		chestDetails17.item_id = "ARMOUR_HELM_DEMON_T5_ROGUE";
		chestDetails17.item_level = item_level;
		chestDetails17.item_tier = 5;
		ChestDetails chestDetails18 = new ChestDetails();
		chestDetails18.item_id = "ARMOUR_SHOULDERS_DEMON_T5_HILL";
		chestDetails18.item_level = item_level;
		chestDetails18.item_tier = 5;
		ChestDetails chestDetails19 = new ChestDetails();
		chestDetails19.item_id = "ARMOUR_CHEST_DEMON_T5_ROGUE";
		chestDetails19.item_level = item_level;
		chestDetails19.item_tier = 5;
		ChestDetails chestDetails20 = new ChestDetails();
		chestDetails20.item_id = "ARMOUR_LEGGINGS_DEMON_T5_SOLDIER";
		chestDetails20.item_level = item_level;
		chestDetails20.item_tier = 5;
		list4.Add(chestDetails16);
		list4.Add(chestDetails17);
		list4.Add(chestDetails18);
		list4.Add(chestDetails19);
		list4.Add(chestDetails20);
		gachaEventsInfo.sets.Add(list4);
		ChestDetails chestDetails21 = new ChestDetails();
		chestDetails21.item_id = "WEAPON_COLOSSUS_T5_BRUTE";
		chestDetails21.item_level = item_level;
		chestDetails21.item_tier = 5;
		ChestDetails chestDetails22 = new ChestDetails();
		chestDetails22.item_id = "ARMOUR_HELM_COLOSSUS_T5_HILL";
		chestDetails22.item_level = item_level;
		chestDetails22.item_tier = 5;
		ChestDetails chestDetails23 = new ChestDetails();
		chestDetails23.item_id = "ARMOUR_SHOULDERS_COLOSSUS_T5_ROGUE";
		chestDetails23.item_level = item_level;
		chestDetails23.item_tier = 5;
		ChestDetails chestDetails24 = new ChestDetails();
		chestDetails24.item_id = "ARMOUR_CHEST_COLOSSUS_T5_SOLDIER";
		chestDetails24.item_level = item_level;
		chestDetails24.item_tier = 5;
		ChestDetails chestDetails25 = new ChestDetails();
		chestDetails25.item_id = "ARMOUR_LEGGINGS_COLOSSUS_T5_HILL";
		chestDetails25.item_level = item_level;
		chestDetails25.item_tier = 5;
		list5.Add(chestDetails21);
		list5.Add(chestDetails22);
		list5.Add(chestDetails23);
		list5.Add(chestDetails24);
		list5.Add(chestDetails25);
		gachaEventsInfo.sets.Add(list5);
		return gachaEventsInfo;
	}
}
