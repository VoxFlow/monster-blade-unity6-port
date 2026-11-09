using UnityEngine;

public class Anim_Camera_Social : MonoBehaviour
{
	private void Start()
	{
		iTween.MoveTo(base.gameObject, iTween.Hash("path", iTweenPath.GetPath("CAMERA_SOCIAL"), "time", 3, "easetype", iTween.EaseType.easeInOutSine));
	}

	private void Update()
	{
	}
}
