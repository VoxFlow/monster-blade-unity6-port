using UnityEngine;

public class ItemInfo : MonoBehaviour
{
	public InventoryCell m_cellTemplate;

	public GameObject m_tierTemplate;

	public Transform m_cellTemplatePos;

	public Transform m_tierTemplatePos;

	public UISprite m_healthLabel;

	public UISprite m_armourLabel;

	public UISprite m_damageLabel;

	public UISprite m_criticalLabel;

	public UISprite m_spriteSkillSelf;

	public UISprite m_spriteSkillAssist;

	public UILabel m_labelSkillSelf;

	public UILabel m_labelSkillAssist;

	public UILabel m_labelSkillSelfDesc;

	public UILabel m_labelSkillAssistDesc;

	public UILabel m_descriptionHeadline;

	public UILabel m_descriptionContent;

	public UILabel m_name;

	public UILabel m_setName;

	public UILabel m_level;

	public UILabel m_health;

	public UILabel m_armour;

	public UILabel m_damage;

	public UILabel m_damageMax;

	public UILabel m_critical;

	[SerializeField]
	private UILabel m_noSkillLabel;

	public float m_CellSpriteScaleFactor = 1.65f;

	public bool useCollider = true;

	public Color betterColor = Color.green;

	public Color worseColor = Color.red;

	public Color sameColor = Color.white;

	private InventoryCell m_cell;

	private TiersStars m_tiers;

	private InventoryManager m_inventoryMgr;

	private float m_alpha = 1f;

	public TiersStars.Alignment m_starsAlignment;

	[SerializeField]
	private UISprite m_highlight1;

	[SerializeField]
	private UISprite m_highlight2;

	public float alpha
	{
		get
		{
			return m_alpha;
		}
		set
		{
			m_alpha = value;
			m_healthLabel.alpha = m_alpha;
			m_armourLabel.alpha = m_alpha;
			m_damageLabel.alpha = m_alpha;
			m_criticalLabel.alpha = m_alpha;
			m_name.alpha = m_alpha;
			if (null != m_setName)
			{
				m_setName.alpha = m_alpha;
			}
			m_level.alpha = m_alpha;
			m_health.alpha = m_alpha;
			m_armour.alpha = m_alpha;
			m_damage.alpha = m_alpha;
			m_critical.alpha = m_alpha;
			if (m_noSkillLabel != null)
			{
				m_noSkillLabel.alpha = m_alpha;
			}
			m_spriteSkillSelf.alpha = m_alpha;
			m_spriteSkillAssist.alpha = m_alpha;
			m_labelSkillSelf.alpha = m_alpha;
			m_labelSkillAssist.alpha = m_alpha;
			m_labelSkillSelfDesc.alpha = m_alpha;
			m_labelSkillAssistDesc.alpha = m_alpha;
			if (!m_tiers)
			{
				InitTiers();
			}
			if (m_tiers != null)
			{
				UISprite[] stars = m_tiers.m_stars;
				foreach (UISprite uISprite in stars)
				{
					uISprite.alpha = m_alpha;
				}
			}
		}
	}

	public InventoryCell InventoryCell
	{
		get
		{
			return m_cell;
		}
	}

	private void Awake()
	{
		if (!m_cell)
		{
			InitInventoryCell();
		}
		if (!m_tiers)
		{
			InitTiers();
		}
		m_inventoryMgr = SingletonComponent<InventoryManager>.Instance;
		Dbg.Assert(m_noSkillLabel);
		Dbg.Assert(m_spriteSkillSelf);
		Dbg.Assert(m_labelSkillSelf);
		Dbg.Assert(m_labelSkillSelfDesc);
		Dbg.Assert(m_spriteSkillAssist);
		Dbg.Assert(m_labelSkillAssist);
		Dbg.Assert(m_labelSkillAssistDesc);
	}

	private void InitInventoryCell()
	{
		m_cell = Object.Instantiate(m_cellTemplate, m_cellTemplatePos.position, m_cellTemplatePos.rotation) as InventoryCell;
		m_cell.transform.parent = m_cellTemplatePos.parent;
		m_cell.transform.localScale = Vector3.one;
		m_cell.transform.localScale *= m_CellSpriteScaleFactor;
		m_cell.SetLayerRecursively(base.gameObject.layer);
		BoxCollider component = m_cell.GetComponent<BoxCollider>();
		if (!useCollider && component != null)
		{
			Object.Destroy(component);
		}
	}

