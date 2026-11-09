using UnityEngine;

public class TitleScreenUiReceiver : MonoBehaviour
{
	public string m_nextScreen = string.Empty;

	public float m_timeClickRestriction = 5f;

	private bool m_clicked;

	public GameObject m_tapToContinueSprite;

	private bool m_isTapButtonAnimationRunning;

	private float m_updatedTime;

	[SerializeField]
	private AnimationClip m_cameraAnimation;

	[SerializeField]
	private AnimationClip m_flyAnimation;

	[SerializeField]
	private Animation m_cameraAnimationComp;

	[SerializeField]
	private Animation m_dragonAnimComp;

	[SerializeField]
	private TitleDrangonAnimController dragonAnimController;

	[SerializeField]
	private UILabel m_nubeeID;

	[SerializeField]
	private UILabel m_versionNumber;

	private float alphaValue;

	public void Awake()
	{
		m_clicked = false;
		NBProfiler.PrintUsedHeap("[TITLE_SCREEN_HEAP_AT_AWAKE]");
		PlayMusicOnStart component = GetComponent<PlayMusicOnStart>();
		if (component != null)
		{
			component.m_music = "list_e_maintheme_music";
		}
	}

	private void Start()
	{
		NBProfiler.EndTask("[LOAD_TITLE_SCREEN]");
		NBProfiler.BeginTask("[TITLE_SCREEN]");
		base.enabled = false;
		UpdateDisplayInfo();
	}

	public void UpdateDisplayInfo(string p_nubeeID = "...")
	{
		if (null != SingletonComponent<SaveData>.Instance && !SingletonComponent<SaveData>.Instance.GameData.IsFirstTimeToRun)
		{
			p_nubeeID = SingletonComponent<SaveData>.Instance.GameData.NubeeId;
		}
		if (null != m_nubeeID)
		{
			m_nubeeID.text = "Nubee ID: " + p_nubeeID;
			if (!m_nubeeID.gameObject.active)
			{
				m_nubeeID.gameObject.SetActiveRecursively(true);
			}
		}
		if (null != m_versionNumber)
		{
			m_versionNumber.text = "App Version: " + GLOBAL.STORE_APP_VERSION;
			if (!m_versionNumber.gameObject.active)
			{
				m_versionNumber.gameObject.SetActiveRecursively(true);
			}
		}
	}

	private void PlayFlyAnimation()
	{
		m_clicked = true;
		if (m_cameraAnimation == null || m_cameraAnimationComp == null || m_flyAnimation == null || m_dragonAnimComp == null)
		{
			Dbg.LogError("No animation given");
			GoToMenu();
		}
		if (dragonAnimController != null)
		{
			dragonAnimController.enabled = false;
		}
		m_cameraAnimationComp.CrossFade(m_cameraAnimation.name);
		m_dragonAnimComp.CrossFade(m_flyAnimation.name);
	}

	private void GoToMenu()
	{
		Dbg.Log("Go To Menu Start");
		NBProfiler.EndTask("[TITLE_SCREEN]");
		NBProfiler.BeginTask("[LOAD_MAINMENU_INIT]");
		AnalyticsManager.FirstInstallEvents("tut_titlecontinue");
		SoundManager instance = SingletonComponent<SoundManager>.Instance;
		if (instance != null)
		{
			instance.StopTitleMusic();
			instance.StopMusic(true);
		}
		if (SingletonComponent<AnalyticsManager>.Instance != null)
		{
			SingletonComponent<AnalyticsManager>.Instance.StartPinger();
		}
		if (UIPopupController.Instance != null)
		{
			UIPopupController.Instance.ShowFullScreenLoading(UIPopupController.LoadingScreenMode.FULLSCREEN_MONSTER_BLADE);
		}
		Dbg.Log(" Should Load Next Scene : " + m_nextScreen);
		if (SingletonComponent<GameManager>.Instance != null)
		{
			SingletonComponent<GameManager>.Instance.LoadScene(m_nextScreen, true, false);
		}
		base.enabled = false;
	}

	public void Update()
	{
		if (m_dragonAnimComp != null)
		{
			if (!m_dragonAnimComp.gameObject.active)
			{
				return;
			}
			if (!m_dragonAnimComp.isPlaying && m_clicked)
			{
				GoToMenu();
				return;
			}
			if (m_dragonAnimComp.isPlaying && m_clicked)
			{
				return;
			}
		}
		if (MBInput.anyKey || MBInput.touchCount > 0)
		{
			PlayFlyAnimation();
			SingletonComponent<SoundManager>.Instance.PlayTitleTapSound();
		}
		if (m_isTapButtonAnimationRunning)
		{
			return;
		}
		m_updatedTime += Time.deltaTime;
		if (m_updatedTime > 2f)
		{
			alphaValue += Time.deltaTime;
			float num = 1f - alphaValue * 0.4f;
			if (num < 0f)
			{
				num = 0f;
			}
		}
		if (m_updatedTime > 7f)
		{
			StartAnimationForTapToContinue();
			m_isTapButtonAnimationRunning = true;
			if (m_tapToContinueSprite != null)
			{
				m_tapToContinueSprite.SetActiveRecursively(true);
			}
		}
	}

	public void StartAnimationForTapToContinue()
	{
		if (m_tapToContinueSprite != null)
		{
			Vector3 vector = new Vector3(385f, 48f, 0f);
			iTween.ScaleFrom(m_tapToContinueSprite, iTween.Hash("scale", vector, "easeType", "easeInOutExpo", "loopType", "pingPong"));
		}
	}
}
