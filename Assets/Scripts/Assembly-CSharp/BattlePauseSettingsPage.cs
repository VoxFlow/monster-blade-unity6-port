using UnityEngine;

public class BattlePauseSettingsPage : MonoBehaviour
{
	[SerializeField]
	private UILabel m_muteBtnText;

	[SerializeField]
	private UISprite m_muteBtnTex;

	[SerializeField]
	private UILabel m_vibrateBtnText;

	[SerializeField]
	private UISlider m_musicSlider;

	[SerializeField]
	private UISlider m_sfxSlider;

	private void Start()
	{
		LoadLocalVolumeSetting();
		ChangeVibrateBtnText();
	}

	private void Update()
	{
	}

	public void Show()
	{
		base.gameObject.SetActiveRecursively(true);
	}

	public void Hide()
	{
		base.gameObject.SetActiveRecursively(false);
	}

	public void SetMute()
	{
		SingletonComponent<SoundManager>.Instance.IsMute = ((MBPrefs.GetInt("MS:IsMute", 0) > 0) ? true : false);
		m_muteBtnText.text = SingletonComponent<NBLocalization>.Instance.Get((!SingletonComponent<SoundManager>.Instance.IsMute) ? "ui_settings_mute_on" : "ui_settings_mute_off");
		if (SingletonComponent<SoundManager>.Instance.IsMute)
		{
			SingletonComponent<SoundManager>.Instance.MusicVolume = 0f;
			SingletonComponent<SoundManager>.Instance.SfxVolume = 0f;
		}
	}

	public void OnVibrateBtnClick()
	{
		if (SingletonComponent<SaveData>.Instance == null || SingletonComponent<SaveData>.Instance.GameData == null)
		{
			Dbg.LogWarn("SaveData.Instance or SaveData.Instance.GameData is null, cannot set vibratation");
			return;
		}
		SingletonComponent<SaveData>.Instance.GameData.NeedVibrate = !SingletonComponent<SaveData>.Instance.GameData.NeedVibrate;
		ChangeVibrateBtnText();
	}

	public void OnMuteBtnClick()
	{
		SingletonComponent<SoundManager>.Instance.IsMute = !SingletonComponent<SoundManager>.Instance.IsMute;
		SingletonComponent<SoundManager>.Instance.ChangeMuteBtnText(ref m_muteBtnText);
		SingletonComponent<SoundManager>.Instance.SetSliders(ref m_musicSlider, ref m_sfxSlider);
	}

	public void OnMusicSliderChange(float p_val)
	{
		SingletonComponent<SoundManager>.Instance.OnMusicSliderChange(p_val, ref m_musicSlider, ref m_sfxSlider, ref m_muteBtnText, ref m_muteBtnTex, false);
		MBPrefs.SetFloat("MS:MusicVolume", p_val);
	}

	public void OnSfxSliderChange(float p_val)
	{
		SingletonComponent<SoundManager>.Instance.OnSfxSliderChange(p_val, ref m_musicSlider, ref m_sfxSlider, ref m_muteBtnText, ref m_muteBtnTex, false);
		MBPrefs.SetFloat("MS:SfxVolume", p_val);
	}

	private void OnDisable()
	{
		MBPrefs.SetFloat("MS:MusicVolume", m_musicSlider.sliderValue);
		MBPrefs.SetFloat("MS:SfxVolume", m_sfxSlider.sliderValue);
		MBPrefs.SetInt("MS:IsMute", SingletonComponent<SoundManager>.Instance.IsMute ? 1 : 0);
	}

	private void LoadLocalVolumeSetting()
	{
		int num = MBPrefs.GetInt("MS:IsMute");
		SingletonComponent<SoundManager>.Instance.IsMute = ((num > 0) ? true : false);
		m_musicSlider.sliderValue = MBPrefs.GetFloat("MS:MusicVolume", 0.5f);
		m_sfxSlider.sliderValue = MBPrefs.GetFloat("MS:SfxVolume", 0.5f);
		if (SingletonComponent<SoundManager>.Instance.IsMute)
		{
			m_musicSlider.SetActive(false);
			m_sfxSlider.SetActive(false);
		}
		else
		{
			m_musicSlider.SetActive(true);
			m_sfxSlider.SetActive(true);
		}
		SingletonComponent<SoundManager>.Instance.ChangeMuteBtnText(ref m_muteBtnText);
	}

	private void ChangeVibrateBtnText()
	{
		m_vibrateBtnText.text = SingletonComponent<NBLocalization>.Instance.Get((!SingletonComponent<SaveData>.Instance.GameData.NeedVibrate) ? "ui_settings_vibrate_off" : "ui_settings_vibrate_on");
	}
}
