using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FusionLevelup : MonoBehaviour
{
	public FusionFX m_fxPrefab;

	public UISprite m_spriteSuccess;

	public ItemInfo m_itemInfo;

	public UILabel m_labelXp;

	public UISlider m_sliderXp;

	public UILabel m_labelXpPercent;

	public GameObject m_groupXp;

	public UISprite m_spriteXpFlash;

	public Component eventTarget;

	private FusionFX m_fx;

	private ItemDetails m_baseItem = ItemManager.s_NULL_DATA;

	private ItemDetails m_resultItem;

	private List<ItemDetails> m_otherItems;

	private bool m_playing;

	private bool needToSkip;

	private float timeCounter;

	public FusionEvent evt
	{
		set
		{
			m_baseItem = value.baseItem;
			m_otherItems = value.otherItems;
			m_resultItem = value.resultItem;
		}
	}

	private void OnEnable()
	{
		Init();
	}

	private void OnDisable()
	{
		Reset();
	}

	private void Update()
	{
		if ((MBInput.GetMouseButtonDown(0) || MBInput.touchCount > 0) && m_fx != null)
		{
			m_fx.GetComponent<Animation>()["Anim_Fusion"].normalizedTime = 1f;
			needToSkip = true;
			m_fx.EnableAllParticleEffect(false);
		}
	}

	private void Init()
	{
		Reset();
		if (!m_playing && m_resultItem != null)
		{
			StartCoroutine(PlayFX());
		}
	}

	private void Reset()
	{
		StopCoroutine("PlayFX");
		m_spriteSuccess.enabled = false;
		m_itemInfo.ShowItem(ItemManager.s_NULL_DATA);
		m_itemInfo.alpha = 0f;
		m_itemInfo.GetComponent<Collider>().enabled = false;
		m_sliderXp.sliderValue = 0f;
		m_groupXp.transform.localScale = Vector3.zero;
		if ((bool)m_fx && (bool)m_fx.gameObject)
		{
			Object.Destroy(m_fx.gameObject);
		}
	}

	private IEnumerator PlayFX()
	{
		m_playing = true;
		m_fx = (FusionFX)Object.Instantiate(m_fxPrefab, base.transform.position, Quaternion.identity);
		m_fx.transform.parent = base.transform;
		m_fx.transform.localScale = Vector3.one;
		m_fx.transform.localPosition = Vector3.zero;
		if (m_baseItem != null)
		{
			m_fx.baseSprite.atlas = m_baseItem.BGAtlas;
			m_fx.baseSprite.spriteName = m_baseItem.BGSpriteName;
			m_fx.baseItem.ShowItemTierStars(m_baseItem);
			m_fx.baseFrame.spriteName = m_baseItem.ElementSprite;
		}
		if (m_resultItem != null)
		{
			ItemDetails item = SingletonComponent<InventoryManager>.Instance.GetByDBID(m_resultItem.DB_ID);
			if (item != null && m_fx.newItemSprite != null)
			{
				m_fx.newItemSprite.atlas = item.BGAtlas;
				m_fx.newItemSprite.spriteName = item.BGSpriteName;
			}
		}
		if (m_otherItems != null)
		{
			int count = m_otherItems.Count;
			int maxItems = m_fx.items.Length;
			for (int i = count; i < maxItems; i++)
			{
				m_fx.items[i].gameObject.SetActiveRecursively(false);
			}
			for (int j = 0; j < count; j++)
			{
				m_fx.otherSprites[j].atlas = m_otherItems[j].BGAtlas;
				m_fx.otherSprites[j].spriteName = m_otherItems[j].BGSpriteName;
				m_fx.otherFrames[j].spriteName = m_otherItems[j].ElementSpriteSmall;
				m_fx.items[j].ShowItemTierStars(m_otherItems[j]);
			}
		}
		yield return null;
		FusionMaterialItem[] items = m_fx.items;
		foreach (FusionMaterialItem go in items)
		{
			iTweenPath path = go.GetComponent<iTweenPath>();
			int pathCount = path.nodes.Count;
			for (int l = 0; l < pathCount; l++)
			{
				List<Vector3> nodes;
				List<Vector3> list = (nodes = path.nodes);
				int index2;
				int index = (index2 = l);
				Vector3 vector = nodes[index2];
				list[index] = vector * GLOBAL.UI_SCALE_FACTOR;
				List<Vector3> nodes2;
				List<Vector3> list2 = (nodes2 = path.nodes);
				int index3 = (index2 = l);
				vector = nodes2[index2];
				list2[index3] = vector + base.transform.position;
			}
		}
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_fusion_activation");
		float waitTime = m_fx.GetComponent<Animation>().clip.length * 0.6f;
		while (timeCounter < waitTime)
		{
			if (needToSkip)
			{
				timeCounter = waitTime;
			}
			timeCounter += Time.deltaTime;
			yield return null;
		}
		if (!needToSkip)
		{
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_progression_bar_fill");
		}
		timeCounter = 0f;
		waitTime = m_fx.GetComponent<Animation>().clip.length * 0.4f;
		while (timeCounter < waitTime)
		{
			if (needToSkip)
			{
				timeCounter = waitTime;
			}
			timeCounter += Time.deltaTime;
			yield return null;
		}
		if (!needToSkip)
		{
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_fusion_complete");
		}
		m_spriteSuccess.enabled = true;
		Vector3 toScale = m_spriteSuccess.transform.localScale;
		m_spriteSuccess.transform.localScale = Vector3.zero;
		iTween.ScaleTo(m_spriteSuccess.gameObject, iTween.Hash("scale", toScale, "time", 0.25f, "easetype", iTween.EaseType.easeOutBack));
		timeCounter = 0f;
		waitTime = 0.5f;
		while (timeCounter < waitTime)
		{
			if (needToSkip)
			{
				timeCounter = waitTime;
			}
			timeCounter += Time.deltaTime;
			yield return null;
		}
		m_itemInfo.ShowItem(m_baseItem, false, true, true);
		m_itemInfo.m_level.text = "LVL " + m_baseItem.Level + "/" + m_baseItem.MaxLevelInTier;
		float r = (float)m_baseItem.XP * 1f / (float)m_baseItem.fusion.xpToLevelUp;
		m_labelXpPercent.text = (int)(r * 100f) + " %";
		m_sliderXp.sliderValue = r;
		StartCoroutine(FadeInItemInfo(0.5f));
		iTween.ScaleTo(m_groupXp, iTween.Hash("scale", Vector3.one, "time", 0.5f, "easetype", iTween.EaseType.easeInOutBack));
		int levelUps = m_resultItem.Level - m_baseItem.Level;
		float s = (float)m_resultItem.XP * 1f / (float)m_resultItem.fusion.xpToLevelUp;
		float myXp = r * 100f;
		float targetXp = s * 100f;
		float countDuration = 1.5f;
		float dx = 100f * Time.fixedDeltaTime / countDuration;
		int myLevel = m_baseItem.Level;
		int endLevel = m_resultItem.Level;
		Dbg.LogWarn("levelups = " + levelUps + ", s = " + s + ", targetXp = " + targetXp + ", myXp = " + myXp);
		int counter = 0;
		while (counter <= levelUps)
		{
			yield return null;
			myXp += dx;
			if (counter == levelUps && myXp > targetXp)
			{
				myXp = targetXp;
				counter++;
			}
			float val = myXp / 100f;
			m_sliderXp.sliderValue = val;
			m_labelXpPercent.text = (int)(val * 100f) + " %";
			if (needToSkip)
			{
				break;
			}
			SetFlashXpBarAlphaValue(val, counter);
			if (val >= 1f && counter < levelUps)
			{
				myXp = 0f;
				m_sliderXp.sliderValue = 1f;
				m_labelXpPercent.text = "100 %";
				m_itemInfo.ShowItemVsOld(m_resultItem, m_baseItem, (float)(myLevel + counter + 1) / (float)endLevel);
				m_itemInfo.m_level.text = "LVL " + myLevel + "/" + m_baseItem.MaxLevelInTier + NBColorTags.TAG_GREEN + "->LVL " + NBColorTags.TAG_GREEN + (myLevel + counter + 1) + "/" + m_baseItem.MaxLevelInTier + NBColorTags.TAG_END;
				counter++;
				yield return null;
			}
		}
		if (needToSkip)
		{
			m_itemInfo.ShowItemVsOld(m_resultItem, m_baseItem, 1f);
			m_sliderXp.sliderValue = s;
			m_labelXpPercent.text = (int)targetXp + " %";
			if (levelUps > 0)
			{
				m_itemInfo.m_level.text = "LVL " + myLevel + "/" + m_baseItem.MaxLevelInTier + NBColorTags.TAG_GREEN + "->LVL " + endLevel + "/" + m_baseItem.MaxLevelInTier + NBColorTags.TAG_END;
			}
			else
			{
				m_itemInfo.m_level.text = "LVL " + myLevel + "/" + m_baseItem.MaxLevelInTier;
			}
			needToSkip = false;
		}
		m_spriteXpFlash.alpha = 0f;
		m_itemInfo.m_level.cachedTransform.localScale = Vector3.one * 30f;
		m_itemInfo.m_level.color = Color.white;
		m_playing = false;
		Messenger.Invoke("fusion_show_result");
		m_itemInfo.GetComponent<Collider>().enabled = true;
	}

	private IEnumerator FadeInItemInfo(float duration)
	{
		for (float fadeTime = 0f; fadeTime <= duration; fadeTime += Time.fixedDeltaTime)
		{
			yield return null;
			m_itemInfo.alpha = Mathf.Lerp(0f, 1f, fadeTime / duration);
		}
		m_itemInfo.alpha = 1f;
	}

	private void SetFlashXpBarAlphaValue(float val, int k)
	{
		if (val >= 0.95f)
		{
			float num = (val - 0.95f) / 0.050000012f;
			m_spriteXpFlash.alpha = num;
			m_itemInfo.m_level.cachedTransform.localScale = Vector3.one * (30f * (1f + num * 0.2f));
			m_itemInfo.m_level.color = Color.Lerp(Color.white, Color.yellow, num);
		}
		else if (k >= 1 && val <= 0.050000012f)
		{
			float num2 = val / 0.050000012f;
			float num3 = 1f - num2;
			m_spriteXpFlash.alpha = num3;
			m_itemInfo.m_level.cachedTransform.localScale = Vector3.one * (30f * (1f + num3 * 0.2f));
			m_itemInfo.m_level.color = Color.Lerp(Color.white, Color.yellow, num3);
		}
		else
		{
			m_spriteXpFlash.alpha = 0f;
			m_itemInfo.m_level.cachedTransform.localScale = Vector3.one * 30f;
			m_itemInfo.m_level.color = Color.white;
		}
	}
}
