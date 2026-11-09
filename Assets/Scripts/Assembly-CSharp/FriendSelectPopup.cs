using UnityEngine;

public class FriendSelectPopup : MonoBehaviour
{
	[SerializeField]
	private InventoryCell m_weaponCell;

	[SerializeField]
	private UILabel m_weaponName;

	[SerializeField]
	private UISprite m_weaponSkillSprite;

	[SerializeField]
	private UILabel m_weaponSkillName;

	[SerializeField]
	private UILabel m_weaponSkillDesc;

	[SerializeField]
	private UILabel m_popupDescription;

	[SerializeField]
	private UITexture m_friendPic;

	[SerializeField]
	private UILabel m_friendName;

	[SerializeField]
	private UILabel m_noSkillLabel;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void SetData(PlayerData p_playerData)
	{
		NBLocalization instance = SingletonComponent<NBLocalization>.Instance;
		if (instance == null)
		{
			return;
		}
		Equipment.Data equipmentData = p_playerData.GetEquipmentData(Equipment.Part.Weapon);
		if (equipmentData == null || equipmentData.itemDetails == null || equipmentData.itemDetails.skill == null)
		{
			return;
		}
		if (m_weaponName != null)
		{
			m_weaponName.text = instance.Get(equipmentData.itemDetails.Name);
		}
		if (m_weaponCell != null)
		{
			m_weaponCell.data = equipmentData.itemDetails;
		}
		if (m_friendName != null)
		{
			m_friendName.text = p_playerData.Name;
		}
		if (equipmentData.itemDetails.skill != null && equipmentData.itemDetails.skill.assistAction != SkillEnum.AssistSkill.None && equipmentData.itemDetails.skill.visual.skill != ItemEnum.Skill.WEAPON_NONE)
		{
			SkillVisual visual = equipmentData.itemDetails.skill.visual;
			if (m_weaponSkillSprite != null)
			{
				m_weaponSkillSprite.spriteName = visual.sprite_assist;
			}
			if (m_weaponSkillName != null)
			{
				m_weaponSkillName.text = instance.Get(visual.label_assist);
			}
			if (m_weaponSkillDesc != null)
			{
				string text = SingletonComponent<NBLocalization>.Instance.Get(visual.description_assist);
				if (text.Contains("{0"))
				{
					m_weaponSkillDesc.text = string.Format(text, NBColorTags.TAG_GREEN + equipmentData.itemDetails.skill.assistValue + NBColorTags.TAG_END);
				}
			}
			if (m_noSkillLabel != null)
			{
				m_noSkillLabel.enabled = false;
			}
		}
		else
		{
			if (m_weaponSkillSprite != null)
			{
			}
			if (m_weaponSkillName != null)
			{
				m_weaponSkillName.enabled = false;
			}
			if (m_weaponSkillDesc != null)
			{
				m_weaponSkillDesc.enabled = false;
			}
		}
	}

	private void SetFriendPicture(Texture p_tex)
	{
		if (m_friendPic == null)
		{
			return;
		}
		Shader shader = Shader.Find("Unlit/Transparent Colored");
		if (!(shader == null))
		{
			Material material = new Material(shader);
			if (!(material == null))
			{
				material.mainTexture = p_tex;
				m_friendPic.material = material;
			}
		}
	}
}
