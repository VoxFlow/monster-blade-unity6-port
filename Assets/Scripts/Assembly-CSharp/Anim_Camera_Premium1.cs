using UnityEngine;

public class Anim_Camera_Premium1 : MonoBehaviour
{
	private void Start()
	{
		iTween.MoveTo(base.gameObject, iTween.Hash("path", iTweenPath.GetPath("CAMERA_PREMIUM1"), "time", 1.5, "easetype", iTween.EaseType.easeInOutSine));
	}

	private void Update()
	{
	}
}
