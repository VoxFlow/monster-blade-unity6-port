using System.Collections.Generic;
using UnityEngine;

public class OptionMenuPanel : StandardMenuPanel
{
	[SerializeField]
	private UIBadge m_promoCodeBadge;

	[SerializeField]
	private GameObject m_promoCodeButton;

	[SerializeField]
	private float m_offset = 150f;

	private List<Vector3> buttonOrigin;

	public override void OnEnable()
	{
		base.OnEnable();
		Messenger.AddListener("promocode_update_badge", OnPromoCodeUpdate);
		Init();
	}

	public override void OnDisable()
	{
		base.OnDisable();
		Messenger.RemoveListener("promocode_update_badge", OnPromoCodeUpdate);
	}

	private void OnPromoCodeUpdate()
	{
		EnablePromoCode();
		if (null != m_promoCodeBadge)
		{
			DailyManager instance = SingletonComponent<DailyManager>.Instance;
			if (null != instance && instance.m_promoCodeDatas != null)
			{
				int activePromoCodeDataCount = instance.GetActivePromoCodeDataCount(true);
				m_promoCodeBadge.SetNumber(activePromoCodeDataCount);
			}
		}
	}

	private void EnablePromoCode()
	{
		bool enable_promo_code = GLOBAL.GAME_SETTINGS.enable_promo_code;
		if (null != m_promoCodeButton)
		{
			m_promoCodeButton.SetActiveRecursively(enable_promo_code);
		}
		for (int i = 0; i < buttons.Length; i++)
		{
			GameObject gameObject = buttons[i];
			if (!(null == gameObject) && 0 == 0)
			{
				float p_value = ((!enable_promo_code) ? (buttonOrigin[i].x + m_offset) : buttonOrigin[i].x);
				gameObject.transform.SetLocalX(p_value);
			}
		}
	}

	private void Init()
	{
		OnPromoCodeUpdate();
	}

	private void GetButtonOrigin()
	{
		buttonOrigin = new List<Vector3>();
		for (int i = 0; i < buttons.Length; i++)
		{
			GameObject gameObject = buttons[i];
			if (null != gameObject)
			{
				buttonOrigin.Add(gameObject.transform.position);
			}
		}
	}

	protected new void Awake()
	{
		GetButtonOrigin();
	}

	private void OnDestroy()
	{
		buttonOrigin.Clear();
		buttonOrigin = null;
	}

	protected override void Start()
	{
		if (buttons.Length < 2)
		{
			return;
		}
		buttonOrigin = new List<Vector3>();
		for (int i = 0; i < buttons.Length; i++)
		{
			GameObject gameObject = buttons[i];
			if (null != gameObject)
			{
				buttonOrigin.Add(gameObject.transform.position);
			}
		}
		buttons[0].SetActiveRecursively(false);
		buttons[1].SetActiveRecursively(true);
	}
}
