using UnityEngine;

public class Anim_Spiral_Slow : MonoBehaviour
{
	public string m_pathName = string.Empty;

	private void Start()
	{
		Invoke("PlayEffect", 1.5f);
	}

	private void PlayEffect()
	{
		iTween.MoveTo(base.gameObject, iTween.Hash("path", iTweenPath.GetPath(m_pathName), "time", 2, "easetype", iTween.EaseType.easeInCubic));
	}
}
