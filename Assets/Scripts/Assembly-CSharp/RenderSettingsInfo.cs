using UnityEngine;

public class RenderSettingsInfo : MonoBehaviour
{
	public bool m_fogEnabled;

	public Color m_fogColor = new Color(0f, 0f, 0f, 255f);

	public FogMode m_fogMode = FogMode.Linear;

	public float m_fogDensity = 0.01f;

	public float m_fogLinearStart = 1f;

	public float m_fogLinearEnd = 1f;

	public Color m_ambientColor = new Color(0f, 0f, 0f, 255f);

	public float m_haloStrength = 0.5f;

	public float m_flareStrength = 0.2f;

	public Material m_skybox;

	private void Start()
	{
		base.enabled = true;
		RenderSettings.fog = m_fogEnabled;
		RenderSettings.fogColor = m_fogColor;
		RenderSettings.fogMode = m_fogMode;
		RenderSettings.fogDensity = m_fogDensity;
		RenderSettings.fogStartDistance = m_fogLinearStart;
		RenderSettings.fogEndDistance = m_fogLinearEnd;
		RenderSettings.ambientLight = m_ambientColor;
		RenderSettings.haloStrength = m_haloStrength;
		RenderSettings.flareStrength = m_flareStrength;
		RenderSettings.skybox = m_skybox;
		base.enabled = false;
	}
}
