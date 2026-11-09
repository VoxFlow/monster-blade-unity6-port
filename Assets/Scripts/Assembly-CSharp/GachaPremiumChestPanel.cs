using System.Collections;
using System.Collections.Generic;
using Models;
using UnityEngine;

public class GachaPremiumChestPanel : NBUIMenuPanel
{
	public GachaItem gachaItemPrefab;

	public float smallMoveDistance = 5f;

	public float minSwipeDistance = 15f;

	public float maxSwipeDistance = 40f;

	public GameObject eventTarget;

	public GameObject premiumCamera2;

	public GameObject premiumChestFx;

	public GameObject premiumChest;

	public GameObject premiumDoor;

	public Vector3[] itemScreenPositions = new Vector3[6];

	private List<ServerInventoryItem> _items;

	private GameObject _characterBase;

	private GameObject _prevItem;

	private Dictionary<string, GameObject> _3DModels;

	private GameObject m_resultAnimGameObject;

	private GachaResult m_resultAnim;

	private int m_lastChestTier = 3;

	private bool m_bLoading;

	private float m_lastShowItemTime;

	private void OnDisable()
	{
		Reset();
	}

	private void Clear3DModels()
	{
		foreach (KeyValuePair<string, GameObject> _3DModel in _3DModels)
		{
			ItemDetails byDBID = SingletonComponent<InventoryManager>.Instance.GetByDBID(_3DModel.Key);
			if (byDBID != null)
			{
				byDBID.equipment.characterElement.ReleaseAssets();
			}
			GameObject value = _3DModel.Value;
			if (value != null)
			{
				CharacterElement.DestroyAssetBundleCharacterModel(value);
			}
		}
		_3DModels.Clear();
	}

	private void DebugClear3DModels()
	{
		if (_items != null)
		{
			foreach (ServerInventoryItem item in _items)
			{
				if (item != null)
				{
					ItemDetails itemDetails = SingletonComponent<ItemManager>.Instance.CreateItemDetails(item.item_id);
					if (itemDetails != null)
					{
						itemDetails.equipment.characterElement.ReleaseAssets();
					}
				}
			}
		}
		foreach (KeyValuePair<string, GameObject> _3DModel in _3DModels)
		{
			GameObject value = _3DModel.Value;
			if (value != null)
			{
				CharacterElement.DestroyAssetBundleCharacterModel(value);
			}
		}
		_3DModels.Clear();
	}

	public override void Reset()
	{
		if ((bool)premiumChest)
		{
			premiumChest.GetComponent<Animation>().Rewind();
			premiumChest.GetComponent<Animation>().Play();
			premiumChest.GetComponent<Animation>().Sample();
			premiumChest.GetComponent<Animation>().Stop();
		}
		if (premiumDoor != null)
		{
			premiumDoor.GetComponent<Animation>().Rewind();
			premiumDoor.GetComponent<Animation>().Play();
			premiumDoor.GetComponent<Animation>().Sample();
			premiumDoor.GetComponent<Animation>().Stop();
		}
		if ((bool)premiumCamera2)
		{
			premiumCamera2.SendMessage("Reset", null, SendMessageOptions.DontRequireReceiver);
			ChangeNGUIAlpha componentInChildren = MBHier.GetComponentInChildren<ChangeNGUIAlpha>(premiumCamera2);
			if ((bool)componentInChildren)
			{
				componentInChildren.m_alpha = 0f;
			}
		}
		if ((bool)premiumChestFx)
		{
			ParticleSystem[] componentsInChildren = MBHier.GetComponentsInChildren<ParticleSystem>(premiumChestFx);
			ParticleSystem[] array = componentsInChildren;
			foreach (ParticleSystem particleSystem in array)
			{
				particleSystem.Stop();
				particleSystem.Clear();
			}
		}
		if (_items != null)
		{
			_items.Clear();
			_items = null;
		}
		Clear3DModels();
		_prevItem = null;
		if (m_resultAnim != null)
		{
			Object.Destroy(m_resultAnim);
		}
		Resources.UnloadUnusedAssets();
	}

	private new void Awake()
	{
		premiumCamera2 = GachaCave.Instance.premiumCamera2;
		premiumChestFx = GachaCave.Instance.premiumChestFx;
		premiumChest = GachaCave.Instance.premiumChest;
		premiumDoor = GachaCave.Instance.premiumDoor;
		_3DModels = new Dictionary<string, GameObject>();
	}