	private void InitTiers()
	{
		if (!(m_tierTemplatePos == null))
		{
			GameObject gameObject = (GameObject)Object.Instantiate(m_tierTemplate, m_tierTemplatePos.position, m_tierTemplatePos.rotation);
			UIPanel component = gameObject.GetComponent<UIPanel>();
			if (component != null)
			{
				Object.Destroy(component);
			}
			m_tiers = gameObject.GetComponent<TiersStars>();
			m_tiers.m_alignment = m_starsAlignment;
			gameObject.transform.parent = m_tierTemplatePos.parent;
			Transform[] componentsInChildren = MBHier.GetComponentsInChildren<Transform>(gameObject, true);
			Transform[] array = componentsInChildren;
			foreach (Transform transform in array)
			{
				transform.gameObject.layer = base.gameObject.layer;
			}
		}
	}

	private void ShowStats(bool _weapon, bool _armour)
	{
		m_healthLabel.enabled = _armour;
		m_armourLabel.enabled = _armour;
		m_health.enabled = _armour;
		m_armour.enabled = _armour;
		m_damageLabel.enabled = _weapon;
		m_criticalLabel.enabled = _weapon;
		m_damage.enabled = _weapon;
		m_critical.enabled = _weapon;
		if ((_weapon || _armour) && m_descriptionHeadline != null && m_descriptionContent != null)
		{
			m_descriptionHeadline.text = string.Empty;
			m_descriptionContent.text = string.Empty;
		}
	}

	private void ShowWeapon()
	{
		ShowStats(true, false);
	}

	private void ShowArmour()
	{
		ShowStats(false, true);
	}

	private void ShowEssence(ItemDetails pcItemDetails)
	{
		ShowStats(false, false);
		if (m_descriptionHeadline != null && m_descriptionContent != null)
		{
			if (pcItemDetails != null && pcItemDetails.loot != null)
			{
				m_descriptionContent.text = SingletonComponent<NBLocalization>.Instance.Get(pcItemDetails.loot.description);
			}
			m_descriptionHeadline.enabled = true;
			m_descriptionContent.enabled = true;
			m_descriptionContent.transform.localPosition = new Vector3(-130f, m_descriptionContent.transform.localPosition.y, m_descriptionContent.transform.localPosition.z);
		}
		HideHighlights();
	}

	private void HideHighlights()
	{
		if (null != m_highlight1 && null != m_highlight2)
		{
			m_highlight1.enabled = false;
			m_highlight2.enabled = false;
		}
	}

	public void ShowItem(string _db_id, bool showCell = true, bool showSkillDescriptions = true)
	{
		ItemDetails byDBID = m_inventoryMgr.GetByDBID(_db_id);
		ShowItem(byDBID, showCell, showSkillDescriptions);
	}

