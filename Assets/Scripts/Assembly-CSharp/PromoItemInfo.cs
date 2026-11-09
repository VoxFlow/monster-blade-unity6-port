using System.Collections.Generic;
using UnityEngine;

public class PromoItemInfo : MonoBehaviour
{
	public Dictionary<string, ItemDetails> PromoItemMap = new Dictionary<string, ItemDetails>();

	public UILabel HealthLabel;

	public UILabel ArmorLabel;

	public UILabel AttackLabel;

	public UILabel CriticalLabel;

	public UILabel ItemName;

	public UILabel SkillName;

	public UILabel SkillDesc;

	public UISprite SkillIcon;

	public UILabel AssistSkillName;

	public UILabel AssistSkillDesc;

	public UISprite AssistSkillIcon;

	public GameObject TierTemplate;

	public Transform TierTemplatePos;

	private TiersStars tiers;

	public UILabel ItemSetName;

	public UILabel ItemSetDesc;

	private PlayerData m_playerData;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void ShowItemInfo(string p_itemId)
	{
		Dbg.Log("[Info] Show promotion item : " + p_itemId);
		if (!PromoItemMap.ContainsKey(p_itemId))
		{
			return;
		}
		ItemDetails itemDetails = PromoItemMap[p_itemId];
		m_playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		ItemDetails itemDetails2 = m_playerData.GetEquipmentData(itemDetails.equipment.part).itemDetails;
		if (!tiers)
		{
			InitTiers();
		}
		tiers.SetTier(itemDetails);
		ItemName.text = SingletonComponent<NBLocalization>.Instance.Get(itemDetails.Name);
		SetBaseAbilityValue(ref HealthLabel, itemDetails.itemTypeWithSkill.hp, itemDetails2.itemTypeWithSkill.hp);
		SetBaseAbilityValue(ref ArmorLabel, itemDetails.itemTypeWithSkill.armour, itemDetails2.itemTypeWithSkill.armour);
		SetBaseAbilityValue(ref CriticalLabel, itemDetails.itemTypeWithSkill.critical, itemDetails2.itemTypeWithSkill.critical, true);
		if (itemDetails.IsWeapon)
		{
			int num = Mathf.RoundToInt(itemDetails.itemTypeWithSkill.minDamage);
			int num2 = Mathf.RoundToInt(itemDetails2.itemTypeWithSkill.minDamage);
			AttackLabel.color = SingletonComponent<ItemManager>.Instance.GetCompareColor(num, num2);
			AttackLabel.text = num + "-" + Mathf.RoundToInt(itemDetails.itemTypeWithSkill.maxDamage);
		}
		else
		{
			AttackLabel.color = Color.white;
			AttackLabel.text = "0 - 0";
		}
		if (string.IsNullOrEmpty(itemDetails.skill.visual.label_self))
		{
			SkillName.text = string.Empty;
			SkillDesc.text = string.Empty;
			SkillIcon.gameObject.SetActiveRecursively(false);
		}
		else
		{
			SkillIcon.gameObject.SetActiveRecursively(true);
			SkillName.text = SingletonComponent<NBLocalization>.Instance.Get(itemDetails.skill.visual.label_self);
			if (itemDetails2 != null)
			{
				SkillDesc.text = ProcessSkillDesc(SingletonComponent<NBLocalization>.Instance.Get(itemDetails.skill.visual.description_self), itemDetails.skill.directValue, itemDetails2.skill.directValue);
				SkillIcon.spriteName = itemDetails.skill.visual.sprite_self;
			}
			else
			{
				SkillDesc.gameObject.SetActiveRecursively(false);
				SkillIcon.gameObject.SetActiveRecursively(false);
			}
		}
		if (string.IsNullOrEmpty(itemDetails.skill.visual.label_assist))
		{
			AssistSkillName.text = string.Empty;
			AssistSkillDesc.text = string.Empty;
			AssistSkillIcon.gameObject.SetActiveRecursively(false);
			return;
		}
		AssistSkillIcon.gameObject.SetActiveRecursively(true);
		AssistSkillName.text = SingletonComponent<NBLocalization>.Instance.Get(itemDetails.skill.visual.label_assist);
		if (itemDetails2 != null)
		{
			AssistSkillDesc.text = ProcessSkillDesc(SingletonComponent<NBLocalization>.Instance.Get(itemDetails.skill.visual.description_assist), itemDetails.skill.assistValue, itemDetails2.skill.assistValue);
			AssistSkillIcon.spriteName = itemDetails.skill.visual.sprite_assist;
		}
		else
		{
			AssistSkillDesc.gameObject.SetActiveRecursively(false);
			AssistSkillIcon.gameObject.SetActiveRecursively(false);
		}
	}

	private string ProcessSkillDesc(string p_rawSkillDec, float p_itemSkillValue, float p_equippedItemSkillValue)
	{
		string text = p_rawSkillDec;
		int num = p_rawSkillDec.IndexOf("{");
		if (num > 0)
		{
			text = text.Insert(num, SingletonComponent<ItemManager>.Instance.GetCompareColorTag(p_itemSkillValue, p_equippedItemSkillValue));
			int num2 = text.IndexOf("}") + 1;
			if (num2 > 0)
			{
				text = text.Insert(num2, "[-]");
			}
		}
		if (text.Contains("{0"))
		{
			text = string.Format(text, p_itemSkillValue);
		}
		return text;
	}

	private void SetBaseAbilityValue(ref UILabel p_label, float p_itemValue, float p_equippedItemValue, bool p_isCritical = false)
	{
		p_label.color = SingletonComponent<ItemManager>.Instance.GetCompareColor(p_itemValue, p_equippedItemValue);
		if (p_isCritical)
		{
			p_label.text = string.Format("{0:0.00%}", p_itemValue);
		}
		else
		{
			p_label.text = p_itemValue.ToString();
		}
	}

	private void InitTiers()
	{
		GameObject gameObject = (GameObject)Object.Instantiate(TierTemplate, TierTemplatePos.position, TierTemplatePos.rotation);
		tiers = gameObject.GetComponent<TiersStars>();
		gameObject.transform.parent = TierTemplatePos.parent;
		Transform[] componentsInChildren = MBHier.GetComponentsInChildren<Transform>(gameObject, true);
		Transform[] array = componentsInChildren;
		foreach (Transform transform in array)
		{
			transform.gameObject.layer = base.gameObject.layer;
		}
	}
}
