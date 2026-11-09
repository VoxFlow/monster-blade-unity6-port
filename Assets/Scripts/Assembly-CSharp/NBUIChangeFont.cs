using UnityEngine;

public class NBUIChangeFont : MonoBehaviour
{
	private void Start()
	{
		UILabel component = GetComponent<UILabel>();
		if (!Dbg.Assert(component, "label null ref") && SingletonComponent<NBLocalization>.Instance.currentFont != null)
		{
			component.font = SingletonComponent<NBLocalization>.Instance.currentFont;
		}
	}
}