	public void ShowItemVsOld(ItemDetails p_newItem, ItemDetails p_oldItem, float p_ratio)
	{
		if (p_newItem == null || p_oldItem == null || !p_newItem.IsEquipment)
		{
			return;
		}
		ShowStats(true, true);
		m_health.color = Color.white;
		m_armour.color = Color.white;
		m_damage.color = Color.white;
		m_critical.color = Color.white;
		int num = Mathf.FloorToInt((p_newItem.itemTypeWithSkill.hp - p_oldItem.itemTypeWithSkill.hp) * p_ratio + p_oldItem.itemTypeWithSkill.hp);
		int num2 = Mathf.FloorToInt((p_newItem.itemTypeWithSkill.armour - p_oldItem.itemTypeWithSkill.armour) * p_ratio + p_oldItem.itemTypeWithSkill.armour);
		int num3 = Mathf.FloorToInt((p_newItem.itemTypeWithSkill.minDamage - p_oldItem.itemTypeWithSkill.minDamage) * p_ratio + p_oldItem.itemTypeWithSkill.minDamage);
		int num4 = Mathf.FloorToInt((p_newItem.itemTypeWithSkill.maxDamage - p_oldItem.itemTypeWithSkill.maxDamage) * p_ratio + p_oldItem.itemTypeWithSkill.maxDamage);
		float num5 = (p_newItem.itemTypeWithSkill.critical - p_oldItem.itemTypeWithSkill.critical) * p_ratio + p_oldItem.itemTypeWithSkill.critical;
		int num6 = Mathf.FloorToInt(p_oldItem.itemTypeWithSkill.hp);
		int num7 = Mathf.FloorToInt(p_oldItem.itemTypeWithSkill.armour);
		int num8 = Mathf.FloorToInt(p_oldItem.itemTypeWithSkill.minDamage);
		float critical = p_oldItem.itemTypeWithSkill.critical;
		if (p_newItem.IsWeapon)
		{
			if (num != 0 && num > num6)
			{
				m_health.text = "+" + num6 + NBColorTags.TAG_GREEN + "->+" + num + NBColorTags.TAG_END;
			}
			else
			{
				m_health.text = num.ToString();
			}
			if (num2 != 0 && num2 > num7)
			{
				m_armour.text = "+" + num7 + NBColorTags.TAG_GREEN + "->+" + num2 + NBColorTags.TAG_END;
			}
			else
			{
				m_armour.text = num2.ToString();
			}
			if (num3 > num8)
			{
				m_damage.text = Mathf.FloorToInt(p_oldItem.itemTypeWithSkill.minDamage) + "-" + Mathf.FloorToInt(p_oldItem.itemTypeWithSkill.maxDamage) + NBColorTags.TAG_GREEN + "->" + num3 + "-" + num4 + NBColorTags.TAG_END;
			}
			if (num5 > critical)
			{
				m_critical.text = string.Format("{0:0.00}%" + NBColorTags.TAG_GREEN + "->{1:0.00}%" + NBColorTags.TAG_END, critical, num5);
			}
		}
		else if (p_newItem.IsArmour)
		{
			if (num3 > num8)
			{
				if (num3 != 0)
				{
					m_damage.text = "+" + num8 + NBColorTags.TAG_GREEN + "->+" + num3 + NBColorTags.TAG_END;
				}
				else
				{
					m_damage.text = Mathf.FloorToInt(p_newItem.itemTypeWithSkill.minDamage) + "-" + Mathf.FloorToInt(p_newItem.itemTypeWithSkill.maxDamage) + "->" + NBColorTags.TAG_GREEN + num3 + "-" + num4 + NBColorTags.TAG_END;
				}
			}
			if (num5 > 0f && num5 > critical)
			{
				m_critical.text = string.Format("+{0:0.00}%" + NBColorTags.TAG_GREEN + "->+{1:0.00}%" + NBColorTags.TAG_END, critical, num5);
			}
			if (num > num6)
			{
				m_health.text = num6 + NBColorTags.TAG_GREEN + "->" + num + NBColorTags.TAG_END;
			}
			if (num2 > num7)
			{
				m_armour.text = num7 + NBColorTags.TAG_GREEN + "->" + num2 + NBColorTags.TAG_END;
			}
		}
		SkillVisual visual = p_newItem.skill.visual;
		float num9 = (p_newItem.skill.directValue - p_oldItem.skill.directValue) * p_ratio + p_oldItem.skill.directValue;
		float num10 = (p_newItem.skill.assistValue - p_oldItem.skill.assistValue) * p_ratio + p_oldItem.skill.assistValue;
		if (!p_newItem.IsEquipment || p_newItem.IsBaseItem || p_newItem.IsEssence || visual.skill == ItemEnum.Skill.WEAPON_NONE || visual.skill == ItemEnum.Skill.ARMOUR_NONE)
		{
			return;
		}
		m_labelSkillSelf.text = SingletonComponent<NBLocalization>.Instance.Get(visual.label_self);
		m_labelSkillSelfDesc.enabled = true;
		string text = SingletonComponent<NBLocalization>.Instance.Get(visual.description_self);
		int num11 = text.IndexOf("{");
		if (num11 >= 0)
		{
			text = text.Insert(num11, NBColorTags.TAG_GREEN);
		}
		int num12 = text.IndexOf(" ", num11);
		if (num12 > num11)
		{
			text = text.Insert(num12, NBColorTags.TAG_END);
		}
		if (text.Contains("{0"))
		{
			if (p_newItem.skill.selfAction == SkillEnum.SelfSkill.Critical)
			{
				m_labelSkillSelfDesc.text = string.Format(text, num9.ToString("#.00"));
			}
			else
			{
				m_labelSkillSelfDesc.text = string.Format(text, num9.ToString("#.##"));
			}
		}
		else
		{
			Dbg.Log("No text need to be replaced in self skill description");
			m_labelSkillSelfDesc.text = text;
		}
		if (!p_newItem.IsWeapon)
		{
			return;
		}
		m_labelSkillAssist.text = SingletonComponent<NBLocalization>.Instance.Get(visual.label_assist);
		string text2 = SingletonComponent<NBLocalization>.Instance.Get(visual.description_assist);
		int num13 = text2.IndexOf("{");
		if (num13 >= 0)
		{
			text2 = text2.Insert(num13, NBColorTags.TAG_GREEN);
		}
		int num14 = text2.IndexOf(" ", num13);
		if (num14 > num13)
		{
			text2 = text2.Insert(num14, NBColorTags.TAG_END);
		}
		if (text2.Contains("{0"))
		{
			if (p_newItem.skill.assistAction == SkillEnum.AssistSkill.BuffCritical)
			{
				m_labelSkillAssistDesc.text = string.Format(text2, num10.ToString("#.00"));
			}
			else
			{
				m_labelSkillAssistDesc.text = string.Format(text2, num10.ToString("#.##"));
			}
		}
		else
		{
			Dbg.Log("No text need to be replaced in assist skill description");
			m_labelSkillAssistDesc.text = text2;
		}
	}

