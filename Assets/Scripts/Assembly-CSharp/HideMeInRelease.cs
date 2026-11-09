using UnityEngine;

public class HideMeInRelease : MonoBehaviour
{
	private void Start()
	{
		base.gameObject.SetActiveRecursively(false);
		base.enabled = false;
	}
}
