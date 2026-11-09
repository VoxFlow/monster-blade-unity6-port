using UnityEngine;

public class HalfDungeonWolfCutscene : HalfDungeonCutscene
{
	protected override void Awake()
	{
		base.Awake();
		if (m_playerGameObject != null)
		{
			m_playerGameObject.GetComponent<Animation>().Play("Runloop");
		}
	}

	protected override void OnCutsceneEnabled(object p_objectName)
	{
		if (!(m_playerGameObject == null))
		{
			GameObject gameObject = GameObject.Find((string)p_objectName);
			if (gameObject != null)
			{
				Transform parent = gameObject.transform.FindInHierarchy("Male");
				m_playerGameObject.transform.parent = parent;
				m_playerGameObject.transform.localPosition = Vector3.zero;
				m_playerGameObject.transform.localRotation = Quaternion.identity;
			}
		}
	}
}
