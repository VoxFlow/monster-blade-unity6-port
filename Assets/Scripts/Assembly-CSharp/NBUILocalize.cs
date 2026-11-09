using UnityEngine;

[RequireComponent(typeof(UILabel))]
[AddComponentMenu("Nubee/Localize")]
public class NBUILocalize : MonoBehaviour
{
	public string key;

	public GameObject eventTarget;

	private string mLanguage;

	public string Key
	{
		get
		{
			return key;
		}
		set
		{
			key = value;
			Start();
		}
	}

	private void OnLocalize(NBLocalization loc)
	{
		UILabel component = GetComponent<UILabel>();
		if (Dbg.Assert(component, "label null ref"))
		{
			return;
		}
		if (SingletonComponent<NBLocalization>.Instance.currentFont != null)
		{
			component.font = SingletonComponent<NBLocalization>.Instance.currentFont;
		}
		if (string.IsNullOrEmpty(key))
		{
			return;
		}
		string text = loc.Get(key);
		if (!string.IsNullOrEmpty(text))
		{
			if (component != null)
			{
				component.text = text;
				if (null != eventTarget)
				{
					eventTarget.SendMessage("OnLocalizationChanged", component, SendMessageOptions.DontRequireReceiver);
				}
			}
		}
		else
		{
			component.text = key;
		}
	}

	private void Start()
	{
		if (SingletonComponent<NBLocalization>.Instance != null)
		{
			OnLocalize(SingletonComponent<NBLocalization>.Instance);
		}
	}

	private void OnEnable()
	{
		Start();
	}
}
