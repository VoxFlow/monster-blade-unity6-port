using UnityEngine;

public class CollectionViewerCell : NBUIGridCell<CollectionData>
{
	[SerializeField]
	private UISprite Background;

	[SerializeField]
	private UISprite Foreground;

	[SerializeField]
	private UISprite ElementIcon;

	[SerializeField]
	private UILabel HealthLabel;

	[SerializeField]
	private UILabel ArmorLabel;

	[SerializeField]
	private UILabel AttackLabel;

	[SerializeField]
	private UILabel CriticalLabel;

	[SerializeField]
	private NBUILocalize SetName;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Start()
	{
		base.Start();
		ValidateHelper.Begin().NotNull(Background).NotNull(HealthLabel)
			.NotNull(ArmorLabel)
			.NotNull(AttackLabel)
			.NotNull(CriticalLabel)
			.NotNull(SetName);
	}

	private void Update()
	{
	}

	public void ShowDummy()
	{
		Background.enabled = false;
		HealthLabel.enabled = false;
		ElementIcon.enabled = false;
		Foreground.enabled = false;
		ArmorLabel.enabled = false;
		AttackLabel.enabled = false;
		CriticalLabel.enabled = false;
		SetName.enabled = false;
	}

	public void ShowItemsInfo(UIAtlas p_atlas, bool p_isSingle = true)
	{
		PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		float num5 = 0f;
		float num6 = 0f;
		float num7 = 0f;
		float num8 = 0f;
		float num9 = 0f;
		float num10 = 0f;
		Element.Type type = Element.Type.Fire;
		ItemDetails itemDetails = null;
		foreach (ItemDetails item in data.Items)
		{
			ItemDetails itemDetails2 = playerData.GetEquipmentData(item.equipment.part).itemDetails;
			itemDetails = item;
			type = item.Element;
			num += item.itemTypeWithSkill.hp;
			num2 += item.itemTypeWithSkill.armour;
			num3 += item.itemTypeWithSkill.critical;
			num6 += itemDetails2.itemTypeWithSkill.hp;
			num7 += itemDetails2.itemTypeWithSkill.armour;
			num8 += itemDetails2.itemTypeWithSkill.critical;
			if (item.IsWeapon)
			{
				num4 += item.itemTypeWithSkill.minDamage;
				num5 += item.itemTypeWithSkill.maxDamage;
				num9 += itemDetails2.itemTypeWithSkill.minDamage;
				num10 += itemDetails2.itemTypeWithSkill.maxDamage;
			}
			if (p_isSingle)
			{
				break;
			}
		}
		Dbg.Log(string.Format("Set item info: accumItemHp:{0}, accumItemArmor:{1}, accumItemCritical:{2}, accumMinDmg:{3}, accumMaxDmg:{4}", num, num2, num3, num4, num5));
		Dbg.Log(string.Format("Equipped item info: curAccumItemHp:{0}, curAccumItemArmor:{1}, curAccumItemCritical:{2}, curAccumMinDmg:{3}, curAccumMaxDmg:{4}", num6, num7, num8, num9, num10));
		base.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
		Foreground.atlas = p_atlas;
		if (p_isSingle)
		{
			Foreground.transform.localScale = new Vector3(279f, 446f, 1f);
		}
		else
		{
			Foreground.transform.localScale = new Vector3(334f, 368f, 1f);
		}
		string text = type.ToString().ToLower();
		if (text == "earth")
		{
			text = "nature";
		}
		string text2 = "View_" + text + ((!p_isSingle) ? "_set" : "_weapon");
		Dbg.Log("Show Sprite : " + text2);
		Foreground.spriteName = text2;
		ElementIcon.spriteName = text + "_icon";
		if (p_isSingle)
		{
			SetName.Key = itemDetails.Name;
		}
		else
		{
			SetName.Key = string.Empty;
			if (null != SingletonComponent<ItemManager>.Instance)
			{
				ArmorSet armorSet = SingletonComponent<ItemManager>.Instance.GetArmorSet(itemDetails.equipment.setID);
				if (armorSet != null)
				{
					SetName.Key = armorSet.name;
				}
			}
		}
		SetBaseAbilityValue(ref HealthLabel, num, num6);
		SetBaseAbilityValue(ref ArmorLabel, num2, num7);
		SetBaseAbilityValue(ref CriticalLabel, num3, num8, true);
		SetAttackValue(ref AttackLabel, num4, num9, num5);
	}

	private void SetBaseAbilityValue(ref UILabel p_label, float p_itemValue, float p_equippedItemValue, bool p_isCritical = false)
	{
		p_label.color = Color.white;
		if (p_isCritical)
		{
			p_label.text = string.Format("{0:0.00}%", p_itemValue);
		}
		else
		{
			p_label.text = p_itemValue.ToString();
		}
	}

	private void SetAttackValue(ref UILabel p_label, float p_itemMinAtk, float p_equipMinAtk, float p_itemMaxAtk)
	{
		if (p_itemMinAtk > 0f)
		{
			p_label.color = Color.white;
			p_label.text = Mathf.RoundToInt(p_itemMinAtk) + " - " + Mathf.RoundToInt(p_itemMaxAtk);
		}
		else
		{
			p_label.color = Color.white;
			p_label.text = "0 - 0";
		}
	}
}