	private void OnEnable()
	{
		Init();
	}

	private void Start()
	{
	}

	private void OnCharacterBaseModelCreated(GameObject obj)
	{
		if (!(obj != null))
		{
			return;
		}
		_characterBase = obj;
		Clear3DModels();
		foreach (ServerInventoryItem item in _items)
		{
			string empty = string.Empty;
			empty = item.db_id;
			if (_3DModels.ContainsKey(empty))
			{
				continue;
			}
			_3DModels.Add(empty, null);
			ItemDetails itemDetails = null;
			itemDetails = SingletonComponent<InventoryManager>.Instance.GetByDBID(item.db_id);
			if (itemDetails == null)
			{
				continue;
			}
			CharacterGenerator.GenerateCharacterElementModel(_characterBase, delegate(GameObject _obj)
			{
				if (_obj != null)
				{
					_obj.active = false;
					string empty2 = string.Empty;
					empty2 = itemDetails.DB_ID;
					_3DModels[empty2] = _obj;
				}
			}, itemDetails.equipment.characterElement);
		}
	}

	private void Init()
	{
		parentScreen.backButton.SetActiveRecursively(false);
		_items = SingletonComponent<GachaManager>.Instance.GetLastGachaResult();
		if (_items.Count > 0)
		{
			int p_tier = (m_lastChestTier = _items[0].tier);
			GachaCave.Instance.ShowChest(p_tier, GachaScreen.Choice.PREMIUM);
			Singleton<CharacterGenerator>.Instance.LoadMainMenuCharacterBase(OnCharacterBaseModelCreated);
		}
		premiumCamera2.SendMessage("PlayAnimation", null, SendMessageOptions.DontRequireReceiver);
		StartCoroutine(DoAnimation());
	}

	private IEnumerator DoAnimation()
	{
		premiumDoor.GetComponent<Animation>().Play();
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_premium_gacha_intro");
		yield return new MBWaitForSeconds(3f);   // PORT (CD-059)
		yield return new MBWaitForSeconds(0.5f);   // PORT (CD-059)
		yield return StartCoroutine(DoOpenChest());
	}

	private IEnumerator DoOpenChest()
	{
		premiumChest.GetComponent<Animation>().Play();
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_premium_gacha_reveal");
		yield return new MBWaitForSeconds(0.5f);   // PORT (CD-059)
		ParticleSystem[] particles = MBHier.GetComponentsInChildren<ParticleSystem>(premiumChestFx);
		ParticleSystem[] array = particles;
		foreach (ParticleSystem p in array)
		{
			p.Play();
		}
		yield return new MBWaitForSeconds(1.5f);   // PORT (CD-059)
		ServerInventoryItem item = _items[0];
		StartCoroutine(DoGachaResultAnimation(item));
		_items.RemoveAt(0);
		UIPopupController.Instance.EnableUI();
	}

	private void ShowGachaCave(bool p_show)
	{
		if (null == GachaCave.Instance)
		{
			return;
		}
		ParticleSystem[] componentsInChildren = MBHier.GetComponentsInChildren<ParticleSystem>(premiumChestFx);
		ParticleSystem[] array = componentsInChildren;
		foreach (ParticleSystem particleSystem in array)
		{
			if (p_show)
			{
				particleSystem.Play();
				continue;
			}
			particleSystem.Stop();
			particleSystem.Clear();
		}
		GachaCave.Instance.premiumCave.SetActiveRecursively(p_show);
		if (p_show)
		{
			GachaCave.Instance.ShowChest(m_lastChestTier, GachaScreen.Choice.PREMIUM);
		}
	}

