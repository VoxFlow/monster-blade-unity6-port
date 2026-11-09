using System.Collections;
using UnityEngine;

public class ArmorCollectionsDetailPanel : NBUIMenuPanel
{
	public ArmorCollectionsDetail m_detail;

	private ArmorCollectionsScreen m_screen;

	private ArmorSet m_armorSet;

	private new void Awake()
	{
		if (null != parentScreen)
		{
			m_screen = parentScreen as ArmorCollectionsScreen;
		}
	}

	private void OnEnable()
	{
		Init();
		SetCharacterPosition();
	}

	private void OnDisable()
	{
		StopCoroutine("GenerateCharacter");
		GameObject playerTemplate = SingletonComponent<StageManager>.Instance.PlayerTemplate;
		if (null != playerTemplate)
		{
			playerTemplate.SetActiveRecursively(false);
		}
		CharacterGenerator.SetOneMaterialPerPart(false);
		SingletonComponent<StageManager>.Instance.StopCharacterGeneration();
		MenuBackground3D.Instance.ShowBackground(Element.Type.Earth, false);
		CharacterSetting3D characterSetting3D = Object.FindObjectOfType(typeof(CharacterSetting3D)) as CharacterSetting3D;
		if (null != characterSetting3D)
		{
			characterSetting3D.AttachCamera("MainMenuCameraRoot");
		}
	}

	private void Init()
	{
		m_armorSet = m_screen.selectedArmorSet;
		if (m_armorSet == null)
		{
			return;
		}
		ItemManager instance = SingletonComponent<ItemManager>.Instance;
		if (null != instance)
		{
			NBBinaryFormat armorCollectionsCsv = instance.GetArmorCollectionsCsv();
			if (armorCollectionsCsv != null)
			{
				string data = armorCollectionsCsv.GetData(m_armorSet.id, "SET_ITEMS");
				string[] array = data.Split(',');
				foreach (string text in array)
				{
					Equipment.Part p_part = Equipment.FromString(text);
					if (m_armorSet.GetItem(p_part) == null)
					{
						ItemDetails itemDetails = instance.CreateItemDetails(text, m_armorSet.tier);
						itemDetails.isEnabled = false;
						m_armorSet.AddItem(itemDetails);
					}
				}
			}
		}
		PlayerData value = null;
		if (null != m_detail && null != m_screen)
		{
			m_detail.SetArmor(m_armorSet);
			value = m_detail.GetPlayerData();
		}
		StartCoroutine("GenerateCharacter", value);
		MenuBackground3D.Instance.ShowBackground(m_armorSet.elementType, false);
	}

	private IEnumerator GenerateCharacter(PlayerData p_playerData)
	{
		if (p_playerData != null)
		{
			while (SingletonComponent<StageManager>.Instance.IsGenerating)
			{
				yield return null;
			}
			CharacterGenerator.SetOneMaterialPerPart(true);
			SingletonComponent<StageManager>.Instance.NeedUpdatePlayerTemp = true;
			SingletonComponent<StageManager>.Instance.CreatePlayerMainMenuTemplate(OnPlayerCreated, p_playerData, false, false);
		}
	}

	private void OnPlayerCreated()
	{
		if (base.enabled)
		{
			GameObject playerTemplate = SingletonComponent<StageManager>.Instance.PlayerTemplate;
			if (null != playerTemplate)
			{
				playerTemplate.SetActiveRecursively(true);
				playerTemplate.GetComponent<Animation>().CrossFade(CharacterInfoPanel.s_idleAnimation, 0f);
				playerTemplate.GetComponent<Animation>()[CharacterInfoPanel.s_idleAnimation].wrapMode = WrapMode.Loop;
				SetCharacterPosition();
			}
		}
	}

	private void SetCharacterPosition()
	{
		CharacterSetting3D characterSetting3D = Object.FindObjectOfType(typeof(CharacterSetting3D)) as CharacterSetting3D;
		GameObject playerTemplate = SingletonComponent<StageManager>.Instance.PlayerTemplate;
		if (null != characterSetting3D)
		{
			characterSetting3D.AttachObject(playerTemplate, "ArmorCollectionPlayerRoot");
			characterSetting3D.AttachCamera("ArmorCollectionCameraRoot");
			Color mainDisableColor = characterSetting3D.GetMainDisableColor();
			Color flashDisableColor = characterSetting3D.GetFlashDisableColor();
			Color ambientDisableColor = characterSetting3D.GetAmbientDisableColor();
			Color ambientLight = characterSetting3D.AmbientLight;
			ShowOnlyCollectedParts(mainDisableColor, flashDisableColor, ambientDisableColor, ambientLight);
		}
	}

	private void ShowOnlyCollectedParts(Color p_mainDisablecolor, Color p_flashDisablecolor, Color p_ambientDisablecolor, Color p_ambientEnable)
	{
		if (m_armorSet == null)
		{
			return;
		}
		Equipment.Part[] array = new Equipment.Part[5]
		{
			Equipment.Part.Pants,
			Equipment.Part.Shoulder,
			Equipment.Part.Weapon,
			Equipment.Part.Chest,
			Equipment.Part.Helm
		};
		GameObject playerTemplate = SingletonComponent<StageManager>.Instance.PlayerTemplate;
		if (!(null != playerTemplate))
		{
			return;
		}
		SkinnedMeshRenderer component = playerTemplate.GetComponent<SkinnedMeshRenderer>();
		Material[] materials = component.materials;
		int num = array.Length + 1;
		num = array.Length;
		if (materials.Length != num)
		{
			Dbg.LogWarn("Not the same number of materials");
			return;
		}
		ItemDetails item = m_armorSet.GetItem(Equipment.Part.Pants);
		if (!item.isEnabled)
		{
			materials[0].SetColor("_Color", p_mainDisablecolor);
			materials[0].SetColor("_FlashColor", p_flashDisablecolor);
			materials[0].SetColor("_AmbientColor", p_ambientDisablecolor);
		}
		int num2 = array.Length;
		int num3 = 0;
		num3 = 1;
		num2--;
		for (int i = 0; i < num2; i++)
		{
			item = m_armorSet.GetItem(array[num3 + i]);
			if (!item.isEnabled)
			{
				materials[i + 1].SetColor("_Color", p_mainDisablecolor);
				materials[i + 1].SetColor("_FlashColor", p_flashDisablecolor);
				materials[i + 1].SetColor("_AmbientColor", p_ambientDisablecolor);
			}
		}
	}
}
