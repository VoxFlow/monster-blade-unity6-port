using UnityEngine;

public class Anim_Spiral_Fast : MonoBehaviour
{
	public string m_pathName = string.Empty;

	private void Start()
	{
		Invoke("PlayEffect", 3f);
	}

	private void PlayEffect()
	{
		iTween.MoveTo(base.gameObject, iTween.Hash("path", iTweenPath.GetPath(m_pathName), "time", 2, "easetype", iTween.EaseType.easeInExpo));
	}
}
