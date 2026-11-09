using UnityEngine;

public class ShopScreen : NBUIMenuScreen
{
	private StoreManager store;

	public UISprite m_bgLeft;

	public UISprite m_bgRight;

	private void Awake()
	{
		Camera camera = null;
		Transform parent = base.transform;
		while (parent.parent != null)
		{
			camera = parent.gameObject.GetComponent<Camera>();
			if (camera != null)
			{
				break;
			}
			parent = parent.parent;
		}
		if (camera != null)
		{
			Vector2 vector = new Vector2(camera.aspect * camera.orthographicSize * 2f, camera.orthographicSize * 2f);
			float num = vector.x * 0.51f;
			if (m_bgLeft != null)
			{
				m_bgLeft.transform.localScale = new Vector3(num, vector.y, 1f);
			}
			if (m_bgRight != null)
			{
				m_bgRight.transform.localScale = new Vector3(0f - num, vector.y, 1f);
			}
		}
	}

	public override void OnEnable()
	{
		store = SingletonComponent<StoreManager>.Instance;
		if (store != null && !store.IsEnabled)
		{
			UIPopupController.Instance.ShowLoadingPopup("shop_header", "shop_not_enable");
			store.ReEnableStore(OnCompleted);
		}
		if (MainMenu.Instance != null && backButton == null && MainMenu.Instance.m_sharedBackButton != null)
		{
			backButton = MainMenu.Instance.m_sharedBackButton.gameObject;
			m_backButton = MainMenu.Instance.m_sharedBackButton;
		}
		PushPanel(menuPanels[0]);
		if (backButton != null)
		{
			backButton.gameObject.SetActiveRecursively(false);
		}
	}

	public override void OnDisable()
	{
		ClearStack();
	}

	private void OnCompleted(bool _completed)
	{
		UIPopupController.Instance.HidePopup();
		if (_completed)
		{
			UIPopupController.Instance.ShowOKPopupMSG("shop_header", "shop_became_enable");
		}
		else
		{
			UIPopupController.Instance.ShowOKPopupMSG("shop_header", "shop_not_enable");
		}
	}
}
