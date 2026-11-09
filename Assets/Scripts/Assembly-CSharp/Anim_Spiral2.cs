using UnityEngine;

public class Anim_Spiral2 : MonoBehaviour
{
	private void Start()
	{
		iTween.MoveTo(base.gameObject, iTween.Hash("path", iTweenPath.GetPath("SPIRAL2"), "time", 2, "easetype", iTween.EaseType.easeInCubic));
	}

	private void Update()
	{
	}
}