	public void ShowItem(ItemDetails _item, bool showCell = true, bool showSkillDescriptions = true, bool p_ignoreColor = false)
	{
		if (!m_cell)
		{
			InitInventoryCell();
		}
		if (!m_tiers)
		{
			InitTiers();
		}
		if (_item == null || _item == ItemManager.s_NULL_DATA)
		{
			m_name.text = string.Empty;
			if (null != m_setName)
			{
				m_setName.text = string.Empty;
			}
			m_level.text = "LVL ";
			m_health.text = string.Empty;
			m_armour.text = string.Empty;
			m_damage.text = string.Empty;
			m_critical.text = string.Empty;
			m_spriteSkillSelf.enabled = false;
			m_spriteSkillAssist.enabled = false;
			m_labelSkillSelf.enabled = false;
			m_labelSkillAssist.enabled = false;
			if ((bool)m_descriptionHeadline)
			{
				m_descriptionHeadline.enabled = false;
				m_descriptionHeadline.text = string.Empty;
			}
			if ((bool)m_descriptionContent)
			{
				m_descriptionContent.enabled = false;
				m_descriptionContent.text = string.Empty;
			}
			if ((bool)m_labelSkillSelfDesc)
			{
				m_labelSkillSelfDesc.enabled = false;
			}
			if ((bool)m_labelSkillAssistDesc)
			{
				m_labelSkillAssistDesc.enabled = false;
			}
			m_cell.m_hidden = true;
			m_cell.Empty = true;
			if (m_tiers != null)
			{
				m_tiers.SetEmptyTier();
			}
			Dbg.LogWarn("Cannot print null/Dummy Item info.");
			ShowStats(false, false);
			return;
		}
		m_name.text = SingletonComponent<NBLocalization>.Instance.Get(_item.Name);
		if (null != m_setName)
		{
			m_setName.text = string.Empty;
		}
		if (_item.Level <= _item.MaxLevelInTier)
		{
			m_level.text = "LVL " + _item.Level + "/" + _item.MaxLevelInTier;
		}
		else
		{
			Dbg.LogWarn("Strange situation, the level is higher than the max level, item : " + _item.ToString());
			m_level.text = "LVL " + _item.MaxLevelInTier + "/" + _item.MaxLevelInTier;
		}
		PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		if (_item.IsEquipment)
		{
			ShowStats(true, true);
			ArmorSet armorSet = SingletonComponent<ItemManager>.Instance.GetArmorSet(_item.equipment.setID, _item.equipment.isSpecial);
			if (armorSet != null && null != m_setName)
			{
				m_setName.text = SingletonComponent<NBLocalization>.Instance.Get(armorSet.name);
			}
			int num = Mathf.FloorToInt(_item.itemTypeWithSkill.hp);
			int num2 = Mathf.FloorToInt(_item.itemTypeWithSkill.armour);
			int num3 = Mathf.FloorToInt(_item.itemTypeWithSkill.minDamage);
			float critical = _item.itemTypeWithSkill.critical;
			if (_item.IsWeapon)
			{
				if (num != 0)
				{
					m_health.text = "+" + num;
				}
				else
				{
					m_health.text = num.ToString();
				}
				if (num2 != 0)
				{
					m_armour.text = "+" + num2;
				}
				else
				{
					m_armour.text = num2.ToString();
				}
				m_damage.text = Mathf.FloorToInt(_item.itemTypeWithSkill.minDamage) + "-" + Mathf.FloorToInt(_item.itemTypeWithSkill.maxDamage);
				m_critical.text = string.Format("{0:0.00}%", critical);
			}
			else if (_item.IsArmour)
			{
				if (num3 != 0)
				{
					m_damage.text = "+" + num3;
				}
				else
				{
					m_damage.text = Mathf.FloorToInt(_item.itemTypeWithSkill.minDamage) + "-" + Mathf.FloorToInt(_item.itemTypeWithSkill.maxDamage);
				}
				if (critical > 0f)
				{
					m_critical.text = string.Format("+{0:0.00}%", critical);
				}
				else
				{
					m_critical.text = string.Format("{0:0.00}%", critical);
				}
				m_health.text = num.ToString();
				m_armour.text = num2.ToString();
			}
			bool flag = true;
			ItemDetails itemDetails = playerData.GetEquipmentData(_item.equipment.part).itemDetails;
			if (_item.IsEquipment && itemDetails != null && _item.DB_ID != itemDetails.DB_ID)
			{
				m_health.color = GetCompareColor(_item.itemTypeWithSkill.hp, itemDetails.itemTypeWithSkill.hp);
				m_armour.color = GetCompareColor(_item.itemTypeWithSkill.armour, itemDetails.itemTypeWithSkill.armour);
				m_damage.color = GetCompareColor(_item.itemTypeWithSkill.minDamage, itemDetails.itemTypeWithSkill.minDamage);
				m_critical.color = GetCompareColor(_item.itemTypeWithSkill.critical, itemDetails.itemTypeWithSkill.critical);
				flag = false;
			}
			if (flag || p_ignoreColor)
			{
				Color white = Color.white;
				white.a = m_alpha;
				m_health.color = white;
				m_armour.color = white;
				m_damage.color = white;
				m_critical.color = white;
			}
		}
		else
		{
			ShowEssence(_item);
			if (_item.IsEssence)
			{
				Object.DestroyImmediate(m_tiers.gameObject);
			}
			else
			{
				m_tiers.SetEmptyTier();
			}
		}
		if (_item.Level <= 0)
		{
			m_level.text = string.Empty;
		}
		SkillVisual visual = _item.skill.visual;
		if (_item.IsEquipment && !_item.IsBaseItem && !_item.IsEssence)
		{
			if (visual.skill != ItemEnum.Skill.WEAPON_NONE && visual.skill != ItemEnum.Skill.ARMOUR_NONE)
			{
				if (m_noSkillLabel != null)
				{
					m_noSkillLabel.enabled = false;
				}
				m_spriteSkillSelf.enabled = true;
				m_labelSkillSelf.enabled = true;
				m_spriteSkillSelf.spriteName = visual.sprite_self;
				m_labelSkillSelf.text = SingletonComponent<NBLocalization>.Instance.Get(visual.label_self);
				if (showSkillDescriptions)
				{
					m_labelSkillSelfDesc.enabled = true;
					string text = SingletonComponent<NBLocalization>.Instance.Get(visual.description_self);
					ItemDetails itemDetails2 = playerData.GetEquipmentData(_item.equipment.part).itemDetails;
					if (!p_ignoreColor && itemDetails2 != null && ((itemDetails2.skill != null && itemDetails2.skill.type == _item.skill.type) || itemDetails2.IsBaseItem))
					{
						int num4 = text.IndexOf("{");
						if (num4 >= 0)
						{
							text = text.Insert(num4, SingletonComponent<ItemManager>.Instance.GetCompareColorTag(_item.skill.directValue, itemDetails2.skill.directValue));
						}
					}
					if (text.Contains("{0"))
					{
						if (_item.skill.selfAction == SkillEnum.SelfSkill.Critical)
						{
							m_labelSkillSelfDesc.text = string.Format(text, _item.skill.directValue.ToString("#.00"));
						}
						else
						{
							m_labelSkillSelfDesc.text = string.Format(text, _item.skill.directValue.ToString("#.##"));
						}
					}
					else
					{
						Dbg.Log("No text need to be replaced in self skill description");
						m_labelSkillSelfDesc.text = text;
					}
				}
				if (_item.IsWeapon)
				{
					m_spriteSkillAssist.enabled = true;
					m_labelSkillAssist.enabled = true;
					m_spriteSkillAssist.spriteName = visual.sprite_assist;
					m_labelSkillAssist.text = SingletonComponent<NBLocalization>.Instance.Get(visual.label_assist);
					if (showSkillDescriptions)
					{
						m_labelSkillAssistDesc.enabled = true;
						string text2 = SingletonComponent<NBLocalization>.Instance.Get(visual.description_assist);
						ItemDetails itemDetails3 = playerData.GetEquipmentData(_item.equipment.part).itemDetails;
						if (!p_ignoreColor && itemDetails3 != null && ((itemDetails3.skill != null && itemDetails3.skill.type == _item.skill.type) || itemDetails3.IsBaseItem))
						{
							int num5 = text2.IndexOf("{");
							if (num5 >= 0)
							{
								text2 = text2.Insert(num5, SingletonComponent<ItemManager>.Instance.GetCompareColorTag(_item.skill.assistValue, itemDetails3.skill.assistValue));
							}
						}
						if (text2.Contains("{0"))
						{
							if (_item.skill.assistAction == SkillEnum.AssistSkill.BuffCritical)
							{
								m_labelSkillAssistDesc.text = string.Format(text2, _item.skill.assistValue.ToString("#.00"));
							}
							else
							{
								m_labelSkillAssistDesc.text = string.Format(text2, _item.skill.assistValue.ToString("#.##"));
							}
						}
						else
						{
							Dbg.Log("No text need to be replaced in assist skill description");
							m_labelSkillAssistDesc.text = text2;
						}
					}
					else
					{
						m_spriteSkillSelf.enabled = false;
						m_labelSkillSelf.enabled = false;
						m_labelSkillSelfDesc.enabled = false;
						m_spriteSkillAssist.enabled = false;
						m_labelSkillAssist.enabled = false;
						if ((bool)m_labelSkillAssistDesc)
						{
							m_labelSkillAssistDesc.enabled = false;
						}
					}
				}
				else
				{
					if (!showSkillDescriptions)
					{
						m_spriteSkillSelf.enabled = false;
						m_labelSkillSelf.enabled = false;
						m_labelSkillSelfDesc.enabled = false;
					}
					m_spriteSkillAssist.enabled = false;
					m_labelSkillAssist.enabled = false;
					if ((bool)m_labelSkillAssistDesc)
					{
						m_labelSkillAssistDesc.enabled = false;
					}
				}
			}
			else
			{
				if (m_noSkillLabel != null)
				{
					m_noSkillLabel.enabled = true;
				}
				m_spriteSkillSelf.enabled = false;
				m_labelSkillSelf.enabled = false;
				m_labelSkillSelfDesc.enabled = false;
				m_spriteSkillAssist.enabled = false;
				m_labelSkillAssist.enabled = false;
				m_labelSkillAssistDesc.enabled = false;
			}
		}
		else
		{
			m_spriteSkillSelf.enabled = false;
			m_labelSkillSelf.enabled = false;
			if ((bool)m_labelSkillSelfDesc)
			{
				m_labelSkillSelfDesc.enabled = false;
			}
			m_spriteSkillAssist.enabled = false;
			m_labelSkillAssist.enabled = false;
			if ((bool)m_labelSkillAssistDesc)
			{
				m_labelSkillAssistDesc.enabled = false;
			}
			if (m_noSkillLabel != null)
			{
				if (!_item.IsEssence)
				{
					m_noSkillLabel.enabled = true;
				}
				else
				{
					m_noSkillLabel.enabled = false;
				}
			}
		}
		if (!showCell)
		{
			m_cell.m_hidden = true;
			m_cell.Empty = true;
		}
		else
		{
			m_cell.data = _item;
		}
		if (m_tiers != null && _item.IsEquipment)
		{
			m_tiers.m_alignment = m_starsAlignment;
			m_tiers.SetTier(_item);
		}
	}

