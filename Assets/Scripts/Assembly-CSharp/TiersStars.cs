using UnityEngine;

public class TiersStars : MonoBehaviour
{
	public enum Alignment
	{
		Left = 0,
		Center = 1,
		Right = 2
	}

	public UIAtlas m_atlas;

	public string m_enableSprite;

	public string m_disableSprite;

	public UISprite[] m_stars;

	public float m_gap = 20f;

	public Alignment m_alignment;

	private void Awake()
	{
		if (m_stars == null)
		{
			Dbg.LogError("m_stars is null. No stars given.");
			base.enabled = false;
		}
		else
		{
			InitStars();
		}
	}

	private void InitStars()
	{
		UISprite[] stars = m_stars;
		foreach (UISprite uISprite in stars)
		{
			uISprite.atlas = m_atlas;
			uISprite.spriteName = m_disableSprite;
		}
	}

	private void EnableStars(bool _enable)
	{
		for (int i = 0; i < m_stars.Length; i++)
		{
			m_stars[i].gameObject.SetActiveRecursively(_enable);
			m_stars[i].enabled = _enable;
		}
	}

	public void SetTier(int _tier, int _maxTier)
	{
		if (_tier < 0 || _maxTier < 0 || _tier > _maxTier || _tier > TierRange.TIER_MAX || _maxTier > TierRange.TIER_MAX)
		{
			Dbg.LogError("TIERS_STARS : Hide tier. Wrong tier number given : " + _tier + " " + _maxTier);
			EnableStars(false);
			return;
		}
		EnableStars(true);
		if (m_alignment == Alignment.Center)
		{
			float num = (float)(-(_maxTier - 1)) * m_gap * 0.5f;
			for (int i = 0; i < m_stars.Length; i++)
			{
				Vector3 localPosition = m_stars[i].transform.localPosition;
				localPosition.x = num + (float)i * m_gap;
				m_stars[i].transform.localPosition = localPosition;
			}
		}
		for (int j = 0; j < _maxTier; j++)
		{
			if (j < m_stars.Length)
			{
				m_stars[j].gameObject.SetActiveRecursively(true);
				m_stars[j].enabled = true;
				if (j < _tier)
				{
					m_stars[j].spriteName = m_enableSprite;
				}
				else
				{
					m_stars[j].spriteName = m_disableSprite;
				}
			}
		}
		for (int k = _maxTier; k < m_stars.Length; k++)
		{
			m_stars[k].enabled = false;
		}
	}

	public void SetTier(ItemDetails _item)
	{
		if (_item == null || _item == ItemManager.s_NULL_DATA)
		{
			SetEmptyTier();
			Dbg.LogWarn("TiersStars : Cannot display this item.");
			return;
		}
		if (_item.Level == 0)
		{
			SetEmptyTier();
			return;
		}
		int tier = _item.Tier;
		int maxTier = _item.equipment.maxTier;
		if (_item.IsEssence)
		{
			maxTier = tier;
		}
		SetTier(tier, maxTier);
	}

	public void SetEmptyTier()
	{
		for (int i = 0; i < m_stars.Length; i++)
		{
			m_stars[i].enabled = false;
			m_stars[i].gameObject.SetActiveRecursively(false);
		}
	}
}
