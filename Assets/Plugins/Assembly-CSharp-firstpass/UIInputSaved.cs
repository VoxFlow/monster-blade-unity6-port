using UnityEngine;

[AddComponentMenu("NGUI/UI/Input (Saved)")]
public class UIInputSaved : UIInput
{
	public string playerPrefsField;

	private void Start()
	{
		Init();
		if (!string.IsNullOrEmpty(playerPrefsField) && MBPrefs.HasKey(playerPrefsField))
		{
			base.text = MBPrefs.GetString(playerPrefsField);
		}
	}

	private void OnApplicationQuit()
	{
		if (!string.IsNullOrEmpty(playerPrefsField))
		{
			MBPrefs.SetString(playerPrefsField, base.text);
		}
	}
}