	public void ShowDoubleXpOrLoot(bool p_isDoubleXp, UIAtlas p_atlas)
	{
		if (!m_cell)
		{
			InitInventoryCell();
		}
		if (p_isDoubleXp)
		{
			m_cell.ShowDoubleXp(p_atlas);
			m_name.text = SingletonComponent<NBLocalization>.Instance.Get("daily_bonus_double_xp_desc");
		}
		else
		{
			m_cell.ShowDoubleLoot(p_atlas);
			m_name.text = SingletonComponent<NBLocalization>.Instance.Get("daily_bonus_double_loot_desc");
		}
		m_level.text = string.Empty;
		m_descriptionContent.text = string.Empty;
		m_descriptionHeadline.text = string.Empty;
		m_spriteSkillSelf.enabled = false;
		m_spriteSkillAssist.enabled = false;
		m_labelSkillSelf.text = string.Empty;
		m_labelSkillSelfDesc.text = string.Empty;
		m_labelSkillAssist.text = string.Empty;
		m_labelSkillAssistDesc.text = string.Empty;
		if (m_tiers != null)
		{
			Object.DestroyImmediate(m_tiers.gameObject);
		}
		ShowStats(false, false);
	}

	public void ShowSocialOrPremGacha(bool p_isSocialGacha, UIAtlas p_atlas)
	{
		if (!m_cell)
		{
			InitInventoryCell();
		}
		if (p_isSocialGacha)
		{
			m_cell.ShowSocialGacha(p_atlas);
			m_name.text = SingletonComponent<NBLocalization>.Instance.Get("daily_bonus_social_gacha");
		}
		else
		{
			m_cell.ShowPremGacha(p_atlas);
			m_name.text = SingletonComponent<NBLocalization>.Instance.Get("daily_bonus_prem_gacha");
		}
		m_level.text = string.Empty;
		m_descriptionContent.text = string.Empty;
		m_descriptionHeadline.text = string.Empty;
		m_spriteSkillSelf.enabled = false;
		m_spriteSkillAssist.enabled = false;
		m_labelSkillSelf.text = string.Empty;
		m_labelSkillSelfDesc.text = string.Empty;
		m_labelSkillAssist.text = string.Empty;
		m_labelSkillAssistDesc.text = string.Empty;
		if (m_tiers != null)
		{
			Object.DestroyImmediate(m_tiers.gameObject);
		}
		ShowStats(false, false);
	}

