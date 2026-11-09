using UnityEngine;

public class SettingsMenuPanel : NBUIMenuPanel
{
	[SerializeField]
	private UISlider m_musicSlider;

	[SerializeField]
	private UISlider m_sfxSlider;

	[SerializeField]
	private UILabel m_muteOnOffBtnText;

	[SerializeField]
	private UISprite m_muteBtnTex;

	[SerializeField]
	private UILabel m_vibrateBtnText;

	[SerializeField]
	private UISprite m_vibrateBtnTex;

	private void OnEnable()
	{
	}

	private void Start()
	{
	}

	private new void Update()
	{
	}

	public void LoadLocalVolumeSetting()
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
		SingletonComponent<SoundManager>.Instance.ChangeMuteBtnText(ref m_muteOnOffBtnText);
		SingletonComponent<SoundManager>.Instance.ChangeOptionBtnTexture(ref m_muteBtnTex, SingletonComponent<SoundManager>.Instance.IsMute);
	}

	public void SetVibrateBtnState()
	{
		ChangeVibrateBtnText();
		SingletonComponent<SoundManager>.Instance.ChangeOptionBtnTexture(ref m_vibrateBtnTex, !SingletonComponent<SaveData>.Instance.GameData.NeedVibrate);
	}

	public void OnClose()
	{
		if (!(SingletonComponent<SoundManager>.Instance.MusicVolume <= 0f) && !(SingletonComponent<SoundManager>.Instance.SfxVolume <= 0f))
		{
			MBPrefs.SetFloat("MS:MusicVolume", SingletonComponent<SoundManager>.Instance.MusicVolume);
			MBPrefs.SetFloat("MS:SfxVolume", SingletonComponent<SoundManager>.Instance.SfxVolume);
		}
		MBPrefs.SetInt("MS:IsMute", SingletonComponent<SoundManager>.Instance.IsMute ? 1 : 0);
		UIPopupController.Instance.CloseCurrentPopup();
	}

	public void OnMuteClick()
	{
		SingletonComponent<SoundManager>.Instance.IsMute = !SingletonComponent<SoundManager>.Instance.IsMute;
		SingletonComponent<SoundManager>.Instance.ChangeMuteBtnText(ref m_muteOnOffBtnText);
		SingletonComponent<SoundManager>.Instance.ChangeOptionBtnTexture(ref m_muteBtnTex, SingletonComponent<SoundManager>.Instance.IsMute);
		SingletonComponent<SoundManager>.Instance.SetSliders(ref m_musicSlider, ref m_sfxSlider);
	}

	public void OnMusicSliderChange(float p_val)
	{
		SingletonComponent<SoundManager>.Instance.OnMusicSliderChange(p_val, ref m_musicSlider, ref m_sfxSlider, ref m_muteOnOffBtnText, ref m_muteBtnTex, true);
	}

	public void OnSfxSliderChange(float p_val)
	{
		SingletonComponent<SoundManager>.Instance.OnSfxSliderChange(p_val, ref m_musicSlider, ref m_sfxSlider, ref m_muteOnOffBtnText, ref m_muteBtnTex, true);
	}

	public void OnVibrateClick()
	{
		if (SingletonComponent<SaveData>.Instance == null || SingletonComponent<SaveData>.Instance.GameData == null)
		{
			Dbg.LogWarn("SaveData.Instance or SaveData.Instance.GameData is null, cannot set vibratation");
			return;
		}
		SingletonComponent<SaveData>.Instance.GameData.NeedVibrate = !SingletonComponent<SaveData>.Instance.GameData.NeedVibrate;
		ChangeVibrateBtnText();
		SingletonComponent<SoundManager>.Instance.ChangeOptionBtnTexture(ref m_vibrateBtnTex, !SingletonComponent<SaveData>.Instance.GameData.NeedVibrate);
	}

	private void ChangeVibrateBtnText()
	{
		m_vibrateBtnText.text = SingletonComponent<NBLocalization>.Instance.Get((!SingletonComponent<SaveData>.Instance.GameData.NeedVibrate) ? "ui_settings_vibrate_off" : "ui_settings_vibrate_on");
	}
}
