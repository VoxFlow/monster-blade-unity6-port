using System.Collections;
using System.Collections.Generic;
using Models;
using UnityEngine;

public class GachaSocialChestPanel : NBUIMenuPanel
{
	public GameObject gachaItemPrefab;

	public float smallMoveDistance = 5f;

	public float minSwipeDistance = 15f;

	public float maxSwipeDistance = 40f;

	public GameObject eventTarget;

	public GameObject socialCamera;

	public GameObject socialChestFx;

	public GameObject socialChest;

	private List<ServerInventoryItem> _items;

	private GameObject m_resultAnimGameObject;

	private GachaResult m_resultAnim;

	private int m_lastChestTier = 1;

	private bool m_bLoading;

	private GameObject _prevItem;

	private GameObject _characterBase;

	private Dictionary<string, GameObject> _3DModels;

	private float m_lastShowItemTime;

	private void OnDestroy()
	{
	}

	private void OnDisable()
	{
		Reset();
	}

	public override void Reset()
	{
		if ((bool)socialChest)
		{
			socialChest.GetComponent<Animation>().Rewind();
			socialChest.GetComponent<Animation>().Play();
			socialChest.GetComponent<Animation>().Sample();
			socialChest.GetComponent<Animation>().Stop();
		}
		if ((bool)socialCamera)
		{
			socialCamera.SendMessage("Reset", null, SendMessageOptions.DontRequireReceiver);
			ChangeNGUIAlpha componentInChildren = MBHier.GetComponentInChildren<ChangeNGUIAlpha>(socialCamera);
			if ((bool)componentInChildren)
			{
				componentInChildren.m_alpha = 0f;
			}
		}
		if ((bool)socialChestFx)
		{
			ParticleSystem[] componentsInChildren = MBHier.GetComponentsInChildren<ParticleSystem>(socialChestFx);
			ParticleSystem[] array = componentsInChildren;
			foreach (ParticleSystem particleSystem in array)
			{
				particleSystem.Stop();
				particleSystem.Clear();
			}
		}
	}

	private void OnEnable()
	{
		Init();
	}

	private new void Awake()
	{
		socialCamera = GachaCave.Instance.socialCamera1;
		socialChestFx = GachaCave.Instance.socialChestFx;
		socialChest = GachaCave.Instance.socialChest;
		_3DModels = new Dictionary<string, GameObject>();
	}

	private void Start()
	{
	}

	private void Init()
	{
		parentScreen.backButton.SetActiveRecursively(false);
		_items = SingletonComponent<GachaManager>.Instance.GetLastGachaResult();
		if (_items.Count > 0)
		{
			m_lastChestTier = _items[0].tier;
			Singleton<CharacterGenerator>.Instance.LoadMainMenuCharacterBase(OnCharacterBaseModelCreated);
		}
		Dbg.LogWarn("Got items = " + _items.Count);
		StartCoroutine(DoAnimation());
	}

	private IEnumerator DoAnimation()
	{
		MainMenu.Instance.ShowTopAndBottomBar(false);
		socialCamera.SendMessage("PlayAnimation", null, SendMessageOptions.DontRequireReceiver);
		yield return new MBWaitForSeconds(3f);   // PORT (CD-059)
		yield return new MBWaitForSeconds(0.5f);   // PORT (CD-059)
		yield return StartCoroutine(DoOpenChest());
	}

	private IEnumerator DoOpenChest()
	{
		socialChest.GetComponent<Animation>().Play();
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_social_gacha_reveal");
		yield return new MBWaitForSeconds(0.5f);   // PORT (CD-059)
		ParticleSystem[] particles = MBHier.GetComponentsInChildren<ParticleSystem>(socialChestFx);
		ParticleSystem[] array = particles;
		foreach (ParticleSystem p in array)
		{
			p.Play();
		}
		yield return new MBWaitForSeconds(1.5f);   // PORT (CD-059)
		if (_items.Count > 0)
		{
			ServerInventoryItem item = _items[0];
			StartCoroutine(DoGachaResultAnimation(item));
			_items.RemoveAt(0);
		}
		UIPopupController.Instance.EnableUI();
	}

