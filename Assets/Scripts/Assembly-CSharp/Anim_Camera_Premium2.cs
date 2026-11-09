using UnityEngine;

public class Anim_Camera_Premium2 : MonoBehaviour
{
	private void Start()
	{
		iTween.MoveTo(base.gameObject, iTween.Hash("path", iTweenPath.GetPath("CAMERA_PREMIUM2"), "time", 3, "easetype", iTween.EaseType.easeInOutSine));
	}

	private void Update()
	{
	}
}
