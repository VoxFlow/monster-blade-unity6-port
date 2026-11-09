using UnityEngine;

public class Anim_Spiral4 : MonoBehaviour
{
	private void Start()
	{
		iTween.MoveTo(base.gameObject, iTween.Hash("path", iTweenPath.GetPath("SPIRAL4"), "time", 2, "easetype", iTween.EaseType.easeInCubic));
	}

	private void Update()
	{
	}
}