	private IEnumerator DoGachaResultAnimation(ServerInventoryItem p_item)
	{
		ShowGachaCave(false);
		GameObject go = Resources.Load(GLOBAL.GACHA_RESULT_ANIM_PATH) as GameObject;
		if (go != null)
		{
			m_resultAnimGameObject = Object.Instantiate(go) as GameObject;
			if (m_resultAnimGameObject != null)
			{
				m_resultAnim = m_resultAnimGameObject.GetComponent<GachaResult>();
			}
			else
			{
				yield return null;
			}
		}
		else
		{
			yield return null;
		}
		if (m_resultAnim == null)
		{
			yield return null;
		}
		m_resultAnim.m_type = GachaResult.Type.Premium;
		m_resultAnim.m_itemInfo.GetComponent<Collider>().enabled = false;
		UIPanel panel = MBHier.GetComponentInChildren<UIPanel>(UIPopupController.Instance);
		m_resultAnim.transform.parent = panel.transform;
		m_resultAnim.transform.localPosition = Vector3.zero;
		m_resultAnim.transform.localScale = Vector3.one;
		ItemDetails item = null;
		item = SingletonComponent<InventoryManager>.Instance.GetByDBID(p_item.db_id);
		if (item != null)
		{
			StartCoroutine(ShowItemStats(item));
			UIButtonMessage button = m_resultAnim.m_itemInfo.GetComponent<UIButtonMessage>();
			button.target = base.gameObject;
		}
		yield return new MBWaitForSeconds(m_resultAnim.GetComponent<Animation>().clip.length);   // PORT (CD-059)
		m_resultAnim.m_uvAnimBackground.gameObject.SetActiveRecursively(false);
		m_resultAnim.m_itemInfo.alpha = 1f;
		m_resultAnim.m_itemInfo.GetComponent<Collider>().enabled = true;
		Messenger.Invoke("gacha_show_iteminfo");
	}

	private IEnumerator ShowItemStats(ItemDetails item)
	{
		m_resultAnim.elementType = item.Element;
		m_resultAnim.m_spriteFrame.spriteName = item.ElementSprite;
		m_resultAnim.m_spriteItem.atlas = item.BGAtlas;
		m_resultAnim.m_spriteItem.spriteName = item.BGSpriteName;
		if (_prevItem != null)
		{
			_prevItem.active = false;
			_prevItem = null;
		}
		string key = string.Empty;
		key = item.DB_ID;
		GameObject obj;
		if (!_3DModels.TryGetValue(key, out obj) || obj == null)
		{
			m_bLoading = true;
			UIPopupController.Instance.ShowLoadingPopup(string.Empty, string.Empty);
			do
			{
				yield return null;
			}
			while (!_3DModels.TryGetValue(key, out obj) || obj == null);
			UIPopupController.Instance.HideLoadingPopup();
			m_bLoading = false;
		}
		obj.active = true;
		m_lastShowItemTime = UnityEngine.Time.realtimeSinceStartup;
		m_resultAnim.Show3DModel(item, _characterBase, obj.GetComponent<SkinnedMeshRenderer>());
		_prevItem = obj;
		m_resultAnim.m_itemInfo.ShowItem(item, false);
	}

	private void OnClose(GameObject source)
	{
		OnGachaItemPopupClose();
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_ok");
	}

	private IEnumerator SpawnGachaItem(ServerInventoryItem item)
	{
		GachaItem gacha = Object.Instantiate(gachaItemPrefab, Vector3.up * 1000f, Quaternion.identity) as GachaItem;
		Collider c = MBHier.GetComponentInChildren<Collider>(gacha);
		c.enabled = false;
		gacha.transform.parent = base.transform;
		GachaItem gachaItem = gacha.GetComponent<GachaItem>();
		gachaItem.itemDetails = SingletonComponent<InventoryManager>.Instance.GetByDBID(item.db_id);
		gacha.sprite.atlas = gachaItem.itemDetails.BGAtlas;
		gacha.sprite.spriteName = gachaItem.itemDetails.BGSpriteName;
		gachaItem.eventTarget = base.gameObject;
		gachaItem.PlayEffects();
		yield return new MBWaitForSeconds(gachaItem.GetComponent<Animation>().clip.length);   // PORT (CD-059)
		c.enabled = true;
	}

	private void OnGachaItemSelected(GameObject go)
	{
		GachaItem component = go.GetComponent<GachaItem>();
		UIPopupController.Instance.ShowItemStats(component.itemDetails.DB_ID, OnGachaItemPopupClose);
		Object.Destroy(component.gameObject);
	}

