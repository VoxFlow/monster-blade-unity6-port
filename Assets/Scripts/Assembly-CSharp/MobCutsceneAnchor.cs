using UnityEngine;

public class MobCutsceneAnchor : MonoBehaviour
{
	public Transform m_anchor;

	private void Awake()
	{
		Messenger<GameObject>.AddListener(CutsceneMessage.CUTSCENE_EVENT_ENABLED, OnCutsceneEnabled);
	}

	private void OnDestroy()
	{
		Messenger<GameObject>.RemoveListener(CutsceneMessage.CUTSCENE_EVENT_ENABLED, OnCutsceneEnabled);
	}

	private void OnCutsceneEnabled(GameObject p_obj)
	{
		if (base.transform.IsChildOf(p_obj.transform))
		{
			BattleManager instance = SingletonComponent<BattleManager>.Instance;
			if (m_anchor != null && instance != null)
			{
				Vector3 position = m_anchor.position;
				Vector3 normalized = (base.transform.position - m_anchor.position).normalized;
				position += normalized * instance.BattleReferenceDistance;
				base.transform.position = position;
			}
		}
	}
}
