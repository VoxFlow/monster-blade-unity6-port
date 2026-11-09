using UnityEngine;

[ExecuteInEditMode]
public class CharacterAmbientColor : MonoBehaviour
{
	private SkinnedMeshRenderer[] m_renderers;

	private void Start()
	{
		if (SingletonComponent<LevelSetting>.Instance != null)
		{
			m_renderers = MBHier.GetComponentsInChildren<SkinnedMeshRenderer>(base.gameObject);
			SkinnedMeshRenderer[] renderers = m_renderers;
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in renderers)
			{
				skinnedMeshRenderer.sharedMaterial.SetColor("_AmbientColor", SingletonComponent<LevelSetting>.Instance.ambientLight);
			}
		}
	}
}
