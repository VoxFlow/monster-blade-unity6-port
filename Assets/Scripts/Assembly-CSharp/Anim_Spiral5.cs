using UnityEngine;

public class Anim_Spiral5 : MonoBehaviour
{
	private void Start()
	{
		iTween.MoveTo(base.gameObject, iTween.Hash("path", iTweenPath.GetPath("SPIRAL5"), "time", 2, "easetype", iTween.EaseType.easeInCubic));
	}

	private void Update()
	{
	}
}
