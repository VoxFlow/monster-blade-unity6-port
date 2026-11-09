using UnityEngine;

public class DebugPlayerTemplate : MonoBehaviour
{
	public void OnApplicationPause(bool p_pause)
	{
		if (!p_pause)
		{
			PrintOutMaterialInfo();
		}
	}

	public void OnApplicationFocus(bool p_focus)
	{
		if (p_focus)
		{
			PrintOutMaterialInfo();
		}
	}

	private void PrintOutMaterialInfo()
	{
	}
}
