using System.Collections;
using UnityEngine;

public class NewRevealItemInfo : MonoBehaviour
{
	[SerializeField]
	private GameObject[] m_elementEffects;

	[SerializeField]
	private InventoryCell m_inventoryCell;

	[SerializeField]
	private UILabel m_itemNameText;

	[SerializeField]
	private UILabel m_itemHitPointText;

	[SerializeField]
	private UILabel m_itemArmorText;

	[SerializeField]
	private UILabel m_itemAttackText;

	[SerializeField]
	private UILabel m_itemCriticalText;

	[SerializeField]
	private UILabel m_skillNameSelfText;

	[SerializeField]
	private UILabel m_skillDescSelfText;

	[SerializeField]
	private UISprite m_skillSelfIcon;

	[SerializeField]
	private UILabel m_skillNameAssistText;

	[SerializeField]
	private UILabel m_skillDescAssistText;

	[SerializeField]
	private UISprite m_skillAssistIcon;

	[SerializeField]
	private UISprite m_statsLeftIcon;

	[SerializeField]
	private UISprite m_statRightIcon;

	private ItemDetails m_itemDetails;

	private bool m_isShowing;

	private Animation m_animation;

	private float tempColor;

	private void OnEnable()
	{
		for (int i = 0; i < m_elementEffects.Length; i++)
		{
			if (i != (int)m_itemDetails.Element)
			{
				m_elementEffects[i].SetActiveRecursively(false);
			}
		}
		m_itemNameText.text = SingletonComponent<NBLocalization>.Instance.Get(m_itemDetails.Name);
		m_itemNameText.color = new Color(1f, 1f, 1f, 0f);
		m_itemNameText.enabled = false;
		m_itemHitPointText.text = Mathf.RoundToInt(m_itemDetails.itemTypeWithSkill.hp).ToString();
		m_itemHitPointText.color = new Color(1f, 1f, 1f, 0f);
		m_itemHitPointText.enabled = false;
		m_itemArmorText.text = Mathf.RoundToInt(m_itemDetails.itemTypeWithSkill.armour).ToString();
		m_itemArmorText.color = new Color(1f, 1f, 1f, 0f);
		m_itemArmorText.enabled = false;
		m_itemAttackText.text = Mathf.RoundToInt(m_itemDetails.itemTypeWithSkill.minDamage) + "-" + Mathf.RoundToInt(m_itemDetails.itemTypeWithSkill.maxDamage);
		m_itemAttackText.color = new Color(1f, 1f, 1f, 0f);
		m_itemAttackText.enabled = false;
		m_itemCriticalText.text = string.Format("{0:0.00}%", m_itemDetails.itemTypeWithSkill.critical);
		m_itemCriticalText.color = new Color(1f, 1f, 1f, 0f);
		m_itemCriticalText.enabled = false;
		m_statsLeftIcon.color = new Color(1f, 1f, 1f, 0f);
		m_statRightIcon.color = new Color(1f, 1f, 1f, 0f);
		m_skillNameSelfText.text = SingletonComponent<NBLocalization>.Instance.Get(m_itemDetails.skill.visual.label_self);
		m_skillNameSelfText.enabled = false;
		m_skillNameAssistText.text = SingletonComponent<NBLocalization>.Instance.Get(m_itemDetails.skill.visual.label_assist);
		m_skillNameAssistText.enabled = false;
		string format = SingletonComponent<NBLocalization>.Instance.Get(m_itemDetails.skill.visual.description_self);
		m_skillDescSelfText.text = string.Format(format, m_itemDetails.skill.directValue);
		m_skillDescSelfText.enabled = false;
		string format2 = SingletonComponent<NBLocalization>.Instance.Get(m_itemDetails.skill.visual.description_assist);
		m_skillDescAssistText.text = string.Format(format2, m_itemDetails.skill.assistValue);
		m_skillDescAssistText.enabled = false;
		m_skillSelfIcon.spriteName = m_itemDetails.skill.visual.sprite_self;
		m_skillSelfIcon.enabled = false;
		m_skillAssistIcon.spriteName = m_itemDetails.skill.visual.sprite_assist;
		m_skillAssistIcon.enabled = false;
		m_inventoryCell.HideChildren();
		m_inventoryCell.data = m_itemDetails;
		m_isShowing = true;
	}

	private void OnDisable()
	{
		tempColor = 0f;
		m_skillNameSelfText.text = string.Empty;
		m_skillNameAssistText.text = string.Empty;
		m_isShowing = false;
	}

	private void Awake()
	{
		m_animation = GetComponent<Animation>();
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (m_animation.isPlaying && m_animation["Anim_Reveal"].normalizedTime > 0.99f)
		{
			m_animation.Stop("Anim_Reveal");
			StartCoroutine(FadeInTexts());
		}
		if (MBInput.touchCount > 0 || MBInput.GetMouseButtonDown(0))
		{
			m_animation.Stop("Anim_Reveal");
			m_isShowing = false;
			m_itemDetails = null;
			base.gameObject.SetActiveRecursively(false);
		}
	}

	private IEnumerator FadeInTexts()
	{
		yield return new MBWaitForSeconds(0.4f);   // PORT (CD-059)
		m_itemNameText.enabled = true;
		m_itemHitPointText.enabled = true;
		m_itemArmorText.enabled = true;
		m_itemAttackText.enabled = true;
		m_itemCriticalText.enabled = true;
		m_statsLeftIcon.enabled = true;
		m_statRightIcon.enabled = true;
		bool needChangeSelfSkill = false;
		if (!string.IsNullOrEmpty(m_skillNameSelfText.text))
		{
			m_skillNameSelfText.enabled = true;
			m_skillDescSelfText.enabled = true;
			m_skillSelfIcon.enabled = true;
			needChangeSelfSkill = true;
		}
		bool needChangeAssistSkill = false;
		if (!string.IsNullOrEmpty(m_skillNameAssistText.text))
		{
			m_skillNameAssistText.enabled = true;
			m_skillDescAssistText.enabled = true;
			m_skillAssistIcon.enabled = true;
			needChangeAssistSkill = true;
		}
		while (tempColor < 1f)
		{
			tempColor += Time.deltaTime;
			m_itemNameText.color += new Color(0f, 0f, 0f, tempColor);
			m_itemHitPointText.color += new Color(0f, 0f, 0f, tempColor);
			m_itemArmorText.color += new Color(0f, 0f, 0f, tempColor);
			m_itemAttackText.color += new Color(0f, 0f, 0f, tempColor);
			m_itemCriticalText.color += new Color(0f, 0f, 0f, tempColor);
			m_statsLeftIcon.color += new Color(0f, 0f, 0f, tempColor);
			m_statRightIcon.color += new Color(0f, 0f, 0f, tempColor);
			if (needChangeSelfSkill)
			{
				m_skillNameSelfText.color += new Color(0f, 0f, 0f, tempColor);
				m_skillDescSelfText.color += new Color(0f, 0f, 0f, tempColor);
				m_skillSelfIcon.color += new Color(0f, 0f, 0f, tempColor);
			}
			if (needChangeAssistSkill)
			{
				m_skillNameAssistText.color += new Color(0f, 0f, 0f, tempColor);
				m_skillDescAssistText.color += new Color(0f, 0f, 0f, tempColor);
				m_skillAssistIcon.color += new Color(0f, 0f, 0f, tempColor);
			}
			yield return null;
		}
	}

	public void SetItemDetails(ItemDetails p_itemDetails)
	{
		m_itemDetails = p_itemDetails;
	}

	public bool IsShowing()
	{
		return m_isShowing;
	}
}