	private void OnGachaItemPopupClose()
	{
		if (_items.Count > 0)
		{
			if (!m_bLoading && !(UnityEngine.Time.realtimeSinceStartup < m_lastShowItemTime + 1f))
			{
				ItemDetails itemDetails = null;
				itemDetails = SingletonComponent<InventoryManager>.Instance.GetByDBID(_items[0].db_id);
				StartCoroutine(ShowItemStats(itemDetails));
				_items.RemoveAt(0);
			}
			return;
		}
		m_resultAnim = null;
		if ((bool)m_resultAnimGameObject)
		{
			Object.Destroy(m_resultAnimGameObject);
			m_resultAnimGameObject = null;
		}
		Messenger.Invoke("gacha_done");
		if (SingletonComponent<TutorialManager>.Instance.enabled && SingletonComponent<TutorialManager>.Instance.IsTutorialExisting(Tutorial.State.Gacha2))
		{
			OnGoToGachaHome();
			return;
		}
		ShowGachaCave(true);
		EnableBackButton();
		if (!MainMenu.Instance.CheckExcessItem(null, OnPromptRetry))
		{
			OnPromptRetry();
		}
	}

	private void EnableBackButton()
	{
		BackButtonMapping backButtonMapping = Object.FindObjectOfType(typeof(BackButtonMapping)) as BackButtonMapping;
		if (backButtonMapping != null)
		{
			backButtonMapping.enabled = true;
		}
	}

	private void OnPromptRetry()
	{
		UIPopupController.Instance.ShowMediumPopupMSG("common_prompt_gacha_get_more_header", "common_prompt_gacha_get_more", OnTryAgainYes, OnTryAgainNo);
		EnableBackButton();
	}

	private void OnTryAgainYes()
	{
		if (null != UIPopupController.Instance)
		{
			UIPopupController.Instance.EnableCurPopupButtons(false);
		}
		int previousGachaPrice = SingletonComponent<GachaManager>.Instance.PreviousGachaPrice;
		if (previousGachaPrice < 0)
		{
			OnGoToPremiumOptions();
			return;
		}
		PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		if (!playerData.HasEnoughPremium(previousGachaPrice))
		{
			SingletonComponent<StoreManager>.Instance.ShowDiamondsShop(OnPurchaseDiamondsYes, OnPurchaseDiamondsNo, "common_prompt_gacha_need_diamonds", previousGachaPrice - playerData.m_userData.premiumCurrency);
		}
		else
		{
			OnReplayGacha();
		}
	}

	private void OnTryAgainNo()
	{
		OnGoToPremiumOptions();
	}

	private void OnPurchaseSlotsYes()
	{
		if (!MainMenu.Instance.CheckExcessItem())
		{
			OnPromptRetry();
		}
	}

	private void OnPurchaseSlotsNo()
	{
		OnGoToCharacterScreen();
	}

	private void OnPurchaseDiamondsYes()
	{
		OnReplayGacha();
	}

	private void OnPurchaseDiamondsNo()
	{
		OnGoToPremiumOptions();
	}

	private void OnReplayGacha()
	{
		GachaManager instance = SingletonComponent<GachaManager>.Instance;
		if ((bool)instance)
		{
			GachaEventsInfo lastGachaEvent = instance.GetLastGachaEvent();
			if (lastGachaEvent.filter_by != "element")
			{
				eventTarget.SendMessage("OnGachaPremiumTryAgain", true, SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				eventTarget.SendMessage("OnGachaElementTryAgain", true, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	private void OnGoToPremiumOptions()
	{
		ShowGachaCave(false);
		GachaManager instance = SingletonComponent<GachaManager>.Instance;
		if ((bool)instance)
		{
			GachaEventsInfo lastGachaEvent = instance.GetLastGachaEvent();
			if (lastGachaEvent.filter_by != "element")
			{
				eventTarget.SendMessage("OnGachaPremiumTryAgain", false, SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				eventTarget.SendMessage("OnGachaElementTryAgain", false, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	private void OnGoToCharacterScreen()
	{
		MainMenu instance = MainMenu.Instance;
		if (null != instance && !(instance.currScreen is CharacterMenuScreen))
		{
			instance.LockdownPlayerToCharacterMenuOnly();
		}
	}

	private void OnGoToGachaHome()
	{
		eventTarget.SendMessage("Home", null, SendMessageOptions.DontRequireReceiver);
	}
}
