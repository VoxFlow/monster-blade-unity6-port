using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvolveLevelup : MonoBehaviour
{
	public FusionFX[] m_fxPrefabs = new FusionFX[6];

	public UISprite m_spriteSuccess;

	public ItemInfo m_itemInfo;

	private ItemDetails m_baseItem = ItemManager.s_NULL_DATA;

	private ItemDetails m_resultItem;

	private List<ItemDetails> m_otherItems;

	private bool m_playing;

	private FusionFX m_fx;

	public EvolveEvent evt
	{
		set
		{
			m_baseItem = value.itemDetails;
			Dbg.LogWarn("assign evt = " + value.otherDetails.Count);
			m_otherItems = value.otherDetails;
			Dbg.LogWarn("other items = " + m_otherItems.Count);
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

	private void Reset()
	{
		m_spriteSuccess.enabled = false;
		m_itemInfo.ShowItem(ItemManager.s_NULL_DATA);
		m_itemInfo.alpha = 0f;
	}

	private void Init()
	{
		Reset();
		if (!m_playing && m_resultItem != null)
		{
			StartCoroutine(PlayFX());
		}
	}

	private IEnumerator PlayFX()
	{
		m_playing = true;
		int count = m_otherItems.Count;
		if (count > m_fxPrefabs.Length)
		{
			m_playing = false;
			yield break;
		}
		m_fx = Object.Instantiate(m_fxPrefabs[count - 1], Vector3.up * 1000f, Quaternion.identity) as FusionFX;
		yield return null;
		FusionMaterialItem[] items = m_fx.items;
		foreach (FusionMaterialItem go in items)
		{
			iTweenPath path = go.gameObject.GetComponent<iTweenPath>();
			int pathCount = path.nodes.Count;
			for (int j = 0; j < pathCount; j++)
			{
				List<Vector3> nodes;
				List<Vector3> list = (nodes = path.nodes);
				int index2;
				int index = (index2 = j);
				Vector3 vector = nodes[index2];
				list[index] = vector * GLOBAL.UI_SCALE_FACTOR;
				List<Vector3> nodes2;
				List<Vector3> list2 = (nodes2 = path.nodes);
				int index3 = (index2 = j);
				vector = nodes2[index2];
				list2[index3] = vector + base.transform.position;
			}
		}
		m_fx.transform.parent = base.transform;
		m_fx.transform.localPosition = Vector3.zero;
		m_fx.baseSprite.atlas = m_baseItem.BGAtlas;
		m_fx.baseSprite.spriteName = m_baseItem.BGSpriteName;
		m_fx.baseFrame.spriteName = m_baseItem.ElementSprite;
		if (count > 1)
		{
			for (int k = 0; k < count; k++)
			{
				m_fx.otherSprites[k].atlas = m_otherItems[k].BGAtlas;
				m_fx.otherSprites[k].spriteName = m_otherItems[k].BGSpriteName;
				m_fx.otherFrames[k].spriteName = m_otherItems[k].ElementSpriteSmall;
			}
		}
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_evolution_activation");
		yield return new MBWaitForSeconds(m_fx.GetComponent<Animation>().clip.length);   // PORT (CD-059)
		m_spriteSuccess.enabled = true;
		Vector3 toScale = m_spriteSuccess.transform.localScale;
		m_spriteSuccess.transform.localScale = Vector3.zero;
		iTween.ScaleTo(m_spriteSuccess.gameObject, iTween.Hash("scale", toScale, "time", 0.25f, "easetype", iTween.EaseType.easeOutBack));
		yield return new MBWaitForSeconds(0.5f);   // PORT (CD-059)
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_evolution_complete");
		m_itemInfo.ShowItem(m_baseItem);
		StartCoroutine(FadeInItemInfo(0.5f));
		m_playing = false;
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

	private void Start()
	{
	}
}