	private Color GetCompareColor(float itemValue, float equippedValue)
	{
		Color result = ((itemValue > equippedValue) ? betterColor : ((!(itemValue < equippedValue)) ? sameColor : worseColor));
		result.a = alpha;
		return result;
	}

	public void ShowIgc(int p_gold)
	{
		if (!m_cell)
		{
			InitInventoryCell();
		}
		if (!m_tiers)
		{
			InitTiers();
		}
		if (m_tiers != null)
		{
			Object.DestroyImmediate(m_tiers.gameObject);
		}
		m_name.text = string.Empty;
		m_level.text = string.Empty;
		m_health.text = string.Empty;
		m_armour.text = string.Empty;
		m_damage.text = string.Empty;
		m_critical.text = string.Empty;
		m_spriteSkillSelf.enabled = false;
		m_spriteSkillAssist.enabled = false;
		m_labelSkillSelf.enabled = false;
		m_labelSkillAssist.enabled = false;
		if ((bool)m_labelSkillSelfDesc)
		{
			m_labelSkillSelfDesc.enabled = false;
		}
		if ((bool)m_labelSkillAssistDesc)
		{
			m_labelSkillAssistDesc.enabled = false;
		}
		m_cell.m_hidden = false;
		m_healthLabel.enabled = false;
		m_armourLabel.enabled = false;
		m_damageLabel.enabled = false;
		m_criticalLabel.enabled = false;
		m_spriteSkillSelf.enabled = false;
		m_cell.ShowIgc(p_gold);
	}
}
