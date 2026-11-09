using UnityEngine;

public class CutscenePlaySequence : MonoBehaviour
{
	public GameObject m_nextObject;

	private void PlayNextCutscene()
	{
		if (m_nextObject != null)
		{
			m_nextObject.SetActiveRecursively(true);
			m_nextObject.SendMessage("Start", SendMessageOptions.DontRequireReceiver);
			base.gameObject.SetActiveRecursively(false);
		}
	}
}