	private void ShowGachaCave(bool p_show)
	{
		ParticleSystem[] componentsInChildren = MBHier.GetComponentsInChildren<ParticleSystem>(socialChestFx);
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
		GachaCave.Instance.socialCave.SetActiveRecursively(p_show);
		if (p_show)
		{
			GachaCave.Instance.ShowChest(m_lastChestTier, GachaScreen.Choice.FRIENDSHIP);
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
		m_resultAnim.m_type = GachaResult.Type.Social;
		m_resultAnim.m_itemInfo.GetComponent<Collider>().enabled = false;
		UIPanel panel = MBHier.GetComponentInChildren<UIPanel>(UIPopupController.Instance);
		m_resultAnim.transform.parent = panel.transform;
		m_resultAnim.transform.localPosition = Vector3.zero;
		m_resultAnim.transform.localScale = Vector3.one;
		ItemDetails item = SingletonComponent<InventoryManager>.Instance.GetByDBID(p_item.db_id);
		if (item != null)
		{
			StartCoroutine(ShowItemStatsEx(item));
			UIButtonMessage button = m_resultAnim.m_itemInfo.GetComponent<UIButtonMessage>();
			button.target = base.gameObject;
		}
		yield return new MBWaitForSeconds(m_resultAnim.GetComponent<Animation>().clip.length);   // PORT (CD-059)
		m_resultAnim.m_uvAnimBackground.gameObject.SetActiveRecursively(false);
		m_resultAnim.m_itemInfo.alpha = 1f;
		m_resultAnim.m_itemInfo.GetComponent<Collider>().enabled = true;
		Messenger.Invoke("gacha_show_iteminfo");
	}

	private void ShowItemStats(ItemDetails item)
	{
		m_resultAnim.elementType = item.Element;
		m_resultAnim.m_spriteFrame.spriteName = item.ElementSprite;
		m_resultAnim.m_spriteItem.atlas = item.BGAtlas;
		m_resultAnim.m_spriteItem.spriteName = item.BGSpriteName;
		m_resultAnim.Show3DModel(item);
		m_resultAnim.m_itemInfo.ShowItem(item, false);
	}

	private void OnClose(GameObject source)
	{
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_ok");
		OnGachaItemPopupClose();
	}

	private void OnGachaItemSelected(GameObject go)
	{
		GachaItem component = go.GetComponent<GachaItem>();
		UIPopupController.Instance.ShowItemStats(component.itemDetails.DB_ID, OnGachaItemPopupClose);
		Object.Destroy(component.gameObject);
	}

	private void OnGachaItemPopupClose()
	{
		Dbg.Log("Trying to close the social chest panel");
		if (_items.Count > 0)
		{
			if (!m_bLoading && !(UnityEngine.Time.realtimeSinceStartup < m_lastShowItemTime + 1f))
			{
				ItemDetails itemDetails = null;
				itemDetails = SingletonComponent<InventoryManager>.Instance.GetByDBID(_items[0].db_id);
				StartCoroutine(ShowItemStatsEx(itemDetails));
				_items.RemoveAt(0);
			}
			return;
		}
		m_resultAnim = null;
		if (m_resultAnimGameObject != null)
		{
			Object.Destroy(m_resultAnimGameObject);
			m_resultAnimGameObject = null;
		}
		Messenger.Invoke("gacha_done");
		if (SingletonComponent<TutorialManager>.Instance.enabled && SingletonComponent<TutorialManager>.Instance.IsTutorialExisting(Tutorial.State.Gacha))
		{
			OnFinished();
			return;
		}
		int num = MainMenu.Instance.inventory.HowManyExcessItems();
		if (num > 0)
		{
			if (SingletonComponent<InventoryManager>.Instance.MaxSpace >= GLOBAL.GAME_SETTINGS.slot_limitation)
			{
				MainMenu.Instance.LockdownPlayerToCharacterMenuOnly();
				return;
			}
			ShowGachaCave(true);
			SingletonComponent<StoreManager>.Instance.InventoryExcess(num, OnPurchaseSlotsYes, OnPurchaseSlotsNo);
			BackButtonMapping backButtonMapping = Object.FindObjectOfType(typeof(BackButtonMapping)) as BackButtonMapping;
			if (backButtonMapping != null)
			{
				backButtonMapping.enabled = true;
			}
		}
		else
		{
			OnFinished();
		}
	}

	private IEnumerator ShowItemStatsEx(ItemDetails item)
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

	private void OnPurchaseSlotsYes()
	{
		int num = MainMenu.Instance.inventory.HowManyExcessItems();
		if (num > 0)
		{
			OnGoToCharacterScreen();
		}
		else
		{
			OnFinished();
		}
	}

	private void OnPurchaseSlotsNo()
	{
		OnGoToCharacterScreen();
	}

	private void OnGoToCharacterScreen()
	{
		MainMenu instance = MainMenu.Instance;
		if (null != instance && !(instance.currScreen is CharacterMenuScreen))
		{
			instance.LockdownPlayerToCharacterMenuOnly();
		}
	}

	private void OnFinished()
	{
		eventTarget.SendMessage("Home", null, SendMessageOptions.DontRequireReceiver);
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
}
