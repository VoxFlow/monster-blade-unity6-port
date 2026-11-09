using UnityEngine;

public class AutoUpdateQualitySetting : MonoBehaviour
{
	private float m_avgFps;

	private float m_time;

	private float m_refreshTime;

	private int m_frame;

	private static readonly float maxRefreshTime = 120f;

	private void Update()
	{
		if (!GameManager.autoDetectQualitySetting)
		{
			return;
		}
		if (m_refreshTime > 0f)
		{
			m_refreshTime += Time.deltaTime;
			return;
		}
		m_time += Time.deltaTime;
		m_frame++;
		if (m_time >= 1f)
		{
			m_avgFps = (float)m_frame / m_time;
			GameManager.QualitySetting qualitySetting = GameManager.qualitySetting;
			if (m_avgFps <= 25f && GameManager.qualitySetting != GameManager.QualitySetting.Low)
			{
				qualitySetting = GameManager.qualitySetting + 1;
			}
			else if (m_avgFps >= 45f && GameManager.qualitySetting != GameManager.QualitySetting.High)
			{
				qualitySetting = GameManager.qualitySetting - 1;
			}
			if (GameManager.qualitySetting != qualitySetting)
			{
				GameManager.qualitySetting = qualitySetting;
				Dbg.LogWarn(string.Concat("Quality setting is changing from ", GameManager.qualitySetting, " to ", qualitySetting));
				m_refreshTime = maxRefreshTime;
			}
			m_frame = 0;
			m_time = 0f;
		}
	}
}
