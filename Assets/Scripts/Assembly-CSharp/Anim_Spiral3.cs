using UnityEngine;

public class Anim_Spiral3 : MonoBehaviour
{
	private void Start()
	{
		iTween.MoveTo(base.gameObject, iTween.Hash("path", iTweenPath.GetPath("SPIRAL3"), "time", 2, "easetype", iTween.EaseType.easeInCubic));
	}

	private void Update()
	{
	}
}
